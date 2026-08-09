using System.Collections.Generic;
using Item;
using Manager;
using UnityEngine;

public class MangroveSubmitStation : MonoBehaviour, IInteractable
{
    [SerializeField] private List<ItemBaseSO> _acceptedItems;

    public string GetInteractText()
    {
        return "Submit Mangrove";
    }

    public void Interact()
    {
        List<InventoryItem> usableItems = InventoryController.Instance.GetUsableItems(_acceptedItems);

        if (usableItems.Count == 0)
        {
            return;
        }

        UIManager.Instance.ShowItemSelector(usableItems, OnItemSelected);
    }

    private void OnItemSelected(ItemBaseSO selectedItem)
    {
        ItemMangroveSO mangroveItem = selectedItem as ItemMangroveSO;
        if (mangroveItem == null || mangroveItem.itemType != ItemType.Mangrove) return;

        int itemIndex = InventoryController.Instance.FindItem(selectedItem);
        InventoryController.Instance.UseItem(itemIndex, 1);

        Debug.Log("Submitted " + selectedItem.ItemName);
    }

}
