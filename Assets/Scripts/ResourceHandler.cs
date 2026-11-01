using TMPro;
using UnityEngine;

public class ResourceHandler : MonoBehaviour
{
    public static ResourceHandler Instance { get; private set; }

    [Header("Player Ship Resources")]
    [SerializeField] private int localScrap = 0;
    [SerializeField] private int localResearchSamples = 0;

    [Header("Resources (Data Values)")]
    [SerializeField] private int scrap = 0;
    [SerializeField] private int metalBars = 0;
    [SerializeField] private int researchSamples = 0;

    [Header("Refinery Processing Settings")]
    [SerializeField] private float baseConversionRateSeconds = 1.0f;
    [SerializeField] private int scrapPerConversion = 3;
    [SerializeField] private bool isRefineryActive = true;
    [SerializeField] private GameObject refineryConversionVFX;

    [Header("Energy System")]
    [SerializeField] private int maxEnergyThreshold = 20;
    [SerializeField] private int currentEnergyDrain = 0;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI localScrapText;
    [SerializeField] private TextMeshProUGUI localResearchText;
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private TextMeshProUGUI metalBarsText;
    [SerializeField] private TextMeshProUGUI researchSamplesText;
    [SerializeField] private TextMeshProUGUI energyThresholdText;

    [Header("UI Scale Punch Animation Containers")]
    [SerializeField] private RectTransform localScrapUIContainer;
    [SerializeField] private RectTransform localResearchUIContainer;
    [SerializeField] private RectTransform scrapUIContainer;
    [SerializeField] private RectTransform metalBarsUIContainer;
    [SerializeField] private RectTransform researchUIContainer;

    [Header("Animation Settings")]
    [SerializeField] private float punchDuration = 0.4f;
    [SerializeField] private Vector3 punchScaleTarget = new Vector3(1.3f, 1.3f, 1.3f);
    [SerializeField] private float counterDuration = 0.5f;

    private int displayedLocalScrap, startLocalScrap;
    private int displayedLocalResearch, startLocalResearch;
    private int displayedScrap, startScrap;
    private int displayedMetalBars, startMetalBars;
    private int displayedResearch, startResearch;

    private float localScrapCounterTime, localResearchCounterTime;
    private float scrapCounterTime, metalCounterTime, researchCounterTime;

    private bool isLocalScrapCounting, isLocalResearchCounting;
    private bool isScrapCounting, isMetalCounting, isResearchCounting;

    private float localScrapPunchTime, localResearchPunchTime;
    private float scrapPunchTime, metalPunchTime, researchPunchTime;

    private bool isLocalScrapPunching, isLocalResearchPunching;
    private bool isScrapPunching, isMetalPunching, isResearchPunching;

    private Vector3 origLocalScrapScale = Vector3.one;
    private Vector3 origLocalResearchScale = Vector3.one;
    private Vector3 origScrapScale = Vector3.one;
    private Vector3 origMetalScale = Vector3.one;
    private Vector3 origResearchScale = Vector3.one;

    private float nextConversionTime;
    private float currentConversionRateSeconds;

    public int LocalScrap => localScrap;
    public int LocalResearchSamples => localResearchSamples;
    public int Scrap => scrap;
    public int MetalBars => metalBars;
    public int ResearchSamples => researchSamples;
    public int MaxEnergyThreshold => maxEnergyThreshold;
    public int CurrentEnergyDrain => currentEnergyDrain;
    public bool IsRefineryActive => isRefineryActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentConversionRateSeconds = baseConversionRateSeconds;

