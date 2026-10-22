using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public static PlayerHealthUI Instance { get; private set; }

    [Header("UI Image Fill References")]
    [SerializeField] private Image realHealthBarFill;
    [SerializeField] private Image ghostHealthBarFill;

    [Header("UI Text Reference")]
    [SerializeField] private TextMeshProUGUI chuHealthPercentText;

    [Header("Settings")]
    [SerializeField] private float ghostDelay = 0.5f;
    [SerializeField] private float ghostDuration = 1.0f;

    private float currentGhostFill = 1f;
    private float startGhostFill = 1f;
    private float targetFill = 1f;

    private float delayTimer;
    private float easeElapsedTime;
    private bool isGhostEasing;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        targetFill = Mathf.Clamp01((float)currentHealth / maxHealth);

        if (realHealthBarFill != null)
        {
            realHealthBarFill.fillAmount = targetFill;
        }

        if (chuHealthPercentText != null)
        {
            int percent = Mathf.RoundToInt(targetFill * 100f);
            chuHealthPercentText.text = $"{percent}%";
        }

        if (ghostHealthBarFill != null)
        {
            startGhostFill = ghostHealthBarFill.fillAmount;
        }
        else
        {
            startGhostFill = currentGhostFill;
        }

        delayTimer = ghostDelay;
        easeElapsedTime = 0f;
        isGhostEasing = false;
    }

    private void Update()
    {
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                isGhostEasing = true;
            }
            return;
        }

        if (isGhostEasing)
        {
            easeElapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(easeElapsedTime / ghostDuration);

            float easedT = EaseOutQuad(t);

            currentGhostFill = Mathf.Lerp(startGhostFill, targetFill, easedT);

            if (ghostHealthBarFill != null)
            {
                ghostHealthBarFill.fillAmount = currentGhostFill;
            }

            if (t >= 1f)
            {
                isGhostEasing = false;
                currentGhostFill = targetFill;
            }
        }
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}