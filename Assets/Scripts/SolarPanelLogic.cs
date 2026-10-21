using UnityEngine;

public class SolarPanelLogic : MonoBehaviour
{
    [Header("Energy Stats")]
    [SerializeField] private int energyProduction = 10; // GDD: +10 Energy Threshold[cite: 19]

    public int EnergyProduction => energyProduction;

    private void OnEnable()
    {
        if (ResourceHandler.Instance != null)
        {
            ResourceHandler.Instance.ModifyMaxEnergy(energyProduction);
        }
    }

    private void OnDisable()
    {
        if (ResourceHandler.Instance != null)
        {
            ResourceHandler.Instance.ModifyMaxEnergy(-energyProduction);
        }
    }
}