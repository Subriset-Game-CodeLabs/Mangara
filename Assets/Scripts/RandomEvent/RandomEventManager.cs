using System.Collections.Generic;
using Item;
using Manager;
using Save;
using UnityEngine;

namespace RandomEvent
{
    public class RandomEventManager : PersistentSingleton<RandomEventManager>
    {
        [System.Serializable]
        public class SpawnedItemRecord
        {
            public string ZoneID;
            public ItemObject GameObjectInstance;
            public ItemBaseSO ItemData;
            public int Quantity;
            public Vector3 Position;
        }

        [SerializeField] private List<EventSpawnZone> _spawnZones = new List<EventSpawnZone>();
        [SerializeField] private GameObject _itemObjectPrefab;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

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
        }

        public void EvaluateAndSpawnEvents()
        {
            DespawnAllActiveItems();
            RefreshZonesIfEmpty();

            if (_spawnZones == null || _spawnZones.Count == 0)
            {
                Debug.Log("[RandomEventManager] No EventSpawnZones found in scene.");
                return;
            }

            foreach (var zone in _spawnZones)
            {
                if (zone == null || zone.AssignedEvent == null) continue;

                RandomEventSO eventSO = zone.AssignedEvent;
                float roll = Random.value;

                if (roll <= eventSO.Probability)
                {
                    int count = Random.Range(eventSO.MinSpawnCount, eventSO.MaxSpawnCount + 1);
                    if (count <= 0 || eventSO.SpawnEntries == null || eventSO.SpawnEntries.Count == 0) continue;

                    List<Vector3> spawnPositions = zone.GetRandomSpawnPositions(count);

                    foreach (Vector3 pos in spawnPositions)
                    {
                        SpawnEntry selectedEntry = SelectWeightedItem(eventSO.SpawnEntries);
                        if (selectedEntry.Item == null) continue;

                        SpawnItem(zone.ZoneID, selectedEntry.Item, selectedEntry.Quantity, pos);
                    }

                    Debug.Log($"[RandomEventManager] Event '{eventSO.EventName}' triggered in Zone '{zone.ZoneID}'! Spawned {spawnPositions.Count} items.");
                }
            }
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

        private void SpawnItem(string zoneID, ItemBaseSO itemData, int quantity, Vector3 position)
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
                    SpawnItem(zoneData.ZoneID, itemSO, itemData.Quantity, position);
                }
            }
        }
    }
}
