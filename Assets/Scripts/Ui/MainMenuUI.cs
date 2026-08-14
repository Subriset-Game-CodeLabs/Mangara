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
        [Tooltip("The parent panel object for the Main Menu UI.")]
        [SerializeField] private GameObject _mainMenuPanel;

        [Header("Button References")]
        [Tooltip("Button that starts or plays the game.")]
        [SerializeField] private Button _playButton;

        [Tooltip("Button that exits the game application.")]
        [SerializeField] private Button _exitButton;

        [Header("Scene Settings")]
        [Tooltip("The name of the scene to load when the Play button is clicked.")]
        [SerializeField] private string _playSceneName = "MVP";

        [Tooltip("If true, scene will load asynchronously.")]
        [SerializeField] private bool _useAsyncLoading = false;

        [Header("Audio Settings (Optional)")]
        [Tooltip("Audio SFX ID to play on button click (requires AudioManager). Leave empty to disable.")]
        [SerializeField] private string _buttonClickSfxId = "";

        private void Awake()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(PlayGame);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(ExitGame);
            }
        }

        private void Start()
        {
            // Reset time scale in case returning from a paused state
            Time.timeScale = 1f;

            // Configure input mode and cursor state for UI navigation
            if (InputManager.Instance != null)
            {
                InputManager.Instance.UIMode();
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(PlayGame);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(ExitGame);
            }
        }

        /// <summary>
        /// Call this method to play/start the game by loading the specified play scene.
        /// </summary>
        public void PlayGame()
        {
            PlayButtonClickSound();

            if (string.IsNullOrEmpty(_playSceneName))
            {
                Debug.LogError("[MainMenuUI] Play scene name is empty! Please assign a valid scene name in the Inspector.");
                return;
            }

            Debug.Log($"[MainMenuUI] Loading scene: {_playSceneName}");

            if (_useAsyncLoading)
            {
                SceneManager.LoadSceneAsync(_playSceneName);
            }
            else
            {
                SceneManager.LoadScene(_playSceneName);
            }
        }

        /// <summary>
        /// Call this method to exit/quit the application.
        /// </summary>
        public void ExitGame()
        {
            PlayButtonClickSound();

            Debug.Log("[MainMenuUI] Exiting Application...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void PlayButtonClickSound()
        {
            if (!string.IsNullOrEmpty(_buttonClickSfxId) && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_buttonClickSfxId);
            }
        }
    }
}
