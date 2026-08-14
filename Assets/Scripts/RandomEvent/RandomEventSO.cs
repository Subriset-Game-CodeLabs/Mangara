using System.Collections.Generic;
using UnityEngine;

namespace RandomEvent
{
    [System.Serializable]
    public struct SpawnEntry
    {
        public ItemBaseSO Item;
        public int Quantity;
        public float Weight;
    }

    [CreateAssetMenu(menuName = "Events/Random Event")]
    public class RandomEventSO : ScriptableObject
    {
        public string EventID;
        public string EventName;
        [Range(0f, 1f)] public float Probability = 1f;
        public List<SpawnEntry> SpawnEntries = new List<SpawnEntry>();
        public int MinSpawnCount = 1;
        public int MaxSpawnCount = 1;
        [Tooltip("Max active items spawned by this event allowed in the world at once (0 = unlimited).")]
        public int MaxActiveCap = 0;
    }
}
