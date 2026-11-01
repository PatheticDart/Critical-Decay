using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchItemButton : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI techNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Button Reference")]
    [SerializeField] private Button buyButton;

    private string techKey;

    public void Setup(string key, string nameStr, string costStr, string detailsStr, bool isUnlocked)
    {
        techKey = key;

        if (techNameText != null) techNameText.text = nameStr;
        if (costText != null) costText.text = costStr;
        if (detailsText != null) detailsText.text = detailsStr;

        bool isPrerequisiteMet = TruePrerequisiteMet(key);
        bool canAfford = CheckAffordability(key);

        if (costText != null)
        {
            costText.color = canAfford ? Color.white : Color.red;
        }

        if (isUnlocked)
        {
            if (statusText != null)
            {
                statusText.text = "RESEARCHED";
                statusText.color = Color.green;
            }
            if (buyButton != null) buyButton.interactable = false;
        }
        else if (!isPrerequisiteMet)
        {
            if (statusText != null)
            {
                statusText.text = "LOCKED (REQ: IMPROVED REFINING)";
                statusText.color = Color.red;
            }
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            if (statusText != null)
            {
                statusText.text = "RESEARCH";
                statusText.color = Color.white;
            }

            if (buyButton != null)
            {
                buyButton.interactable = canAfford;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }
    }

    private bool TruePrerequisiteMet(string key)
    {
        if (key == "EfficientRefining")
        {
            return ResearchHandler.Instance != null && ResearchHandler.Instance.unlockedImprovedRefining;
        }
        return true;
    }

    private bool CheckAffordability(string key)
    {
        if (ResourceHandler.Instance == null) return false;

        switch (key)
        {
            case "EnergyProduction": return ResourceHandler.Instance.ResearchSamples >= 5;
            case "MoltenMetal": return ResourceHandler.Instance.ResearchSamples >= 10;
            case "BoltAcquisition": return ResourceHandler.Instance.ResearchSamples >= 20;
            case "WorkshopKnowledge": return ResourceHandler.Instance.ResearchSamples >= 50;
            case "CombatOutfit": return ResourceHandler.Instance.MetalBars >= 100;
            case "ImprovedRefining": return ResourceHandler.Instance.ResearchSamples >= 40;
            case "EfficientRefining": return ResourceHandler.Instance.ResearchSamples >= 80;
            case "CollectorDrones": return ResourceHandler.Instance.ResearchSamples >= 50;
            default: return true;
        }
    }

    private void OnBuyClicked()
    {
        if (ResearchHandler.Instance != null)
        {
            bool success = ResearchHandler.Instance.UnlockTechnology(techKey);
            if (success && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick(2);
            }
        }
    }
}