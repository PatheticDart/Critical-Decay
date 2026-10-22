using System.Collections.Generic;
using UnityEngine;

public class ModuleHealth : MonoBehaviour
{
    public static List<ModuleHealth> ActivePlayerModules = new List<ModuleHealth>();

    [Header("Module Identification")]
    [SerializeField] private string moduleName = "Module";
    [SerializeField] private bool isCHUModule = false;
    [SerializeField] private bool isRefinery = false;
    [SerializeField] private bool isObservatory = false;
    [SerializeField] private bool isTurret = false;

    [Header("Energy Drain Stat")]
    [SerializeField] private int energyDrain = 10;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float hitRadius = 0.5f;

    [Header("Upgrade System")]
    [SerializeField] private int currentLevel = 1;

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private int currentHealth;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip deathSFX;
    [Range(0f, 1f)][SerializeField] private float deathSFXVolume = 1f;
    private bool isDestroyed = false;
    private bool isDeactivatedByEnergy = false;

    public string ModuleName => moduleName;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsCHUModule => isCHUModule;
    public bool IsRefinery => isRefinery;
    public bool IsObservatory => isObservatory;
    public bool IsTurret => isTurret;
    public int EnergyDrain => energyDrain;
    public int CurrentLevel => currentLevel;
    public bool IsDeactivated => isDeactivatedByEnergy;
    public Vector2Int GridPosition { get; set; }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        if (!ActivePlayerModules.Contains(this))
        {
            ActivePlayerModules.Add(this);
        }

        if (ResourceHandler.Instance != null && !isCHUModule)
        {
            ResourceHandler.Instance.ModifyEnergyDrain(energyDrain);
        }
    }

    private void OnDisable()
    {
        ActivePlayerModules.Remove(this);

        if (ResourceHandler.Instance != null && !isCHUModule)
        {
            ResourceHandler.Instance.ModifyEnergyDrain(-energyDrain);
        }
    }

    private void Update()
    {
        if (!isDeactivatedByEnergy)
        {
            CheckEnemyProjectilesMath();
        }
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

                proj.SpawnHitVFX();
                proj.Despawn();

                if (isDestroyed) break;
            }
        }
    }

    public void SetEnergyDeactivated(bool deactivated)
    {
        isDeactivatedByEnergy = deactivated;

        TurretLogic[] turrets = GetComponentsInChildren<TurretLogic>(true);
        foreach (var turret in turrets)
        {
            turret.enabled = !deactivated;
        }

        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (var lr in lineRenderers)
        {
            lr.enabled = !deactivated;
        }

        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        Color targetColor = deactivated ? new Color(0.4f, 0.4f, 0.4f, 1f) : Color.white;
        foreach (var sr in spriteRenderers)
        {
            sr.color = targetColor;
        }
    }

    public void RepairModule()
    {
        currentHealth = maxHealth;
    }

    public void UpgradeModule(int baseCost, CurrencyType costType)
    {
        currentLevel++;

        maxHealth = Mathf.RoundToInt(maxHealth * 1.25f);
        currentHealth = maxHealth;

        int newEnergyDrain = energyDrain + 5;
        if (ResourceHandler.Instance != null)
        {
            ResourceHandler.Instance.ModifyEnergyDrain(newEnergyDrain - energyDrain);
        }
        energyDrain = newEnergyDrain;

        TurretLogic childTurret = GetComponentInChildren<TurretLogic>(true);
        if (childTurret != null)
        {
            childTurret.UpgradeTurretStats(currentLevel);
        }
    }

    public int GetUpgradeCost(int baseCost)
    {
        float calculatedCost = baseCost * Mathf.Pow(1.20f, currentLevel - 1);
        return Mathf.CeilToInt(calculatedCost);
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

    public void Die()
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

        ActivePlayerModules.Remove(this);

        if (isCHUModule)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }

            for (int i = ActivePlayerModules.Count - 1; i >= 0; i--)
            {
                if (ActivePlayerModules[i] != null)
                {
                    ActivePlayerModules[i].DieDirectly();
                }
            }
        }
        else
        {
            if (BuildSystem.Instance != null)
            {
                BuildSystem.Instance.OnModuleDestroyed(GridPosition, this);
            }
        }

        Destroy(gameObject);
    }

    public void DieDirectly()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        ActivePlayerModules.Remove(this);
        Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        if (isCHUModule) return;

        ModuleActionPanel actionPanel = FindObjectOfType<ModuleActionPanel>(true);
        if (actionPanel != null)
        {
            actionPanel.OpenPanel(this);
        }
    }
}