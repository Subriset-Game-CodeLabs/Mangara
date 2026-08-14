using System.Collections.Generic;
using Item;
using Manager;
using Save;
using UnityEngine;

namespace RandomEvent
{
    public class RandomEventManager : Singleton<RandomEventManager>
    {
        [System.Serializable]
        public class SpawnedItemRecord
        {
            public string ZoneID;
            public string EventID;
            public ItemObject GameObjectInstance;
            public ItemBaseSO ItemData;
            public int Quantity;
            public Vector3 Position;
        }

        [SerializeField] private List<EventSpawnZone> _spawnZones = new List<EventSpawnZone>();
        [SerializeField] private GameObject _itemObjectPrefab;
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private bool _spawnOnFirstLaunch = true;

        private readonly List<SpawnedItemRecord> _activeSpawnedItems = new List<SpawnedItemRecord>();

        public IReadOnlyList<SpawnedItemRecord> ActiveSpawnedItems => _activeSpawnedItems;

        protected override void Awake()
        {
            base.Awake();
            RefreshZonesIfEmpty();
        }

        private void Start()
        {
            SubscribeToGameManager();
            if (_spawnOnFirstLaunch && _activeSpawnedItems.Count == 0)
            {
                EvaluateAndSpawnEvents();
            }
        }

        private void OnEnable()
        {
            SubscribeToGameManager();
        }

        private void OnDisable()
        {
            UnsubscribeFromGameManager();
        }

        private void SubscribeToGameManager()
        {
            if (GameManager.Instance != null)
            {
                // Unsubscribe first to prevent duplicate subscriptions
                GameManager.Instance.OnNewDay -= HandleNewDay;
                GameManager.Instance.OnNewDay += HandleNewDay;
            }
        }

