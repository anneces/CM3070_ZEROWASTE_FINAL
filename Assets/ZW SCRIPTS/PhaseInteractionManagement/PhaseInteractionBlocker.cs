using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(CanvasGroup))]
public class PhaseInteractionBlocker : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI warningMessageText;
    public TextMeshProUGUI[] warningMessageTexts;

    [Header("XR Interaction Controls")]
    public XRBaseInteractable[] xrInteractablesToDisable;

    /// <summary>
    /// Property indicating whether this station is currently blocked.
    /// </summary>
    public bool IsBlocked { get; private set; }

    private void Awake()
    {
        EnsureCanvasGroup();
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Block(string message)
    {
        IsBlocked = true;
        EnsureCanvasGroup();

        if (gameObject != null && !gameObject.activeSelf)
            gameObject.SetActive(true);

        if (warningMessageText != null)
            warningMessageText.text = message;

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

        SetXRInteractablesState(false);
    }

    public void Unblock()
    {
        IsBlocked = false;
        EnsureCanvasGroup();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        SetXRInteractablesState(true);
    }

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
}