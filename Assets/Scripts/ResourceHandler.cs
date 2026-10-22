using TMPro;
using UnityEngine;

public class ResourceHandler : MonoBehaviour
{
    public static ResourceHandler Instance { get; private set; }

    [Header("Resources (Data Values)")]
    [SerializeField] private int scrap = 0;
    [SerializeField] private int metalBars = 0;
    [SerializeField] private int researchSamples = 0;

    [Header("Refinery Processing Settings")]
    [SerializeField] private float conversionRateSeconds = 1.0f;
    [SerializeField] private int scrapPerConversion = 3;

    [Header("Energy System")]
    [SerializeField] private int maxEnergyThreshold = 20;
    [SerializeField] private int currentEnergyDrain = 0;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private TextMeshProUGUI metalBarsText;
    [SerializeField] private TextMeshProUGUI researchSamplesText;
    [SerializeField] private TextMeshProUGUI energyThresholdText;

    [Header("UI Scale Punch Animation Containers")]
    [SerializeField] private RectTransform scrapUIContainer;
    [SerializeField] private RectTransform metalBarsUIContainer;
    [SerializeField] private RectTransform researchUIContainer;

    [Header("Animation Settings")]
    [SerializeField] private float punchDuration = 0.4f;
    [SerializeField] private Vector3 punchScaleTarget = new Vector3(1.3f, 1.3f, 1.3f);
    [SerializeField] private float counterDuration = 0.5f;

    private int displayedScrap;
    private int startScrap;

    private int displayedMetalBars;
    private int startMetalBars;

    private int displayedResearch;
    private int startResearch;

    private float scrapCounterTime, metalCounterTime, researchCounterTime;
    private bool isScrapCounting, isMetalCounting, isResearchCounting;

    private float scrapPunchTime, metalPunchTime, researchPunchTime;
    private bool isScrapPunching, isMetalPunching, isResearchPunching;

    private Vector3 origScrapScale = Vector3.one;
    private Vector3 origMetalScale = Vector3.one;
    private Vector3 origResearchScale = Vector3.one;

    private float nextConversionTime;

