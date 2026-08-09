using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

namespace Item
{
    [ManageableData]
    [CreateAssetMenu]
    public class ItemDatabaseSO : ScriptableObject
    {
        public List<ItemBaseSO> Items;

        public ItemBaseSO GetItemByID(string ID)
        {
            return Items.Find(item =>  item.ItemID == ID);
        }
    }
}