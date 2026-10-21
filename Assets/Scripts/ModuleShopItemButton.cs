using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleShopItemButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI energyDrainText;
    [SerializeField] private Button actionButton;

    private ModuleScriptableObject moduleData;

    public void Setup(ModuleScriptableObject data)
    {
        moduleData = data;

        if (iconImage != null) iconImage.sprite = data.moduleIcon;
        if (nameText != null) nameText.text = data.moduleName;
        if (costText != null)
        {
            string currencyLabel = data.costType == CurrencyType.Scrap ? "Scrap" : "Bars";
            costText.text = $"{data.buildCost} {currencyLabel}";
        }
        if (energyDrainText != null) energyDrainText.text = $"{data.energyDrain} Energy";

        // Check unique module existence (Refinery & Observatory)
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
            actionButton.interactable = !isAlreadyBuilt;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                if (BuildSystem.Instance != null)
                {
                    BuildSystem.Instance.SelectModuleToBuild(moduleData);
                }
            });
        }
    }
}