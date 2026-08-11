using System;
using System.Collections.Generic;
using Input;
using Inventory;
using TMPro;
using UnityEngine;

using Ui;

namespace Manager
{
    public class UIManager : PersistentSingleton<UIManager>
    {
        [SerializeField] private UiInventoryPage _playerInventoryPage;
        [SerializeField] private UiInventoryPage _externalInventoryPage;
        [SerializeField] private UIItemSelector _itemSelector;
        [SerializeField] private UiSleepPage _sleepPage;
        [SerializeField] private ProgressionUI _progressionUI;
        [SerializeField] private UIHotbar _hotbar;

        public UIHotbar Hotbar => _hotbar;

        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private UIDayDisplay _dayDisplay;
        public UIDayDisplay DayDisplay => _dayDisplay;


        // From HUD UIManager
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI interactionText;

        [Header("Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0);

        private Transform seedTarget;

        public void ToogleInventory(InventoryHandler inventoryHandler)
        {
            if (_playerInventoryPage.gameObject.activeSelf)
            {
                // If the chest is open, close it together with the player inventory
                if (_externalInventoryPage.gameObject.activeSelf)
                {
                    _externalInventoryPage.Hide();
                    InventoryStateData.ChestInventory = null;
                }

                _playerInventoryPage.Hide();
                Time.timeScale = 1;
                InputManager.Instance.PlayerMode();
            }
            else
            {
                InputManager.Instance.UIMode();
                Time.timeScale = 0;
                _playerInventoryPage.Show();
                foreach (var item in inventoryHandler.InventoryData.GetCurrentInventoryState())
                {
                    _playerInventoryPage.UpdateData(item.Key, item.Value.Item.ItemSprite, item.Value.Quantity);
                }
            }
        }

        public void ToogleChest(InventoryHandler inventoryHandler)
        {
            if (_externalInventoryPage.gameObject.activeSelf)
            {
                // Closing the chest also closes the player inventory
                _externalInventoryPage.Hide();
                InventoryStateData.ChestInventory = null;

                _playerInventoryPage.Hide();
                Time.timeScale = 1;
                InputManager.Instance.PlayerMode();
            }
            else
            {
                InventoryStateData.ChestInventory = inventoryHandler;
                InputManager.Instance.UIMode();
                Time.timeScale = 0;
                _externalInventoryPage.Show();
                foreach (var item in inventoryHandler.InventoryData.GetCurrentInventoryState())
                {
                    _externalInventoryPage.UpdateData(item.Key, item.Value.Item.ItemSprite, item.Value.Quantity);
                }

                // Ensure the player inventory is visible alongside the chest
                if (!_playerInventoryPage.gameObject.activeSelf)
                {
                    _playerInventoryPage.Show();
                    var playerHandler = InventoryController.Instance.InventoryHandler;
                    foreach (var item in playerHandler.InventoryData.GetCurrentInventoryState())
                    {
                        _playerInventoryPage.UpdateData(item.Key, item.Value.Item.ItemSprite, item.Value.Quantity);
                    }
                }
            }
        }

        public void ShowItemSelector(List<InventoryItem> items, Action<ItemBaseSO> onItemSelected)
        {
            Time.timeScale = 0;
            InputManager.Instance.UIMode();
            _itemSelector.Show(items, onItemSelected);
        }

        public void StartSleepSequence(bool isForcedSleep = false)
        {
            if (_sleepPage != null)
            {
                InputManager.Instance.UIMode();
                _sleepPage.ShowSleepSequence(GameManager.Instance.DayNumber, () =>
                {
                    GameManager.Instance.CompleteSleep();
                }, isForcedSleep);
            }
            else
            {
                GameManager.Instance.CompleteSleep();
            }
        }

        public void OnSleepSequenceFinished()
        {
            InputManager.Instance.PlayerMode();
        }



        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewDay += UpdateDayText;
                UpdateDayText();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewDay -= UpdateDayText;
            }
        }

        public void UpdateDayText()
        {
            if (_dayDisplay != null)
            {
                _dayDisplay.UpdateDayText();
            }
        }

        public void ToggleTodoList()
        {
            if (_progressionUI != null)
            {
                _progressionUI.ToggleTodoList();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && _timeText != null)
            {
                ConvertTime(GameManager.Instance.TimeOfDay);
            }

            // From HUD UIManager
            if (seedTarget != null && interactionText != null && interactionText.gameObject.activeSelf)
            {
                Vector2 posisiLayar = Camera.main.WorldToScreenPoint(seedTarget.position + offset);
                interactionText.transform.position = posisiLayar;
            }
        }

        private void ConvertTime(float timeValue)
        {
            int hour = (int)timeValue;
            float fraction = timeValue - hour;
            int interval = (int)Math.Floor(fraction * 6);
            if (interval >= 6)
            {
                hour++;
                interval = 0;
            }
            int minutes = interval * 10;

            if (_timeText != null)
            {
                _timeText.text = $"{hour:00}:{minutes:00}";
            }
        }



        // From HUD UIManager
        public void ShowText(string text, Transform target)
        {
            interactionText.text = text;
            seedTarget = target;
            interactionText.gameObject.SetActive(true);
        }

        public void HideText()
        {
            seedTarget = null;
            interactionText.gameObject.SetActive(false);
        }
    }
}