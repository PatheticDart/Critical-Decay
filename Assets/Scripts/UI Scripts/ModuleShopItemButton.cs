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

    [SerializeField] private AudioClip buttonClickSFX;

    private ModuleScriptableObject moduleData;

    public void Setup(ModuleScriptableObject data)
    {
        moduleData = data;

        if (iconImage != null) iconImage.sprite = data.moduleIcon;
        if (nameText != null) nameText.text = data.moduleName;
        if (costText != null)
        {
            string currencyLabel = data.costType == CurrencyType.Scrap ? "Scrap" : " Metal Bars";
            costText.text = $"Cost: {data.buildCost} {currencyLabel}";
        }
        if (energyDrainText != null) energyDrainText.text = $"Energy Drain: {data.energyDrain}";

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
            actionButton.onClick.AddListener(OnItemClicked);
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