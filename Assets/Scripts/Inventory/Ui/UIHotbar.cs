using System.Collections.Generic;
using Inventory;
using Manager;
using UnityEngine;

namespace Ui
{
    public class UIHotbar : MonoBehaviour
    {
        [SerializeField] private UiInventoryItem _itemPrefab;
        [SerializeField] private RectTransform _contentPanel;

        private List<UiInventoryItem> _hotbarSlots = new List<UiInventoryItem>();
        private InventorySO _inventoryData;

        private void Start()
        {
            InitializeHotbar();
        }

        private void Update()
        {
            if (_inventoryData == null)
            {
                InitializeHotbar();
            }
        }

        public void InitializeHotbar()
        {
            if (_inventoryData != null) return;

            var playerHandler = InventoryController.Instance?.InventoryHandler;
            if (playerHandler == null) return;

            _inventoryData = playerHandler.InventoryData;
            int count = Mathf.Min(_inventoryData.HotbarSize, _inventoryData.Size);

            for (int i = 0; i < count; i++)
            {
                UiInventoryItem slot = Instantiate(_itemPrefab, _contentPanel);
                _hotbarSlots.Add(slot);
                int slotIndex = i;
                slot.OnItemClicked += _ => HandleSlotClicked(slotIndex);
            }

            _inventoryData.OnInventoryUpdated += UpdateHotbarItems;
            _inventoryData.OnSelectedSlotChanged += UpdateSelectedSlot;

            UpdateHotbarItems(_inventoryData.GetCurrentInventoryState());
            UpdateSelectedSlot(_inventoryData.SelectedSlotIndex);
        }

        private void OnDestroy()
        {
            if (_inventoryData != null)
            {
                _inventoryData.OnInventoryUpdated -= UpdateHotbarItems;
                _inventoryData.OnSelectedSlotChanged -= UpdateSelectedSlot;
            }
        }

        private void HandleSlotClicked(int index)
        {
            _inventoryData?.SetSelectedSlot(index);
        }

        private void UpdateHotbarItems(Dictionary<int, InventoryItem> inventoryState)
        {
            for (int i = 0; i < _hotbarSlots.Count; i++)
            {
                _hotbarSlots[i].ResetData();
                if (inventoryState.TryGetValue(i, out InventoryItem item) && !item.IsEmpty)
                {
                    _hotbarSlots[i].SetData(item.Item.ItemSprite, item.Quantity);
                }
            }
            UpdateSelectedSlot(_inventoryData.SelectedSlotIndex);
        }

        private void UpdateSelectedSlot(int selectedIndex)
        {
            for (int i = 0; i < _hotbarSlots.Count; i++)
            {
                if (i == selectedIndex)
                    _hotbarSlots[i].Select();
                else
                    _hotbarSlots[i].Deselect();
            }
        }
    }
}
