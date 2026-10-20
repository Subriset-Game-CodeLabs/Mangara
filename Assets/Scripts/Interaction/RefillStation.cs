
using FMODUnity;
using Item;
using UnityEngine;

public class RefillStation : MonoBehaviour, IInteractable
{
    [Header("Station Settings")]
    [SerializeField] private ToolType _acceptedToolType = ToolType.WateringCan;
    [SerializeField] private string _stationName = "Water Source";
    [SerializeField] private EventReference _refillSFX;

    public string GetInteractText()
    {
        InventoryItem selected = InventoryController.Instance.GetSelectedItem();
        if (!selected.IsEmpty && selected.Item is ItemToolSO tool && tool.ToolType == _acceptedToolType)
        {
            if (selected.Quantity >= tool.MaxCharges)
            {
                return $"{tool.ItemName} is Full";
            }
            return $"Refill {tool.ItemName} ({selected.Quantity}/{tool.MaxCharges})";
        }
        return "";
    }

    public void Interact()
    {
        InventoryItem selected = InventoryController.Instance.GetSelectedItem();
        if (!selected.IsEmpty && selected.Item is ItemToolSO tool && tool.ToolType == _acceptedToolType)
        {
            if (selected.Quantity < tool.MaxCharges)
            {
                InventoryController.Instance.RefillSelectedTool(tool.MaxCharges);
                if (!_refillSFX.IsNull)
                {
                    RuntimeManager.PlayOneShot(_refillSFX);
                }
            }
        }
    }
}