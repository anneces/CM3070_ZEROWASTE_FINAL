using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class PhaseInteractionBlocker : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup canvasGroup;

    [Tooltip("Single text component (optional if using array below).")]
    public TextMeshProUGUI warningMessageText;

    [Tooltip("Multiple text components (e.g., Fridge, Pantry, Freezer labels).")]
    public TextMeshProUGUI[] warningMessageTexts;

    private void Awake()
    {
        EnsureCanvasGroup();
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Blocks interaction and displays the phase warning message across all assigned texts.
    /// </summary>
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

        // Update array of text components if assigned
        if (warningMessageTexts != null && warningMessageTexts.Length > 0)
        {
            foreach (TextMeshProUGUI txt in warningMessageTexts)
            {
                if (txt != null) txt.text = message;
            }
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Unblocks interaction for the current phase, explicitly allowing XR raycasts to pass through.
    /// </summary>
    public void Unblock()
    {
        EnsureCanvasGroup();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false; // Ensures VR Ray Interactors pass straight through to physical objects/UI behind it
        }

        gameObject.SetActive(false);
    }
}