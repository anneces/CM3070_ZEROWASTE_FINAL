using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Controls CanvasGroup UI messages AND 3D physical colliders to restrict interaction with specific 
/// kitchen stations during unauthorized day phases (e.g., blocking cooking or storage insertion).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PhaseInteractionBlocker : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("CanvasGroup controlling UI visibility for this station.")]
    public CanvasGroup canvasGroup;

    [Tooltip("Single warning message text component (optional if using array below).")]
    public TextMeshProUGUI warningMessageText;

    [Tooltip("Multiple warning message text components (e.g., for multi-compartment storage labels).")]
    public TextMeshProUGUI[] warningMessageTexts;

    [Header("Physical Blocker Controls")]
    [Tooltip("Primary 3D Collider (e.g., BoxCollider) that physically blocks physical hands, food items, and VR rays.")]
    public Collider physicalBlockerCollider;

    [Tooltip("Optional array of colliders if blocking multiple doors or openings (e.g., Fridge + Freezer + Pantry).")]
    public Collider[] physicalBlockerColliders;

    [Header("XR Interaction Controls")]
    [Tooltip("Optional array of XR interactable components (e.g., door handles, stove knobs) to disable during blocked phases.")]
    public XRBaseInteractable[] xrInteractablesToDisable;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        EnsureCanvasGroup();

        // Automatically cache a collider on this GameObject if not explicitly assigned
        if (physicalBlockerCollider == null)
        {
            physicalBlockerCollider = GetComponent<Collider>();
        }
    }

    #endregion

    #region Blocker Control Methods

    /// <summary>
    /// Internal validation helper to guarantee CanvasGroup component reference.
    /// </summary>
    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Blocks interactions by enabling physical colliders and displaying the phase warning overlay UI message.
    /// </summary>
    /// <param name="message">The warning text explaining why the station is restricted.</param>
    public void Block(string message)
    {
        EnsureCanvasGroup();

        if (gameObject != null && !gameObject.activeSelf)
            gameObject.SetActive(true);

        // 1. Update UI Text Messages
        if (warningMessageText != null)
        {
            warningMessageText.text = message;
        }

        if (warningMessageTexts != null && warningMessageTexts.Length > 0)
        {
            foreach (TextMeshProUGUI txt in warningMessageTexts)
            {
                if (txt != null) txt.text = message;
            }
        }

        // 2. Display UI Overlay Canvas
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // 3. ENABLE Physical Colliders to physically block player rays, hands, and food items
        SetCollidersState(true);

        // 4. DISABLE XR Interactables so players cannot grab or interact with doors/knobs
        SetXRInteractablesState(false);
    }

    /// <summary>
    /// Removes station restrictions, hiding the UI overlay and disabling physical colliders.
    /// </summary>
    public void Unblock()
    {
        EnsureCanvasGroup();

        // 1. Hide UI Overlay Canvas
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 2. DISABLE Physical Colliders to allow items/interactions through
        SetCollidersState(false);

        // 3. ENABLE XR Interactables to allow interaction again
        SetXRInteractablesState(true);
    }

    /// <summary>
    /// Enables or disables assigned 3D physical colliders.
    /// </summary>
    private void SetCollidersState(bool isEnabled)
    {
        if (physicalBlockerCollider != null)
        {
            physicalBlockerCollider.enabled = isEnabled;
        }

        if (physicalBlockerColliders != null && physicalBlockerColliders.Length > 0)
        {
            foreach (Collider col in physicalBlockerColliders)
            {
                if (col != null)
                {
                    col.enabled = isEnabled;
                }
            }
        }
    }

    /// <summary>
    /// Enables or disables assigned XR Interactable components.
    /// </summary>
    private void SetXRInteractablesState(bool isEnabled)
    {
        if (xrInteractablesToDisable != null && xrInteractablesToDisable.Length > 0)
        {
            foreach (XRBaseInteractable interactable in xrInteractablesToDisable)
            {
                if (interactable != null)
                {
                    interactable.enabled = isEnabled;
                }
            }
        }
    }

    #endregion
}