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
    [SerializeField] private float projectileSpeed = 10f; // Ensure this matches ProjectileLogic speed
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

        // --- 1. EXTRACT REAL TARGET VELOCITY ---
        Vector2 targetVelocity = Vector2.zero;

        EnemyMoveLogic enemyMove = target.GetComponent<EnemyMoveLogic>();
        if (enemyMove != null)
        {
            // Access velocity from EnemyMoveLogic
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

        // --- 2. ITERATIVE LEAD PREDICTION ---
        Vector2 predictedTargetPos = targetPos;
        float travelTime = 0f;

        // 2 iterations give near-exact lead points without performance cost
        for (int i = 0; i < 2; i++)
        {
            float dist = (predictedTargetPos - myPos).magnitude;
            travelTime = dist / Mathf.Max(projectileSpeed, 0.1f);
            predictedTargetPos = targetPos + (targetVelocity * travelTime);
        }

        Vector2 aimDirection = (predictedTargetPos - myPos).normalized;
        float leadAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, leadAngle);

        // Calculate distance to current target for range checks
        float distance = (targetPos - myPos).magnitude;

        // Alignment verification between barrel direction and lead angle
        float alignmentDot = Vector2.Dot((Vector2)transform.up, aimDirection);
        bool isAlignedWithLead = alignmentDot >= alignmentTolerance;

        // --- 3. CONSTANT ROTATION ---
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // --- 4. FIRING WITH ALIGNMENT CHECK ---
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
                // ShotgunTurretLineRenderer();
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

            float x = Mathf.Cos(rad);
            float y = Mathf.Sin(rad);
            Vector2 direction = new Vector2(x, y);

            float projRot = currentAngle - 90f;
            GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.Euler(0f, 0f, projRot));

            ProjectileLogic rocketLogic = newProjectile.GetComponent<ProjectileLogic>();
            if (rocketLogic != null)
            {
                rocketLogic.direction = direction;
                rocketLogic.Damage = damagePerRound;
            }
            Destroy(newProjectile, projectileLifeTime);

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
        float x = Mathf.Cos(radians);
        float y = Mathf.Sin(radians);

        Vector2 direction = new Vector2(x, y);

        if (inSights)
        {
            float projRot = (radians * Mathf.Rad2Deg) - 90f;
            GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.Euler(0f, 0f, projRot));

            ProjectileLogic rocketLogic = newProjectile.GetComponent<ProjectileLogic>();
            if (rocketLogic != null)
            {
                rocketLogic.direction = direction;
                rocketLogic.Damage = damagePerRound;
            }
            Destroy(newProjectile, projectileLifeTime);
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

            float x = Mathf.Cos(rad);
            float y = Mathf.Sin(rad);
            Vector2 direction = new Vector2(x, y);

            float projRot = currentAngle - 90f;
            GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.Euler(0f, 0f, projRot));

            ProjectileLogic rocketLogic = newProjectile.GetComponent<ProjectileLogic>();
            if (rocketLogic != null)
            {
                rocketLogic.direction = direction;
                rocketLogic.Damage = damagePerRound;
            }
            Destroy(newProjectile, projectileLifeTime);

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

    void ShotgunTurretLineRenderer()
    {
        Vector3 origin = transform.position;

        float forwardRad = Mathf.Atan2(transform.up.y, transform.up.x);
        float halfCone = coneAngle / 2f;
        float angleSpacing = coneAngle / shotCount;
        float offset = angleSpacing / 2f;
        float startAngle = (forwardRad * Mathf.Rad2Deg) - halfCone + offset;

        int circleSegments = 40;

        lineRenderer.positionCount = (shotCount * 3) + (circleSegments + 1);

        int pointIndex = 0;
        float lastPelletAngleDeg = 0f;

        for (int i = 0; i < shotCount; i++)
        {
            float currentAngle = startAngle + (i * angleSpacing);
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Vector3 endPoint = origin + (direction * range);

            lineRenderer.SetPosition(pointIndex++, origin);
            lineRenderer.SetPosition(pointIndex++, endPoint);

            if (i == shotCount - 1)
            {
                lineRenderer.SetPosition(pointIndex++, endPoint);
                lastPelletAngleDeg = currentAngle;
            }
            else
            {
                lineRenderer.SetPosition(pointIndex++, origin);
            }
        }

        float step = 360f / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float currentCircleAngle = lastPelletAngleDeg + (i * step);
            float rad = currentCircleAngle * Mathf.Deg2Rad;

            Vector3 circlePoint = origin + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * range;
            lineRenderer.SetPosition(pointIndex++, circlePoint);
        }

        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }
}