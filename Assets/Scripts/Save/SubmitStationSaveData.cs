using System;

namespace Save
{
    [Serializable]
    public class SubmitStationSaveData
    {
        public string stationID;
        public string pendingItemID;
        public bool isPermanentPlantSpawned;
        public string permanentItemID;
    }
}
