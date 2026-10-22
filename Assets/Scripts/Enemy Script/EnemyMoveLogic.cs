using UnityEngine;

public class EnemyMoveLogic : MonoBehaviour
{
    [Header("Zero-G Propulsion")]
    [SerializeField] private float thrustAcceleration = 8f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float turningSpeed = 180f;
    [SerializeField] private float angularDrag = 2f;

    [Header("Engagement & Jousting")]
    [SerializeField] private float ramRadius = 0.6f;
    [SerializeField] private float minEngagementDist = 6f;
    [SerializeField] private float maxEngagementDist = 12f;
    [SerializeField] private int ramDamage = 2;

    private EnemyTargetingLogic targeting;
    private EnemyLogic enemyLogic;
    private Vector2 velocity;
    public Vector2 Velocity => velocity;
    private float angularVelocity;

    private bool isDisengaging = false;

    private void Awake()
    {
        targeting = GetComponent<EnemyTargetingLogic>();
        enemyLogic = GetComponent<EnemyLogic>();
    }

    private void OnEnable()
    {
        velocity = Vector2.zero;
        angularVelocity = 0f;
        isDisengaging = false;
    }

    private void Update()
    {
        if (targeting == null || targeting.CurrentTarget == null)
        {
            ApplyDrift();
            return;
        }

        Transform target = targeting.CurrentTarget;
        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        switch (targeting.Type)
        {
            case EnemyType.Drone:
                HandleDroneBehavior(toTarget, distance, target);
                break;

            case EnemyType.Ranger:
                HandleRangerJousting(toTarget, distance);
                break;

            case EnemyType.Basilisk:
                HandleBasiliskBehavior(toTarget, distance);
                break;
        }

        ApplyDrift();
    }

    private void HandleDroneBehavior(Vector2 toTarget, float distance, Transform target)
    {
        SteerTowards(toTarget);

        velocity += (Vector2)transform.up * thrustAcceleration * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        if (distance <= ramRadius)
        {
            ModuleHealth module = target.GetComponent<ModuleHealth>();
            if (module != null)
            {
                module.TakeDamage(ramDamage);
            }

            if (enemyLogic != null)
            {
                enemyLogic.TakeDamage(9999);
            }
        }
    }

    private void HandleRangerJousting(Vector2 toTarget, float distance)
    {
        if (!isDisengaging && distance <= minEngagementDist)
        {
            isDisengaging = true;
        }
        else if (isDisengaging && distance >= maxEngagementDist)
        {
            isDisengaging = false;
        }

        Vector2 moveDirection = isDisengaging ? -toTarget : toTarget;
        SteerTowards(moveDirection);

        velocity += (Vector2)transform.up * thrustAcceleration * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
    }

    private void HandleBasiliskBehavior(Vector2 toTarget, float distance)
    {
        SteerTowards(toTarget);

        Vector2 desiredThrust = Vector2.zero;

        if (distance > maxEngagementDist)
        {
            desiredThrust = transform.up;
        }
        else if (distance < minEngagementDist)
        {
            desiredThrust = -transform.up;
        }
        else
        {
            desiredThrust = new Vector2(-toTarget.y, toTarget.x).normalized;
        }

        velocity += desiredThrust * thrustAcceleration * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
    }

    private void SteerTowards(Vector2 targetDirection)
    {
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.deltaTime);
    }

    private void ApplyDrift()
    {
        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}