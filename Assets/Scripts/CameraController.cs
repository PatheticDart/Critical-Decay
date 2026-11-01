using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("Zoom Settings")]
    [Tooltip("Set to 128 for Player Camera, 64 for Build Camera")]
    [SerializeField] private int defaultPPU = 128;
    [SerializeField] private float zoomSpeed = 100f;
    [SerializeField] private float scrollZoomMultiplier = 30f;
    [SerializeField] private int minPPU = 32;
    [SerializeField] private int maxPPU = 256;
    [SerializeField] private float zoomLerpSpeed = 8f;

    [Header("Build Camera Movement Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private bool isFollowing = true;
    private PixelPerfectCamera pixelPerfectCamera;

    private float targetPPU;
    [SerializeField] private float currentFloatPPU;

    public int DefaultPPU => defaultPPU;
    public int CurrentPPU => pixelPerfectCamera != null ? pixelPerfectCamera.assetsPPU : defaultPPU;

    void Start()
    {
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();

        PlayerController pc = FindObjectOfType<PlayerController>(true);
        if (pc != null)
        {
            player = pc.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        targetPPU = defaultPPU;
        currentFloatPPU = defaultPPU;

        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.assetsPPU = defaultPPU;
        }
    }

    void Update()
    {
        if (isFollowing && player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            HandleMovement();
        }

        HandleZoom();
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(x, y, 0f).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        if (pixelPerfectCamera == null) return;

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            targetPPU -= zoomSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
        {
            targetPPU += zoomSpeed * Time.deltaTime;
        }

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0f)
        {
            targetPPU -= -scrollDelta * scrollZoomMultiplier;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetMouseButtonDown(2))
        {
            targetPPU = defaultPPU;

            if (!isFollowing)
            {
                Transform targetTrans = player;

                if (targetTrans == null || !targetTrans.gameObject.activeInHierarchy)
                {
                    ModuleHealth chu = ModuleHealth.ActivePlayerModules.Find(m => m != null && m.IsCHUModule);
                    if (chu != null) targetTrans = chu.transform;
                }

                if (targetTrans != null)
                {
                    Vector3 resetPos = targetTrans.position;
                    resetPos.z = transform.position.z;
                    transform.position = resetPos;
                }
            }
        }

        targetPPU = Mathf.Clamp(targetPPU, minPPU, maxPPU);

        if (Mathf.Abs(currentFloatPPU - targetPPU) > 0.01f)
        {
            currentFloatPPU = Mathf.Lerp(currentFloatPPU, targetPPU, Time.deltaTime * zoomLerpSpeed);
            pixelPerfectCamera.assetsPPU = Mathf.RoundToInt(currentFloatPPU);
        }
    }

    public void SetFollowing(bool follow)
    {
        isFollowing = follow;
    }
}