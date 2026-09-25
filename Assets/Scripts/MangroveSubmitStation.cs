using System.Collections.Generic;
using Item;
using Manager;
using Progression;
using Save;
using UnityEngine;

public class MangroveSubmitStation : MonoBehaviour, IInteractable
{
    [Header("Station Settings")]
    [SerializeField] private string _stationID;
    [SerializeField] private List<ItemBaseSO> _acceptedItems;

    [Header("Goal Phase Settings")]
    [Tooltip("0 = Goal Phase 1, 1 = Goal Phase 2, 2 = Goal Phase 3, 3 = Goal Phase 4")]
    [SerializeField] private int _goalPhaseIndex = 0;

    [Header("Spawn Locations")]
    [SerializeField] private Transform _displaySpawnPoint;
    [SerializeField] private Transform _permanentSpawnPoint;

    private ItemBaseSO _pendingSubmittedItem;
    private GameObject _currentDisplayInstance;

    private bool _isPermanentPlantSpawned;
    private ItemBaseSO _permanentItem;
    private GameObject _permanentPlantInstance;

    public string StationID => _stationID;
    public int GoalPhaseIndex => _goalPhaseIndex;
    public bool HasPendingSubmission => _pendingSubmittedItem != null;
    public bool IsPermanentPlantSpawned => _isPermanentPlantSpawned;

