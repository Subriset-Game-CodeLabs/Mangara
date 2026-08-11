using System.Text;
using DG.Tweening;
using Input;
using Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class ProgressionUI : MonoBehaviour
    {
        [Header("UI Container")]
        [SerializeField] private GameObject _container;

        [Header("To Do List Display References")]
        [SerializeField] private TextMeshProUGUI _todoContentText;
        [SerializeField] private TextMeshProUGUI _todoTitleText;

        [Header("Animation Settings")]
        [SerializeField] private bool _startHidden = false;
        [SerializeField] private float _slideDuration = 0.4f;
        [SerializeField] private float _slideOffset = 500f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private Ease _hideEase = Ease.InCubic;

        private RectTransform _containerRectTransform;
        private Vector2 _originalAnchoredPosition;
        private Vector2 _offscreenPosition;
        private Tween _slideTween;
        private bool _isShowing = true;

        public bool IsVisible => _isShowing && _container != null && _container.activeSelf;

        private void Awake()
        {
            if (_container != null)
            {
                _containerRectTransform = _container.GetComponent<RectTransform>();
                if (_containerRectTransform != null)
                {
                    _originalAnchoredPosition = _containerRectTransform.anchoredPosition;
                    float offset = _slideOffset > 0 ? _slideOffset : Mathf.Max(500f, _containerRectTransform.rect.width + 100f);
                    _offscreenPosition = _originalAnchoredPosition + new Vector2(offset, 0f);
                }
            }
        }

        private void Start()
        {
            if (_startHidden)
            {
                _isShowing = false;
                if (_containerRectTransform != null)
                {
                    _containerRectTransform.anchoredPosition = _offscreenPosition;
                }
                if (_container != null)
                {
                    _container.SetActive(false);
                }
            }
            else
            {
                _isShowing = true;
                if (_containerRectTransform != null)
                {
                    _containerRectTransform.anchoredPosition = _originalAnchoredPosition;
                }
                if (_container != null)
                {
                    _container.SetActive(true);
                }
            }
        }

        public void ToggleTodoList()
        {
            if (_isShowing)
            {
                HideTodoList();
            }
            else
            {
                ShowTodoList();
            }
        }

        public void ShowTodoList()
        {
            if (_container == null || _containerRectTransform == null) return;

            _slideTween?.Kill();
            _isShowing = true;
            _container.SetActive(true);

            _slideTween = _containerRectTransform
                .DOAnchorPos(_originalAnchoredPosition, _slideDuration)
                .SetEase(_showEase)
                .SetUpdate(true);
        }

        public void HideTodoList()
        {
            if (_container == null || _containerRectTransform == null) return;

            _slideTween?.Kill();
            _isShowing = false;

            _slideTween = _containerRectTransform
                .DOAnchorPos(_offscreenPosition, _slideDuration)
                .SetEase(_hideEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (!_isShowing && _container != null)
                    {
                        _container.SetActive(false);
                    }
                });
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.PlayerInput.Todo.OnDown += OnToggleTodoInput;
                InputManager.Instance.UIInput.Todo.OnDown += OnToggleTodoInput;
            }

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressUpdated += HandleProgressUpdated;
                ProgressionManager.Instance.OnGoalChanged += HandleGoalChanged;
                ProgressionManager.Instance.OnGoalCompleted += HandleGoalCompleted;

                // Sync current state
                ProgressionManager.Instance.NotifyStateChanged();
            }
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.PlayerInput.Todo.OnDown -= OnToggleTodoInput;
                InputManager.Instance.UIInput.Todo.OnDown -= OnToggleTodoInput;
            }

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressUpdated -= HandleProgressUpdated;
                ProgressionManager.Instance.OnGoalChanged -= HandleGoalChanged;
                ProgressionManager.Instance.OnGoalCompleted -= HandleGoalCompleted;
            }

        }

        private void OnToggleTodoInput()
        {
            ToggleTodoList();
        }

        private void HandleGoalChanged(ProgressionGoalSO goal)
        {
            int currentAmount = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentAmount : 0;
            UpdateAllDisplays(goal, currentAmount, animate: false);
        }

        private void HandleProgressUpdated(int current, int target)
        {
            ProgressionGoalSO goal = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentGoal : null;
            UpdateAllDisplays(goal, current, animate: true);
        }

        private void HandleGoalCompleted(ProgressionGoalSO goal)
        {
            

            if (_todoContentText != null)
            {
                _todoContentText.transform.DOPunchScale(Vector3.one * 0.05f, 0.3f);
            }
        }

        private void UpdateAllDisplays(ProgressionGoalSO currentGoal, int currentAmount, bool animate)
        {
            int targetAmount = currentGoal != null ? currentGoal.TargetAmount : 0;
            UpdateTodoListText();
        }

        private void UpdateTodoListText()
        {
            string todoString = BuildTodoListString();

            if (_todoTitleText != null)
            {
                _todoTitleText.text = "Todo";
            }

            if (_todoContentText != null)
            {
                _todoContentText.text = todoString;
            }
        }

        private string BuildTodoListString()
        {
            if (ProgressionManager.Instance == null)
            {
                return "";
            }

            var goals = ProgressionManager.Instance.Goals;
            bool allCompleted = ProgressionManager.Instance.IsAllGoalsCompleted;
            int currentIndex = ProgressionManager.Instance.CurrentGoalIndex;
            int currentAmount = ProgressionManager.Instance.CurrentAmount;

            if (goals == null || goals.Count == 0)
            {
                var goal = ProgressionManager.Instance.CurrentGoal;
                if (goal != null)
                {
                    return $"- {goal.GoalTitle}\n  {currentAmount} / {goal.TargetAmount}";
                }
                return allCompleted ? "- All tasks completed!" : "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < goals.Count; i++)
            {
                var goal = goals[i];
                if (goal == null) continue;

                if (i < currentIndex || allCompleted)
                {
                    sb.AppendLine($"<s>- {goal.GoalTitle}</s>");
                }
                else if (i == currentIndex)
                {
                    sb.AppendLine($"- {goal.GoalTitle}");
                    sb.AppendLine($"  {currentAmount} / {goal.TargetAmount}");
                }
                else
                {
                    sb.AppendLine($"- {goal.GoalTitle}");
                }
            }

            return sb.ToString().TrimEnd();
        }

       
    }
}
