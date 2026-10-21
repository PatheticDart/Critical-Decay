using System.Collections.Generic;
using UnityEngine;

public class ScrapSpawner : MonoBehaviour
{
    [Header("Scrap Prefabs")]
    [SerializeField] private GameObject[] scrapPrefabs; // Assign your 3 scrap prefabs here

    [Header("Clustering Settings")]
    [SerializeField] private int numberOfClusters = 4;     // How many distinct scrap fields
    [SerializeField] private int scrapPerCluster = 5;       // Scrap items per cluster
    [SerializeField] private float clusterRadius = 2.5f;    // Spread radius within each cluster

    [Header("Placement Boundaries")]
    [SerializeField] private float minSpawnRadius = 8f;     // Distance from base center
    [SerializeField] private float maxSpawnRadius = 22f;
    [SerializeField] private Transform originAnchor;

    private void Start()
    {
        if (originAnchor == null)
        {
            originAnchor = transform;
        }

        SpawnClusteredScrap();
    }

    public void SpawnClusteredScrap()
    {
        if (scrapPrefabs == null || scrapPrefabs.Length == 0)
        {
            Debug.LogWarning("[ScrapSpawner] No Scrap Prefabs assigned!");
            return;
        }

        Vector2 center = originAnchor != null ? (Vector2)originAnchor.position : Vector2.zero;

        for (int c = 0; c < numberOfClusters; c++)
        {
            // 1. Pick a cluster center point within the min/max radius from base
            float randomAngle = Random.Range(0f, Mathf.PI * 2f);
            float randomDist = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector2 clusterCenter = center + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDist;

            // 2. Spawn individual scrap items bunched around the cluster center
            for (int i = 0; i < scrapPerCluster; i++)
            {
                // Random position within the cluster radius
                Vector2 localOffset = Random.insideUnitCircle * clusterRadius;
                Vector2 spawnPos = clusterCenter + localOffset;

                // Pick a random prefab from the 3 scrap types
                GameObject selectedPrefab = scrapPrefabs[Random.Range(0, scrapPrefabs.Length)];

                if (selectedPrefab != null)
                {
                    GameObject scrapObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

                    PickupLogic pickup = scrapObj.GetComponent<PickupLogic>();
                    if (pickup != null)
                    {
                        pickup.Initialize(PickupType.Scrap, 1);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = originAnchor != null ? originAnchor.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, minSpawnRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center, maxSpawnRadius);
    }
}