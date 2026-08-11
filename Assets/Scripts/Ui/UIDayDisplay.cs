using Manager;
using TMPro;
using UnityEngine;

namespace Ui
{
    public class UIDayDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dayText;

        private void Awake()
        {
            if (_dayText == null)
            {
                _dayText = GetComponent<TMP_Text>();
            }
        }

        private void Start()
        {
            UpdateDayText();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewDay += UpdateDayText;
            }
            UpdateDayText();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewDay -= UpdateDayText;
            }
        }

        public void UpdateDayText()
        {
            if (_dayText == null)
            {
                _dayText = GetComponent<TMP_Text>();
            }

            if (_dayText != null && GameManager.Instance != null)
            {
                _dayText.text = GameManager.Instance.DayNumber.ToString();
            }
        }
    }
}
