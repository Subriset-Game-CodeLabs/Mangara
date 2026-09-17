using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Manager;
using Save;

namespace Ui
{
    /// <summary>
    /// Manages the Settings Panel UI in the Main Menu, including BGM volume, SFX volume, and reset save data.
    /// </summary>
    public class SettingsMenuUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [Tooltip("The root GameObject of the Settings Panel.")]
        [SerializeField] private GameObject _settingsPanel;

        [Tooltip("The root GameObject of the Main Menu Panel (to re-enable when back is pressed).")]
        [SerializeField] private GameObject _mainMenuPanel;

        [Header("Audio Sliders")]
        [Tooltip("Slider control for BGM Volume (0.0 to 1.0).")]
        [SerializeField] private Slider _bgmSlider;

        [Tooltip("Optional TextMeshPro text displaying BGM volume percentage (e.g. '80%').")]
        [SerializeField] private TextMeshProUGUI _bgmValueText;

        [Tooltip("Slider control for SFX Volume (0.0 to 1.0).")]
        [SerializeField] private Slider _sfxSlider;

        [Tooltip("Optional TextMeshPro text displaying SFX volume percentage (e.g. '80%').")]
        [SerializeField] private TextMeshProUGUI _sfxValueText;

        [Header("SFX Feedback Settings")]
        [Tooltip("If true, playing a preview sound when SFX volume slider is changed.")]
        [SerializeField] private bool _playPreviewSfx = true;

        [Tooltip("Audio SFX ID to play when testing SFX slider.")]
        [SerializeField] private string _previewSfxId = "click";

        [Header("Reset Progress / Save Data")]
        [Tooltip("Button to reset/delete game progress.")]
        [SerializeField] private Button _resetProgressButton;

        [Tooltip("Optional confirmation dialog modal window before deleting save data.")]
        [SerializeField] private GameObject _confirmationDialog;

        [Tooltip("Button inside confirmation dialog to confirm save deletion.")]
        [SerializeField] private Button _confirmResetButton;

        [Tooltip("Button inside confirmation dialog to cancel save deletion.")]
        [SerializeField] private Button _cancelResetButton;

        [Tooltip("Optional TextMeshPro label displaying status after reset (e.g., 'Save Data Deleted!').")]
        [SerializeField] private TextMeshProUGUI _resetStatusText;

        [Header("Navigation Buttons")]
        [Tooltip("Button to close settings and return to Main Menu.")]
        [SerializeField] private Button _backButton;

        [Header("Audio Settings (Optional)")]
        [Tooltip("Audio SFX ID to play on UI button clicks.")]
        [SerializeField] private string _buttonClickSfxId = "click";

        private void Awake()
        {
            RegisterListeners();
        }

        private void OnEnable()
        {
            InitializeUIValues();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        private void RegisterListeners()
        {
            if (_bgmSlider != null)
            {
                _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (_resetProgressButton != null)
            {
                _resetProgressButton.onClick.AddListener(OnClickResetProgress);
            }

            if (_confirmResetButton != null)
            {
                _confirmResetButton.onClick.AddListener(ExecuteResetSaveData);
            }

            if (_cancelResetButton != null)
            {
                _cancelResetButton.onClick.AddListener(CloseConfirmationDialog);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(CloseSettings);
            }
        }

        private void UnregisterListeners()
        {
            if (_bgmSlider != null)
            {
                _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            }

            if (_resetProgressButton != null)
            {
                _resetProgressButton.onClick.RemoveListener(OnClickResetProgress);
            }

            if (_confirmResetButton != null)
            {
                _confirmResetButton.onClick.RemoveListener(ExecuteResetSaveData);
            }

            if (_cancelResetButton != null)
            {
                _cancelResetButton.onClick.RemoveListener(CloseConfirmationDialog);
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(CloseSettings);
            }
        }

        /// <summary>
        /// Initializes slider positions and UI state according to existing save/audio data.
        /// </summary>
        public void InitializeUIValues()
        {
            // Close confirmation popup by default
            if (_confirmationDialog != null)
            {
                _confirmationDialog.SetActive(false);
            }

            if (_resetStatusText != null)
            {
                _resetStatusText.text = string.Empty;
            }

            // Sync BGM Slider
            if (_bgmSlider != null)
            {
                float bgmVol = AudioManager.Instance != null ? AudioManager.Instance.BGMVolume : 1.0f;
                _bgmSlider.SetValueWithoutNotify(bgmVol);
                UpdateBGMText(bgmVol);
            }

            // Sync SFX Slider
            if (_sfxSlider != null)
            {
                float sfxVol = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 1.0f;
                _sfxSlider.SetValueWithoutNotify(sfxVol);
                UpdateSFXText(sfxVol);
            }

            UpdateResetButtonState();
        }

        #region Audio Handlers

        public void OnBGMVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.BGMVolume = value;
            }

            UpdateBGMText(value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SFXVolume = value;

                if (_playPreviewSfx && !string.IsNullOrEmpty(_previewSfxId))
                {
                    AudioManager.Instance.PlaySFX(_previewSfxId);
                }
            }

            UpdateSFXText(value);
        }

        private void UpdateBGMText(float value)
        {
            if (_bgmValueText != null)
            {
                _bgmValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private void UpdateSFXText(float value)
        {
            if (_sfxValueText != null)
            {
                _sfxValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }

        #endregion

        #region Reset Save Data Handlers

        public void OnClickResetProgress()
        {
            PlayButtonClickSound();

            if (_confirmationDialog != null)
            {
                _confirmationDialog.SetActive(true);
            }
            else
            {
                ExecuteResetSaveData();
            }
        }

        public void ExecuteResetSaveData()
        {
            PlayButtonClickSound();

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSave();
                Debug.Log("[SettingsMenuUI] Save data successfully deleted via Settings Menu.");

                if (_resetStatusText != null)
                {
                    _resetStatusText.text = "Save Data Reset Successfully!";
                }
            }
            else
            {
                Debug.LogWarning("[SettingsMenuUI] SaveManager instance not found!");
            }

            CloseConfirmationDialog();
            UpdateResetButtonState();
        }

        public void CloseConfirmationDialog()
        {
            PlayButtonClickSound();

            if (_confirmationDialog != null)
            {
                _confirmationDialog.SetActive(false);
            }
        }

        private void UpdateResetButtonState()
        {
            if (_resetProgressButton != null && SaveManager.Instance != null)
            {
                bool hasSave = SaveManager.Instance.HasSaveFile();
                _resetProgressButton.interactable = hasSave;
            }
        }

        #endregion

        #region Panel Navigation

        public void OpenSettings()
        {
            PlayButtonClickSound();

            if (_mainMenuPanel != null)
            {
                _mainMenuPanel.SetActive(false);
            }

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(true);
            }

            InitializeUIValues();
        }

        public void CloseSettings()
        {
            PlayButtonClickSound();

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }

            if (_mainMenuPanel != null)
            {
                _mainMenuPanel.SetActive(true);
            }
        }

        public void ToggleSettings()
        {
            if (_settingsPanel != null)
            {
                bool isActive = _settingsPanel.activeSelf;
                if (isActive)
                {
                    CloseSettings();
                }
                else
                {
                    OpenSettings();
                }
            }
        }

        #endregion

        private void PlayButtonClickSound()
        {
            if (!string.IsNullOrEmpty(_buttonClickSfxId) && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_buttonClickSfxId);
            }
        }
    }
}
