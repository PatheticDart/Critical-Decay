using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchItemButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private Button unlockButton;

    [SerializeField] private AudioClip buttonClickSFX;

    private string techKey;

    public void Setup(string key, string nameStr, string costStr, string detailsStr, bool isUnlocked)
    {
        techKey = key;

        if (nameText != null) nameText.text = nameStr;
        if (costText != null) costText.text = isUnlocked ? "UNLOCKED" : costStr;
        if (detailsText != null) detailsText.text = detailsStr;

        if (unlockButton != null)
        {
            unlockButton.interactable = !isUnlocked;
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }
    }

    private void OnUnlockClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick(1);
        }

        if (ResearchHandler.Instance != null)
        {
            ResearchHandler.Instance.UnlockTechnology(techKey);
        }
    }
}