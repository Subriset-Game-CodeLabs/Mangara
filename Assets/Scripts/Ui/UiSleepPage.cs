using System;
using DG.Tweening;
using Manager;
using Progression;
using RandomEvent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class UiSleepPage : MonoBehaviour
    {
        [Header("Canvas Group References")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private CanvasGroup _summaryPanelCanvasGroup;

        [Header("Text References")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _dayCountText;
        [SerializeField] private TMP_Text _statsSummaryText;

        [Header("Ecosystem Health UI Bar References")]
        [SerializeField] private Slider _ecosystemHealthSlider;
        [SerializeField] private Slider _ecosystemCapSlider;
        [SerializeField] private RectTransform _capMarker;
        [SerializeField] private TMP_Text _healthValueText;
        [SerializeField] private TMP_Text _capValueText;

        [Header("Button References")]
        [SerializeField] private Button _wakeUpButton;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeInDuration = 1.0f;
        [SerializeField] private float _panelAppearDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 1.0f;

        private Action _onWakeUpClicked;

        private void Awake()
        {
            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.alpha = 0f;
                _fadeCanvasGroup.gameObject.SetActive(false);
            }

            if (_summaryPanelCanvasGroup != null)
            {
                _summaryPanelCanvasGroup.alpha = 0f;
                _summaryPanelCanvasGroup.gameObject.SetActive(false);
            }

            if (_wakeUpButton != null)
            {
                _wakeUpButton.onClick.AddListener(OnWakeUpButtonPressed);
            }
        }

        public void ShowSleepSequence(int currentDay, Action onWakeUpCallback, bool isForcedSleep = false)
        {
            _onWakeUpClicked = onWakeUpCallback;

            if (_titleText != null) _titleText.text = isForcedSleep ? "You Passed Out!" : "Good Night!";
            if (_dayCountText != null) _dayCountText.text = $"Day {currentDay}";

            // Fetch live statistics
            float currentHealth = 0f;
            float currentCap = 100f;
            int trashToday = 0;
            int trashTotal = 0;
            int mangrovesToday = 0;
            int mangrovesTotal = 0;
            string goalText = "N/A";

            if (ProgressionManager.Instance != null)
            {
                currentHealth = ProgressionManager.Instance.EcosystemHealthIndex;
                currentCap = ProgressionManager.Instance.CurrentDayCap;
                trashToday = ProgressionManager.Instance.TrashCleanedToday;
                trashTotal = ProgressionManager.Instance.TrashCleanedCount;
                mangrovesToday = ProgressionManager.Instance.MangrovesSubmittedToday;
                mangrovesTotal = ProgressionManager.Instance.MangrovesSubmittedCount;

                var goal = ProgressionManager.Instance.CurrentGoal;
                if (goal != null)
                {
                    goalText = $"{goal.GoalTitle} ({ProgressionManager.Instance.CurrentAmount}/{goal.TargetAmount})";
                }
                else if (ProgressionManager.Instance.IsAllGoalsCompleted)
                {
                    goalText = "All Goals Completed!";
                }
            }

            int activeTrash = RandomEventManager.Instance != null 
                ? RandomEventManager.Instance.GetActiveTrashCount() 
                : 0;

            // Setup Ecosystem Health Progress Bar & Cap UI
            if (_ecosystemHealthSlider != null)
            {
                _ecosystemHealthSlider.minValue = 0f;
                _ecosystemHealthSlider.maxValue = 100f;
                _ecosystemHealthSlider.value = 0f;
            }

            if (_ecosystemCapSlider != null)
            {
                _ecosystemCapSlider.minValue = 0f;
                _ecosystemCapSlider.maxValue = 100f;
                _ecosystemCapSlider.value = currentCap;
            }

            if (_capMarker != null && _capMarker.parent is RectTransform parentRect)
            {
                float width = parentRect.rect.width;
                float targetX = (currentCap / 100f) * width - (width * parentRect.pivot.x);
                _capMarker.anchoredPosition = new Vector2(targetX, _capMarker.anchoredPosition.y);
            }

            if (_healthValueText != null)
            {
                _healthValueText.text = $"{currentHealth:F0}%";
            }

            if (_capValueText != null)
            {
                _capValueText.text = $"Cap: {currentCap:F0}%";
            }

            if (_statsSummaryText != null)
            {
                _statsSummaryText.text = $"Ecosystem Health: {currentHealth:F0}% / {currentCap:F0}% Cap\n" +
                                         $"Mangroves Submitted: {mangrovesToday} Today ({mangrovesTotal} Total)\n" +
                                         $"Trash Cleaned: {trashToday} Today ({trashTotal} Total)\n" +
                                         $"Active Trash Remaining: {activeTrash}\n" +
                                         $"Objective: {goalText}";
            }

            gameObject.SetActive(true);

            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.gameObject.SetActive(true);
                _fadeCanvasGroup.alpha = 0f;
                _fadeCanvasGroup.blocksRaycasts = true;

                // Step 1: Fade to black using DOTween
                _fadeCanvasGroup.DOFade(1f, _fadeInDuration).OnComplete(() =>
                {
                    if (_summaryPanelCanvasGroup != null)
                    {
                        // Step 2: Show recap summary panel with smooth scale & fade in
                        _summaryPanelCanvasGroup.gameObject.SetActive(true);
                        _summaryPanelCanvasGroup.alpha = 0f;
                        _summaryPanelCanvasGroup.transform.localScale = Vector3.one * 0.85f;

                        _summaryPanelCanvasGroup.DOFade(1f, _panelAppearDuration);
                        _summaryPanelCanvasGroup.transform
                            .DOScale(Vector3.one, _panelAppearDuration)
                            .SetEase(Ease.OutBack);

                        if (_ecosystemHealthSlider != null)
                        {
                            _ecosystemHealthSlider.DOValue(currentHealth, _panelAppearDuration).SetEase(Ease.OutCubic);
                        }
                    }
                });
            }
        }

        private void OnWakeUpButtonPressed()
        {
            if (_wakeUpButton != null)
            {
                _wakeUpButton.interactable = false;
            }

            if (_summaryPanelCanvasGroup != null)
            {
                // Step 3: Fade out recap summary panel
                _summaryPanelCanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
                {
                    _summaryPanelCanvasGroup.gameObject.SetActive(false);

                    // Execute day progression logic
                    _onWakeUpClicked?.Invoke();

                    // Step 4: Fade out black screen to start the new day
                    if (_fadeCanvasGroup != null)
                    {
                        _fadeCanvasGroup.DOFade(0f, _fadeOutDuration).OnComplete(() =>
                        {
                            _fadeCanvasGroup.blocksRaycasts = false;
                            _fadeCanvasGroup.gameObject.SetActive(false);
                            gameObject.SetActive(false);

                            if (_wakeUpButton != null)
                            {
                                _wakeUpButton.interactable = true;
                            }

                            UIManager.Instance.OnSleepSequenceFinished();
                        });
                    }
                });
            }
            else
            {
                _onWakeUpClicked?.Invoke();
                gameObject.SetActive(false);
                UIManager.Instance.OnSleepSequenceFinished();
            }
        }
    }
}
