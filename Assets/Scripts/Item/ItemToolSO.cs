using UnityEngine;

namespace Item
{
    public enum ToolType
    {
        WateringCan,
        // could add later
    }

    [ManageableData]
    [CreateAssetMenu(menuName = "Items/Tool")]
    public class ItemToolSO : ItemBaseSO
    {
        [Header("Tool Settings")]
        [SerializeField] private ToolType _toolType;
        [SerializeField] private int _maxCharges = 5;

        public ToolType ToolType => _toolType;
        public int MaxCharges => _maxCharges;
    }
}