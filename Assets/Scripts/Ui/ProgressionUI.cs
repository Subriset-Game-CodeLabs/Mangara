using DG.Tweening;
using Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class ProgressionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _container;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider _progressBar;

        [Header("Settings")]
        [SerializeField] private float _tweenDuration = 0.4f;

        private Tween _sliderTween;

        private void OnEnable()
        {
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
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressUpdated -= HandleProgressUpdated;
                ProgressionManager.Instance.OnGoalChanged -= HandleGoalChanged;
                ProgressionManager.Instance.OnGoalCompleted -= HandleGoalCompleted;
            }

            _sliderTween?.Kill();
        }

        private void HandleGoalChanged(ProgressionGoalSO goal)
        {
            if (goal == null || ProgressionManager.Instance.IsAllGoalsCompleted)
            {
                if (_container != null)
                {
                    _container.SetActive(false);
                }
                return;
            }

            if (_container != null)
            {
                _container.SetActive(true);
            }

            if (_titleText != null)
            {
                _titleText.text = goal.GoalTitle;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = goal.GoalDescription;
            }

            if (_progressBar != null)
            {
                _progressBar.maxValue = goal.TargetAmount;
            }

            UpdateProgressDisplay(ProgressionManager.Instance.CurrentAmount, goal.TargetAmount, animate: false);
        }

        private void HandleProgressUpdated(int current, int target)
        {
            UpdateProgressDisplay(current, target, animate: true);
        }

        private void HandleGoalCompleted(ProgressionGoalSO goal)
        {
            // Optional feedback effect when goal finishes
            if (_progressBar != null)
            {
                _progressBar.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            }
        }

        private void UpdateProgressDisplay(int current, int target, bool animate)
        {
            if (_progressText != null)
            {
                _progressText.text = $"{current} / {target}";
            }

            if (_progressBar != null)
            {
                _progressBar.maxValue = Mathf.Max(1, target);

                _sliderTween?.Kill();

                if (animate)
                {
                    _sliderTween = _progressBar.DOValue(current, _tweenDuration).SetEase(Ease.OutCubic);
                }
                else
                {
                    _progressBar.value = current;
                }
            }
        }
    }
}
