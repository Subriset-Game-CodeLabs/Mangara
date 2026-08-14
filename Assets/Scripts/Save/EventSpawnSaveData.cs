using System.Collections.Generic;

namespace Save
{
    [System.Serializable]
    public class SpawnedItemData
    {
        public string ItemID;
        public string EventID;
        public int Quantity;
        public float PosX;
        public float PosY;
        public float PosZ;
    }

    [System.Serializable]
    public class EventSpawnSaveData
    {
        public string ZoneID;
        public List<SpawnedItemData> SpawnedItems = new List<SpawnedItemData>();
    }
}
