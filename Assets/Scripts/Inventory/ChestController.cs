using System;
using System.Collections.Generic;
using Inventory;
using Manager;
using Save;
using Unity.VisualScripting;
using UnityEngine;

public class ChestController : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryHandler _inventoryHandler;
    [SerializeField] private List<InventoryItem> _initialItems;

    private void Awake()
    {
        bool hasSaveFile = SaveManager.Instance.HasSaveFile();

        if (hasSaveFile)
        {
            _inventoryHandler.Initialize();
        }
        else
        {
            _inventoryHandler.Initialize(_initialItems);
        }
    }

    public string GetInteractText()
    {
        return "Open Chest";
    }

    public void Interact()
    {
        UIManager.Instance.ToogleChest(_inventoryHandler);
    }
    
}