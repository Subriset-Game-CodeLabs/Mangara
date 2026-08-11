using System;
using Input;
using Save;
using UnityEngine;

namespace Manager
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        [SerializeField] private float _dayDurationInSeconds = 120f;
        [SerializeField] private bool _enableForcedSleep = true;
        [SerializeField] private float _forcedSleepTime = 2.0f; // 2:00 AM by default

        public float TimeOfDay;
        public int DayNumber { get; private set; } = 1;
        public bool IsSleeping { get; private set; }

        public event Action OnNewDay;
        public event Action OnForcedSleep;

        private const float WAKE_UP_TIME = 6.0f;

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
            OnNewDay?.Invoke();

            SaveManager.Instance.SaveGame();
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