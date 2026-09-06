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
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Blocks interaction and displays the phase warning message across all assigned texts.
    /// </summary>
    public void Block(string message)
    {
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

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true; // Blocks VR controller rays from hitting objects behind it
    }

    /// <summary>
    /// Unblocks interaction for the current phase.
    /// </summary>
    public void Unblock()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}