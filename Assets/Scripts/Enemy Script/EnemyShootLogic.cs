using UnityEngine;

public enum EnemyWeaponType
{
    Turret, // 360-degree constant tracking turret (Basilisk)
    Gun     // Forward-facing weapon requiring ship alignment (Ranger)
}

public class EnemyShootLogic : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private EnemyWeaponType weaponType = EnemyWeaponType.Turret;
    [SerializeField] private int damagePerRound = 1;
    [SerializeField] private float fireRate = 1.5f;       // Shot interval in seconds
    [SerializeField] private float range = 12f;           // Engagement firing range
    [SerializeField] private float rotationSpeed = 180f;  // Degrees per second (constant)
    [SerializeField] private float alignmentThreshold = 0.95f; // Dot product tolerance (~18 deg)

    [Header("Projectile Configuration")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifeTime = 3f;

    private EnemyTargetingLogic targeting;
    private float nextFireTime;

    private void Awake()
    {
        targeting = GetComponent<EnemyTargetingLogic>();
    }

    private void Update()
    {
        if (targeting == null || targeting.CurrentTarget == null) return;

        Transform target = targeting.CurrentTarget;
        Vector2 myPos = transform.position;
        Vector2 targetPos = target.position;

        // Vector direction & distance to target[cite: 4, 18, 21]
        Vector2 dirToTarget = targetPos - myPos;
        float distance = dirToTarget.magnitude;

        if (distance > range) return;

        // --- PRE-AIMING / LEAD SHOOTING MATH ---
        Vector2 targetVelocity = Vector2.zero;

        // Try reading velocity if targeting the Player ship
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            // Optional: reference player.velocity if public, or calculate displacement delta
        }

        // Travel time formula: t = distance / projectileSpeed
        float travelTime = distance / Mathf.Max(projectileSpeed, 0.1f);
        Vector2 predictedTargetPos = targetPos + (targetVelocity * travelTime);
        Vector2 aimDirection = (predictedTargetPos - myPos).normalized;

        // Compute angle relative to +X axis, offset by -90° for +Y facing sprites[cite: 4, 5]
        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        // --- TURRET TYPE (360 Tracking) ---
        if (weaponType == EnemyWeaponType.Turret)
        {
            // Constant speed rotation replacing Slerp easing[cite: 50, 53]
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Time.time >= nextFireTime)
            {
                Shoot(transform.up);
                nextFireTime = Time.time + fireRate;
            }
        }
        // --- GUN TYPE (Forward Aligning) ---
        else if (weaponType == EnemyWeaponType.Gun)
        {
            // Constant speed rotation replacing Slerp easing[cite: 50, 53]
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Alignment check using Dot Product[cite: 7, 8]
            float dot = Vector2.Dot((Vector2)transform.up, aimDirection);
            bool isFacingTarget = dot >= alignmentThreshold;

            if (isFacingTarget && Time.time >= nextFireTime)
            {
                Shoot(transform.up);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Shoot(Vector2 shootDirection)
    {
        if (projectilePrefab == null) return;

        // Calculate rotation angle for projectile[cite: 4, 5]
        float projAngle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg - 90f;
        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0f, 0f, projAngle));

        ProjectileLogic projectile = projObj.GetComponent<ProjectileLogic>();
        if (projectile != null)
        {
            projectile.direction = shootDirection;
            projectile.Damage = damagePerRound;
        }

        Destroy(projObj, projectileLifeTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}