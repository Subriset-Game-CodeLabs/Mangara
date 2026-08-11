using System;
using System.Collections.Generic;
using System.Linq;
using Input;
using Inventory;
using Manager;
using Save;
using UnityEngine;

public class InventoryController : PersistentSingleton<InventoryController>
{
    [SerializeField] private InventoryHandler _inventoryHandler;
    [SerializeField] private List<InventoryItem> _initialItems;

    public InventoryHandler InventoryHandler => _inventoryHandler;

    private void Start()
    {
        bool hasSaveFile = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();

        if (hasSaveFile)
        {
            Debug.Log("intiialize player");
            _inventoryHandler.Initialize();
        }
        else
        {
            _inventoryHandler.Initialize(_initialItems);
        }
    }

    private void OnEnable()
    {
        InputManager.Instance.PlayerInput.Inventory.OnDown += OnInventory;
        InputManager.Instance.UIInput.Inventory.OnDown += OnInventory;

        InputManager.Instance.PlayerInput.Next.OnDown += OnNextSlot;
        InputManager.Instance.PlayerInput.Previous.OnDown += OnPreviousSlot;

        var hotbarSlots = InputManager.Instance.PlayerInput.HotbarSlots;
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            int index = i;
            hotbarSlots[i].OnDown += () => SetSelectedSlot(index);
        }
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerInput.Inventory.OnDown -= OnInventory;
        InputManager.Instance.UIInput.Inventory.OnDown -= OnInventory;

        InputManager.Instance.PlayerInput.Next.OnDown -= OnNextSlot;
        InputManager.Instance.PlayerInput.Previous.OnDown -= OnPreviousSlot;
    }

    private void OnNextSlot()
    {
        SelectNextSlot();
    }

    private void OnPreviousSlot()
    {
        SelectPreviousSlot();
    }

    private void OnInventory()
    {
        UIManager.Instance.ToogleInventory(_inventoryHandler);
    }

    public void AddItem(ItemBaseSO item, int quantity)
    {
        _inventoryHandler.AddItem(item, quantity);
    }

    public int FindItem(ItemBaseSO item)
    {
        int itemIndex = _inventoryHandler.InventoryData.FindItemIndex(item);
        return itemIndex;
    }

    public void UseItem(int itemIndex, int quantity)
    {
        _inventoryHandler.InventoryData.RemoveItem(itemIndex, quantity);
    }

    public List<InventoryItem> GetUsableItems(List<ItemBaseSO> acceptedItems)
    {
        return acceptedItems.Select(item => _inventoryHandler.InventoryData.FindItemIndex(item))
            .Where(index => index != -1).Select(index => _inventoryHandler.InventoryData.GetItemAt(index)).ToList();
    }

    public InventoryItem GetSelectedItem()
    {
        return _inventoryHandler.InventoryData.GetSelectedItem();
    }

    public void SelectNextSlot()
    {
        _inventoryHandler.InventoryData.SelectNextSlot();
    }

    public void SelectPreviousSlot()
    {
        _inventoryHandler.InventoryData.SelectPreviousSlot();
    }

    public void SetSelectedSlot(int index)
    {
        _inventoryHandler.InventoryData.SetSelectedSlot(index);
    }

    public void UseSelectedItem(int quantity)
    {
        int activeIndex = _inventoryHandler.InventoryData.SelectedSlotIndex;
        _inventoryHandler.InventoryData.RemoveItem(activeIndex, quantity);
    }
}