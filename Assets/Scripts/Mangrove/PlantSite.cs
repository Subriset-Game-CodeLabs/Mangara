using System.Collections.Generic;
using Item;
using Manager;
using Mangrove;
using UnityEngine;

public class PlantSite : MonoBehaviour, IInteractable
{
    [SerializeField] private List<ItemBaseSO> _acceptedItems;
    [SerializeField] private MangroveController _mangroveController;

    public string GetInteractText()
    {
        switch (_mangroveController.PlantState)
        {
            case PlantState.Empty:
            {
                return "Choose seed to plant";
            }
            case PlantState.Planted:
            case PlantState.Growing:
            {
                if (!_mangroveController.IsWatered)
                {
                    return "Water Plant";
                }
                break;
            }
            case PlantState.Harvestable:
            {
                return "Harvest Plant";
            }
        }
        return "";
    }

    public void Interact()
    {
        switch (_mangroveController.PlantState)
        {
            case PlantState.Empty:
            {
                List<InventoryItem> usableItems = InventoryController.Instance.GetUsableItems(_acceptedItems);

                if (usableItems.Count == 0)
                {
                    return;
                }

                UIManager.Instance.ShowItemSelector(usableItems, OnItemSelected);
                break;
            }
            case PlantState.Planted:
            case PlantState.Growing:
            {
                if (_mangroveController.IsWatered)
                    return;
                _mangroveController.Water();
                break;
            }
            case PlantState.Harvestable:
            {
                var harvest= _mangroveController.Harvest();
                if (harvest != null) 
                    InventoryController.Instance.AddItem(harvest.Value.item, harvest.Value.quantity);
                break;
            }
        }
        
    }

    private void OnItemSelected(ItemBaseSO selectedItem)
    {
        ItemMangroveSO mangroveItem = selectedItem as ItemMangroveSO;

        int itemIndex = InventoryController.Instance.FindItem(selectedItem);
        InventoryController.Instance.UseItem(itemIndex, 1);

        _mangroveController.Plant(mangroveItem?.mangroveData);
        
    }
}