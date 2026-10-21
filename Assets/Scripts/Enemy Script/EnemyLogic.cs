using System.Collections.Generic;
using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    public ScoreHandler scoreHandler;

    public static List<EnemyLogic> ActiveEnemies { get; private set; } = new List<EnemyLogic>();

    [Header("Enemy Classification")]
    [SerializeField] private EnemyType enemyType = EnemyType.Drone;
    [SerializeField] private float proximityRadius = 0.5f;
    [SerializeField] private int maxHealth = 100;

    [Header("Pickup Prefabs")]
    [SerializeField] private GameObject scrapPickupPrefab;
    [SerializeField] private GameObject researchSamplePickupPrefab;

    [Header("Drop Rates")]
    [Range(0f, 1f)][SerializeField] private float scrapDropChance = 0.6f;
    [Range(0f, 1f)][SerializeField] private float researchSampleDropChance = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameObject rm = GameObject.Find("Resource Manager");
        if (rm != null)
        {
            scoreHandler = rm.GetComponent<ScoreHandler>();
        }
    }

    private void Update()
    {
        CheckPlayerProjectilesMath();
    }

    private void CheckPlayerProjectilesMath()
    {
        // Iterate backwards through active player projectiles
        for (int i = ProjectileLogic.ActivePlayerProjectiles.Count - 1; i >= 0; i--)
        {
            ProjectileLogic projectile = ProjectileLogic.ActivePlayerProjectiles[i];
            if (projectile == null || !projectile.gameObject.activeInHierarchy) continue;

            // Pure vector distance check
            float distance = Vector2.Distance(projectile.transform.position, transform.position);

            if (distance <= proximityRadius)
            {
                // Take damage from projectile
                TakeDamage(projectile.Damage);

                // Destroy/despawn projectile
                projectile.Despawn();

                if (currentHealth <= 0) break;
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        // Instantiate explosion visual effects
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (scoreHandler != null)
        {
            scoreHandler.AddScore(100);
        }

        SpawnLootDrops();

        gameObject.SetActive(false); // Compatible with Object Pooling
    }

    private void SpawnLootDrops()
    {
        // 1. Scrap Metal Drop Chance
        if (scrapPickupPrefab != null && Random.value <= scrapDropChance)
        {
            GameObject scrapObj = Instantiate(scrapPickupPrefab, transform.position, Quaternion.identity);
            PickupLogic pickup = scrapObj.GetComponent<PickupLogic>();
            if (pickup != null)
            {
                pickup.Initialize(PickupType.Scrap, 1);
            }
        }

        // 2. Research Sample Drop Chance (GDD drop rates: Drone 1-2, Ranger 2-3, Basilisk 5-10)
        if (researchSamplePickupPrefab != null && Random.value <= researchSampleDropChance)
        {
            int sampleCount = 1;
            switch (enemyType)
            {
                case EnemyType.Drone:
                    sampleCount = Random.Range(1, 3); // 1-2
                    break;
                case EnemyType.Ranger:
                    sampleCount = Random.Range(2, 4); // 2-3
                    break;
                case EnemyType.Basilisk:
                    sampleCount = Random.Range(5, 11); // 5-10
                    break;
            }

            GameObject sampleObj = Instantiate(researchSamplePickupPrefab, transform.position, Quaternion.identity);
            PickupLogic pickup = sampleObj.GetComponent<PickupLogic>();
            if (pickup != null)
            {
                pickup.Initialize(PickupType.ResearchSample, sampleCount);
            }
        }
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        if (!ActiveEnemies.Contains(this))
            ActiveEnemies.Add(this);
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);
    }
}