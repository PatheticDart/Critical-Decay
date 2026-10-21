using UnityEngine;

public class TurretTargetingLogic : MonoBehaviour
{
    [Header("Targeting Settings")]
    [SerializeField] private float range = 10f;
    [SerializeField] private float targetSearchInterval = 0.1f;

    [Header("Current Target")]
    [SerializeField] private Transform currentTarget;

    private TurretLogic turretLogic;
    private float nextSearchTime;

    public Transform CurrentTarget => currentTarget;
    public float Range => range;

    private void Awake()
    {
        // Cache reference to TurretLogic on the same GameObject
        turretLogic = GetComponent<TurretLogic>();
    }

    private void Update()
    {
        // 1. If target is destroyed, inactive, or goes out of range, clear it
        if (currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy || IsOutOfRange(currentTarget.position))
            {
                SetTarget(null);
            }
        }

        // 2. Search for a target on a throttled interval
        if (Time.time >= nextSearchTime)
        {
            nextSearchTime = Time.time + targetSearchInterval;

            if (currentTarget == null)
            {
                FindClosestTarget();
            }
        }
    }

    private void FindClosestTarget()
    {
        var activeEnemies = EnemyLogic.ActiveEnemies;
        if (activeEnemies.Count == 0) return;

        Transform closestEnemy = null;
        float closestDistanceSqr = range * range;
        Vector2 turretPos = transform.position;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            EnemyLogic enemy = activeEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float sqrDistance = ((Vector2)enemy.transform.position - turretPos).sqrMagnitude;

            if (sqrDistance <= closestDistanceSqr)
            {
                closestDistanceSqr = sqrDistance;
                closestEnemy = enemy.transform;
            }
        }

        SetTarget(closestEnemy);
    }

    private void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;

        // Assign the target directly to TurretLogic
        if (turretLogic != null)
        {
            turretLogic.SetTarget(newTarget);
        }
    }

    private bool IsOutOfRange(Vector2 enemyPosition)
    {
        float sqrDistance = (enemyPosition - (Vector2)transform.position).sqrMagnitude;
        return sqrDistance > (range * range);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}