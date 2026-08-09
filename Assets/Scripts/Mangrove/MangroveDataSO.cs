using UnityEngine;

[ManageableData]
[CreateAssetMenu(fileName = "MangroveData", menuName = "Scriptable Objects/MangroveData")]
public class MangroveDataSO : ScriptableObject
{
    [SerializeField] private string _name;
    
    [SerializeField] private int[] _daysPerStage;
    [SerializeField] private GameObject[] _stageObjects;

    [SerializeField] private ItemBaseSO _harvestItem;
    [SerializeField] private int _harvestAmount;

    public string ID;
    public int[] DaysPerStage => _daysPerStage;
    public ItemBaseSO HarvestItem => _harvestItem;
    public int HarvestAmount => _harvestAmount;
    public GameObject[]  StageObjects => _stageObjects;
}
