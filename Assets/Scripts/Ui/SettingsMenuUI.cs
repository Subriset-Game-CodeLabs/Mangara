using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Manager;
using Save;
using FMODUnity;

namespace Ui
{
    public class SettingsMenuUI : MonoBehaviour
    {
        [Header("UI Panel References")]
        [SerializeField] private GameObject _settingsPanel;

        [SerializeField] private GameObject _mainMenuPanel;

        [Header("Setting panel References")]
        [SerializeField] private Slider _bgmSlider;

        [SerializeField] private Slider _sfxSlider;

        [SerializeField] private bool _playPreviewSfx = true;

        [SerializeField] private EventReference _previewSfxId;

        [SerializeField] private Button _resetProgressButton;

        // Belum di implemen dialog kalau ngehapus save data
        // [SerializeField] private GameObject _confirmationDialog;
        // [SerializeField] private Button _confirmResetButton;
        // [SerializeField] private Button _cancelResetButton;
        // [SerializeField] private TextMeshProUGUI _resetStatusText;

        [SerializeField] private Button _backButton;

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
            _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            _resetProgressButton.onClick.AddListener(OnClickResetProgress);
            // _confirmResetButton.onClick.AddListener(ExecuteResetSaveData);
            _backButton.onClick.AddListener(CloseSettings);
        }

        private void UnregisterListeners()
        {
            _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            _resetProgressButton.onClick.RemoveListener(OnClickResetProgress);
            // _confirmResetButton.onClick.RemoveListener(ExecuteResetSaveData);
            _backButton.onClick.RemoveListener(CloseSettings);
        }

        public void InitializeUIValues()
        {
            float bgmVol = AudioManager.GetMusicVolume();
            _bgmSlider.SetValueWithoutNotify(bgmVol);

            float sfxVol = AudioManager.GetSfxVolume();
            _sfxSlider.SetValueWithoutNotify(sfxVol);

            UpdateResetButtonState();
        }

        public void OnBGMVolumeChanged(float value)
        {
            AudioManager.SetMusicVolume(value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            AudioManager.SetSfxVolume(value);
        }
        
        public void OnClickResetProgress()
        {

            // if (_confirmationDialog != null)
            // {
            //     _confirmationDialog.SetActive(true);
            // }
            // else
            // {
                ExecuteResetSaveData();
            // }
        }

        public void ExecuteResetSaveData()
        {

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSave();
                Debug.Log("[SettingsMenuUI] Save data successfully deleted via Settings Menu.");

                // if (_resetStatusText != null)
                // {
                //     _resetStatusText.text = "Save Data Reset Successfully!";
                // }
            }
            else
            {
                Debug.LogWarning("[SettingsMenuUI] SaveManager instance not found!");
            }

            UpdateResetButtonState();
        }

        private void UpdateResetButtonState()
        {
            if (_resetProgressButton != null && SaveManager.Instance != null)
            {
                bool hasSave = SaveManager.Instance.HasSaveFile();
                _resetProgressButton.interactable = hasSave;
            }
        }

        public void OpenSettings()
        {
            _mainMenuPanel.SetActive(false);
            _settingsPanel.SetActive(true);

            InitializeUIValues();
        }

        public void CloseSettings()
        {
            _settingsPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }
    }
}
