using System;
using System.Collections.Generic;
using Item;
using Manager;
using Save;
using UnityEngine;
using UnityEngine.Events;

namespace Progression
{
    public class ProgressionManager : Singleton<ProgressionManager>
    {
        [Header("Goal Configuration")]
        [SerializeField] private List<ProgressionGoalSO> _goals = new List<ProgressionGoalSO>();

        [Header("Ecosystem Health Settings")]
        [SerializeField] private float _baseHealthPercentage = 0f;
        [SerializeField] private float _healthPerCleanedTrash = 2.0f;
        [SerializeField] private float _healthPerSubmittedMangrove = 5.0f;
        [SerializeField] private float _penaltyPerActiveTrash = 3.0f;
        [SerializeField] private float _maxTrashContribution = 50.0f;

        [Header("Global Events")]
        [SerializeField] private UnityEvent<ProgressionGoalSO> _onGoalCompletedGlobal;
        [SerializeField] private UnityEvent _onAllGoalsCompletedGlobal;

        private readonly float[] _milestones = new float[] { 25f, 50f, 75f, 100f };
        private int _unlockedMilestoneIndex = 0;

        private int _trashCleanedCount = 0;
        private int _mangrovesSubmittedCount = 0;
        private int _trashCleanedToday = 0;
        private int _mangrovesSubmittedToday = 0;

        private int _currentGoalIndex = 0;
        private int _currentAmount = 0;
        private bool _isAllGoalsCompleted = false;

        public ProgressionGoalSO CurrentGoal => (_goals != null && _currentGoalIndex >= 0 && _currentGoalIndex < _goals.Count) 
            ? _goals[_currentGoalIndex] 
            : null;

        public IReadOnlyList<ProgressionGoalSO> Goals => _goals;
        public int CurrentGoalIndex => _currentGoalIndex;
        public int CurrentAmount => _currentAmount;
        public int TargetAmount => CurrentGoal != null ? CurrentGoal.TargetAmount : 0;
        public bool IsAllGoalsCompleted => _isAllGoalsCompleted;

        public int UnlockedMilestoneIndex => _unlockedMilestoneIndex;
        public float CurrentDayCap => (_milestones != null && _unlockedMilestoneIndex >= 0 && _unlockedMilestoneIndex < _milestones.Length) 
            ? _milestones[_unlockedMilestoneIndex] 
            : 100f;
        public float CurrentMilestoneFloor => (_milestones != null && _unlockedMilestoneIndex > 0 && _unlockedMilestoneIndex - 1 < _milestones.Length)
            ? _milestones[_unlockedMilestoneIndex - 1]
            : 0f;
        public float EcosystemHealthIndex { get; private set; }
        public float UncappedHealth { get; private set; }
        public int TrashCleanedCount => _trashCleanedCount;
        public int MangrovesSubmittedCount => _mangrovesSubmittedCount;
        public int TrashCleanedToday => _trashCleanedToday;
        public int MangrovesSubmittedToday => _mangrovesSubmittedToday;

        // C# Events for UI and Systems
        public event Action<int, int> OnProgressUpdated;
        public event Action<ProgressionGoalSO> OnGoalChanged;
        public event Action<ProgressionGoalSO> OnGoalCompleted;
        public event Action<float> OnEcosystemHealthChanged;
        public event Action<int, float> OnHealthMilestoneUnlocked;

        private void Start()
        {
            SubscribeToGameManager();
            NotifyStateChanged();
            RecalculateEcosystemHealth();
        }

        private void OnEnable()
        {
            SubscribeToGameManager();
        }

        private void OnDisable()
        {
            UnsubscribeFromGameManager();
        }

