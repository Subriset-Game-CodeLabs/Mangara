using System;
using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemBaseSO _itemData;
    [SerializeField] private int _quantity = 1;

    public event Action<ItemObject> OnCollected;

    public ItemBaseSO ItemData => _itemData;
    public int Quantity => _quantity;

    public void Initialize(ItemBaseSO itemData, int quantity)
    {
        _itemData = itemData;
        _quantity = quantity;
    }

    public string GetInteractText()
    {
        string name = _itemData != null ? _itemData.ItemName : "Item";
        return $"Press [E] to get {_quantity} {name}";
    }

    public void Interact()
    {
        if (_itemData == null) return;

        InventoryController.Instance.AddItem(_itemData, _quantity);
        Debug.Log(_itemData.name + " picked up");

        OnCollected?.Invoke(this);
        Destroy(gameObject);
    }
}
