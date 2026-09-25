using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Input;
using Manager;

namespace Ui
{
    /// <summary>
    /// Manages the Main Menu UI interactions, including Play and Exit buttons.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Panel References")]
        [SerializeField] private SettingsMenuUI _settingsMenuUI;

        [Header("Button References")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        [Header("Scene Settings")]
        [SerializeField] private string _playSceneName = "MVP";

        [SerializeField] private bool _useAsyncLoading = false;

        private void Awake()
        {
            _playButton.onClick.AddListener(PlayGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(ExitGame);
        }

        private void OnEnable()
        {
            EnsureCursorVisible();
        }

        private void Start()
        {
            Time.timeScale = 1f;

            EnsureCursorVisible();
        }

        private void Update()
        {
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                EnsureCursorVisible();
            }
        }

        private void EnsureCursorVisible()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Configure input mode for UI navigation
            if (InputManager.Instance != null)
            {
                InputManager.Instance.UIMode();
            }
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(PlayGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitButton.onClick.RemoveListener(ExitGame);
        }

        public void OpenSettings()
        {
            _settingsMenuUI.OpenSettings();
        }

        public void PlayGame()
        {
            if (_useAsyncLoading)
            {
                SceneManager.LoadSceneAsync(_playSceneName);
            }
            else
            {
                SceneManager.LoadScene(_playSceneName);
            }
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }
}
