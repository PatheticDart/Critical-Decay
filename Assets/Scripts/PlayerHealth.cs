using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float hitRadius = 0.5f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private int currentHealth;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip deathSFX;
    [Range(0f, 1f)][SerializeField] private float deathSFXVolume = 1f;
    private bool isDestroyed = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        NotifyHealthUI();
    }

    private void Update()
    {
        CheckEnemyProjectilesMath();
    }

    private void CheckEnemyProjectilesMath()
    {
        if (isDestroyed) return;

        for (int i = ProjectileLogic.ActiveEnemyProjectiles.Count - 1; i >= 0; i--)
        {
            ProjectileLogic proj = ProjectileLogic.ActiveEnemyProjectiles[i];
            if (proj == null || !proj.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(proj.transform.position, transform.position);

            if (distance <= hitRadius)
            {
                TakeDamage(proj.Damage);

                Destroy(proj.gameObject);

                if (isDestroyed) break;
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDestroyed) return;

        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            NotifyHealthUI();
            Die();
        }
        else
        {
            NotifyHealthUI();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        NotifyHealthUI();
    }

    private void NotifyHealthUI()
    {
        if (PlayerHealthUI.Instance != null)
        {
            PlayerHealthUI.Instance.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (deathSFX != null)
        {
            AudioSource.PlayClipAtPoint(deathSFX, transform.position, deathSFXVolume);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}