using System.Collections.Generic;
using FMODUnity;
using Item;
using Manager;
using Mangrove;
using UnityEngine;

public class PlantSite : MonoBehaviour, IInteractable
{
    [SerializeField] private List<ItemBaseSO> _acceptedItems;
    [SerializeField] private MangroveController _mangroveController;
    [SerializeField] private EventReference _sfxNyiramEvent;
    [SerializeField] private EventReference _sfxTanamEvent;

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
                        InventoryItem selected = InventoryController.Instance.GetSelectedItem();

                        if (!selected.IsEmpty && selected.Item is ItemToolSO tool && tool.ToolType == ToolType.WateringCan)
                        {
                            if (selected.Quantity > 0)
                            {
                                return $"Water Plant ({selected.Quantity}/{tool.MaxCharges})";
                            }
                            return "Watering Can is Empty";
                        }

                        Debug.Log("No Water Can");

                        var inventory = InventoryController.Instance.InventoryHandler.InventoryData.GetCurrentInventoryState();
                        bool hasWateringCanInInventory = false;
                        foreach (var slot in inventory.Values)
                        {
                            if (slot.Item is ItemToolSO t && t.ToolType == ToolType.WateringCan)
                            {
                                hasWateringCanInInventory = true;
                                break;
                            }
                        }
                        return hasWateringCanInInventory ? "Equip Watering Can" : "Requires Watering Can";
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

                    // if (AudioManager.Instance != null)
                    // {
                    //     AudioManager.Instance.PlaySFX("tanam_sfx", transform.position);
                    // }

                    RuntimeManager.PlayOneShot(_sfxTanamEvent);
                    break;
                }
            case PlantState.Planted:
            case PlantState.Growing:
                {
                    if (_mangroveController.IsWatered)
                        return;

                    InventoryItem selected = InventoryController.Instance.GetSelectedItem();
                    if (!selected.IsEmpty && selected.Item is ItemToolSO tool && tool.ToolType == ToolType.WateringCan)
                    {
                        if (selected.Quantity <= 0) return;

                        _mangroveController.Water();
                        InventoryController.Instance.ConsumeSelectedToolCharge(1);
                        RuntimeManager.PlayOneShot(_sfxNyiramEvent);
                    }

                    break;
                }
            case PlantState.Harvestable:
                {
                    var harvest = _mangroveController.Harvest();
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