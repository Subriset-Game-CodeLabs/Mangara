using System.Collections.Generic;
using UnityEngine;

namespace Mangrove
{
    [ManageableData]
    [CreateAssetMenu]
    public class MangroveDatabaseSO : ScriptableObject
    {
        public List<MangroveDataSO> MangroveDatas;

        public MangroveDataSO GetMangroveDataByID(string ID)
        {
            return MangroveDatas.Find(item =>  item.ID == ID);
        }
    }
}