    private void Awake()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSubmitStation(this);
        }
    }

    private void OnEnable()
    {
        SubscribeToManagers();
        UpdateStationVisibility();
    }

    private void OnDisable()
    {
        UnsubscribeFromManagers();
    }

    private void Start()
    {
        SubscribeToManagers();
        UpdateStationVisibility();
    }

    private void SubscribeToManagers()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.OnHealthMilestoneUnlocked -= HandleMilestoneUnlocked;
            ProgressionManager.Instance.OnHealthMilestoneUnlocked += HandleMilestoneUnlocked;
            ProgressionManager.Instance.OnGoalChanged -= HandleGoalChanged;
            ProgressionManager.Instance.OnGoalChanged += HandleGoalChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewDay -= HandleNewDay;
            GameManager.Instance.OnNewDay += HandleNewDay;
        }
    }

    private void UnsubscribeFromManagers()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.OnHealthMilestoneUnlocked -= HandleMilestoneUnlocked;
            ProgressionManager.Instance.OnGoalChanged -= HandleGoalChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewDay -= HandleNewDay;
        }
    }

    public void UpdateStationVisibility(int currentGoalIndex, bool isAllCompleted)
    {
        bool shouldBeVisible = isAllCompleted || (_goalPhaseIndex <= currentGoalIndex);
        if (gameObject.activeSelf != shouldBeVisible)
        {
            gameObject.SetActive(shouldBeVisible);
        }
    }

    public void UpdateStationVisibility()
    {
        if (ProgressionManager.Instance == null) return;
        UpdateStationVisibility(ProgressionManager.Instance.CurrentGoalIndex, ProgressionManager.Instance.IsAllGoalsCompleted);
    }

    private void HandleGoalChanged(ProgressionGoalSO newGoal)
    {
        UpdateStationVisibility();
        CheckPendingToPermanentConversion();
    }

    private void HandleNewDay()
    {
        UpdateStationVisibility();
        CheckPendingToPermanentConversion();
    }

    private void HandleMilestoneUnlocked(int milestoneIndex, float cap)
    {
        CheckPendingToPermanentConversion();
    }

    private void CheckPendingToPermanentConversion()
    {
        if (_pendingSubmittedItem == null || _isPermanentPlantSpawned)
        {
            return;
        }

        if (ProgressionManager.Instance != null)
        {
            bool isGoalCompleted = ProgressionManager.Instance.IsAllGoalsCompleted ||
                                   (_goalPhaseIndex < ProgressionManager.Instance.CurrentGoalIndex);

            if (isGoalCompleted)
            {
                ConvertPendingToPermanent();
            }
        }
    }

    private void ConvertPendingToPermanent()
    {
        if (_pendingSubmittedItem == null || _isPermanentPlantSpawned)
        {
            return;
        }

        DestroyPreviewModel();

        _permanentItem = _pendingSubmittedItem;
        _pendingSubmittedItem = null;
        _isPermanentPlantSpawned = true;

        SpawnPermanentModel();
    }

    public string GetInteractText()
    {
        if (_isPermanentPlantSpawned)
        {
            return "Station Complete";
        }

        if (_pendingSubmittedItem != null)
        {
            return "Mangrove Submitted (Awaiting Sleep)";
        }

        InventoryItem selectedSlotItem = InventoryController.Instance.GetSelectedItem();
        if (!selectedSlotItem.IsEmpty && IsAcceptedItem(selectedSlotItem.Item))
        {
            return $"Submit {selectedSlotItem.Item.ItemName}";
        }
        else if (InventoryController.Instance.GetUsableItems(_acceptedItems).Count > 0)
        {
            return "Equip Mangrove to Submit";
        }
        return "Requires Mangrove to Submit";
    }

    public void Interact()
    {
        if (_isPermanentPlantSpawned || _pendingSubmittedItem != null)
        {
            return;
        }

        InventoryItem selectedSlotItem = InventoryController.Instance.GetSelectedItem();
        if (selectedSlotItem.IsEmpty || !IsAcceptedItem(selectedSlotItem.Item))
        {
            return;
        }

        ItemBaseSO itemToSubmit = selectedSlotItem.Item;
        InventoryController.Instance.UseSelectedItem(1);

        _pendingSubmittedItem = itemToSubmit;
        SpawnPreviewModel();

        // if (AudioManager.Instance != null)
        // {
        //     AudioManager.Instance.PlaySFX("tanam_sfx", transform.position);
        // }

        Debug.Log($"[MangroveSubmitStation '{_stationID}'] Submitted {itemToSubmit.ItemName}");

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.SubmitItem(itemToSubmit, 1);
        }
    }

    private bool IsAcceptedItem(ItemBaseSO item)
    {
        if (item == null) return false;
        if (_acceptedItems != null && _acceptedItems.Count > 0)
        {
            return _acceptedItems.Contains(item);
        }
        ItemMangroveSO mangroveItem = item as ItemMangroveSO;
        return mangroveItem != null && mangroveItem.itemType == ItemType.Mangrove;
    }

    private void SpawnPreviewModel()
    {
        DestroyPreviewModel();

        if (_pendingSubmittedItem == null || _pendingSubmittedItem.ItemPrefab == null)
        {
            return;
        }

        Transform targetTransform = _displaySpawnPoint != null ? _displaySpawnPoint : transform;
        _currentDisplayInstance = Instantiate(_pendingSubmittedItem.ItemPrefab, targetTransform.position, targetTransform.rotation, targetTransform);
    }

    private void DestroyPreviewModel()
    {
        if (_displaySpawnPoint != null)
        {
            for (int i = _displaySpawnPoint.childCount - 1; i >= 0; i--)
            {
                GameObject childObj = _displaySpawnPoint.GetChild(i).gameObject;
                if (childObj != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(childObj);
                    }
                    else
                    {
                        DestroyImmediate(childObj);
                    }
                }
            }
        }

        if (_currentDisplayInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(_currentDisplayInstance);
            }
            else
            {
                DestroyImmediate(_currentDisplayInstance);
            }
            _currentDisplayInstance = null;
        }
    }

    private void SpawnPermanentModel()
    {
        if (_permanentPlantInstance != null)
        {
            Destroy(_permanentPlantInstance);
            _permanentPlantInstance = null;
        }

        if (_permanentItem == null)
        {
            return;
        }

        GameObject prefabToSpawn = null;
        ItemMangroveSO mangroveItem = _permanentItem as ItemMangroveSO;
        if (mangroveItem != null)
        {
            prefabToSpawn = mangroveItem.PermanentPlantPrefab;
        }
        else
        {
            prefabToSpawn = _permanentItem.ItemPrefab;
        }

        if (prefabToSpawn != null)
        {
            Transform targetTransform = _permanentSpawnPoint != null ? _permanentSpawnPoint : transform;
            _permanentPlantInstance = Instantiate(prefabToSpawn, targetTransform.position, targetTransform.rotation, transform);
        }
    }

    public SubmitStationSaveData GetSaveData()
    {
        return new SubmitStationSaveData
        {
            stationID = _stationID,
            pendingItemID = _pendingSubmittedItem != null ? _pendingSubmittedItem.ItemID : null,
            isPermanentPlantSpawned = _isPermanentPlantSpawned,
            permanentItemID = _permanentItem != null ? _permanentItem.ItemID : null
        };
    }

    public void LoadFromSaveData(SubmitStationSaveData saveData, ItemDatabaseSO itemDatabase)
    {
        if (saveData == null || itemDatabase == null) return;

        if (_permanentPlantInstance != null)
        {
            Destroy(_permanentPlantInstance);
            _permanentPlantInstance = null;
        }

        _isPermanentPlantSpawned = saveData.isPermanentPlantSpawned;

        bool hasPendingItem = !string.IsNullOrEmpty(saveData.pendingItemID);
        bool hasPermanentItem = !string.IsNullOrEmpty(saveData.permanentItemID);

        // Only destroy the indicator object / preview model if an item was actually submitted or permanently spawned
        if (hasPendingItem || _isPermanentPlantSpawned || hasPermanentItem)
        {
            DestroyPreviewModel();
        }

        if (hasPendingItem)
        {
            _pendingSubmittedItem = itemDatabase.GetItemByID(saveData.pendingItemID);
            if (_pendingSubmittedItem != null && !_isPermanentPlantSpawned)
            {
                SpawnPreviewModel();
            }
        }
        else
        {
            _pendingSubmittedItem = null;
        }

        if (hasPermanentItem)
        {
            _permanentItem = itemDatabase.GetItemByID(saveData.permanentItemID);
            if (_permanentItem != null && _isPermanentPlantSpawned)
            {
                SpawnPermanentModel();
            }
        }
        else
        {
            _permanentItem = null;
        }
    }
}
