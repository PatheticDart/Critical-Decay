using UnityEngine;

public class PlayerShootLogic : MonoBehaviour
{
    [Header("Weapon Mount References")]
    [SerializeField] private Transform weaponMount; // Assign child Gun/Pivot transform
    [SerializeField] private Transform firePoint;   // Assign child FirePoint transform

    [Header("Combat Settings")]
    [SerializeField] private string projectilePoolKey = "PlayerBullet";
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private int damagePerRound = 15;

    private float nextFireTime;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        // Auto-create invisible weapon mount if missing
        if (weaponMount == null)
        {
            GameObject mountObj = new GameObject("WeaponMount");
            mountObj.transform.SetParent(transform);
            mountObj.transform.localPosition = Vector3.zero;
            weaponMount = mountObj.transform;
        }

        if (firePoint == null)
        {
            firePoint = weaponMount;
        }
    }

    private void Update()
    {
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