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
        [SerializeField] private GameObject _permanentPlantPrefab;

        public GameObject PermanentPlantPrefab
        {
            get
            {
                if (_permanentPlantPrefab != null) return _permanentPlantPrefab;
                if (mangroveData != null && mangroveData.StageObjects != null && mangroveData.StageObjects.Length > 0)
                {
                    return mangroveData.StageObjects[mangroveData.StageObjects.Length - 1];
                }
                return ItemPrefab;
            }
        }
    }

    public enum ItemType { Seed, Mangrove }

}