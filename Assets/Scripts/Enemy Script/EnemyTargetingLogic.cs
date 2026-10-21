using UnityEngine;

public enum EnemyType
{
    Drone,
    Ranger,
    Basilisk
}

public class EnemyTargetingLogic : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType = EnemyType.Drone;
    [SerializeField] private float searchInterval = 0.2f;

    private Transform currentTarget;
    private float nextSearchTime;

    public Transform CurrentTarget => currentTarget;
    public EnemyType Type => enemyType;

    private void Update()
    {
        if (Time.time >= nextSearchTime)
        {
            nextSearchTime = Time.time + searchInterval;
            FindTarget();
        }
    }

    private void FindTarget()
    {
        var activeModules = ModuleHealth.ActivePlayerModules;
        if (activeModules.Count == 0)
        {
            currentTarget = null;
            return;
        }

        Vector2 myPos = transform.position;

        // Basilisk logic: Filter for turrets first
        if (enemyType == EnemyType.Basilisk)
        {
            ModuleHealth closestTurret = null;
            float closestTurretSqrDist = float.MaxValue;

            for (int i = 0; i < activeModules.Count; i++)
            {
                ModuleHealth module = activeModules[i];
                if (module == null || !module.IsTurret) continue;

                float sqrDist = ((Vector2)module.transform.position - myPos).sqrMagnitude;
                if (sqrDist < closestTurretSqrDist)
                {
                    closestTurretSqrDist = sqrDist;
                    closestTurret = module;
                }
            }

            if (closestTurret != null)
            {
                currentTarget = closestTurret.transform;
                return;
            }
        }

        // Fallback for Drone, Ranger, or Basilisk when no turrets are found
        ModuleHealth closestModule = null;
        float closestSqrDist = float.MaxValue;

        for (int i = 0; i < activeModules.Count; i++)
        {
            ModuleHealth module = activeModules[i];
            if (module == null) continue;

            float sqrDist = ((Vector2)module.transform.position - myPos).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closestModule = module;
            }
        }

        currentTarget = closestModule != null ? closestModule.transform : null;
    }
}