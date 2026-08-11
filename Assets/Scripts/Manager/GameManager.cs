using System;
using Input;
using Save;
using UnityEngine;

namespace Manager
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        [SerializeField] private float _dayDurationInSeconds;
        public float TimeOfDay;
        public int DayNumber { get; private set; } = 1;

        public event Action OnNewDay;

        private void Update()
        {
            TimeOfDay += (24f / _dayDurationInSeconds) * Time.deltaTime;
            TimeOfDay %= 24;
            
        }

        public void Sleep()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.StartSleepSequence();
            }
            else
            {
                CompleteSleep();
            }
        }

        public void CompleteSleep()
        {
            TimeOfDay = 6;
            DayNumber += 1;
            OnNewDay?.Invoke();

            SaveManager.Instance.SaveGame();
        }

        public void SetDayNumber(int dayNumber)
        {
            DayNumber = Mathf.Max(1, dayNumber);
        }

        private void Start()
        {
            SaveManager.Instance.LoadGame();
        }
    }
}