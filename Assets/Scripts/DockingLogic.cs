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

    private void EnterBuildMode()
    {
        isDocked = true;

        if (dockingPromptUI != null)
        {
            dockingPromptUI.SetActive(false);
        }

        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }

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

    public void ExitBuildMode()
    {
        isDocked = false;

        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dockingRange);
    }
}