using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
    Player,
    Enemy
}

public class ProjectileLogic : MonoBehaviour
{
    public static List<ProjectileLogic> ActivePlayerProjectiles { get; private set; } = new List<ProjectileLogic>();
    public static List<ProjectileLogic> ActiveEnemyProjectiles { get; private set; } = new List<ProjectileLogic>();

    [Header("Projectile Settings")]
    [SerializeField] private ProjectileType type = ProjectileType.Player;
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxLifetime = 3f;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip projectileFireSFX;
    [SerializeField] private AudioSource fireAudioSource;

    [Header("Visual Effects (Hit SFX is attached to this Prefab)")]
    [SerializeField] private GameObject hitVfxPrefab;

    public Vector2 direction;
    private float currentLifetime;

    public int Damage
    {
        get => damage;
        set => damage = value;
    }

    public ProjectileType Type => type;

    private void Start()
    {
        PlayFireSFX();
    }

    public void Initialize(Vector2 startPosition, Vector2 moveDirection, float zRotation, int damageValue, float speedValue, float lifetimeValue)
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
        direction = moveDirection.normalized;
        damage = damageValue;
        speed = speedValue;
        maxLifetime = lifetimeValue;
        currentLifetime = 0f;
    }

    private void OnEnable()
    {
        currentLifetime = 0f;

        if (type == ProjectileType.Player)
        {
            if (!ActivePlayerProjectiles.Contains(this))
                ActivePlayerProjectiles.Add(this);
        }
        else if (type == ProjectileType.Enemy)
        {
            if (!ActiveEnemyProjectiles.Contains(this))
                ActiveEnemyProjectiles.Add(this);
        }

        PlayFireSFX();
    }

    private void OnDisable()
    {
        if (type == ProjectileType.Player)
        {
            ActivePlayerProjectiles.Remove(this);
        }
        else if (type == ProjectileType.Enemy)
        {
            ActiveEnemyProjectiles.Remove(this);
        }

        SpawnHitVFX();
    }

    private void Update()
    {
        if (direction != Vector2.zero)
        {
            Vector2 currentPos = transform.position;
            currentPos += direction.normalized * (speed * Time.deltaTime);
            transform.position = currentPos;
        }

        currentLifetime += Time.deltaTime;
        if (currentLifetime >= maxLifetime)
        {
            Despawn(false);
        }
    }

    public void Initialize(Vector2 startPosition, Vector2 moveDirection, float zRotation, int damageValue)
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
        direction = moveDirection.normalized;
        damage = damageValue;
    }

    public void SpawnHitVFX()
    {
        if (hitVfxPrefab != null)
        {
            Instantiate(hitVfxPrefab, transform.position, transform.rotation);
        }
    }

    private void PlayFireSFX()
    {
        if (projectileFireSFX == null) return;

        if (fireAudioSource != null)
        {
            fireAudioSource.PlayOneShot(projectileFireSFX);
        }
    }

    public void Despawn(bool hitTarget = true)
    {
        gameObject.SetActive(false);
    }
}