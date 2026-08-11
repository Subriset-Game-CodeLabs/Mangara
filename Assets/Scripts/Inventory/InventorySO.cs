using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Item;
using Save;
using Sirenix.OdinInspector;
using UnityEngine;

[ManageableData]
[CreateAssetMenu]
public class InventorySO : ScriptableObject
{
    [SerializeField] [TableList] private List<InventoryItem> _inventoryItems;
    [SerializeField] private InventoryType _inventoryType;

    [SerializeField] private string _inventoryID;
    public string InventoryID => _inventoryID;

    [OnValueChanged("ResizeList")] 
    [field: SerializeField] 
    public int Size { get; private set; } = 10;

    [field: SerializeField]
    public int HotbarSize { get; private set; } = 5;

    public int SelectedSlotIndex { get; private set; } = 0;

    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;
    public event Action<int> OnSelectedSlotChanged;

    public void SetSelectedSlot(int index)
    {
        if (Size <= 0) return;
        int clamped = (index % Size + Size) % Size;
        SelectedSlotIndex = clamped;
        OnSelectedSlotChanged?.Invoke(SelectedSlotIndex);
    }

    public void SelectNextSlot()
    {
        int maxIndex = HotbarSize > 0 ? HotbarSize : Size;
        SetSelectedSlot((SelectedSlotIndex + 1) % maxIndex);
    }

    public void SelectPreviousSlot()
    {
        int maxIndex = HotbarSize > 0 ? HotbarSize : Size;
        SetSelectedSlot((SelectedSlotIndex - 1 + maxIndex) % maxIndex);
    }

    public InventoryItem GetSelectedItem()
    {
        if (SelectedSlotIndex >= 0 && SelectedSlotIndex < _inventoryItems.Count)
        {
            return _inventoryItems[SelectedSlotIndex];
        }
        return InventoryItem.GetEmptyItem();
    }

    public void Initialize()
    {
        _inventoryItems = new List<InventoryItem>();
        for (int i = 0; i < Size; i++)
        {
            _inventoryItems.Add(InventoryItem.GetEmptyItem());
        }
    }

    private void ResizeList()
    {
        if (Size < 0) Size = 0;

        while (_inventoryItems.Count < Size)
            _inventoryItems.Add(new InventoryItem());

        while (_inventoryItems.Count > Size)
            _inventoryItems.RemoveAt(_inventoryItems.Count - 1);
    }

    public int AddItem(ItemBaseSO item, int quantity)
    {
        if (item.IsStackable == false)
        {
            for (int i = 0; i < _inventoryItems.Count; i++)
            {
                while (quantity > 0 && IsInventoryFull() == false)
                {
                    quantity -= AddItemToFirstFreeSlot(item, 1);
                }

                InformAboutChange();
                return quantity;
            }
        }

        quantity = AddStackableItem(item, quantity);
        InformAboutChange();
        return quantity;
    }

    public int FindItemIndex(ItemBaseSO item)
    {
        for (int i = 0; i < _inventoryItems.Count; i++)
        {
            if (_inventoryItems[i].IsEmpty)
                continue;
            if (item.ItemName == _inventoryItems[i].Item.ItemName)
                return i;
        }

        return -1;
    }

    public void RemoveItem(int itemIndex, int amount)
    {
        if (_inventoryItems.Count > itemIndex)
        {
            if (_inventoryItems[itemIndex].IsEmpty)
                return;
            int reminder = _inventoryItems[itemIndex].Quantity - amount;
            if (reminder <= 0)
                _inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
            else
                _inventoryItems[itemIndex] = _inventoryItems[itemIndex]
                    .ChangeQuantity(reminder);

            InformAboutChange();
        }
    }

    private int AddItemToFirstFreeSlot(ItemBaseSO item, int quantity)
    {
        InventoryItem newItem = new InventoryItem
        {
            Item = item,
            Quantity = quantity
        };
        for (int i = 0; i < _inventoryItems.Count; i++)
        {
            if (_inventoryItems[i].IsEmpty)
            {
                _inventoryItems[i] = newItem;
                return quantity;
            }
        }

        return 0;
    }

    private int AddStackableItem(ItemBaseSO item, int quantity)
    {
        for (int i = 0; i < _inventoryItems.Count; i++)
        {
            if (_inventoryItems[i].IsEmpty)
                continue;
            if (_inventoryItems[i].Item.ItemID == item.ItemID)
            {
                int amountPossibleToTake = _inventoryItems[i].Item.MaxStackSize - _inventoryItems[i].Quantity;

                if (quantity > amountPossibleToTake)
                {
                    _inventoryItems[i] = _inventoryItems[i].ChangeQuantity(_inventoryItems[i].Item.MaxStackSize);
                    quantity -= amountPossibleToTake;
                }
                else
                {
                    _inventoryItems[i] = _inventoryItems[i].ChangeQuantity(_inventoryItems[i].Quantity + quantity);
                    InformAboutChange();
                    return 0;
                }
            }
        }

        while (quantity > 0 && IsInventoryFull() == false)
        {
            int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
            quantity -= newQuantity;
            AddItemToFirstFreeSlot(item, newQuantity);
        }

        return quantity;
    }

