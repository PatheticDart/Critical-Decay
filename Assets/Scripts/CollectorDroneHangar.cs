using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ModuleHealth))]
public class CollectorDroneHangar : MonoBehaviour
{
    [Header("Drone Settings")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private float scanRadius = 20f;
    [SerializeField] private Transform spawnPoint;

    private ModuleHealth moduleHealth;
    private bool isHangarActive = true;
    private List<CollectorDrone> activeDrones = new List<CollectorDrone>();

    public bool IsHangarActive => isHangarActive;
    public float ScanRadius => scanRadius;

    private void Awake()
    {
        moduleHealth = GetComponent<ModuleHealth>();
        if (spawnPoint == null) spawnPoint = transform;
    }

    private void Update()
    {
        if (moduleHealth.IsDeactivated)
        {
            SetHangarActive(false);
            return;
        }

        activeDrones.RemoveAll(d => d == null || !d.gameObject.activeInHierarchy);

        int maxDrones = Mathf.Clamp(moduleHealth.CurrentLevel, 1, 4);

        if (isHangarActive && activeDrones.Count < maxDrones)
        {
            SpawnDrone();
        }
    }

    private void SpawnDrone()
    {
        if (dronePrefab == null) return;

        GameObject droneObj = Instantiate(dronePrefab, spawnPoint.position, Quaternion.identity);
        CollectorDrone drone = droneObj.GetComponent<CollectorDrone>();

        if (drone != null)
        {
            drone.Initialize(this);
            activeDrones.Add(drone);
        }
    }

    public void SetHangarActive(bool active)
    {
        isHangarActive = active;
        if (!active)
        {
            foreach (var drone in activeDrones)
            {
                if (drone != null) drone.ForceReturn();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}