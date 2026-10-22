using System.Collections;
using UnityEngine;

public enum TurretType
{
    Flame,
    Sniper,
    Shotgun
}

public class TurretLogic : MonoBehaviour
{
    public TurretType turretType;
    public int damagePerRound = 10;

    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float range = 10f;
    [SerializeField] private float coneAngle = 90f;
    [SerializeField] private float burstInterval = 3.0f;
    [SerializeField] private float fireRate = 0.05f;
    [SerializeField] private int shotCount = 4;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float alignmentTolerance = 0.98f;
    [SerializeField] private float projectileLifeTime = 3f;
    private float nextBurstTime = 0f;

    private LineRenderer lineRenderer;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Target is not assigned in TurretLogic.");
        }

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (target == null) return;

        Vector2 myPos = transform.position;
        Vector2 targetPos = target.position;

        Vector2 targetVelocity = Vector2.zero;

        EnemyMoveLogic enemyMove = target.GetComponent<EnemyMoveLogic>();
        if (enemyMove != null)
        {
            targetVelocity = enemyMove.Velocity;
        }
        else
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                targetVelocity = player.Velocity;
            }
        }

        Vector2 predictedTargetPos = targetPos;
        float travelTime = 0f;

        for (int i = 0; i < 2; i++)
        {
            float dist = (predictedTargetPos - myPos).magnitude;
            travelTime = dist / Mathf.Max(projectileSpeed, 0.1f);
            predictedTargetPos = targetPos + (targetVelocity * travelTime);
        }

        Vector2 aimDirection = (predictedTargetPos - myPos).normalized;
        float leadAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, leadAngle);

        float distance = (targetPos - myPos).magnitude;

        float alignmentDot = Vector2.Dot((Vector2)transform.up, aimDirection);
        bool isAlignedWithLead = alignmentDot >= alignmentTolerance;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        switch (turretType)
        {
            case TurretType.Flame:
                FlameTurretLineRenderer();
                if (IsInCone(target, range, coneAngle))
                {
                    if (Time.time >= nextBurstTime)
                    {
                        StartCoroutine(FlameShoot());
                        nextBurstTime = Time.time + burstInterval;
                    }
                }
                break;

            case TurretType.Sniper:
                SniperTurretLineRenderer();
                if (isAlignedWithLead && distance < range)
                {
                    if (Time.time >= nextBurstTime)
                    {
                        StartCoroutine(SniperShoot());
                        nextBurstTime = Time.time + burstInterval;
                    }
                }
                break;

            case TurretType.Shotgun:
                if (isAlignedWithLead && distance < range)
                {
                    if (Time.time >= nextBurstTime)
                    {
                        StartCoroutine(ShotgunShoot());
                        nextBurstTime = Time.time + burstInterval;
                    }
                }
                break;
        }
    }

    private void SpawnAndConfigureProjectile(Vector2 direction, float rotationAngle)
    {
        GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.Euler(0f, 0f, rotationAngle));

        ProjectileLogic projLogic = newProjectile.GetComponent<ProjectileLogic>();
        if (projLogic != null)
        {
            projLogic.Initialize(transform.position, direction, rotationAngle, damagePerRound, projectileSpeed, projectileLifeTime);
        }
    }

    IEnumerator FlameShoot()
    {
        float forwardRad = Mathf.Atan2(transform.up.y, transform.up.x);
        float halfCone = coneAngle / 2f;
        float angleSpacing = coneAngle / shotCount;
        float offset = angleSpacing / 2f;

        float startAngle = (forwardRad * Mathf.Rad2Deg) - halfCone + offset;

        for (int i = 0; i < shotCount; i++)
        {
            float currentAngle = startAngle + (angleSpacing * i);
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            float projRot = currentAngle - 90f;

            SpawnAndConfigureProjectile(direction, projRot);

            yield return new WaitForSeconds(fireRate);
        }
    }

    IEnumerator SniperShoot()
    {
        Vector2 barrel = this.transform.up;
        Vector2 toPlayer = ((Vector2)target.position - (Vector2)this.transform.position).normalized;
        float dot = Vector2.Dot(barrel, toPlayer);
        bool inSights = dot >= 0.98f;

        float radians = Mathf.Atan2(transform.up.y, transform.up.x);
        Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        if (inSights)
        {
            float projRot = (radians * Mathf.Rad2Deg) - 90f;
            SpawnAndConfigureProjectile(direction, projRot);
        }

        yield return new WaitForSeconds(fireRate);
    }

    IEnumerator ShotgunShoot()
    {
        float forwardRad = Mathf.Atan2(transform.up.y, transform.up.x);
        float halfCone = coneAngle / 2f;
        float angleSpacing = coneAngle / shotCount;
        float offset = angleSpacing / 2f;

        float startAngle = (forwardRad * Mathf.Rad2Deg) - halfCone + offset;

        for (int i = 0; i < shotCount; i++)
        {
            float currentAngle = startAngle + (angleSpacing * i);
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            float projRot = currentAngle - 90f;

            SpawnAndConfigureProjectile(direction, projRot);

            yield return new WaitForSeconds(fireRate);
        }
    }

    bool IsInCone(Transform target, float range, float coneAngle)
    {
        Vector2 dir = target.position - this.transform.position;
        if (dir.magnitude > range) return false;

        float playerAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float turretAngle = transform.eulerAngles.z + 90f;

        float delta = Mathf.Abs(Mathf.DeltaAngle(turretAngle, playerAngle));

        return delta <= coneAngle / 2f;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void FlameTurretLineRenderer()
    {
        int segments = 20;
        lineRenderer.positionCount = segments + 2;

        Vector3 origin = transform.position;
        lineRenderer.SetPosition(0, origin);

        float baseAngle = transform.eulerAngles.z + 90f;
        float halfCone = coneAngle / 2f;
        float startAngle = baseAngle - halfCone;
        float step = coneAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + (i * step);
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector3 pointOffset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * range;
            lineRenderer.SetPosition(i + 1, origin + pointOffset);
        }

        lineRenderer.SetPosition(segments + 1, origin);

        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.red;
    }

    void SniperTurretLineRenderer()
    {
        lineRenderer.positionCount = 2;

        Vector3 origin = transform.position;
        Vector3 targetPoint = origin + (transform.up * range);

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, targetPoint);

        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    public void UpgradeTurretStats(int level)
    {
        damagePerRound = Mathf.RoundToInt(damagePerRound * 1.20f);
        fireRate = fireRate * 1.10f;
        range = range * 1.05f;
    }
}