        private void SubscribeToGameManager()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewDay -= HandleNewDay;
                GameManager.Instance.OnNewDay += HandleNewDay;
            }
        }

        private void UnsubscribeFromGameManager()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewDay -= HandleNewDay;
            }
        }

        private void HandleNewDay()
        {
            _trashCleanedToday = 0;
            _mangrovesSubmittedToday = 0;

            RecalculateEcosystemHealth();

            bool reachedHealthCap = UncappedHealth >= CurrentDayCap - 0.01f;
            bool goalCompletedConditionMet = _currentGoalIndex > _unlockedMilestoneIndex || _isAllGoalsCompleted;

            if (reachedHealthCap && goalCompletedConditionMet && _unlockedMilestoneIndex < _milestones.Length - 1)
            {
                _unlockedMilestoneIndex++;
                RecalculateEcosystemHealth();
                OnHealthMilestoneUnlocked?.Invoke(_unlockedMilestoneIndex, CurrentDayCap);
                Debug.Log($"[ProgressionManager] Unlocked next Ecosystem Milestone Tier {_unlockedMilestoneIndex} (Cap: {CurrentDayCap}%)!");
            }
        }

        public void RecordTrashCleaned(int count = 1)
        {
            _trashCleanedCount += count;
            _trashCleanedToday += count;
            RecalculateEcosystemHealth();
        }

        public void RecalculateEcosystemHealth()
        {
            float rawTrashHealth = _trashCleanedCount * _healthPerCleanedTrash;
            float trashHealth = Mathf.Min(rawTrashHealth, _maxTrashContribution);
            float mangroveHealth = _mangrovesSubmittedCount * _healthPerSubmittedMangrove;

            int activeTrashInWorld = RandomEvent.RandomEventManager.Instance != null
                ? RandomEvent.RandomEventManager.Instance.GetActiveTrashCount()
                : 0;

            float trashPenalty = activeTrashInWorld * _penaltyPerActiveTrash;

            UncappedHealth = Mathf.Clamp(_baseHealthPercentage + trashHealth + mangroveHealth - trashPenalty, 0f, 100f);
            EcosystemHealthIndex = Mathf.Clamp(UncappedHealth, CurrentMilestoneFloor, CurrentDayCap);

            OnEcosystemHealthChanged?.Invoke(EcosystemHealthIndex);
        }

        public bool SubmitItem(ItemBaseSO item, int count = 1)
        {
            if (_isAllGoalsCompleted || CurrentGoal == null || item == null)
            {
                return false;
            }

            // Check item requirement: if RequiredItem is set, match required item. Otherwise check if it's a Mangrove item.
            if (CurrentGoal.RequiredItem != null)
            {
                if (CurrentGoal.RequiredItem.ItemID != item.ItemID)
                {
                    return false;
                }
            }
            else
            {
                // Default requirement: item must be a mangrove item
                ItemMangroveSO mangroveItem = item as ItemMangroveSO;
                if (mangroveItem == null || mangroveItem.itemType != ItemType.Mangrove)
                {
                    return false;
                }
            }

            _currentAmount += count;
            _mangrovesSubmittedCount += count;
            _mangrovesSubmittedToday += count;

            ItemMangroveSO submittedMangrove = item as ItemMangroveSO;
            if (submittedMangrove != null && BestiaryManager.Instance != null)
            {
                BestiaryManager.Instance.UnlockMangrove(submittedMangrove.MangroveType);
            }

            if (_currentAmount >= CurrentGoal.TargetAmount)
            {
                CompleteCurrentGoal();
            }
            else
            {
                OnProgressUpdated?.Invoke(_currentAmount, TargetAmount);
            }

            RecalculateEcosystemHealth();
            return true;
        }

        private void CompleteCurrentGoal()
        {
            ProgressionGoalSO completedGoal = CurrentGoal;

            // Trigger goal-specific UnityEvent
            completedGoal.OnGoalCompleted?.Invoke();

            // Trigger C# and Global events
            OnGoalCompleted?.Invoke(completedGoal);
            _onGoalCompletedGlobal?.Invoke(completedGoal);

            Debug.Log($"[ProgressionManager] Completed Goal: {completedGoal.GoalTitle}");

            // Advance to next goal
            _currentGoalIndex++;
            _currentAmount = 0;

            if (_currentGoalIndex >= _goals.Count)
            {
                _isAllGoalsCompleted = true;
                _onAllGoalsCompletedGlobal?.Invoke();
                Debug.Log("[ProgressionManager] All progression goals completed!");
                OnGoalChanged?.Invoke(null);
                OnProgressUpdated?.Invoke(0, 0);
            }
            else
            {
                OnGoalChanged?.Invoke(CurrentGoal);
                OnProgressUpdated?.Invoke(_currentAmount, TargetAmount);
            }
        }

        public void NotifyStateChanged()
        {
            if (_isAllGoalsCompleted)
            {
                OnGoalChanged?.Invoke(null);
                OnProgressUpdated?.Invoke(0, 0);
            }
            else
            {
                OnGoalChanged?.Invoke(CurrentGoal);
                OnProgressUpdated?.Invoke(_currentAmount, TargetAmount);
            }

            OnEcosystemHealthChanged?.Invoke(EcosystemHealthIndex);
        }

        public ProgressionSaveData GetSaveData()
        {
            return new ProgressionSaveData
            {
                currentGoalIndex = _currentGoalIndex,
                currentAmount = _currentAmount,
                isAllGoalsCompleted = _isAllGoalsCompleted,
                trashCleanedCount = _trashCleanedCount,
                mangrovesSubmittedCount = _mangrovesSubmittedCount,
                unlockedMilestoneIndex = _unlockedMilestoneIndex
            };
        }

        public void LoadFromSaveData(ProgressionSaveData data)
        {
            if (data == null) return;

            _currentGoalIndex = data.currentGoalIndex;
            _currentAmount = data.currentAmount;
            _isAllGoalsCompleted = data.isAllGoalsCompleted;
            _trashCleanedCount = data.trashCleanedCount;
            _mangrovesSubmittedCount = data.mangrovesSubmittedCount;
            _unlockedMilestoneIndex = Mathf.Clamp(data.unlockedMilestoneIndex, 0, _milestones.Length - 1);

            if (_goals != null && _currentGoalIndex >= _goals.Count)
            {
                _isAllGoalsCompleted = true;
            }

            NotifyStateChanged();
            RecalculateEcosystemHealth();
            Debug.Log($"[ProgressionManager] Loaded progression: Goal Index {_currentGoalIndex}, Amount {_currentAmount}, AllCompleted: {_isAllGoalsCompleted}, MilestoneTier: {_unlockedMilestoneIndex}, TrashCleaned: {_trashCleanedCount}, MangrovesSubmitted: {_mangrovesSubmittedCount}");
        }
    }
}
