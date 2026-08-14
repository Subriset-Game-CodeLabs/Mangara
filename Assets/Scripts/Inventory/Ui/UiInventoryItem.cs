using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiInventoryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private Image _borderImage;

    public event Action<UiInventoryItem> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnRightMouseBtnClicked;
    private bool _empty = true;
    
    public void ResetData()
    {
        if (_itemImage != null)
        {
            _itemImage.gameObject.SetActive(false);
        }
        if (_quantityText != null)
        {
            _quantityText.text = "";
        }
        _empty = true;
    }

    public void Select()
    {
        if (_borderImage != null)
        {
            _borderImage.gameObject.SetActive(true);
            _borderImage.enabled = true;
        }
    }

    public void Deselect()
    {
        if (_borderImage != null)
        {
            _borderImage.enabled = false;
        }
    }
    
    public void SetData(Sprite sprite, int quantity)
    {
        _itemImage.gameObject.SetActive(true);
        _itemImage.sprite = sprite;
        _quantityText.text = quantity + "";
        _empty = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerEventData pointerEventData = eventData;
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            OnRightMouseBtnClicked?.Invoke(this);
        }
        else
        {
            OnItemClicked?.Invoke(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(_empty)
            return;
        OnItemBeginDrag?.Invoke(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnItemEndDrag?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedOn?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // This is added because the Onbegin and OnEnd need this 
    }
}


