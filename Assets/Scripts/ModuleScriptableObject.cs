using UnityEngine;

public enum CurrencyType
{
    Scrap,
    MetalBar
}

[CreateAssetMenu(fileName = "NewModuleData", menuName = "Critical Decay/Module Data")]
public class ModuleScriptableObject : ScriptableObject
{
    public string moduleName;
    public GameObject modulePrefab;
    public Sprite moduleIcon;
    public CurrencyType costType = CurrencyType.MetalBar;
    public int buildCost = 10;
    public int energyDrain = 5;
    public string requiredTechKey = ""; // Empty if available from start
    public bool isRefinery = false;
    public bool isObservatory = false;
}