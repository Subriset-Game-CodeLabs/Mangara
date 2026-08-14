using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Input;

namespace Ui
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The parent GameObject or Canvas panel for the pause menu UI.")]
        [SerializeField] private GameObject _pauseMenuPanel;
        
        [Header("Button References")]
        [Tooltip("Button to resume the game.")]
        [SerializeField] private Button _resumeButton;
        
        [Tooltip("Button to return to the main menu scene.")]
        [SerializeField] private Button _backToMenuButton;

        [Header("Settings")]
        [Tooltip("Name of the main menu scene to load when 'Back to Menu' is clicked.")]
        [SerializeField] private string _menuSceneName = "Start";
        
        [Tooltip("Key used to toggle pause mode.")]
        [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;

        [Tooltip("Whether the game starts paused (default: false).")]
        [SerializeField] private bool _startPaused = false;

        public static bool IsPaused { get; private set; }

        private void Awake()
        {
            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(Resume);
            }

            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.AddListener(BackToMenu);
            }
        }

        private void Start()
        {
            if (_startPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        private void Update()
        {
            if (WasPauseKeyPressed())
            {
                TogglePause();
            }
        }

        private bool WasPauseKeyPressed()
        {
            // Check legacy Input system
            try
            {
                if (UnityEngine.Input.GetKeyDown(_pauseKey)) return true;
            }
            catch
            {
                // Ignored if legacy input is disabled in Unity settings
            }

            // Check New Input System Keyboard if active
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            return false;
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (_pauseMenuPanel != null)
            {
                _pauseMenuPanel.SetActive(true);
            }

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

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            if (_pauseMenuPanel != null)
            {
                _pauseMenuPanel.SetActive(false);
            }

            if (InputManager.Instance != null)
            {
                InputManager.Instance.PlayerMode();
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void BackToMenu()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            if (InputManager.Instance != null)
            {
                InputManager.Instance.UIMode();
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (!string.IsNullOrEmpty(_menuSceneName))
            {
                SceneManager.LoadScene(_menuSceneName);
            }
            else
            {
                Debug.LogWarning("[PauseMenuUI] Menu scene name is empty!");
            }
        }
    }
}
