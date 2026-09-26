using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Controls smooth rotation opening and closing transitions for VR interactive doors/appliances (Fridge, Pantry).
/// Compatible with XR Interactable events.
/// </summary>
public class VRDoorToggle : MonoBehaviour
{
    // CONFIGURATION & SETTINGS

    [Header("Door Angle Settings")]
    [Tooltip("Target local Y angle when open (e.g., -90 or 90 depending on hinge side).")]
    [SerializeField] private float openYAngle = -90f;

    [Tooltip("Speed multiplier for door rotation interpolation.")]
    [SerializeField] private float animationSpeed = 4f;

    // STATE & ROTATION TRACKING

    /// <summary>
    /// Current door state (true = open, false = closed).
    /// </summary>
    private bool isOpen = false;

    /// <summary>
    /// Default closed local rotation captured on Awake.
    /// </summary>
    private Quaternion closedRotation;

    /// <summary>
    /// Target open local rotation calculated from hinge offset angle.
    /// </summary>
    private Quaternion openRotation;

    /// <summary>
    /// Active animation coroutine instance.
    /// </summary>
    private Coroutine animationCoroutine;

    // MONOBEHAVIOUR LIFECYCLE

    private void Awake()
    {
        // Store starting rotation as closed state
        closedRotation = transform.localRotation;

        // Calculate target open rotation relative to initial rotation
        openRotation = closedRotation * Quaternion.Euler(0f, openYAngle, 0f);
    }

    // INTERACTION LOGIC

    /// <summary>
    /// Toggles the door state between open and closed.
    /// Call this method from XR Simple Interactable or XR Grab Interactable events.
    /// </summary>
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Play Door Sound
        AudioManager.Instance?.PlayDoorOpenClose();

        // Stop existing animation coroutine if running
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // Start smooth rotation transition
        animationCoroutine = StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
    }

    /// <summary>
    /// Coroutine that smoothly interpolates door rotation toward the target quaternion using Slerp.
    /// </summary>
    /// <param name="targetRotation">Target Quaternion rotation to animate toward.</param>
    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
            yield return null;
        }

        // Snap precisely to target on completion
        transform.localRotation = targetRotation;
        animationCoroutine = null;
    }
}