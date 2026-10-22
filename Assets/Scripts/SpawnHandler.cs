using System.Collections;
using TMPro;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    [Header("Game Mode Settings")]
    [SerializeField] private bool isEndlessMode = false;
    [SerializeField] private int maxCampaignWave = 10;

    [Header("Spawn Origin Settings")]
    [SerializeField] private Transform chuTransform;
    [SerializeField] private float spawnRadius = 30f;

    [Header("Timing Settings")]
    [SerializeField] private float initialIntermissionTime = 90f;
    [SerializeField] private float standardIntermissionTime = 50f;
    [SerializeField] private float spawnIntervalWithinWave = 0.5f;

    [Header("Wave Progression & Cap Settings")]
    [SerializeField] private int baseWaveEnemies = 10;
    [SerializeField] private int enemyIncrementPerWave = 5;
    [SerializeField] private int maxActiveEnemies = 30;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timerText;

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
    public bool IsEndlessMode => isEndlessMode;

    private void Awake()
    {
        isEndlessMode = PlayerPrefs.GetInt("IsEndlessMode", 0) == 1;
    }

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
        isIntermission = true;
        SetWaveHeader("PREPARATION PHASE");
        yield return StartCoroutine(RunIntermissionCountdown(initialIntermissionTime));

        while (true)
        {
            currentWave++;
            isIntermission = false;
            isSpawningWave = true;

            SetWaveHeader($"WAVE {currentWave}");

            int totalEnemiesForWave = baseWaveEnemies + ((currentWave - 1) * enemyIncrementPerWave);
            remainingWaveEnemies = totalEnemiesForWave;

            UpdateEnemiesLeftUI();

            yield return StartCoroutine(SpawnWaveRoutine(totalEnemiesForWave));

            isSpawningWave = false;

            // Wait until all spawned enemies are deactivated/destroyed
            while (GetActiveEnemyCount() > 0)
            {
                remainingWaveEnemies = GetActiveEnemyCount();
                UpdateEnemiesLeftUI();
                yield return new WaitForSeconds(0.2f);
            }

            remainingWaveEnemies = 0;
            UpdateEnemiesLeftUI();
            yield return new WaitForSeconds(1.0f);

            // Win Condition Check
            if (!isEndlessMode && currentWave >= maxCampaignWave)
            {
                TriggerGameWin();
                yield break;
            }

            isIntermission = true;
            SetWaveHeader("INTERMISSION");
            yield return StartCoroutine(RunIntermissionCountdown(standardIntermissionTime));
        }
    }

    private int GetActiveEnemyCount()
    {
        int count = 0;
        for (int i = EnemyLogic.ActiveEnemies.Count - 1; i >= 0; i--)
        {
            if (EnemyLogic.ActiveEnemies[i] != null && EnemyLogic.ActiveEnemies[i].gameObject.activeInHierarchy)
            {
                count++;
            }
            else if (EnemyLogic.ActiveEnemies[i] == null)
            {
                EnemyLogic.ActiveEnemies.RemoveAt(i);
            }
        }
        return count;
    }

    private void TriggerGameWin()
    {
        SetWaveHeader("VICTORY!");

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "ALL WAVES CLEARED";
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameWin();
        }
        else
        {
            Debug.LogError("[SpawnHandler] GameManager.Instance is NULL! Ensure GameManager is present in the scene.");
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
            if (GetActiveEnemyCount() >= maxActiveEnemies)
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

            remainingWaveEnemies = GetActiveEnemyCount() + (totalEnemies - spawnedCount);
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
            return dronePoolKey;
        }
        else if (currentWave < 10)
        {
            return roll < 0.5f ? dronePoolKey : rangerPoolKey;
        }
        else
        {
            if (roll < 0.50f) return dronePoolKey;
            if (roll < 0.80f) return rangerPoolKey;
            return basiliskPoolKey;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = chuTransform != null ? chuTransform.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}