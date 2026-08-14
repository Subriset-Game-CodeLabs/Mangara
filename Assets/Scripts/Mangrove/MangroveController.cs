using System;
using Manager;
using Save;
using UnityEngine;

namespace Mangrove
{
    public class MangroveController : MonoBehaviour
    {
        [SerializeField] private string _plantSiteID;
        [SerializeField] private float _spawnYOffset = 0f;
        private MangroveDataSO _data;

        private PlantState _plantState;

        private int _currentStage;
        private int _daysInCurrentStage;
        private bool _isWatered;

        private GameObject _currentStagePrefabInstance;

        public event Action OnStatusChanged;

        public PlantState PlantState => _plantState;
        
        public string PlantSiteId => _plantSiteID;
        public float SpawnYOffset => _spawnYOffset;
        public bool IsWatered => _isWatered;
        public bool NeedsWater => (_plantState == PlantState.Planted || _plantState == PlantState.Growing) && !_isWatered;

        private void Awake()
        {
            SaveManager.Instance.RegisterPlantSite(this);
        }

        public void Plant(MangroveDataSO data)
        {
            _data = data;
            _currentStage = 0;
            _daysInCurrentStage = 0;
            _isWatered = false;
            TransitionTo(PlantState.Planted);

            GameManager.Instance.OnNewDay += OnNewDay;
        }

        private void OnNewDay()
        {
            if (_plantState == PlantState.Empty || _plantState == PlantState.Dead)
                return;
            
            if (_isWatered && (_plantState == PlantState.Growing || _plantState == PlantState.Planted))
            {
                AdvanceGrowth();
            }

            _isWatered = false;
            OnStatusChanged?.Invoke();
        }

        private void AdvanceGrowth()
        {
            _daysInCurrentStage++;

            if (_daysInCurrentStage >= _data.DaysPerStage[_currentStage])
            {
                _daysInCurrentStage = 0;
                _currentStage++;

                if (_currentStage >= _data.DaysPerStage.Length)
                {
                    TransitionTo(PlantState.Harvestable);
                }
                else
                {
                    SwapStagePrefab();
                    TransitionTo(PlantState.Growing);
                }
            }
        }

        public void Water()
        {
            if (_plantState == PlantState.Empty || _plantState == PlantState.Dead)
                return;
            _isWatered = true;
            OnStatusChanged?.Invoke();
        }

        private void TransitionTo(PlantState plantState)
        {
            _plantState = plantState;
            SwapStagePrefab();
            OnStatusChanged?.Invoke();
        }

        public (ItemBaseSO item, int quantity)? Harvest()
        {
            if (_plantState != PlantState.Harvestable) return null;

            var result = (_data.HarvestItem, _data.HarvestAmount);
            TransitionTo(PlantState.Empty);
            Destroy(_currentStagePrefabInstance);
            return result;
        }

        private void SwapStagePrefab()
        {
            if (_data == null) return;
            if (_currentStage >= _data.StageObjects.Length) return;
            if (_currentStagePrefabInstance != null)
                Destroy(_currentStagePrefabInstance);

            Vector3 spawnPosition = transform.position + new Vector3(0f, _spawnYOffset, 0f);
            _currentStagePrefabInstance = Instantiate(_data.StageObjects[_currentStage], spawnPosition,
                Quaternion.identity, transform);
        }

        public MangroveSaveData GetSaveData()
        {
            if (_plantState == PlantState.Empty)
                return null;
            
            var saveData = new MangroveSaveData
            {
                CurrentStage = _currentStage,
                DaysInCurrentStage = _daysInCurrentStage,
                MangroveId = _data.ID,
                PlantState = _plantState,
                PlantSiteID = _plantSiteID
            };
            
            return saveData;
        }

        public void LoadFromSaveData(MangroveSaveData saveData, MangroveDatabaseSO mangroveDatabase)
        {
            _currentStage = saveData.CurrentStage;
            _daysInCurrentStage = saveData.DaysInCurrentStage;
            _plantState = saveData.PlantState;
            _data = mangroveDatabase.GetMangroveDataByID(saveData.MangroveId);
            
            SwapStagePrefab();
            GameManager.Instance.OnNewDay += OnNewDay;
            OnStatusChanged?.Invoke();
        }
    }
}