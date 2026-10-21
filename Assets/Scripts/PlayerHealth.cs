using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float hitRadius = 0.5f; // Pure math hit registration radius

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private int currentHealth;
    private bool isDestroyed = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        CheckEnemyProjectilesMath();
    }

    private void CheckEnemyProjectilesMath()
    {
        if (isDestroyed) return;

        // Pure distance math loop against static list of active enemy projectiles
        for (int i = ProjectileLogic.ActiveEnemyProjectiles.Count - 1; i >= 0; i--)
        {
            ProjectileLogic proj = ProjectileLogic.ActiveEnemyProjectiles[i];
            if (proj == null || !proj.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(proj.transform.position, transform.position);

            if (distance <= hitRadius)
            {
                TakeDamage(proj.Damage);

                // Destroy projectile on hit
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
            Die();
        }
    }

    private void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Trigger Game Over
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