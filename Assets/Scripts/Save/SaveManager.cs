using System.Collections.Generic;
using System.IO;
using Item;
using Mangrove;
using Newtonsoft.Json;
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

            var root = new
            {
                inventories = allInventories,
                plantSites = allMangroves
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

            Debug.Log("Game loaded!");
        }

        [System.Serializable]
        private class SaveRoot
        {
            public List<InventorySaveData> inventories;
            public List<MangroveSaveData> plantSites;
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