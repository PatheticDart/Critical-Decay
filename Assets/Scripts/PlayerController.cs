using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ship Movement Settings (Base)")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float reverse = 5f;
    [SerializeField] private float turningSpeed = 180f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float angularDrag = 3f;
    [SerializeField] private float brakeDamping = 5f;

    [Header("Combat Outfit Movement Upgrades")]
    [SerializeField] private float upgradedAcceleration = 15f;
    [SerializeField] private float upgradedReverse = 8f;
    [SerializeField] private float upgradedTurningSpeed = 240f;
    [SerializeField] private float upgradedMaxSpeed = 28f;
    [SerializeField] private float upgradedAngularDrag = 4f;
    [SerializeField] private float upgradedBrakeDamping = 7f;

    [Header("Directional Thruster Particles")]
    [SerializeField] private GameObject[] forwardThrusterParticles;
    [SerializeField] private GameObject[] reverseThrusterParticles;
    [SerializeField] private GameObject[] leftTurnThrusterParticles;
    [SerializeField] private GameObject[] rightTurnThrusterParticles;

    [Header("Audio SFX")]
    [SerializeField] private AudioSource thrusterAudioSource;
    [SerializeField] private AudioClip thrusterLoopSFX;

    [Header("Ship Upgrades")]
    [SerializeField] private Sprite atlasShipSprite;

    private Vector2 velocity;
    private float angularVelocity;
    private SpriteRenderer spriteRenderer;

    public Vector2 Velocity => velocity;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (thrusterAudioSource == null)
        {
            thrusterAudioSource = GetComponent<AudioSource>();
        }

        if (thrusterAudioSource != null && thrusterLoopSFX != null)
        {
            thrusterAudioSource.clip = thrusterLoopSFX;
            thrusterAudioSource.loop = true;
        }
    }

    private void OnEnable()
    {
        if (ResearchHandler.Instance != null && ResearchHandler.Instance.unlockedCombatOutfit)
        {
            UpgradeToCombatOutfit();
        }
    }

    private void Update()
    {
        HandleRotation();
        HandleThrust();
        ApplyMovement();
        HandleThrusterVisualsAndAudio();
    }

    private void HandleRotation()
    {
        float xIn = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(xIn) > 0.25f)
        {
            angularVelocity += -xIn * turningSpeed * Time.deltaTime;
        }

        angularVelocity = Mathf.Lerp(angularVelocity, 0f, angularDrag * Time.deltaTime);
        transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);
    }

    private void HandleThrust()
    {
        float vIn = Input.GetAxis("Vertical");

        if (vIn > 0f)
        {
            velocity += (Vector2)transform.up * acceleration * Time.deltaTime;
            velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        }
        else if (vIn < 0f)
        {
            velocity -= (Vector2)transform.up * reverse * Time.deltaTime;
            velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            velocity = Vector2.Lerp(velocity, Vector2.zero, brakeDamping * Time.deltaTime);
        }
    }

    private void ApplyMovement()
    {
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private void HandleThrusterVisualsAndAudio()
    {
        float xIn = Input.GetAxisRaw("Horizontal");
        float vIn = Input.GetAxisRaw("Vertical");
        bool isBraking = Input.GetKey(KeyCode.Space);

        bool isMovingForward = vIn > 0.1f;
        bool isMovingReverse = vIn < -0.1f || isBraking;
        bool isTurningLeft = xIn < -0.25f;
        bool isTurningRight = xIn > 0.25f;

        ToggleParticleArray(forwardThrusterParticles, isMovingForward);
        ToggleParticleArray(reverseThrusterParticles, isMovingReverse);
        ToggleParticleArray(leftTurnThrusterParticles, isTurningLeft);
        ToggleParticleArray(rightTurnThrusterParticles, isTurningRight);

        bool isAnyInputActive = isMovingForward || isMovingReverse || isTurningLeft || isTurningRight;

        if (thrusterAudioSource != null)
        {
            if (isAnyInputActive)
            {
                if (!thrusterAudioSource.isPlaying)
                {
                    thrusterAudioSource.Play();
                }
            }
            else
            {
                if (thrusterAudioSource.isPlaying)
                {
                    thrusterAudioSource.Stop();
                }
            }
        }
    }

    private void ToggleParticleArray(GameObject[] particles, bool state)
    {
        if (particles == null) return;

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null && particles[i].activeSelf != state)
            {
                particles[i].SetActive(state);
            }
        }
    }

    public void UpgradeToCombatOutfit()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (atlasShipSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = atlasShipSprite;
        }

        acceleration = upgradedAcceleration;
        reverse = upgradedReverse;
        turningSpeed = upgradedTurningSpeed;
        maxSpeed = upgradedMaxSpeed;
        angularDrag = upgradedAngularDrag;
        brakeDamping = upgradedBrakeDamping;

        PlayerShootLogic shootLogic = GetComponent<PlayerShootLogic>();
        if (shootLogic == null)
        {
            shootLogic = gameObject.AddComponent<PlayerShootLogic>();
        }

        shootLogic.EnableCombatOutfit();
    }
}