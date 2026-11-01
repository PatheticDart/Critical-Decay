using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialObjectiveType
{
    None,
    PressWToMove,
    PressSToMoveBack,
    PressADToTurn,
    PressSpaceToBrake,
    CollectScrap,
    DockWithStation,
    BuildRefinery,
    BuildScrapTurret,
    BuildObservatory,
    UnlockTech
}

[System.Serializable]
public struct TutorialStep
{
    public TutorialObjectiveType objectiveType;
    [TextArea] public string objectiveText;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveTextUI;
    [SerializeField] private GameObject objectiveContainer;
    [SerializeField] private GameObject tutorialUIPanel;
    [SerializeField] private GameObject shipUI;

    [Header("Welcome Panel Setup")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private Button continueButton;

    [Header("Tutorial Sequence")]
    [SerializeField] private TutorialStep[] tutorialSteps;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 2.0f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    private int currentStepIndex = 0;
    private bool isTutorialComplete = false;
    private bool isTransitioning = false;
    private bool hasTutorialActive = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        hasTutorialActive = PlayerPrefs.GetInt("HasTutorial", 1) == 1;
    }

    private void Start()
    {
        if (!hasTutorialActive)
        {
            if (tutorialUIPanel != null) tutorialUIPanel.SetActive(false);
            if (welcomePanel != null) welcomePanel.SetActive(false);
            isTutorialComplete = true;
            return;
        }

        if (objectiveTextUI != null) objectiveTextUI.color = defaultColor;

        if (shipUI != null) shipUI.SetActive(false);
        if (tutorialUIPanel != null) tutorialUIPanel.SetActive(true);

        InitializeWelcomePanel();
        UpdateObjectiveUI();
    }

    private void InitializeWelcomePanel()
    {
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
            Time.timeScale = 0f;

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnWelcomeContinue);
            }
        }
    }

    private void OnWelcomeContinue()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick(1);

        if (welcomePanel != null) welcomePanel.SetActive(false);

        if (shipUI != null) shipUI.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (isTutorialComplete || tutorialSteps.Length == 0 || isTransitioning || !hasTutorialActive) return;

        TutorialObjectiveType currentObjective = tutorialSteps[currentStepIndex].objectiveType;

        switch (currentObjective)
        {
            case TutorialObjectiveType.PressWToMove:
                if (Input.GetKeyDown(KeyCode.W)) CompleteCurrentObjective();
                break;
            case TutorialObjectiveType.PressSToMoveBack:
                if (Input.GetKeyDown(KeyCode.S)) CompleteCurrentObjective();
                break;
            case TutorialObjectiveType.PressADToTurn:
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) CompleteCurrentObjective();
                break;
            case TutorialObjectiveType.PressSpaceToBrake:
                if (Input.GetKey(KeyCode.Space)) CompleteCurrentObjective();
                break;
            case TutorialObjectiveType.CollectScrap:
                if (ResourceHandler.Instance != null && ResourceHandler.Instance.LocalScrap >= 50)
                {
                    CompleteCurrentObjective();
                }
                break;
        }
    }

    public void ReportAction(TutorialObjectiveType actionPerformed)
    {
        if (isTutorialComplete || tutorialSteps.Length == 0 || isTransitioning || !hasTutorialActive) return;

        if (tutorialSteps[currentStepIndex].objectiveType == actionPerformed)
        {
            CompleteCurrentObjective();
        }
    }

    private void CompleteCurrentObjective()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick(2);
        }

        StartCoroutine(TransitionToNextObjective());
    }

    private IEnumerator TransitionToNextObjective()
    {
        isTransitioning = true;
        TutorialObjectiveType completedObjective = tutorialSteps[currentStepIndex].objectiveType;

        if (objectiveTextUI != null) objectiveTextUI.color = successColor;

        yield return new WaitForSeconds(transitionDuration);

        if (objectiveTextUI != null) objectiveTextUI.color = defaultColor;

        currentStepIndex++;

        if (completedObjective == TutorialObjectiveType.PressSpaceToBrake)
        {
            if (shipUI != null) shipUI.SetActive(true);
        }

        UpdateObjectiveUI();
        isTransitioning = false;
    }

    private void UpdateObjectiveUI()
    {
        if (currentStepIndex >= tutorialSteps.Length)
        {
            isTutorialComplete = true;
            if (objectiveTextUI != null) objectiveTextUI.text = "Explore the game";
            StartCoroutine(TurnOffTutorialUIPanelAfterDelay());
            return;
        }

        if (objectiveTextUI != null)
        {
            objectiveTextUI.text = tutorialSteps[currentStepIndex].objectiveText;
        }
    }

    private IEnumerator TurnOffTutorialUIPanelAfterDelay()
    {
        yield return new WaitForSeconds(transitionDuration);
        if (tutorialUIPanel != null) tutorialUIPanel.SetActive(false);
    }
}