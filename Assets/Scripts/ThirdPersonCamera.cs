using System;
using Input;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform _player;      // Parent Player (Objek Rigidbody)
    [SerializeField] private Transform _playerObj;   // Child Model Visual
    [SerializeField] private Transform _orientation; // Child Orientation

    [Header("Camera Settings")]
    [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f); // Jarak kamera dari Player
    [SerializeField] private float _followSpeed = 20f;   // Kecepatan ikutan kamera
    [SerializeField] private float _rotationSpeed = 10f; // Kecepatan rotasi karakter

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (_player == null || _orientation == null) return;

        // 1. KAMERA MENGIKUTI POSISI PLAYER DENGAN SMOOTH LERP
        Vector3 targetCameraPosition = _player.position + transform.rotation * _cameraOffset;
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, Time.deltaTime * _followSpeed);

        // 2. UPDATE ORIENTATION (Berdasarkan arah pandang kamera)
        Vector3 viewDir = _player.position - new Vector3(transform.position.x, _player.position.y, transform.position.z);
        if (viewDir.sqrMagnitude > 0.001f)
        {
            _orientation.forward = viewDir.normalized;
        }

        // 3. PUTAR MODEL VISUAL PLAYER
        Vector2 movement = Vector2.zero;
        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null)
        {
            movement = InputManager.Instance.PlayerInput.Movement.Get();
        }

        Vector3 inputDir = (_orientation.forward * movement.y + _orientation.right * movement.x).normalized;

        if (inputDir.sqrMagnitude > 0.001f && _playerObj != null)
        {
            _playerObj.forward = Vector3.Slerp(_playerObj.forward, inputDir, Time.deltaTime * _rotationSpeed);
        }
    }
}