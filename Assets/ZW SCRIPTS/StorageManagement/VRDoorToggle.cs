using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRDoorToggle : MonoBehaviour
{
    [Header("Door Angle Settings")]
    [Tooltip("Target local Y angle when open (e.g., -90 or 90 depending on hinge side).")]
    [SerializeField] private float openYAngle = -90f;
    [SerializeField] private float animationSpeed = 4f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        // Store starting rotation as closed state
        closedRotation = transform.localRotation;

        // Calculate target open rotation relative to initial rotation
        openRotation = closedRotation * Quaternion.Euler(0f, openYAngle, 0f);
    }

    /// <summary>
    /// Call this method from XR Simple Interactable or XR Grab Interactable events.
    /// </summary>
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Play Door Sound
        AudioManager.Instance?.PlayDoorOpenClose();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
            yield return null;
        }

        transform.localRotation = targetRotation;
        animationCoroutine = null;
    }
}