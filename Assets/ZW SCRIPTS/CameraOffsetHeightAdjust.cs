using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class CameraOffsetHeightAdjust : MonoBehaviour
{
    [Header("XR Components")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty crouchAction;    // Set to X Button in Inspector
    [SerializeField] private InputActionProperty standTallAction; // Set to Y Button in Inspector

    [Header("Height Offsets")]
    [SerializeField] private float normalOffset = 0f;
    [SerializeField] private float crouchOffset = -0.5f;
    [SerializeField] private float standTallOffset = 0.5f;
    [SerializeField] private float adjustSpeed = 4f;

    private float initialCameraY;
    private float targetOffset;

    private void Awake()
    {
        if (xrOrigin == null)
            xrOrigin = GetComponent<XROrigin>();

        if (xrOrigin != null)
            initialCameraY = xrOrigin.CameraYOffset;
    }

    private void OnEnable()
    {
        if (crouchAction.action != null) crouchAction.action.Enable();
        if (standTallAction.action != null) standTallAction.action.Enable();
    }

    private void OnDisable()
    {
        if (crouchAction.action != null) crouchAction.action.Disable();
        if (standTallAction.action != null) standTallAction.action.Disable();
    }

    private void Update()
    {
        if (xrOrigin == null) return;

        // Read button states
        bool isCrouching = crouchAction.action != null && crouchAction.action.IsPressed();
        bool isStandingTall = standTallAction.action != null && standTallAction.action.IsPressed();

        // Determine target offset based on active inputs
        if (isCrouching)
        {
            targetOffset = crouchOffset;
        }
        else if (isStandingTall)
        {
            targetOffset = standTallOffset;
        }
        else
        {
            targetOffset = normalOffset;
        }

        // Smoothly adjust the CameraYOffset
        float currentY = xrOrigin.CameraYOffset;
        float desiredY = initialCameraY + targetOffset;

        xrOrigin.CameraYOffset = Mathf.Lerp(currentY, desiredY, Time.deltaTime * adjustSpeed);
    }
}