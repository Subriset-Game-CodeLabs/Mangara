using System;
using Input;
using Save;
using UnityEngine;

namespace Manager
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private float _dayDurationInSeconds = 120f;
        [SerializeField] private bool _enableForcedSleep = true;
        [SerializeField] private float _forcedSleepTime = 2.0f; // 2:00 AM by default

        [Header("Forced Sleep / Spawn Settings")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Transform _forcedSleepWakeUpPoint;
        [SerializeField] private Vector3 _defaultForcedSleepWakeUpPosition;
        [SerializeField] private bool _wakeUpAtPositionOnNormalSleep = false;

        public float TimeOfDay;
        public int DayNumber { get; private set; } = 1;
        public bool IsSleeping { get; private set; }

        public event Action OnNewDay;
        public event Action OnForcedSleep;

        private const float WAKE_UP_TIME = 6.0f;
        private bool _lastSleepWasForced;

        private void Update()
        {
            if (IsSleeping) return;

            TimeOfDay += (24f / _dayDurationInSeconds) * Time.deltaTime;
            TimeOfDay %= 24;

            CheckForcedSleep();
        }

        private void CheckForcedSleep()
        {
            if (!_enableForcedSleep || IsSleeping) return;

            bool shouldForceSleep;
            if (_forcedSleepTime < WAKE_UP_TIME)
            {
                shouldForceSleep = TimeOfDay >= _forcedSleepTime && TimeOfDay < WAKE_UP_TIME;
            }
            else
            {
                shouldForceSleep = TimeOfDay >= _forcedSleepTime || TimeOfDay < WAKE_UP_TIME;
            }

            if (shouldForceSleep)
            {
                OnForcedSleep?.Invoke();
                Sleep(isForcedSleep: true);
            }
        }

        public void Sleep(bool isForcedSleep = false)
        {
            if (IsSleeping) return;
            IsSleeping = true;
            _lastSleepWasForced = isForcedSleep;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.StartSleepSequence(isForcedSleep);
            }
            else
            {
                CompleteSleep();
            }
        }

        public void CompleteSleep()
        {
            TimeOfDay = WAKE_UP_TIME;
            DayNumber += 1;
            IsSleeping = false;

            if (_lastSleepWasForced || _wakeUpAtPositionOnNormalSleep)
            {
                TeleportPlayerToWakeUpPosition();
            }

            _lastSleepWasForced = false;
            OnNewDay?.Invoke();

            SaveManager.Instance.SaveGame();
        }

        private void TeleportPlayerToWakeUpPosition()
        {
            Transform player = GetPlayerTransform();
            if (player == null) return;

            Vector3 targetPosition = _forcedSleepWakeUpPoint != null
                ? _forcedSleepWakeUpPoint.position
                : _defaultForcedSleepWakeUpPosition;

            Quaternion targetRotation = _forcedSleepWakeUpPoint != null
                ? _forcedSleepWakeUpPoint.rotation
                : player.rotation;

            if (player.TryGetComponent<PlayerMovement>(out var playerMovement))
            {
                playerMovement.Teleport(targetPosition, targetRotation);
            }
            else
            {
                if (player.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = targetPosition;
                    rb.rotation = targetRotation;
                }
                player.position = targetPosition;
                player.rotation = targetRotation;
            }
        }

        private Transform GetPlayerTransform()
        {
            if (_playerTransform != null) return _playerTransform;

            var movement = FindObjectOfType<PlayerMovement>();
            if (movement != null)
            {
                _playerTransform = movement.transform;
                return _playerTransform;
            }

            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
                return _playerTransform;
            }

            return null;
        }

        public void SetDayNumber(int dayNumber)
        {
            DayNumber = Mathf.Max(1, dayNumber);
            OnNewDay?.Invoke();
        }

        private void Start()
        {
            SaveManager.Instance.LoadGame();
        }
    }
}