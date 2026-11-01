using UnityEngine;

public class CollectorDrone : MonoBehaviour
{
    [Header("Movement (Newtonian Physics)")]
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float turningSpeed = 200f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float brakeDamping = 4f;

    [Header("Collection Settings")]
    [SerializeField] private int maxInventory = 50;
    [SerializeField] private float collectionRange = 1.5f;
    [SerializeField] private float dockRange = 1.0f;

    private CollectorDroneHangar homeHangar;
    private int currentInventory = 0;
    private bool isReturning = false;

    private Vector2 velocity;
    private PickupLogic targetPickup;

    public void Initialize(CollectorDroneHangar hangar)
    {
        homeHangar = hangar;
        isReturning = false;
        currentInventory = 0;
    }

    public void ForceReturn()
    {
        isReturning = true;
        targetPickup = null;
    }

    private void Update()
    {
        if (homeHangar == null)
        {
            Destroy(gameObject);
            return;
        }

        CheckInventoryAndState();
        HandleLogic();
        ApplyNewtonianMovement();
    }

    private void CheckInventoryAndState()
    {
        if (currentInventory >= maxInventory || !homeHangar.IsHangarActive)
        {
            isReturning = true;
            targetPickup = null;
        }
    }

    private void HandleLogic()
    {
        Vector2 targetPos = transform.position;

        if (isReturning)
        {
            targetPos = homeHangar.transform.position;
            float dist = Vector2.Distance(transform.position, targetPos);

            if (dist <= dockRange)
            {
                DockAndDeposit();
                return;
            }
        }
        else
        {
            if (targetPickup == null || !targetPickup.gameObject.activeInHierarchy)
            {
                FindNearestPickup();
            }

            if (targetPickup != null)
            {
                targetPos = targetPickup.transform.position;
                float dist = Vector2.Distance(transform.position, targetPos);

                if (dist <= collectionRange)
                {
                    CollectPickup();
                }
            }
            else
            {
                // Idle around hangar if no pickups
                targetPos = homeHangar.transform.position;
                if (Vector2.Distance(transform.position, targetPos) < 3f)
                {
                    velocity = Vector2.Lerp(velocity, Vector2.zero, brakeDamping * Time.deltaTime);
                }
            }
        }

        AimAtTarget(targetPos);
        ThrustForward();
    }

    private void FindNearestPickup()
    {
        PickupLogic[] allPickups = FindObjectsOfType<PickupLogic>();
        float closestDist = float.MaxValue;

        foreach (var pickup in allPickups)
        {
            float distToHangar = Vector2.Distance(homeHangar.transform.position, pickup.transform.position);
            if (distToHangar > homeHangar.ScanRadius) continue;

            float distToDrone = Vector2.Distance(transform.position, pickup.transform.position);
            if (distToDrone < closestDist)
            {
                closestDist = distToDrone;
                targetPickup = pickup;
            }
        }
    }

    private void CollectPickup()
    {
        if (targetPickup == null) return;

        currentInventory += targetPickup.Amount;
        Destroy(targetPickup.gameObject);
        targetPickup = null;
    }

    private void DockAndDeposit()
    {
        if (currentInventory > 0 && ResourceHandler.Instance != null)
        {
            ResourceHandler.Instance.AddScrap(currentInventory);
        }

        // Destroy drone; Hangar will respawn it if active
        Destroy(gameObject);
    }

    private void AimAtTarget(Vector2 targetPos)
    {
        Vector2 aimDirection = targetPos - (Vector2)transform.position;
        if (aimDirection.sqrMagnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, turningSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void ThrustForward()
    {
        velocity += (Vector2)transform.up * acceleration * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
    }

    private void ApplyNewtonianMovement()
    {
        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}