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

        if (isUnlocked)
        {
            if (statusText != null) statusText.text = "RESEARCHED";
            if (buyButton != null) buyButton.interactable = false;
        }
        else if (!isPrerequisiteMet)
        {
            if (statusText != null) statusText.text = "LOCKED (REQ: IMPROVED REFINING)";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            if (statusText != null) statusText.text = "RESEARCH";
            if (buyButton != null)
            {
                buyButton.interactable = true;
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