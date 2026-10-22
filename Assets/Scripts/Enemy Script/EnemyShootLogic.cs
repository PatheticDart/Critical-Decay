using UnityEngine;

public enum EnemyWeaponType
{
    Turret,
    Gun
}

public class EnemyShootLogic : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private EnemyWeaponType weaponType = EnemyWeaponType.Turret;
    [SerializeField] private int damagePerRound = 1;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float range = 12f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float alignmentThreshold = 0.95f;

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

        Vector2 dirToTarget = targetPos - myPos;
        float distance = dirToTarget.magnitude;

        if (distance > range) return;

        Vector2 targetVelocity = Vector2.zero;

        PlayerController player = target.GetComponent<PlayerController>();

        float travelTime = distance / Mathf.Max(projectileSpeed, 0.1f);
        Vector2 predictedTargetPos = targetPos + (targetVelocity * travelTime);
        Vector2 aimDirection = (predictedTargetPos - myPos).normalized;

        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        if (weaponType == EnemyWeaponType.Turret)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Time.time >= nextFireTime)
            {
                Shoot(transform.up);
                nextFireTime = Time.time + fireRate;
            }
        }
        else if (weaponType == EnemyWeaponType.Gun)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

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