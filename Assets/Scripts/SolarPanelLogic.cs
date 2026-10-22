using UnityEngine;

public class SolarPanelLogic : MonoBehaviour
{
    [Header("Energy Stats")]
    [SerializeField] private int energyProduction = 10;

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