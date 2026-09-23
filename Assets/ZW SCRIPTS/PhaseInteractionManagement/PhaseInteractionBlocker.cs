using UnityEngine;
using TMPro;

/// <summary>
/// Controls CanvasGroup settings and overlay messages to restrict interaction with specific 
/// kitchen stations during unauthorized day phases (e.g., blocking cooking during Procurement phase).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PhaseInteractionBlocker : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("CanvasGroup controlling raycast blocking and UI visibility for this station.")]
    public CanvasGroup canvasGroup;

    [Tooltip("Single warning message text component (optional if using array below).")]
    public TextMeshProUGUI warningMessageText;

    [Tooltip("Multiple warning message text components (e.g., for multi-compartment storage labels).")]
    public TextMeshProUGUI[] warningMessageTexts;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        EnsureCanvasGroup();
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
    /// Blocks interactions for this station and displays a phase warning overlay message across assigned text components.
    /// </summary>
    /// <param name="message">The warning text explaining why the station is restricted.</param>
    public void Block(string message)
    {
        EnsureCanvasGroup();

        if (gameObject != null && !gameObject.activeSelf)
            gameObject.SetActive(true);

        // Update single text component if assigned
        if (warningMessageText != null)
        {
            warningMessageText.text = message;
        }

        // Update array of text components if assigned (e.g., multiple storage unit doors)
        if (warningMessageTexts != null && warningMessageTexts.Length > 0)
        {
            foreach (TextMeshProUGUI txt in warningMessageTexts)
            {
                if (txt != null) txt.text = message;
            }
        }

        // Enable raycast blocking and set visual opacity
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Removes station restrictions, hiding the overlay and allowing XR Ray Interactors to interact with objects behind it.
    /// </summary>
    public void Unblock()
    {
        EnsureCanvasGroup();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false; // Allows VR Ray Interactors to pass straight through to physical objects/UI behind it
        }

        if (gameObject != null && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    #endregion
}