using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Input
{
    public class InputManager : PersistentSingleton<InputManager>
    {
        private InputActions _inputActions;
        private FiniteStateMachine<ActionMap> _actionMapStates;
        private PlayerActionMap _player;
        private UIActionMap _ui;
        public PlayerActionMap PlayerInput => _player;
        public UIActionMap UIInput => _ui;

        private SchemeType _currentControlScheme;
        public SchemeType CurrentControlScheme => _currentControlScheme;

        [Header("Alt Cursor Settings")]
        [SerializeField] private bool _enableAltCursor = true;

        private bool _isAltCursorActive = false;
        public bool IsAltCursorActive => _isAltCursorActive;
        public bool IsUIMode => _actionMapStates != null && _actionMapStates.CurrentState == _ui;

        protected override void Awake()
        {
            base.Awake();
            InitializedManager();
            // InitializePlayerInput();
        }

        private void InitializedManager()
        {
            _inputActions = new InputActions();
            _player = new PlayerActionMap(_inputActions);
            _ui = new UIActionMap(_inputActions);
            _actionMapStates = new FiniteStateMachine<ActionMap>(_player);
            PlayerMode();
        }

        private void Update()
        {
            HandleAltCursorInput();
        }

        private void HandleAltCursorInput()
        {
            if (!_enableAltCursor) return;

            // Only process Alt cursor when in PlayerMode or when Alt cursor is currently active
            if (_actionMapStates == null) return;
            if (_actionMapStates.CurrentState != _player && !_isAltCursorActive) return;

            bool altPressed = IsAltKeyPressed();

            if (altPressed && !_isAltCursorActive)
            {
                EnableAltCursor();
            }
            else if (!altPressed && _isAltCursorActive)
            {
                DisableAltCursor();
            }
        }

        private bool IsAltKeyPressed()
        {
            try
            {
                if (UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt))
                    return true;
            }
            catch { }

            if (Keyboard.current != null)
            {
                return Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed;
            }

            return false;
        }

        private void EnableAltCursor()
        {
            _isAltCursorActive = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Disable camera look input while Alt is held so camera won't rotate
            if (_inputActions != null)
            {
                _inputActions.Player.Look.Disable();
            }
        }

        private void DisableAltCursor()
        {
            _isAltCursorActive = false;

            // Re-enable camera look input if still in PlayerMode
            if (_actionMapStates != null && _actionMapStates.CurrentState == _player && _inputActions != null)
            {
                _inputActions.Player.Look.Enable();
            }

            // Only lock cursor back if still in PlayerMode (i.e. no UI menu opened)
            if (_actionMapStates != null && _actionMapStates.CurrentState == _player)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void PlayerMode()
        {
            _actionMapStates.ChangeState(_player);

            if (_enableAltCursor && IsAltKeyPressed())
            {
                EnableAltCursor();
            }
            else
            {
                _isAltCursorActive = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                if (_inputActions != null)
                {
                    _inputActions.Player.Look.Enable();
                }
            }
        }

        public void UIMode()
        {
            _actionMapStates.ChangeState(_ui);
            _isAltCursorActive = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // private void InitializePlayerInput()
        // {
        //     PlayerInput playerInput = gameObject.AddComponent<PlayerInput>();
        //     playerInput.actions = _inputActions.asset;
        //     playerInput.defaultControlScheme = "Keyboard&Mouse";
        //     playerInput.onControlsChanged += OnControlsChanged;
        // }

        // private void OnControlsChanged(PlayerInput input)
        // {
        //     Debug.Log("Control scheme changed to: " + input.currentControlScheme);
        //     var scheme = input.currentControlScheme;
        //     _currentControlScheme = scheme == "Gamepad"? SchemeType.Gamepad : SchemeType.Keyboard;
        // }

        public enum SchemeType
        {
            Keyboard,
            Gamepad,
            TouchScreen
        }
    }
}