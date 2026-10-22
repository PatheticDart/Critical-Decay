using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Canvas canvasGroup;

    [Header("Settings")]
    [SerializeField] private bool hideWhenFull = true;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInParent<Canvas>();
        }
    }

    private void OnEnable()
    {
        transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBarFill != null)
        {
            float fill = Mathf.Clamp01((float)currentHealth / maxHealth);
            healthBarFill.fillAmount = fill;
        }

        if (canvasGroup != null && hideWhenFull)
        {
            canvasGroup.enabled = (currentHealth < maxHealth) && (currentHealth > 0);
        }
    }
}