using Item;
using UnityEngine;
using UnityEngine.Events;

namespace Progression
{
    [ManageableData]
    [CreateAssetMenu(fileName = "NewProgressionGoal", menuName = "Progression/Progression Goal")]
    public class ProgressionGoalSO : ScriptableObject
    {
        [Header("Goal Info")]
        [SerializeField] private string _goalID;
        [SerializeField] private string _goalTitle = "Coastal Restoration";
        [SerializeField] private string _goalDescription = "Submit Mangroves to Station";

        [Header("Requirements")]
        [SerializeField] private int _targetAmount = 10;
        [SerializeField] private ItemBaseSO _requiredItem; // If null, accepts any mangrove item

        [Header("Events")]
        public UnityEvent OnGoalCompleted;

        public string GoalID => _goalID;
        public string GoalTitle => _goalTitle;
        public string GoalDescription => _goalDescription;
        public int TargetAmount => _targetAmount;
        public ItemBaseSO RequiredItem => _requiredItem;
    }
}