    private void InformAboutChange()
    {
        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
    }

    public void SwapItems(int itemIndex1, int itemIndex2, InventorySO sourceInventory)
    {
        InventoryItem targetItem = _inventoryItems[itemIndex2];
        InventoryItem sourceItem = sourceInventory._inventoryItems[itemIndex1];
        Debug.Log(targetItem.IsEmpty);
        if (targetItem.IsEmpty == false)
        {
            if (sourceItem.Item.ItemID == targetItem.Item.ItemID && sourceItem.Item.IsStackable &&
                targetItem.Quantity < targetItem.Item.MaxStackSize)
            {
                AddStackableItem(targetItem.Item, targetItem.Quantity);
                sourceInventory._inventoryItems[itemIndex1] = InventoryItem.GetEmptyItem();
            }
            else
            {
                _inventoryItems[itemIndex2] = sourceItem;
                sourceInventory._inventoryItems[itemIndex1] = targetItem;
            }
        }
        else
        {
            _inventoryItems[itemIndex2] = sourceItem;
            sourceInventory._inventoryItems[itemIndex1] = targetItem;
        }


        InformAboutChange();
        sourceInventory.InformAboutChange();
    }

    public void TransferItems(int itemIndex1, InventorySO sourceInventory)
    {
        InventoryItem sourceItem = sourceInventory._inventoryItems[itemIndex1];

        if (sourceInventory == InventoryController.Instance.InventoryHandler.InventoryData)
        {
            if (InventoryStateData.ChestInventory.InventoryData.IsInventoryFull())
            {
                Debug.Log("Inventory is full");
                return;
            }

            InventoryStateData.ChestInventory.InventoryData.AddItem(sourceItem.Item, sourceItem.Quantity);
        }
        else
        {
            if (InventoryController.Instance.InventoryHandler.InventoryData.IsInventoryFull())
            {
                Debug.Log("Inventory is full");
                return;
            }

            InventoryController.Instance.InventoryHandler.InventoryData.AddItem(sourceItem.Item, sourceItem.Quantity);
        }

        sourceInventory.RemoveItem(itemIndex1, sourceItem.Quantity);
        InformAboutChange();
    }

    public InventoryItem GetItemAt(int itemIndex)
    {
        return _inventoryItems[itemIndex];
    }


    private bool IsInventoryFull() => _inventoryItems.Where(item => item.IsEmpty).Any() == false;

    public Dictionary<int, InventoryItem> GetCurrentInventoryState()
    {
        Dictionary<int, InventoryItem> returnValue
            = new Dictionary<int, InventoryItem>();
        for (int i = 0; i < _inventoryItems.Count; i++)
        {
            if (_inventoryItems[i].IsEmpty)
                continue;
            returnValue[i] = _inventoryItems[i];
        }

        return returnValue;
    }
    
    public void SetItemAt(int slotIndex, ItemBaseSO item, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= _inventoryItems.Count) return;
    
        _inventoryItems[slotIndex] = new InventoryItem
        {
            Item     = item,
            Quantity = quantity
        };
    
        InformAboutChange(); 
    }

    public InventorySaveData GetSaveData()
    {
        var saveData = new InventorySaveData
        {
            InventoryID = _inventoryID,
            Size = this.Size,
            Slots = new List<InventorySlotData>()
        };

        foreach (var inventoryItem in GetCurrentInventoryState())
        {
            saveData.Slots.Add(new InventorySlotData
            {
                SlotIndex = inventoryItem.Key,
                ItemID = inventoryItem.Value.Item.ItemID,
                Quantity = inventoryItem.Value.Quantity
            });
        }

        return saveData;
    }

    public void LoadFromSaveData(InventorySaveData saveData, ItemDatabaseSO itemDatabase)
    {
        foreach (InventorySlotData slot in saveData.Slots)
        {
            ItemBaseSO item = itemDatabase.GetItemByID(slot.ItemID);
            
            if (item == null)
            {
                Debug.LogWarning("Item not found: " + slot.ItemID);
                continue;
            }
            
            SetItemAt(slot.SlotIndex, item, slot.Quantity);
        }
    }
}

[Serializable]
public struct InventoryItem
{
    public int Quantity;
    public ItemBaseSO Item;
    public bool IsEmpty => Item == null;

    public InventoryItem ChangeQuantity(int newQuantity)
    {
        return new InventoryItem
        {
            Item = this.Item,
            Quantity = newQuantity
        };
    }

    public static InventoryItem GetEmptyItem()
        => new InventoryItem
        {
            Item = null,
            Quantity = 0
        };
}

public enum InventoryType
{
    PlayerInventory,
    ExternalInventory,
}