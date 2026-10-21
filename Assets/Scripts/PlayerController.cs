using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float reverse = 5f;
    [SerializeField] private float turningSpeed = 180f;

    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float angularDrag = 3f;
    [SerializeField] private float brakeDamping = 5f;

    [Header("Ship Upgrades")]
    [SerializeField] private Sprite atlasShipSprite;

    private Vector2 velocity;
    private float angularVelocity;
    private SpriteRenderer spriteRenderer;

    public Vector2 Velocity => velocity;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        HandleRotation();
        HandleThrust();
        ApplyMovement();
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

    public void UpgradeToCombatOutfit()
    {
        if (atlasShipSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = atlasShipSprite;
        }

        if (GetComponent<PlayerShootLogic>() == null)
        {
            gameObject.AddComponent<PlayerShootLogic>();
        }
    }
}