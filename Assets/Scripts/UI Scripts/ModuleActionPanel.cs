using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleActionPanel : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI moduleNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI repairCostText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

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

        ModuleScriptableObject moduleData = GetModuleData(targetModule.ModuleName);
        bool canUpgrade = targetModule.IsTurret && moduleData != null;
        int upgradeCost = 0;
        string currencyLabel = "Bars";

        if (canUpgrade)
        {
            upgradeCost = targetModule.GetUpgradeCost(moduleData.buildCost);
            currencyLabel = moduleData.costType == CurrencyType.Scrap ? "Scrap" : "Bars";

            bool hasResources = false;
            if (ResourceHandler.Instance != null)
            {
                hasResources = moduleData.costType == CurrencyType.Scrap
                    ? ResourceHandler.Instance.Scrap >= upgradeCost
                    : ResourceHandler.Instance.MetalBars >= upgradeCost;
            }

            if (upgradeButton != null)
            {
                upgradeButton.gameObject.SetActive(true);
                upgradeButton.interactable = hasResources;
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => OnUpgradeClicked(moduleData.buildCost, moduleData.costType, upgradeCost));
            }

            if (upgradeCostText != null)
            {
                upgradeCostText.text = $"Upgrade: {upgradeCost} {currencyLabel}";
            }
        }
        else
        {
            if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
            if (upgradeCostText != null) upgradeCostText.text = "";
        }

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);
        }
    }

    private ModuleScriptableObject GetModuleData(string name)
    {
        if (BuildSystem.Instance == null || BuildSystem.Instance.AllAvailableModules == null) return null;

        return System.Array.Find(
            BuildSystem.Instance.AllAvailableModules,
            data => data != null && data.moduleName == name
        );
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

    private void OnUpgradeClicked(int baseCost, CurrencyType costType, int calculatedCost)
    {
        if (targetModule == null) return;

        bool success = false;
        if (ResourceHandler.Instance != null)
        {
            if (costType == CurrencyType.Scrap)
            {
                success = ResourceHandler.Instance.ConsumeScrap(calculatedCost);
            }
            else
            {
                success = ResourceHandler.Instance.ConsumeMetalBars(calculatedCost);
            }
        }

        if (success)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick(2);
            }

            targetModule.UpgradeModule(baseCost, costType);
            RefreshPanel();
        }
    }

    private void OnSellClicked()
    {
        if (targetModule == null) return;

        if (ResourceHandler.Instance != null)
        {
            ModuleScriptableObject moduleData = GetModuleData(targetModule.ModuleName);

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