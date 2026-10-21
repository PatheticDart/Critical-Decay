using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public struct PoolConfig
    {
        public string poolKey;            // "Drone", "Ranger", "Basilisk", "PlayerBullet", "EnemyBullet"
        public GameObject prefab;
        public int initialCapacity;
    }

    [Header("Pool Configurations")]
    [SerializeField] private List<PoolConfig> poolsToPrewarm;

    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (PoolConfig config in poolsToPrewarm)
        {
            if (config.prefab == null || string.IsNullOrEmpty(config.poolKey)) continue;

            Queue<GameObject> objectQueue = new Queue<GameObject>();
            GameObject parentGroup = new GameObject($"Pool_{config.poolKey}");
            parentGroup.transform.SetParent(transform);

            for (int i = 0; i < config.initialCapacity; i++)
            {
                GameObject obj = Instantiate(config.prefab, parentGroup.transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(config.poolKey, objectQueue);
            prefabLookup.Add(config.poolKey, config.prefab);
        }
    }

    /// <summary>
    /// Spawns or retrieves an inactive GameObject from the pool using pure 2D transform positioning.
    /// </summary>
    public GameObject SpawnFromPool(string poolKey, Vector2 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolKey))
        {
            Debug.LogWarning($"[ObjectPoolManager] Pool with key '{poolKey}' does not exist!");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[poolKey];

        GameObject objToSpawn;

        // Expand pool dynamically if capacity is depleted
        if (queue.Count == 0 || queue.Peek().activeInHierarchy)
        {
            objToSpawn = Instantiate(prefabLookup[poolKey], transform);
        }
        else
        {
            objToSpawn = queue.Dequeue();
        }

        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);

        queue.Enqueue(objToSpawn);

        return objToSpawn;
    }

    /// <summary>
    /// Spawns and initializes a pooled Projectile cleanly.
    /// </summary>
    public ProjectileLogic SpawnProjectile(string poolKey, Vector2 position, Vector2 direction, float zRotation, int damage)
    {
        GameObject projObj = SpawnFromPool(poolKey, position, Quaternion.Euler(0f, 0f, zRotation));
        if (projObj != null)
        {
            ProjectileLogic proj = projObj.GetComponent<ProjectileLogic>();
            if (proj != null)
            {
                proj.Initialize(position, direction, zRotation, damage);
                return proj;
            }
        }
        return null;
    }
}