using System.Collections;
using UnityEngine;

public class DockingLogic : MonoBehaviour
{
    [Header("Docking Settings")]
    [Tooltip("Interaction distance threshold from the center of the CHU.")]
    [SerializeField] private float dockingRange = 3.0f;

    [Tooltip("Key used to initiate docking mode.")]
    [SerializeField] private KeyCode dockKey = KeyCode.E;

    [Header("References")]
    [Tooltip("Reference to the player object (e.g. Theseus or Atlas ship).")]
    [SerializeField] private GameObject playerObject;

    [Tooltip("The camera that becomes active when entering Build Mode.")]
    [SerializeField] private GameObject buildCamera;

    [Tooltip("UI Prompt text object that appears when in docking range.")]
    [SerializeField] private GameObject dockingPromptUI;
    [SerializeField] private GameObject shipUI;
    [SerializeField] private GameObject buildUI;

    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeFrequency = 25f;
    [SerializeField] private float shakeMagnitude = 0.4f;

    private bool isDocked = false;

    private void Start()
    {
        if (buildCamera != null)
        {
            buildCamera.SetActive(false);
        }

        if (dockingPromptUI != null)
        {
            dockingPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerObject == null) return;

        Vector3 difference = playerObject.transform.position - transform.position;
        float distanceToPlayer = difference.magnitude;
        bool inRange = distanceToPlayer <= dockingRange;

        if (!isDocked)
        {
            if (dockingPromptUI != null)
            {
                dockingPromptUI.SetActive(inRange);
            }

            if (inRange && Input.GetKeyDown(dockKey))
            {
                EnterBuildMode();
            }
        }
    }

    public void FocusActiveCameraOnPosition(Vector3 targetPosition)
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.gameObject.activeInHierarchy)
        {
            CameraFollow mainCamFollow = mainCam.GetComponent<CameraFollow>();
            if (mainCamFollow != null)
            {
                mainCamFollow.SetFollowing(false);
            }

            StartCoroutine(SmoothCameraMoveAndShake(mainCam.transform, targetPosition));
        }

        if (buildCamera != null && buildCamera.activeInHierarchy)
        {
            CameraFollow buildCamFollow = buildCamera.GetComponent<CameraFollow>();
            if (buildCamFollow != null)
            {
                buildCamFollow.SetFollowing(false);
            }

            StartCoroutine(SmoothCameraMoveAndShake(buildCamera.transform, targetPosition));
        }
    }

    private IEnumerator SmoothCameraMoveAndShake(Transform camTransform, Vector3 targetWorldPos)
    {
        Vector3 startPos = camTransform.position;
        Vector3 centerPos = new Vector3(targetWorldPos.x, targetWorldPos.y, camTransform.position.z);

        // Phase 1: Smooth Lerp to CHU Explosion Position
        float lerpDuration = 1.0f;
        float lerpTimer = 0f;

        while (lerpTimer < lerpDuration)
        {
            lerpTimer += Time.deltaTime;
            float t = lerpTimer / lerpDuration;
            camTransform.position = Vector3.Lerp(startPos, centerPos, t);
            yield return null;
        }

        camTransform.position = centerPos;

        // Phase 2: Mathematical Screen Shake (Sin/Cos Oscillations over time)
        float totalShakeDuration = 4.0f;
        float shakeTimer = 0f;

        while (shakeTimer < totalShakeDuration)
        {
            shakeTimer += Time.deltaTime;

            // Decay strength gradually as time passes
            float currentDamping = 1.0f - (shakeTimer / totalShakeDuration);
            float currentMagnitude = shakeMagnitude * currentDamping;

            // Math-based offsets using trigonometric frequency formulas
            float offsetX = Mathf.Sin(shakeTimer * shakeFrequency) * currentMagnitude;
            float offsetY = Mathf.Cos(shakeTimer * shakeFrequency * 0.85f) * currentMagnitude;

            camTransform.position = centerPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        camTransform.position = centerPos;
    }

    private void EnterBuildMode()
    {
        isDocked = true;

        if (dockingPromptUI != null) dockingPromptUI.SetActive(false);
        if (playerObject != null) playerObject.SetActive(false);
        if (buildCamera != null) buildCamera.SetActive(true);
        if (shipUI != null) shipUI.SetActive(false);
        if (buildUI != null) buildUI.SetActive(true);
    }

    public void ExitBuildMode()
    {
        isDocked = false;

        // 1. Close Module Action Panel
        ModuleActionPanel actionPanel = FindObjectOfType<ModuleActionPanel>(true);
        if (actionPanel != null)
        {
            actionPanel.ClosePanel();
        }

        // 2. Close Research Panel
        if (ResearchUIManager.Instance != null)
        {
            ResearchUIManager.Instance.CloseResearchPanel();
        }

        // 3. Close Module Shop Selection Panel
        if (BuildSystem.Instance != null)
        {
            BuildSystem.Instance.CloseModuleShopPanel();
        }

        // Restore Player Ship
        if (playerObject != null)
        {
            playerObject.SetActive(true);

            if (ResearchHandler.Instance != null && ResearchHandler.Instance.unlockedCombatOutfit)
            {
                PlayerController player = playerObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.UpgradeToCombatOutfit();
                }
            }
        }

        if (buildCamera != null) buildCamera.SetActive(false);
        if (shipUI != null) shipUI.SetActive(true);
        if (buildUI != null) buildUI.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dockingRange);
    }
}