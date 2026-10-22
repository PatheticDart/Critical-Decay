using UnityEngine;

public enum PickupType
{
    Scrap,
    ResearchSample
}

public class PickupLogic : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private PickupType type = PickupType.Scrap;
    [SerializeField] private int amount = 1;
    [SerializeField] private float collectionRadius = 2.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Fly-to-UI Animation Settings")]
    [SerializeField] private float flyDuration = 0.8f;
    [SerializeField] private RectTransform scrapUIRect;
    [SerializeField] private RectTransform researchUIRect;

    public PickupType Type => type;
    public int Amount => amount;

    private Transform playerTransform;
    private Camera mainCamera;

    private bool isCollected = false;
    private bool isFlying = false;

    private Vector3 startWorldPos;
    private Vector3 controlWorldPos;
    private float flyElapsedTime = 0f;

    private void Start()
    {
        mainCamera = Camera.main;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        FindPlayer();
        FindUIRefs();
    }

    private void FindPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void FindUIRefs()
    {
        if (scrapUIRect == null)
        {
            GameObject scrapObj = GameObject.Find("Scrap Text");
            if (scrapObj != null) scrapUIRect = scrapObj.GetComponent<RectTransform>();
        }

        if (researchUIRect == null)
        {
            GameObject researchObj = GameObject.Find("Research Text");
            if (researchObj != null) researchUIRect = researchObj.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (isFlying)
        {
            AnimateFlyToUI();
            return;
        }

        if (isCollected) return;

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= collectionRadius)
        {
            StartFlyAnimation();
        }
    }

    private void StartFlyAnimation()
    {
        isCollected = true;
        startWorldPos = transform.position;

        PlayPickupSFX();

        Vector3 midPoint = startWorldPos + new Vector3(Random.Range(-1.0f, 1.0f), 1.5f, 0f);
        controlWorldPos = midPoint;

        flyElapsedTime = 0f;
        isFlying = true;
    }

    private void PlayPickupSFX()
    {
        if (audioClip == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick(1, audioClip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }
    }

    private void AnimateFlyToUI()
    {
        flyElapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(flyElapsedTime / flyDuration);

        float easedT = t * t;

        RectTransform targetRect = (type == PickupType.Scrap) ? scrapUIRect : researchUIRect;

        if (targetRect == null)
        {
            FindUIRefs();
            targetRect = (type == PickupType.Scrap) ? scrapUIRect : researchUIRect;
        }

        Vector3 targetWorldPos = startWorldPos;

        if (targetRect != null && mainCamera != null)
        {
            Vector3 screenPos = targetRect.position;
            screenPos.z = Mathf.Abs(mainCamera.transform.position.z);
            targetWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
        }

        Vector3 m1 = Vector3.Lerp(startWorldPos, controlWorldPos, easedT);
        Vector3 m2 = Vector3.Lerp(controlWorldPos, targetWorldPos, easedT);
        transform.position = Vector3.Lerp(m1, m2, easedT);

        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, easedT);

        if (t >= 1f)
        {
            isFlying = false;
            OnArrivedAtUI();
        }
    }

    private void OnArrivedAtUI()
    {
        if (ResourceHandler.Instance != null)
        {
            if (type == PickupType.Scrap)
            {
                ResourceHandler.Instance.AddScrap(amount);
            }
            else if (type == PickupType.ResearchSample)
            {
                ResourceHandler.Instance.AddResearchSamples(amount);
            }
        }

        Destroy(gameObject);
    }

    public void Initialize(PickupType pickupType, int dropAmount = -1)
    {
        type = pickupType;
        if (dropAmount > 0)
        {
            amount = dropAmount;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectionRadius);
    }
}