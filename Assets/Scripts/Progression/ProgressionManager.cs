using System;
using System.Collections.Generic;
using Item;
using Save;
using UnityEngine;
using UnityEngine.Events;

namespace Progression
{
    public class ProgressionManager : PersistentSingleton<ProgressionManager>
    {
        [Header("Goal Configuration")]
        [SerializeField] private List<ProgressionGoalSO> _goals = new List<ProgressionGoalSO>();

        [Header("Global Events")]
        [SerializeField] private UnityEvent<ProgressionGoalSO> _onGoalCompletedGlobal;
        [SerializeField] private UnityEvent _onAllGoalsCompletedGlobal;

        private int _currentGoalIndex = 0;
        private int _currentAmount = 0;
        private bool _isAllGoalsCompleted = false;

        public ProgressionGoalSO CurrentGoal => (_goals != null && _currentGoalIndex >= 0 && _currentGoalIndex < _goals.Count) 
            ? _goals[_currentGoalIndex] 
            : null;

        public int CurrentGoalIndex => _currentGoalIndex;
        public int CurrentAmount => _currentAmount;
        public int TargetAmount => CurrentGoal != null ? CurrentGoal.TargetAmount : 0;
        public bool IsAllGoalsCompleted => _isAllGoalsCompleted;

        // C# Events for UI and Systems
        public event Action<int, int> OnProgressUpdated;
        public event Action<ProgressionGoalSO> OnGoalChanged;
        public event Action<ProgressionGoalSO> OnGoalCompleted;

        private void Start()
        {
            // Initial UI sync
            NotifyStateChanged();
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

            if (_currentAmount >= CurrentGoal.TargetAmount)
            {
                CompleteCurrentGoal();
            }
            else
            {
                OnProgressUpdated?.Invoke(_currentAmount, TargetAmount);
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }

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
        }

        public ProgressionSaveData GetSaveData()
        {
            return new ProgressionSaveData
            {
                currentGoalIndex = _currentGoalIndex,
                currentAmount = _currentAmount,
                isAllGoalsCompleted = _isAllGoalsCompleted
            };
        }

        public void LoadFromSaveData(ProgressionSaveData data)
        {
            if (data == null) return;

            _currentGoalIndex = data.currentGoalIndex;
            _currentAmount = data.currentAmount;
            _isAllGoalsCompleted = data.isAllGoalsCompleted;

            if (_goals != null && _currentGoalIndex >= _goals.Count)
            {
                _isAllGoalsCompleted = true;
            }

            NotifyStateChanged();
            Debug.Log($"[ProgressionManager] Loaded progression: Goal Index {_currentGoalIndex}, Amount {_currentAmount}, AllCompleted: {_isAllGoalsCompleted}");
        }
    }
}
