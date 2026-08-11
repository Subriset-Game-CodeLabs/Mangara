using System;
using DG.Tweening;
using Manager;
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

        public void ShowSleepSequence(int currentDay, Action onWakeUpCallback)
        {
            _onWakeUpClicked = onWakeUpCallback;

            // Update display text with placeholder Stardew Valley style recap info
            if (_titleText != null) _titleText.text = "Good Night!";
            if (_dayCountText != null) _dayCountText.text = $"Day {currentDay} Complete";
            if (_statsSummaryText != null)
            {
                _statsSummaryText.text = "🌾 Crops Harvested: 0\n" +
                                         "📦 Items Submitted: 0\n" +
                                         "💰 Daily Revenue: 0 G\n" +
                                         "⚡ Energy Restored: 100%";
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