        private void UnsubscribeFromGameManager()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewDay -= HandleNewDay;
            }
        }

        private void RefreshZonesIfEmpty()
        {
            if (_spawnZones == null || _spawnZones.Count == 0)
            {
                _spawnZones = new List<EventSpawnZone>(FindObjectsByType<EventSpawnZone>(FindObjectsSortMode.None));
            }
        }

        private void HandleNewDay()
        {
            EvaluateAndSpawnEvents();
        }

        public int GetActiveTrashCount()
        {
            int count = 0;
            foreach (var record in _activeSpawnedItems)
            {
                if (record != null && record.GameObjectInstance != null && record.ItemData != null && record.ItemData.IsTrash)
                {
                    count += record.Quantity;
                }
            }
            return count;
        }

        public int GetActiveItemCountForEvent(RandomEventSO eventSO)
        {
            if (eventSO == null) return 0;
            int count = 0;
            foreach (var record in _activeSpawnedItems)
            {
                if (record != null && record.GameObjectInstance != null)
                {
                    if (!string.IsNullOrEmpty(record.EventID) && !string.IsNullOrEmpty(eventSO.EventID) && record.EventID == eventSO.EventID)
                    {
                        count += record.Quantity;
                    }
                    else if (record.ItemData != null && eventSO.SpawnEntries != null)
                    {
                        foreach (var entry in eventSO.SpawnEntries)
                        {
                            if (entry.Item != null && entry.Item.ItemID == record.ItemData.ItemID)
                            {
                                count += record.Quantity;
                                break;
                            }
                        }
                    }
                }
            }
            return count;
        }

        public void DespawnAllActiveItems()
        {
            for (int i = _activeSpawnedItems.Count - 1; i >= 0; i--)
            {
                var record = _activeSpawnedItems[i];
                if (record.GameObjectInstance != null)
                {
                    record.GameObjectInstance.OnCollected -= HandleItemCollected;
                    Destroy(record.GameObjectInstance.gameObject);
                }
            }
            _activeSpawnedItems.Clear();
            Progression.ProgressionManager.Instance?.RecalculateEcosystemHealth();
        }

        public void EvaluateAndSpawnEvents(bool clearExisting = false)
        {
            if (clearExisting)
            {
                DespawnAllActiveItems();
            }
            RefreshZonesIfEmpty();

            if (_spawnZones == null || _spawnZones.Count == 0)
            {
                Debug.Log("[RandomEventManager] No EventSpawnZones found in scene.");
                return;
            }

            int milestoneIndex = Progression.ProgressionManager.Instance != null 
                ? Progression.ProgressionManager.Instance.UnlockedMilestoneIndex 
                : 0;
            float trashMultiplier = Mathf.Clamp01(1.0f - (milestoneIndex * 0.25f));

            foreach (var zone in _spawnZones)
            {
                if (zone == null || zone.AssignedEvent == null) continue;

                RandomEventSO eventSO = zone.AssignedEvent;

                if (eventSO.MaxActiveCap > 0)
                {
                    int currentActive = GetActiveItemCountForEvent(eventSO);
                    if (currentActive >= eventSO.MaxActiveCap)
                    {
                        Debug.Log($"[RandomEventManager] Event '{eventSO.EventName}' has reached max active cap ({currentActive}/{eventSO.MaxActiveCap}). Skipping spawn.");
                        continue;
                    }
                }

                float roll = Random.value;

                if (roll <= eventSO.Probability)
                {
                    int count = Random.Range(eventSO.MinSpawnCount, eventSO.MaxSpawnCount + 1);
                    if (count <= 0 || eventSO.SpawnEntries == null || eventSO.SpawnEntries.Count == 0) continue;

                    if (eventSO.MaxActiveCap > 0)
                    {
                        int currentActive = GetActiveItemCountForEvent(eventSO);
                        int remainingAllowed = eventSO.MaxActiveCap - currentActive;
                        if (remainingAllowed <= 0) continue;
                        count = Mathf.Min(count, remainingAllowed);
                    }

                    List<Vector3> spawnPositions = zone.GetRandomSpawnPositions(count);

                    foreach (Vector3 pos in spawnPositions)
                    {
                        SpawnEntry selectedEntry = SelectWeightedItem(eventSO.SpawnEntries);
                        if (selectedEntry.Item == null) continue;

                        if (selectedEntry.Item.IsTrash && trashMultiplier <= 0.01f)
                        {
                            continue;
                        }

                        SpawnItem(zone.ZoneID, eventSO.EventID, selectedEntry.Item, selectedEntry.Quantity, pos);
                    }

                    Debug.Log($"[RandomEventManager] Event '{eventSO.EventName}' triggered in Zone '{zone.ZoneID}'! Spawned {spawnPositions.Count} items.");
                }
            }

            Progression.ProgressionManager.Instance?.RecalculateEcosystemHealth();
        }

        private SpawnEntry SelectWeightedItem(List<SpawnEntry> entries)
        {
            if (entries == null || entries.Count == 0) return default;

            float totalWeight = 0f;
            foreach (var entry in entries)
            {
                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0f)
            {
                return entries[Random.Range(0, entries.Count)];
            }

            float randomWeight = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var entry in entries)
            {
                currentWeight += entry.Weight;
                if (randomWeight <= currentWeight)
                {
                    return entry;
                }
            }

            return entries[entries.Count - 1];
        }

        private void SpawnItem(string zoneID, string eventID, ItemBaseSO itemData, int quantity, Vector3 position)
        {
            GameObject prefabToSpawn = (itemData != null && itemData.ItemPrefab != null) 
                ? itemData.ItemPrefab 
                : _itemObjectPrefab;

            if (prefabToSpawn == null)
            {
                Debug.LogError($"[RandomEventManager] Cannot spawn item '{itemData?.ItemName}'! Neither itemData.ItemPrefab nor _itemObjectPrefab is assigned.");
                return;
            }

            GameObject obj = Instantiate(prefabToSpawn, position, Quaternion.identity);
            ItemObject itemObject = obj.GetComponent<ItemObject>();
            if (itemObject == null)
            {
                itemObject = obj.AddComponent<ItemObject>();
            }

            itemObject.Initialize(itemData, quantity);
            itemObject.OnCollected += HandleItemCollected;

            _activeSpawnedItems.Add(new SpawnedItemRecord
            {
                ZoneID = zoneID,
                EventID = eventID,
                GameObjectInstance = itemObject,
                ItemData = itemData,
                Quantity = quantity,
                Position = position
            });
        }

        private void HandleItemCollected(ItemObject itemObject)
        {
            if (itemObject != null)
            {
                itemObject.OnCollected -= HandleItemCollected;
                _activeSpawnedItems.RemoveAll(r => r.GameObjectInstance == itemObject);
                Progression.ProgressionManager.Instance?.RecalculateEcosystemHealth();
            }
        }

        public List<EventSpawnSaveData> GetSaveData()
        {
            var zoneDataMap = new Dictionary<string, EventSpawnSaveData>();

            foreach (var record in _activeSpawnedItems)
            {
                if (record.GameObjectInstance == null || record.ItemData == null) continue;

                string zoneID = record.ZoneID ?? "";
                if (!zoneDataMap.TryGetValue(zoneID, out var zoneSaveData))
                {
                    zoneSaveData = new EventSpawnSaveData
                    {
                        ZoneID = zoneID,
                        SpawnedItems = new List<SpawnedItemData>()
                    };
                    zoneDataMap[zoneID] = zoneSaveData;
                }

                Vector3 currentPos = record.GameObjectInstance.transform.position;
                zoneSaveData.SpawnedItems.Add(new SpawnedItemData
                {
                    ItemID = record.ItemData.ItemID,
                    EventID = record.EventID,
                    Quantity = record.Quantity,
                    PosX = currentPos.x,
                    PosY = currentPos.y,
                    PosZ = currentPos.z
                });
            }

            return new List<EventSpawnSaveData>(zoneDataMap.Values);
        }

        public void LoadFromSaveData(List<EventSpawnSaveData> saveData, ItemDatabaseSO itemDatabase = null)
        {
            DespawnAllActiveItems();

            if (saveData == null) return;

            ItemDatabaseSO db = itemDatabase != null ? itemDatabase : _itemDatabase;
            if (db == null)
            {
                Debug.LogError("[RandomEventManager] Cannot load saved items because ItemDatabaseSO is null!");
                return;
            }

            foreach (var zoneData in saveData)
            {
                if (zoneData.SpawnedItems == null) continue;

                foreach (var itemData in zoneData.SpawnedItems)
                {
                    ItemBaseSO itemSO = db.GetItemByID(itemData.ItemID);
                    if (itemSO == null)
                    {
                        Debug.LogWarning($"[RandomEventManager] Could not find item with ID '{itemData.ItemID}' in ItemDatabaseSO.");
                        continue;
                    }

                    Vector3 position = new Vector3(itemData.PosX, itemData.PosY, itemData.PosZ);
                    SpawnItem(zoneData.ZoneID, itemData.EventID, itemSO, itemData.Quantity, position);
                }
            }
        }
    }
}
