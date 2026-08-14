using System;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _orientation;
    [SerializeField] private Animator _animator;

    [Header("Movement Speeds")]
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _jogSpeed = 6f;
    [SerializeField] private float _crouchSpeed = 1.5f;
    [SerializeField] private float _speedDamping = 10f;

    private Rigidbody _rigidbody;
    private Vector2 _movement;

    private float _currentSpeed;
    private float _targetSpeed;

    private bool _isSprinting;
    private bool _isCrouching;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsCrouchingHash = Animator.StringToHash("isCrouching");

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // Mengaktifkan Interpolation wajib dilakukan
        if (_rigidbody != null)
        {
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation; // Bekukan semua rotasi fisika
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        ReadInput();
        UpdateAnimations();
    }

    private void ReadInput()
    {
        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null)
        {
            _movement = InputManager.Instance.PlayerInput.Movement.Get();
        }

        if (Keyboard.current != null)
        {
            _isSprinting = Keyboard.current.leftShiftKey.isPressed;
            _isCrouching = Keyboard.current.leftCtrlKey.isPressed;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 forward = _orientation.forward;
        Vector3 right = _orientation.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * _movement.y + right * _movement.x).normalized;
        bool hasInput = _movement.sqrMagnitude > 0.01f;

        if (!hasInput)
        {
            _targetSpeed = 0f;
        }
        else if (_isCrouching)
        {
            _targetSpeed = _crouchSpeed;
        }
        else if (_isSprinting)
        {
            _targetSpeed = _jogSpeed;
        }
        else
        {
            _targetSpeed = _walkSpeed;
        }

        _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.fixedDeltaTime * _speedDamping);

        // MENGGUNAKAN VELOCITY (Bukan MovePosition)
        // Menjaga nilai Y (gravitasi) tetap sama
        Vector3 targetVelocity = moveDirection * _currentSpeed;
        
#if UNITY_6000_0_OR_NEWER
        _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
#else
        _rigidbody.velocity = new Vector3(targetVelocity.x, _rigidbody.velocity.y, targetVelocity.z);
#endif
    }

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        bool hasInput = _movement.sqrMagnitude > 0.01f;
        float targetAnimSpeedParam = 0f;

        if (hasInput)
        {
            if (_isCrouching)
            {
                targetAnimSpeedParam = 0.5f;
            }
            else if (_isSprinting)
            {
                targetAnimSpeedParam = 1.0f;
            }
            else
            {
                targetAnimSpeedParam = 0.5f;
            }
        }

        _animator.SetFloat(SpeedHash, targetAnimSpeedParam, 0.05f, Time.deltaTime);
        _animator.SetBool(IsCrouchingHash, _isCrouching);
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = position;
            _rigidbody.rotation = rotation;
        }

        transform.position = position;
        transform.rotation = rotation;
    }
}