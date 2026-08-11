using System.Collections.Generic;
using Item;
using Manager;
using Progression;
using UnityEngine;

public class MangroveSubmitStation : MonoBehaviour, IInteractable
{
    [SerializeField] private List<ItemBaseSO> _acceptedItems;

    public string GetInteractText()
    {
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
        InventoryItem selectedSlotItem = InventoryController.Instance.GetSelectedItem();
        if (selectedSlotItem.IsEmpty || !IsAcceptedItem(selectedSlotItem.Item))
        {
            return;
        }

        ItemBaseSO itemToSubmit = selectedSlotItem.Item;
        InventoryController.Instance.UseSelectedItem(1);

        Debug.Log("Submitted " + itemToSubmit.ItemName);

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

}

