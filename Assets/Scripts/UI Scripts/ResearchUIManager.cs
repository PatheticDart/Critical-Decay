using UnityEngine;

public class ResearchUIManager : MonoBehaviour
{
    public static ResearchUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject researchOpenButton;
    [SerializeField] private GameObject researchPanelScrollView;
    [SerializeField] private Transform researchContentContainer;
    [SerializeField] private GameObject researchItemPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (researchPanelScrollView != null) researchPanelScrollView.SetActive(false);
    }

    private void Update()
    {
        bool hasObservatory = ModuleHealth.ActivePlayerModules.Exists(m => m != null && m.IsObservatory);
        if (researchOpenButton != null)
        {
            researchOpenButton.SetActive(hasObservatory);
        }

        if (!hasObservatory && researchPanelScrollView != null && researchPanelScrollView.activeSelf)
        {
            CloseResearchPanel();
        }
    }

    public void ToggleResearchPanel()
    {
        if (researchPanelScrollView == null) return;

        bool newState = !researchPanelScrollView.activeSelf;
        researchPanelScrollView.SetActive(newState);

        if (newState)
        {
            RefreshResearchPanel();
        }
    }

    public void CloseResearchPanel()
    {
        if (researchPanelScrollView != null && researchPanelScrollView.activeSelf)
        {
            researchPanelScrollView.SetActive(false);
        }
    }

    public void RefreshResearchPanel()
    {
        if (researchContentContainer == null || researchItemPrefab == null) return;

        foreach (Transform child in researchContentContainer)
        {
            Destroy(child.gameObject);
        }

        ResearchHandler r = ResearchHandler.Instance;
        if (r == null) return;

        CreateResearchItem("EnergyProduction", "Energy Production", "5 Samples", "Unlocks Solar Panel module", r.unlockedEnergyProduction);
        CreateResearchItem("MoltenMetal", "Molten Metal", "10 Samples", "Unlocks Shrapnel Turret", r.unlockedMoltenMetal);
        CreateResearchItem("BoltAcquisition", "Bolt Acquisition", "20 Samples", "Unlocks Bolt Turret", r.unlockedBoltAcquisition);
        CreateResearchItem("WorkshopKnowledge", "Workshop Knowledge", "50 Samples", "Unlocks Turret Upgrading", r.unlockedWorkshopKnowledge);
        CreateResearchItem("CombatOutfit", "Combat Outfit", "100 Metal Bars", "Upgrades Player Ship with Mouse-Aimed Turret", r.unlockedCombatOutfit);
        CreateResearchItem("ImprovedRefining", "Improved Refining", "40 Samples", "Reduces Refinery conversion cost to 2 Scrap per Metal Bar", r.unlockedImprovedRefining);
        CreateResearchItem("EfficientRefining", "Efficient Refining", "80 Samples", "Reduces Refinery conversion cost to 1 Scrap per Metal Bar (Requires Improved Refining)", r.unlockedEfficientRefining);
    }

    private void CreateResearchItem(string key, string nameStr, string costStr, string detailsStr, bool isUnlocked)
    {
        GameObject itemObj = Instantiate(researchItemPrefab, researchContentContainer);
        ResearchItemButton script = itemObj.GetComponent<ResearchItemButton>();
        if (script != null)
        {
            script.Setup(key, nameStr, costStr, detailsStr, isUnlocked);
        }
    }
}