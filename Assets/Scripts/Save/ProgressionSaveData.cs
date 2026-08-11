using System;

namespace Save
{
    [Serializable]
    public class ProgressionSaveData
    {
        public int currentGoalIndex;
        public int currentAmount;
        public bool isAllGoalsCompleted;
    }
}