    public int Scrap => scrap;
    public int MetalBars => metalBars;
    public int ResearchSamples => researchSamples;
    public int MaxEnergyThreshold => maxEnergyThreshold;
    public int CurrentEnergyDrain => currentEnergyDrain;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (scrapUIContainer != null) origScrapScale = scrapUIContainer.localScale;
        if (metalBarsUIContainer != null) origMetalScale = metalBarsUIContainer.localScale;
        if (researchUIContainer != null) origResearchScale = researchUIContainer.localScale;
    }

    private void Start()
    {
        displayedScrap = scrap;
        displayedMetalBars = metalBars;
        displayedResearch = researchSamples;
        UpdateAllTextDirect();
    }

    private void Update()
    {
        ProcessRefineryLogic();
        UpdateAnimationsAndText();
    }

    private void ProcessRefineryLogic()
    {
        bool hasRefinery = ModuleHealth.ActivePlayerModules.Exists(m => m != null && m.IsRefinery);

        if (hasRefinery && scrap >= scrapPerConversion)
        {
            if (Time.time >= nextConversionTime)
            {
                nextConversionTime = Time.time + conversionRateSeconds;

                scrap -= scrapPerConversion;
                TriggerScrapAnimation();

                metalBars += 1;
                TriggerMetalBarsAnimation();
            }
        }
    }

    private void UpdateAnimationsAndText()
    {
        if (isScrapCounting)
        {
            scrapCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(scrapCounterTime / counterDuration);
            displayedScrap = Mathf.RoundToInt(Mathf.Lerp(startScrap, scrap, EaseOutQuad(t)));
            if (t >= 1f)
            {
                displayedScrap = scrap;
                isScrapCounting = false;
            }
        }

        if (isScrapPunching)
        {
            scrapPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(scrapPunchTime / punchDuration);
            if (scrapUIContainer != null) scrapUIContainer.localScale = Vector3.Lerp(origScrapScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f)
            {
                isScrapPunching = false;
                if (scrapUIContainer != null) scrapUIContainer.localScale = origScrapScale;
            }
        }

        if (isMetalCounting)
        {
            metalCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(metalCounterTime / counterDuration);
            displayedMetalBars = Mathf.RoundToInt(Mathf.Lerp(startMetalBars, metalBars, EaseOutQuad(t)));
            if (t >= 1f)
            {
                displayedMetalBars = metalBars;
                isMetalCounting = false;
            }
        }

        if (isMetalPunching)
        {
            metalPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(metalPunchTime / punchDuration);
            if (metalBarsUIContainer != null) metalBarsUIContainer.localScale = Vector3.Lerp(origMetalScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f)
            {
                isMetalPunching = false;
                if (metalBarsUIContainer != null) metalBarsUIContainer.localScale = origMetalScale;
            }
        }

        if (isResearchCounting)
        {
            researchCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(researchCounterTime / counterDuration);
            displayedResearch = Mathf.RoundToInt(Mathf.Lerp(startResearch, researchSamples, EaseOutQuad(t)));
            if (t >= 1f)
            {
                displayedResearch = researchSamples;
                isResearchCounting = false;
            }
        }

        if (isResearchPunching)
        {
            researchPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(researchPunchTime / punchDuration);
            if (researchUIContainer != null) researchUIContainer.localScale = Vector3.Lerp(origResearchScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f)
            {
                isResearchPunching = false;
                if (researchUIContainer != null) researchUIContainer.localScale = origResearchScale;
            }
        }

        if (scrapText != null) scrapText.text = displayedScrap.ToString();
        if (metalBarsText != null) metalBarsText.text = displayedMetalBars.ToString();
        if (researchSamplesText != null) researchSamplesText.text = displayedResearch.ToString();

        if (energyThresholdText != null)
        {
            energyThresholdText.text = $"{currentEnergyDrain} / {maxEnergyThreshold}";
            energyThresholdText.color = currentEnergyDrain > maxEnergyThreshold ? Color.red : Color.white;
        }
    }

    private void UpdateAllTextDirect()
    {
        if (scrapText != null) scrapText.text = scrap.ToString();
        if (metalBarsText != null) metalBarsText.text = metalBars.ToString();
        if (researchSamplesText != null) researchSamplesText.text = researchSamples.ToString();
        if (energyThresholdText != null) energyThresholdText.text = $"{currentEnergyDrain} / {maxEnergyThreshold}";
    }

    public void AddScrap(int amount)
    {
        scrap += amount;
        TriggerScrapAnimation();
    }

    public void AddMetalBars(int amount)
    {
        metalBars += amount;
        TriggerMetalBarsAnimation();
    }

    public void AddResearchSamples(int amount)
    {
        researchSamples += amount;
        TriggerResearchAnimation();
    }

    public bool ConsumeScrap(int amount)
    {
        if (scrap >= amount)
        {
            scrap -= amount;
            TriggerScrapAnimation();
            return true;
        }
        return false;
    }

    public bool ConsumeMetalBars(int amount)
    {
        if (metalBars >= amount)
        {
            metalBars -= amount;
            TriggerMetalBarsAnimation();
            return true;
        }
        return false;
    }

    public bool ConsumeResearchSamples(int amount)
    {
        if (researchSamples >= amount)
        {
            researchSamples -= amount;
            TriggerResearchAnimation();
            return true;
        }
        return false;
    }

    private void TriggerScrapAnimation()
    {
        startScrap = displayedScrap;
        scrapCounterTime = 0f;
        isScrapCounting = true;

        scrapPunchTime = 0f;
        isScrapPunching = true;
    }

    private void TriggerMetalBarsAnimation()
    {
        startMetalBars = displayedMetalBars;
        metalCounterTime = 0f;
        isMetalCounting = true;

        metalPunchTime = 0f;
        isMetalPunching = true;
    }

    private void TriggerResearchAnimation()
    {
        startResearch = displayedResearch;
        researchCounterTime = 0f;
        isResearchCounting = true;

        researchPunchTime = 0f;
        isResearchPunching = true;
    }

    public void ModifyMaxEnergy(int delta)
    {
        maxEnergyThreshold += delta;
        if (BuildSystem.Instance != null) BuildSystem.Instance.CheckEnergyBalance();
    }

    public void ModifyEnergyDrain(int delta)
    {
        currentEnergyDrain += delta;
        if (BuildSystem.Instance != null) BuildSystem.Instance.CheckEnergyBalance();
    }

    private float EaseOutElastic(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;
        return t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}