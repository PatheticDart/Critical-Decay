using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialRadarManager : MonoBehaviour
{
    public static RadialRadarManager Instance { get; private set; }

    [Header("UI Prefabs & Containers")]
    [SerializeField] private RectTransform radarCenterContainer; // Anchor at Screen Center (0.5, 0.5)
    [SerializeField] private GameObject chuIndicatorPrefab;
    [SerializeField] private GameObject enemyIndicatorPrefab;

    [Header("Radial Ring Settings")]
    [SerializeField] private float radarRadius = 200f; // Radius in UI pixels from screen center
    [SerializeField] private bool showIndicatorsForOnScreen = false; // Hide if target is visible on-screen

    private Camera mainCamera;
    private Transform playerTransform;
    private Transform chuTransform;

    private GameObject chuIndicatorInstance;
    private List<GameObject> enemyIndicatorPool = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        FindReferences();

        if (chuIndicatorPrefab != null && radarCenterContainer != null)
        {
            chuIndicatorInstance = Instantiate(chuIndicatorPrefab, radarCenterContainer);
        }
    }

    private void FindReferences()
    {
        if (playerTransform == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null) playerTransform = player.transform;
        }

        if (chuTransform == null)
        {
            ModuleHealth chu = ModuleHealth.ActivePlayerModules.Find(m => m != null && m.IsCHUModule);
            if (chu != null) chuTransform = chu.transform;
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null || !mainCamera.gameObject.activeInHierarchy)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (playerTransform == null || chuTransform == null)
        {
            FindReferences();
        }

        UpdateCHUIndicator();
        UpdateEnemyIndicators();
    }

    private void UpdateCHUIndicator()
    {
        if (chuIndicatorInstance == null || chuTransform == null) return;

        UpdateRadialIndicator(chuTransform.position, chuIndicatorInstance.GetComponent<RectTransform>());
    }

    private void UpdateEnemyIndicators()
    {
        List<EnemyLogic> activeEnemies = EnemyLogic.ActiveEnemies;
        int activeCount = 0;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            EnemyLogic enemy = activeEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            if (activeCount >= enemyIndicatorPool.Count)
            {
                GameObject newIndicator = Instantiate(enemyIndicatorPrefab, radarCenterContainer);
                enemyIndicatorPool.Add(newIndicator);
            }

            GameObject indicatorObj = enemyIndicatorPool[activeCount];
            indicatorObj.SetActive(true);

            UpdateRadialIndicator(enemy.transform.position, indicatorObj.GetComponent<RectTransform>());

            activeCount++;
        }

        for (int i = activeCount; i < enemyIndicatorPool.Count; i++)
        {
            enemyIndicatorPool[i].SetActive(false);
        }
    }

    private void UpdateRadialIndicator(Vector3 worldPosition, RectTransform indicatorRect)
    {
        if (indicatorRect == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPosition);
        bool isOnScreen = viewportPos.x >= 0f && viewportPos.x <= 1f &&
                          viewportPos.y >= 0f && viewportPos.y <= 1f &&
                          viewportPos.z > 0f;

        if (isOnScreen && !showIndicatorsForOnScreen)
        {
            indicatorRect.gameObject.SetActive(false);
            return;
        }

        indicatorRect.gameObject.SetActive(true);

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(worldPosition);

        if (targetScreenPos.z < 0f)
        {
            targetScreenPos *= -1f;
        }

        Vector2 direction = (targetScreenPos - screenCenter).normalized;

        indicatorRect.anchoredPosition = direction * radarRadius;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        indicatorRect.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}