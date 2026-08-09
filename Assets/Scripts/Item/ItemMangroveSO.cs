using UnityEngine;

namespace Item
{
    [ManageableData]
    [CreateAssetMenu(menuName = "Items/Mangrove Item")]
    public class ItemMangroveSO : ItemBaseSO
    {
        public ItemType itemType;
        public MangroveDataSO mangroveData;
        public MangroveType MangroveType;
    }

        public enum ItemType { Seed, Mangrove }

}