        if (localScrapUIContainer != null) origLocalScrapScale = localScrapUIContainer.localScale;
        if (localResearchUIContainer != null) origLocalResearchScale = localResearchUIContainer.localScale;
        if (scrapUIContainer != null) origScrapScale = scrapUIContainer.localScale;
        if (metalBarsUIContainer != null) origMetalScale = metalBarsUIContainer.localScale;
        if (researchUIContainer != null) origResearchScale = researchUIContainer.localScale;
    }

    private void Start()
    {
        displayedLocalScrap = localScrap;
        displayedLocalResearch = localResearchSamples;
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

    public void DepositLocalResources()
    {
        if (localScrap <= 0 && localResearchSamples <= 0) return;

        if (localScrap > 0)
        {
            AddScrap(localScrap);
            localScrap = 0;
            TriggerLocalScrapAnimation();
        }

        if (localResearchSamples > 0)
        {
            AddResearchSamples(localResearchSamples);
            localResearchSamples = 0;
            TriggerLocalResearchAnimation();
        }
    }

    public void SetRefineryActive(bool active)
    {
        isRefineryActive = active;
    }

    public void UpdateRefinerySpeed(int refineryLevel)
    {
        currentConversionRateSeconds = baseConversionRateSeconds * Mathf.Pow(0.80f, refineryLevel - 1);
    }

    private void ProcessRefineryLogic()
    {
        if (!isRefineryActive) return;

        ModuleHealth refineryModule = ModuleHealth.ActivePlayerModules.Find(m => m != null && m.IsRefinery && !m.IsDeactivated);

        if (refineryModule != null && scrap >= scrapPerConversion)
        {
            if (Time.time >= nextConversionTime)
            {
                nextConversionTime = Time.time + currentConversionRateSeconds;

                scrap -= scrapPerConversion;
                TriggerScrapAnimation();

                metalBars += 1;
                TriggerMetalBarsAnimation();

                if (refineryConversionVFX != null)
                {
                    Instantiate(refineryConversionVFX, refineryModule.transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void UpdateAnimationsAndText()
    {
        if (isLocalScrapCounting)
        {
            localScrapCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(localScrapCounterTime / counterDuration);
            displayedLocalScrap = Mathf.RoundToInt(Mathf.Lerp(startLocalScrap, localScrap, EaseOutQuad(t)));
            if (t >= 1f) { displayedLocalScrap = localScrap; isLocalScrapCounting = false; }
        }
        if (isLocalScrapPunching)
        {
            localScrapPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(localScrapPunchTime / punchDuration);
            if (localScrapUIContainer != null) localScrapUIContainer.localScale = Vector3.Lerp(origLocalScrapScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f) { isLocalScrapPunching = false; if (localScrapUIContainer != null) localScrapUIContainer.localScale = origLocalScrapScale; }
        }

        if (isLocalResearchCounting)
        {
            localResearchCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(localResearchCounterTime / counterDuration);
            displayedLocalResearch = Mathf.RoundToInt(Mathf.Lerp(startLocalResearch, localResearchSamples, EaseOutQuad(t)));
            if (t >= 1f) { displayedLocalResearch = localResearchSamples; isLocalResearchCounting = false; }
        }
        if (isLocalResearchPunching)
        {
            localResearchPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(localResearchPunchTime / punchDuration);
            if (localResearchUIContainer != null) localResearchUIContainer.localScale = Vector3.Lerp(origLocalResearchScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f) { isLocalResearchPunching = false; if (localResearchUIContainer != null) localResearchUIContainer.localScale = origLocalResearchScale; }
        }

        if (isScrapCounting)
        {
            scrapCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(scrapCounterTime / counterDuration);
            displayedScrap = Mathf.RoundToInt(Mathf.Lerp(startScrap, scrap, EaseOutQuad(t)));
            if (t >= 1f) { displayedScrap = scrap; isScrapCounting = false; }
        }
        if (isScrapPunching)
        {
            scrapPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(scrapPunchTime / punchDuration);
            if (scrapUIContainer != null) scrapUIContainer.localScale = Vector3.Lerp(origScrapScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f) { isScrapPunching = false; if (scrapUIContainer != null) scrapUIContainer.localScale = origScrapScale; }
        }

        if (isMetalCounting)
        {
            metalCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(metalCounterTime / counterDuration);
            displayedMetalBars = Mathf.RoundToInt(Mathf.Lerp(startMetalBars, metalBars, EaseOutQuad(t)));
            if (t >= 1f) { displayedMetalBars = metalBars; isMetalCounting = false; }
        }
        if (isMetalPunching)
        {
            metalPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(metalPunchTime / punchDuration);
            if (metalBarsUIContainer != null) metalBarsUIContainer.localScale = Vector3.Lerp(origMetalScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f) { isMetalPunching = false; if (metalBarsUIContainer != null) metalBarsUIContainer.localScale = origMetalScale; }
        }

        if (isResearchCounting)
        {
            researchCounterTime += Time.deltaTime;
            float t = Mathf.Clamp01(researchCounterTime / counterDuration);
            displayedResearch = Mathf.RoundToInt(Mathf.Lerp(startResearch, researchSamples, EaseOutQuad(t)));
            if (t >= 1f) { displayedResearch = researchSamples; isResearchCounting = false; }
        }
        if (isResearchPunching)
        {
            researchPunchTime += Time.deltaTime;
            float t = Mathf.Clamp01(researchPunchTime / punchDuration);
            if (researchUIContainer != null) researchUIContainer.localScale = Vector3.Lerp(origResearchScale, punchScaleTarget, EaseOutElastic(t));
            if (t >= 1f) { isResearchPunching = false; if (researchUIContainer != null) researchUIContainer.localScale = origResearchScale; }
        }

        if (localScrapText != null) localScrapText.text = displayedLocalScrap.ToString();
        if (localResearchText != null) localResearchText.text = displayedLocalResearch.ToString();
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
        if (localScrapText != null) localScrapText.text = localScrap.ToString();
        if (localResearchText != null) localResearchText.text = localResearchSamples.ToString();
        if (scrapText != null) scrapText.text = scrap.ToString();
        if (metalBarsText != null) metalBarsText.text = metalBars.ToString();
        if (researchSamplesText != null) researchSamplesText.text = researchSamples.ToString();
        if (energyThresholdText != null) energyThresholdText.text = $"{currentEnergyDrain} / {maxEnergyThreshold}";
    }

    public void AddLocalScrap(int amount)
    {
        localScrap += amount;
        TriggerLocalScrapAnimation();
    }

    public void AddLocalResearchSamples(int amount)
    {
        localResearchSamples += amount;
        TriggerLocalResearchAnimation();
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

    private void TriggerLocalScrapAnimation()
    {
        startLocalScrap = displayedLocalScrap;
        localScrapCounterTime = 0f;
        isLocalScrapCounting = true;

        localScrapPunchTime = 0f;
        isLocalScrapPunching = true;
    }

    private void TriggerLocalResearchAnimation()
    {
        startLocalResearch = displayedLocalResearch;
        localResearchCounterTime = 0f;
        isLocalResearchCounting = true;

        localResearchPunchTime = 0f;
        isLocalResearchPunching = true;
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

    public void SetScrapPerConversion(int newScrapAmount)
    {
        scrapPerConversion = newScrapAmount;
    }
}