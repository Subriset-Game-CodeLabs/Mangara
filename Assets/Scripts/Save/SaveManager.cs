using System.Collections.Generic;
using System.IO;
using Item;
using Manager;
using Mangrove;
using Newtonsoft.Json;
using Progression;
using RandomEvent;
using UnityEngine;

namespace Save
{
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        [SerializeField] private List<InventorySO> _inventories;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        [SerializeField] private MangroveDatabaseSO _mangroveDatabase;
        private List<MangroveController> _plantSites = new List<MangroveController>();
        private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        public void SaveGame()
        {
            // Inventory
            var allInventories = new List<InventorySaveData>();

            // Mangrove
            var allMangroves = new List<MangroveSaveData>();

            // Random Events
            var allRandomEvents = RandomEventManager.Instance != null 
                ? RandomEventManager.Instance.GetSaveData() 
                : new List<EventSpawnSaveData>();

            // Progression
            var progressionData = ProgressionManager.Instance != null 
                ? ProgressionManager.Instance.GetSaveData() 
                : null;

            foreach (InventorySO inventory in _inventories)
            {
                allInventories.Add(inventory.GetSaveData());
            }

            foreach (MangroveController plantSite in _plantSites)
            {
                var plantSiteData = plantSite.GetSaveData();
                if (plantSiteData != null)
                {
                    allMangroves.Add(plantSiteData);
                }
            }

            int dayCount = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;

            var root = new
            {
                dayCount = dayCount,
                inventories = allInventories,
                plantSites = allMangroves,
                randomEvents = allRandomEvents,
                progression = progressionData
            };

            string json = JsonConvert.SerializeObject(root, Formatting.Indented);

            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved to: {SavePath}");
        }

        public void LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("No save file found. Starting fresh.");
                return;
            }

            string json = File.ReadAllText(SavePath);

            var root = JsonConvert.DeserializeObject<SaveRoot>(json);

            if (root != null)
            {
                if (GameManager.Instance != null && root.dayCount > 0)
                {
                    GameManager.Instance.SetDayNumber(root.dayCount);
                }

                if (root.inventories != null)
                {
                    foreach (InventorySaveData inventoryData in root.inventories)
                    {
                        // Find the InventorySO asset that matches this saved ID
                        InventorySO match = _inventories
                            .Find(inv => inv.InventoryID == inventoryData.InventoryID);

                        if (match == null)
                        {
                            Debug.LogWarning($"No InventorySO found with ID: {inventoryData.InventoryID}");
                            continue;
                        }

                        match.LoadFromSaveData(inventoryData, _itemDatabase);
                    }
                }

                if (root.plantSites != null)
                {
                    foreach (MangroveSaveData plantSite in root.plantSites)
                    {
                        MangroveController match = _plantSites.Find(sites => sites.PlantSiteId == plantSite.PlantSiteID);

                        if (match == null)
                        {
                            Debug.LogWarning($"No MangroveController found with ID: {plantSite.PlantSiteID}");
                            continue;
                        }

                        match.LoadFromSaveData(plantSite, _mangroveDatabase);
                    }
                }

                if (root.randomEvents != null && RandomEventManager.Instance != null)
                {
                    RandomEventManager.Instance.LoadFromSaveData(root.randomEvents, _itemDatabase);
                }

                if (root.progression != null && ProgressionManager.Instance != null)
                {
                    ProgressionManager.Instance.LoadFromSaveData(root.progression);
                }
            }

            Debug.Log("Game loaded!");
        }

        [System.Serializable]
        private class SaveRoot
        {
            public int dayCount;
            public List<InventorySaveData> inventories;
            public List<MangroveSaveData> plantSites;
            public List<EventSpawnSaveData> randomEvents;
            public ProgressionSaveData progression;
        }


        public bool HasSaveFile() => File.Exists(SavePath);

        public void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log($"Save file deleted: {SavePath}");
            }
            else
            {
                Debug.Log("No save file found to delete.");
            }
        }

        public void RegisterPlantSite(MangroveController mangroveController)
        {
            _plantSites.Add(mangroveController);
        }
    }
}