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
                InventoryItem selectedSlotItem = InventoryController.Instance.GetSelectedItem();
                if (!selectedSlotItem.IsEmpty && IsAcceptedSeed(selectedSlotItem.Item))
                {
                    return $"Plant {selectedSlotItem.Item.ItemName}";
                }
                else if (InventoryController.Instance.GetUsableItems(_acceptedItems).Count > 0)
                {
                    return "Equip Seed to Plant";
                }
                return "Requires Seed";
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
                InventoryItem selectedSlotItem = InventoryController.Instance.GetSelectedItem();
                if (selectedSlotItem.IsEmpty || !IsAcceptedSeed(selectedSlotItem.Item))
                {
                    return;
                }

                ItemMangroveSO mangroveItem = selectedSlotItem.Item as ItemMangroveSO;
                InventoryController.Instance.UseSelectedItem(1);
                _mangroveController.Plant(mangroveItem?.mangroveData);
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

    private bool IsAcceptedSeed(ItemBaseSO item)
    {
        if (item == null) return false;
        if (_acceptedItems != null && _acceptedItems.Count > 0)
        {
            return _acceptedItems.Contains(item);
        }
        return item is ItemMangroveSO;
    }
}