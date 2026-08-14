using System.Collections.Generic;
using UnityEngine;

public enum MangroveType
{
    // --- Kategori Rhizophora ---
    RhizophoraMucronata, // Urutan 0
    RhizophoraApiculata, // Urutan 1
    RhizophoraStylosa,   // Urutan 2

    // --- Kategori Avicennia ---
    AvicenniaMarina,     // Urutan 3
    AvicenniaAlba,       // Urutan 4
    AvicenniaOfficinalis,// Urutan 5

    // --- Kategori Sonneratia ---
    SonneratiaAlba,      // Urutan 6
    SonneratiaCaseolaris,// Urutan 7
    SonneratiaOvata,     // Urutan 8

    // --- Kategori Bruguiera ---
    BruguieraGymnorhiza, // Urutan 9
    BruguieraCylindrica, // Urutan 10
    BruguieraSexangula   // Urutan 11
}

[System.Serializable]
public struct MangroveData
{
    public MangroveType type;
    public string mangroveName;
    
    [TextArea(3, 10)]
    public string description;
    
    public Sprite mangroveImage;
}

public class BestiaryManager : Singleton<BestiaryManager>
{
    [SerializeField] private List<MangroveType> unlockedMangroves = new List<MangroveType>();

    // Database yang menyimpan semua informasi teks dan gambar
    [SerializeField] private List<MangroveData> mangroveDatabase;

    public void UnlockMangrove(MangroveType type)
    {
        if (!unlockedMangroves.Contains(type))
        {
            unlockedMangroves.Add(type);
        }

        for (int i = 0; i < mangroveDatabase.Count; i++)
        {
            if (mangroveDatabase[i].type == type)
            {
                Debug.Log("Bestiary Unlocked: " + mangroveDatabase[i].mangroveName);
                return;
            }
        }
    }

    public bool IsUnlocked(MangroveType type)
    {
        return unlockedMangroves.Contains(type);
    }

    public MangroveData GetMangroveData(int index)
    {
        if (TryGetMangroveData(index, out MangroveData data))
        {
            return data;
        }

        Debug.LogWarning($"Bestiary index out of range: {index}. Total entries: {mangroveDatabase.Count}.");
        return new MangroveData();
    }

    public bool TryGetMangroveData(int index, out MangroveData data)
    {
        if (index >= 0 && index < mangroveDatabase.Count)
        {
            data = mangroveDatabase[index];
            return true;
        }

        data = default;
        return false;
    }

    public bool TryGetMangroveData(MangroveType type, out MangroveData data)
    {
        for (int i = 0; i < mangroveDatabase.Count; i++)
        {
            if (mangroveDatabase[i].type == type)
            {
                data = mangroveDatabase[i];
                return true;
            }
        }

        data = default;
        return false;
    }

    public MangroveData GetMangroveData(MangroveType type)
    {
        if (TryGetMangroveData(type, out MangroveData data))
        {
            return data;
        }

        Debug.LogWarning($"Mangrove data not found for type: {type}.");
        return default;
    }

    public Save.BestiarySaveData GetSaveData()
    {
        return new Save.BestiarySaveData
        {
            unlockedMangroves = new List<MangroveType>(unlockedMangroves)
        };
    }

    public void LoadFromSaveData(Save.BestiarySaveData data)
    {
        if (data == null || data.unlockedMangroves == null) return;
        unlockedMangroves = new List<MangroveType>(data.unlockedMangroves);
        Debug.Log($"[BestiaryManager] Loaded {unlockedMangroves.Count} unlocked bestiary entries.");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var seen = new HashSet<MangroveType>();
        for (int i = 0; i < mangroveDatabase.Count; i++)
        {
            var entry = mangroveDatabase[i];
            if (!seen.Add(entry.type))
            {
                Debug.LogWarning($"Duplicate mangrove type found in database: {entry.type} (index {i}).", this);
            }
        }
    }
#endif
}