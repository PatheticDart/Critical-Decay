using UnityEngine;

public class ResearchHandler : MonoBehaviour
{
    public static ResearchHandler Instance { get; private set; }

    [Header("Unlocked Technologies")]
    public bool unlockedEnergyProduction = false;
    public bool unlockedMoltenMetal = false;
    public bool unlockedBoltAcquisition = false;
    public bool unlockedWorkshopKnowledge = false;
    public bool unlockedCombatOutfit = false;
    public bool unlockedImprovedRefining = false;
    public bool unlockedEfficientRefining = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool UnlockTechnology(string techKey)
    {
        ResourceHandler res = ResourceHandler.Instance;
        if (res == null) return false;

        switch (techKey)
        {
            case "EnergyProduction":
                if (!unlockedEnergyProduction && res.ConsumeResearchSamples(5))
                {
                    unlockedEnergyProduction = true;
                    OnTechUnlocked();
                    return true;
                }
                break;

            case "MoltenMetal":
                if (!unlockedMoltenMetal && res.ConsumeResearchSamples(10))
                {
                    unlockedMoltenMetal = true;
                    OnTechUnlocked();
                    return true;
                }
                break;

            case "BoltAcquisition":
                if (!unlockedBoltAcquisition && res.ConsumeResearchSamples(20))
                {
                    unlockedBoltAcquisition = true;
                    OnTechUnlocked();
                    return true;
                }
                break;

            case "WorkshopKnowledge":
                if (!unlockedWorkshopKnowledge && res.ConsumeResearchSamples(50))
                {
                    unlockedWorkshopKnowledge = true;
                    OnTechUnlocked();
                    return true;
                }
                break;

            case "CombatOutfit":
                if (!unlockedCombatOutfit && res.ConsumeMetalBars(100))
                {
                    unlockedCombatOutfit = true;
                    ApplyCombatOutfitToPlayer();
                    OnTechUnlocked();
                    return true;
                }
                break;

            case "ImprovedRefining":
                if (!unlockedImprovedRefining && res.ConsumeResearchSamples(40))
                {
                    unlockedImprovedRefining = true;
                    res.SetScrapPerConversion(2);
                    OnTechUnlocked();
                    return true;
                }
                break;

            case "EfficientRefining":
                if (unlockedImprovedRefining && !unlockedEfficientRefining && res.ConsumeResearchSamples(80))
                {
                    unlockedEfficientRefining = true;
                    res.SetScrapPerConversion(1);
                    OnTechUnlocked();
                    return true;
                }
                break;
        }

        return false;
    }

    private void ApplyCombatOutfitToPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>(true);
        if (player != null)
        {
            player.UpgradeToCombatOutfit();
        }
    }

    private void OnTechUnlocked()
    {
        if (BuildSystem.Instance != null)
        {
            BuildSystem.Instance.RefreshShopUI();
        }
        if (ResearchUIManager.Instance != null)
        {
            ResearchUIManager.Instance.RefreshResearchPanel();
        }
    }
}