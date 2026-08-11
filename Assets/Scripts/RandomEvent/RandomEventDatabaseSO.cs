using System.Collections.Generic;
using UnityEngine;

namespace RandomEvent
{
    [ManageableData]
    [CreateAssetMenu(menuName = "Events/Random Event Database")]
    public class RandomEventDatabaseSO : ScriptableObject
    {
        public List<RandomEventSO> Events = new List<RandomEventSO>();

        public RandomEventSO GetEventByID(string id)
        {
            if (Events == null) return null;
            return Events.Find(e => e != null && e.EventID == id);
        }
    }
}
