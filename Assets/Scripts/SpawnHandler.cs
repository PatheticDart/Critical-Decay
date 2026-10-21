using System.Collections;
using TMPro; // Use UnityEngine.UI if using standard Text components
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    [Header("Spawn Origin Settings")]
    [SerializeField] private Transform chuTransform; // Central anchor (CHU Module)
    [SerializeField] private float spawnRadius = 30f; // Distance from CHU where enemies spawn

    [Header("Timing Settings")]
    [SerializeField] private float initialIntermissionTime = 90f; // Wave 0 prep time
    [SerializeField] private float standardIntermissionTime = 50f; // Standardized intermission between waves
    [SerializeField] private float spawnIntervalWithinWave = 0.5f;

    [Header("Wave Progression & Cap Settings")]
    [SerializeField] private int baseWaveEnemies = 10; // Wave 1 enemy count
    [SerializeField] private int enemyIncrementPerWave = 5; // +5 per wave
    [SerializeField] private int maxActiveEnemies = 30; // Max enemies active simultaneously

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveText;   // Wave Header Title
    [SerializeField] private TextMeshProUGUI timerText;  // Intermission Timer & Enemies Remaining Text

    [Header("Pool Keys (ObjectPoolManager)")]
    [SerializeField] private string dronePoolKey = "Drone";
    [SerializeField] private string rangerPoolKey = "Ranger";
    [SerializeField] private string basiliskPoolKey = "Basilisk";

    private int currentWave = 0;
    private bool isSpawningWave = false;
    private bool isIntermission = true;
    private float currentIntermissionTimer = 0f;
    private int remainingWaveEnemies = 0;

    public int CurrentWave => currentWave;
    public bool IsIntermission => isIntermission;
    public float CurrentIntermissionTimer => currentIntermissionTimer;

    private void Start()
    {
        if (chuTransform == null)
        {
            ModuleHealth chu = ModuleHealth.ActivePlayerModules.Find(m => m != null && m.IsCHUModule);
            if (chu != null)
            {
                chuTransform = chu.transform;
            }
        }

        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        // 1. Initial Preparation Phase (Wave 0)
        isIntermission = true;
        SetWaveHeader("PREPARATION PHASE");
        yield return StartCoroutine(RunIntermissionCountdown(initialIntermissionTime));

        while (true)
        {
            // 2. Start Active Wave
            currentWave++;
            isIntermission = false;
            isSpawningWave = true;

            SetWaveHeader($"WAVE {currentWave}");

            int totalEnemiesForWave = baseWaveEnemies + ((currentWave - 1) * enemyIncrementPerWave);
            remainingWaveEnemies = totalEnemiesForWave;

            // Update UI with initial enemy count
            UpdateEnemiesLeftUI();

            // Spawn all wave enemies
            yield return StartCoroutine(SpawnWaveRoutine(totalEnemiesForWave));

            isSpawningWave = false;

            // 3. Wait until all enemies in the scene are destroyed/cleared
            while (remainingWaveEnemies > 0 || EnemyLogic.ActiveEnemies.Count > 0)
            {
                // Update enemy count display in real time based on active pooled enemies + unspawned queue
                remainingWaveEnemies = EnemyLogic.ActiveEnemies.Count;
                UpdateEnemiesLeftUI();
                yield return new WaitForSeconds(0.2f);
            }

            // Ensure 0 ENEMIES LEFT display before transition
            remainingWaveEnemies = 0;
            UpdateEnemiesLeftUI();
            yield return new WaitForSeconds(1.0f);

            // 4. Intermission Phase
            isIntermission = true;
            SetWaveHeader("INTERMISSION");
            yield return StartCoroutine(RunIntermissionCountdown(standardIntermissionTime));
        }
    }

    private IEnumerator RunIntermissionCountdown(float duration)
    {
        currentIntermissionTimer = duration;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        while (currentIntermissionTimer > 0f)
        {
            currentIntermissionTimer -= Time.deltaTime;

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentIntermissionTimer / 60f);
                int seconds = Mathf.FloorToInt(currentIntermissionTimer % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            yield return null;
        }

        currentIntermissionTimer = 0f;
    }

    private void SetWaveHeader(string waveMessage)
    {
        if (waveText != null)
        {
            waveText.gameObject.SetActive(true);
            waveText.text = waveMessage;
        }
    }

    private void UpdateEnemiesLeftUI()
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = $"{remainingWaveEnemies} ENEMIES LEFT";
        }
    }

    private IEnumerator SpawnWaveRoutine(int totalEnemies)
    {
        int spawnedCount = 0;

        while (spawnedCount < totalEnemies)
        {
            if (EnemyLogic.ActiveEnemies.Count >= maxActiveEnemies)
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            Vector2 spawnPosition = GetRandomSpawnPositionOnRadius();
            string selectedPoolKey = SelectEnemyTypeForWave();

            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.SpawnFromPool(selectedPoolKey, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[SpawnHandler] ObjectPoolManager instance not found!");
            }

            spawnedCount++;

            // Recalculate remaining enemies (Active on screen + unspawned)
            remainingWaveEnemies = EnemyLogic.ActiveEnemies.Count + (totalEnemies - spawnedCount);
            UpdateEnemiesLeftUI();

            yield return new WaitForSeconds(spawnIntervalWithinWave);
        }
    }

    private Vector2 GetRandomSpawnPositionOnRadius()
    {
        Vector2 centerPoint = chuTransform != null ? (Vector2)chuTransform.position : Vector2.zero;
        float randomAngleRad = Random.Range(0f, Mathf.PI * 2f);
        Vector2 offset = new Vector2(Mathf.Cos(randomAngleRad), Mathf.Sin(randomAngleRad)) * spawnRadius;

        return centerPoint + offset;
    }

    private string SelectEnemyTypeForWave()
    {
        float roll = Random.value;

        if (currentWave < 5)
        {
            return dronePoolKey; // Waves 1-4: 100% Drone
        }
        else if (currentWave < 10)
        {
            return roll < 0.5f ? dronePoolKey : rangerPoolKey; // Waves 5-9: 50% Drone, 50% Ranger
        }
        else
        {
            if (roll < 0.50f) return dronePoolKey;      // Waves 10+: 50% Drone
            if (roll < 0.80f) return rangerPoolKey;     // 30% Ranger
            return basiliskPoolKey;                   // 20% Basilisk
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = chuTransform != null ? chuTransform.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}