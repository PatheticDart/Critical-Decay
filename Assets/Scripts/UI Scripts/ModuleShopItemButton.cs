using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleShopItemButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI energyDrainText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button actionButton;

    [SerializeField] private AudioClip buttonClickSFX;

    private ModuleScriptableObject moduleData;

    public void Setup(ModuleScriptableObject data)
    {
        moduleData = data;

        if (iconImage != null) iconImage.sprite = data.moduleIcon;
        if (nameText != null) nameText.text = data.moduleName;
        if (energyDrainText != null) energyDrainText.text = $"Energy Drain: {data.energyDrain}";
        if (descriptionText != null) descriptionText.text = data.description;

        bool hasEnoughResources = CheckAffordability(data);

        if (costText != null)
        {
            string currencyLabel = data.costType == CurrencyType.Scrap ? "Scrap" : "Metal Bars";
            costText.text = $"Cost: {data.buildCost} {currencyLabel}";
            costText.color = hasEnoughResources ? Color.white : Color.red;
        }

        bool isAlreadyBuilt = false;
        if (data.isRefinery)
        {
            isAlreadyBuilt = ModuleHealth.ActivePlayerModules.Exists(m => m != null && m.IsRefinery);
        }
        else if (data.isObservatory)
        {
            isAlreadyBuilt = ModuleHealth.ActivePlayerModules.Exists(m => m != null && m.IsObservatory);
        }

        if (actionButton != null)
        {
            actionButton.interactable = !isAlreadyBuilt && hasEnoughResources;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnItemClicked);
        }
    }

    private bool CheckAffordability(ModuleScriptableObject data)
    {
        if (ResourceHandler.Instance == null) return false;

        if (data.costType == CurrencyType.Scrap)
        {
            return ResourceHandler.Instance.Scrap >= data.buildCost;
        }
        else
        {
            return ResourceHandler.Instance.MetalBars >= data.buildCost;
        }
    }

    private void OnItemClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick(1);
        }

        if (BuildSystem.Instance != null)
        {
            BuildSystem.Instance.SelectModuleToBuild(moduleData);
        }
    }
}