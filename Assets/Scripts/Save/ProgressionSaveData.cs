using System;

namespace Save
{
    [Serializable]
    public class ProgressionSaveData
    {
        public int currentGoalIndex;
        public int currentAmount;
        public bool isAllGoalsCompleted;
        public bool isGoalReadyToAdvance;

        public int trashCleanedCount;
        public int mangrovesSubmittedCount;
        public int unlockedMilestoneIndex;
    }
}
