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

    private bool isDocked = false;

    private void Start()
    {
        // Ensure Build Camera starts inactive and Prompt is hidden
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

        // Calculate vector distance from CHU center to the player (Vector Subtraction)[cite: 4, 18, 21]
        Vector3 difference = playerObject.transform.position - transform.position;

        // Compute magnitude of the displacement vector
        float distanceToPlayer = difference.magnitude;

        // Check if player is within docking range
        bool inRange = distanceToPlayer <= dockingRange;

        if (!isDocked)
        {
            // Toggle prompt based on range
            if (dockingPromptUI != null)
            {
                dockingPromptUI.SetActive(inRange);
            }

            // Listen for key input to activate Build Mode when within range
            if (inRange && Input.GetKeyDown(dockKey))
            {
                EnterBuildMode();
            }
        }
    }

    private void EnterBuildMode()
    {
        isDocked = true;

        // Hide UI Prompt
        if (dockingPromptUI != null)
        {
            dockingPromptUI.SetActive(false);
        }

        // Deactivate the Player Object
        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }

        // Activate Build Camera
        if (buildCamera != null)
        {
            buildCamera.SetActive(true);
        }

        if (shipUI != null)
        {
            shipUI.SetActive(false);
        }

        if (buildUI  != null)
        {
            buildUI.SetActive(true);
        }
    }

    // Optional: Call this from a UI Button or Manager to exit Build Mode
    public void ExitBuildMode()
    {
        isDocked = false;

        // Reactivate Player Object
        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }

        // Deactivate Build Camera
        if (buildCamera != null)
        {
            buildCamera.SetActive(false);
        }

        if (shipUI != null)
        {
            shipUI.SetActive(true);
        }

        if (buildUI != null)
        {
            buildUI.SetActive(false);
        }
    }

    // Draw interaction radius in Scene View for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dockingRange);
    }
}