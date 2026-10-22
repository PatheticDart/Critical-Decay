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
    [SerializeField] private TextMeshProUGUI refineryStatusText;

    [Header("Action Buttons")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button repairButton;
    [SerializeField] private Button sellButton;

    [Header("Refinery Toggle Buttons")]
    [SerializeField] private Button startRefineryButton;
    [SerializeField] private Button stopRefineryButton;

    [Header("Upgrade Caps")]
    [SerializeField] private int maxUpgradeLevel = 5;

    private ModuleHealth targetModule;

    public void OpenPanel(ModuleHealth module)
    {
        if (targetModule != null)
        {
            targetModule.SetRangeIndicatorVisible(false);
        }

        targetModule = module;
        gameObject.SetActive(true);

        if (targetModule != null && targetModule.IsTurret)
        {
            targetModule.SetRangeIndicatorVisible(true);
        }

        RefreshPanel();
    }

    public void ClosePanel()
    {
        if (targetModule != null)
        {
            targetModule.SetRangeIndicatorVisible(false);
        }

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
        bool canBeUpgradedType = (targetModule.IsTurret || targetModule.IsRefinery) && moduleData != null;

        if (canBeUpgradedType)
        {
            if (upgradeButton != null) upgradeButton.gameObject.SetActive(true);

            if (targetModule.CurrentLevel >= maxUpgradeLevel)
            {
                if (upgradeButton != null) upgradeButton.interactable = false;
                if (upgradeCostText != null) upgradeCostText.text = "Upgrade: MAX LEVEL";
            }
            else
            {
                int upgradeCost = targetModule.GetUpgradeCost(moduleData.buildCost);
                string currencyLabel = moduleData.costType == CurrencyType.Scrap ? "Scrap" : "Bars";

                bool hasResources = false;
                if (ResourceHandler.Instance != null)
                {
                    hasResources = moduleData.costType == CurrencyType.Scrap
                        ? ResourceHandler.Instance.Scrap >= upgradeCost
                        : ResourceHandler.Instance.MetalBars >= upgradeCost;
                }

                if (upgradeButton != null)
                {
                    upgradeButton.interactable = hasResources;
                    upgradeButton.onClick.RemoveAllListeners();
                    upgradeButton.onClick.AddListener(() => OnUpgradeClicked(moduleData.buildCost, moduleData.costType, upgradeCost));
                }

                if (upgradeCostText != null)
                {
                    upgradeCostText.text = $"Upgrade: {upgradeCost} {currencyLabel}";
                }
            }
        }
        else
        {
            if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
            if (upgradeCostText != null) upgradeCostText.text = "";
        }

        bool isRefinery = targetModule.IsRefinery;
        bool isRefineryRunning = ResourceHandler.Instance != null && ResourceHandler.Instance.IsRefineryActive;

        if (refineryStatusText != null)
        {
            refineryStatusText.gameObject.SetActive(isRefinery);
            if (isRefinery)
            {
                refineryStatusText.text = isRefineryRunning ? "Status: PROCESSING" : "Status: STOPPED";
                refineryStatusText.color = isRefineryRunning ? Color.green : Color.red;
            }
        }

        if (startRefineryButton != null)
        {
            startRefineryButton.gameObject.SetActive(isRefinery);
            startRefineryButton.interactable = isRefinery && !isRefineryRunning;
            startRefineryButton.onClick.RemoveAllListeners();
            startRefineryButton.onClick.AddListener(OnStartRefineryClicked);
        }

        if (stopRefineryButton != null)
        {
            stopRefineryButton.gameObject.SetActive(isRefinery);
            stopRefineryButton.interactable = isRefinery && isRefineryRunning;
            stopRefineryButton.onClick.RemoveAllListeners();
            stopRefineryButton.onClick.AddListener(OnStopRefineryClicked);
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

    private void OnStartRefineryClicked()
    {
        if (ResourceHandler.Instance != null)
        {
            ResourceHandler.Instance.SetRefineryActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick(1);
        }

        RefreshPanel();
    }

    private void OnStopRefineryClicked()
    {
        if (ResourceHandler.Instance != null)
        {
            ResourceHandler.Instance.SetRefineryActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick(1);
        }

        RefreshPanel();
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
        if (targetModule == null || targetModule.CurrentLevel >= maxUpgradeLevel) return;

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

            if (targetModule.IsRefinery && ResourceHandler.Instance != null)
            {
                ResourceHandler.Instance.UpdateRefinerySpeed(targetModule.CurrentLevel);
            }

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