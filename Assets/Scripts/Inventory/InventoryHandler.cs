using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryHandler : MonoBehaviour
    {
        [SerializeField] private UiInventoryPage _uiInventoryPage;
        [SerializeField] private InventorySO _inventoryData;

        public InventorySO InventoryData => _inventoryData;

        public void Initialize(List<InventoryItem> initialItems = null)
        {
            _inventoryData.Initialize();
            _inventoryData.OnInventoryUpdated += UpdateInventoryUI;

            if (initialItems != null)
            {
                foreach (InventoryItem item in initialItems)
                {
                    if (!item.IsEmpty)
                        _inventoryData.AddItem(item.Item, item.Quantity);
                }
            }

            //Ganti nanti
            _uiInventoryPage.InitializeInventoryUI(_inventoryData.Size, this);
            _uiInventoryPage.OnStartDragging += HandleDragging;
            _uiInventoryPage.OnSwapItems += HandleSwapItems;
            _uiInventoryPage.OnShiftClick += HandleTransferItems;
            _uiInventoryPage.Hide();
        }

        private void HandleTransferItems(int index1, InventoryHandler source)
        {
            _inventoryData.TransferItems(index1, source.InventoryData);
        }

        private void HandleSwapItems(int index1, int index2, InventoryHandler source)
        {
            _inventoryData.SwapItems(index1, index2, source.InventoryData);
        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItem item = _inventoryData.GetItemAt(itemIndex);
            if (!item.IsEmpty)
                _uiInventoryPage.CreateDraggedItem(item.Item.ItemSprite, item.Quantity);
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            _uiInventoryPage.ResetAllitems();
            foreach (var item in inventoryState)
                _uiInventoryPage.UpdateData(item.Key, item.Value.Item.ItemSprite, item.Value.Quantity);
            _uiInventoryPage.UpdateSelectedSlot(_inventoryData.SelectedSlotIndex);
        }

        public void Show()
        {
            _uiInventoryPage.Show();
        }

        public void Hide()
        {
            _uiInventoryPage.Hide();
        }

        public void AddItem(ItemBaseSO item, int quantity)
        {
            _inventoryData.AddItem(item, quantity);
        }
    }
}