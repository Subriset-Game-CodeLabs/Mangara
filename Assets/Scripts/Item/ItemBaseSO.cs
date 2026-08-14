using UnityEngine;

[ManageableData]
[CreateAssetMenu(menuName = "Items/Base Item")]
public class ItemBaseSO : ScriptableObject
{
    [field: SerializeField]
    public bool IsStackable { get; set; }
    
    public string ItemID;

    public int MaxStackSize;
    
    public string ItemName;
    public Sprite ItemSprite;
    public string ItemDescription;
    public GameObject ItemPrefab;

    [field: SerializeField]
    public bool IsTrash { get; set; }
}
