using UnityEngine;

public class PlayerShootLogic : MonoBehaviour
{
    [Header("Weapon Mount References")]
    [SerializeField] private Transform weaponMount;
    [SerializeField] private Transform firePoint;

    [Header("Combat Settings")]
    [SerializeField] private string projectilePoolKey = "PlayerBullet";
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private int damagePerRound = 15;

    private float nextFireTime;
    private Camera mainCam;
    private bool isUpgraded = false;

    private void Awake()
    {
        // Start disabled until explicitly enabled via UpgradeToCombatOutfit
        enabled = false;
    }

    private void OnEnable()
    {
        if (isUpgraded)
        {
            InitializeMounts();
        }
    }

    private void Start()
    {
        if (isUpgraded)
        {
            InitializeMounts();
        }
    }

    public void EnableCombatOutfit()
    {
        isUpgraded = true;
        enabled = true;
        InitializeMounts();
    }

    private void InitializeMounts()
    {
        mainCam = Camera.main;

        if (weaponMount == null)
        {
            Transform existingMount = transform.Find("WeaponMount");
            if (existingMount != null)
            {
                weaponMount = existingMount;
            }
            else
            {
                GameObject mountObj = new GameObject("WeaponMount");
                mountObj.transform.SetParent(transform);
                mountObj.transform.localPosition = Vector3.zero;
                weaponMount = mountObj.transform;
            }
        }

        if (firePoint == null)
        {
            firePoint = weaponMount;
        }
    }

    private void Update()
    {
        if (!isUpgraded) return;

        if (mainCam == null || !mainCam.gameObject.activeInHierarchy)
        {
            mainCam = Camera.main;
        }

        AimGunAtMouse();

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void AimGunAtMouse()
    {
        if (mainCam == null || weaponMount == null) return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (Vector2)(mouseWorldPos - weaponMount.position);

        if (aimDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
            weaponMount.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void Shoot()
    {
        Vector2 spawnPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 direction = weaponMount.up;

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.SpawnProjectile(
                projectilePoolKey,
                spawnPos,
                direction,
                weaponMount.eulerAngles.z,
                damagePerRound
            );
        }
    }
}