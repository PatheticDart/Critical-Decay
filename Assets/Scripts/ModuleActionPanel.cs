using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleActionPanel : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI moduleNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI repairCostText;

    [Header("Action Buttons")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button repairButton;
    [SerializeField] private Button sellButton;

    private ModuleHealth targetModule;

    public void OpenPanel(ModuleHealth module)
    {
        targetModule = module;
        gameObject.SetActive(true);
        RefreshPanel();
    }

    public void ClosePanel()
    {
        targetModule = null;
        gameObject.SetActive(false);
    }

    public void RefreshPanel()
    {
        if (targetModule == null)
        {
            ClosePanel();
            return;
        }

        if (moduleNameText != null) moduleNameText.text = $"{targetModule.ModuleName} (Lvl {targetModule.CurrentLevel})";
        if (healthText != null) healthText.text = $"HP: {targetModule.CurrentHealth} / {targetModule.MaxHealth}";

        int missingHp = targetModule.MaxHealth - targetModule.CurrentHealth;
        float damagePercent = (float)missingHp / targetModule.MaxHealth;

        float maxRepairCost = 100f * (1f + (targetModule.CurrentLevel - 1) * 0.10f);
        int finalRepairCost = Mathf.CeilToInt(damagePercent * maxRepairCost);

        if (repairCostText != null) repairCostText.text = $"Repair: {finalRepairCost} Scrap";

        if (repairButton != null)
        {
            repairButton.interactable = (missingHp > 0) && (ResourceHandler.Instance != null && ResourceHandler.Instance.Scrap >= finalRepairCost);
            repairButton.onClick.RemoveAllListeners();
            repairButton.onClick.AddListener(() => OnRepairClicked(finalRepairCost));
        }

        bool canUpgrade = targetModule.IsTurret && ResearchHandler.Instance != null && ResearchHandler.Instance.unlockedWorkshopKnowledge && targetModule.CurrentLevel < 3;
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(targetModule.IsTurret);
            upgradeButton.interactable = canUpgrade;
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);
        }
    }

    private void OnRepairClicked(int cost)
    {
        if (targetModule == null) return;
        if (ResourceHandler.Instance != null && ResourceHandler.Instance.ConsumeScrap(cost))
        {
            targetModule.RepairModule();
            RefreshPanel();
        }
    }

    private void OnUpgradeClicked()
    {
        if (targetModule == null) return;

        int cost = targetModule.CurrentLevel == 1 ? 20 : 40;
        if (ResourceHandler.Instance != null && ResourceHandler.Instance.ConsumeMetalBars(cost))
        {
            targetModule.UpgradeModule(targetModule.MaxHealth + 50, targetModule.EnergyDrain + 10);
            RefreshPanel();
        }
    }

    private void OnSellClicked()
    {
        if (targetModule == null) return;

        if (ResourceHandler.Instance != null)
        {
            ModuleScriptableObject moduleData = System.Array.Find(
                BuildSystem.Instance.AllAvailableModules,
                data => data != null && data.moduleName == targetModule.ModuleName
            );

            if (moduleData != null)
            {
                int refundAmount = Mathf.FloorToInt(moduleData.buildCost * 0.5f);

                if (moduleData.costType == CurrencyType.Scrap)
                {
                    ResourceHandler.Instance.AddScrap(refundAmount);
                }
                else if (moduleData.costType == CurrencyType.MetalBar)
                {
                    ResourceHandler.Instance.AddMetalBars(refundAmount);
                }
            }
            else
            {
                ResourceHandler.Instance.AddScrap(10);
            }
        }

        targetModule.Die();
        ClosePanel();
    }
}