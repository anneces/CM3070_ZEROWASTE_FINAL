using UnityEngine;

public class PhaseTransitionUI : MonoBehaviour
{
    [Header("UI Panels & Buttons")]
    public GameObject nextPhaseButton;     // The main 'Next Phase' button
    public GameObject confirmationPanel;   // Container holding Text, CONFIRM, CANCEL
    public GameObject clockText;           // Specific reference to clocktxt

    [Header("Confirmation Controls")]
    public GameObject confirmButton;       // Specific reference to confirmbtn
    public GameObject cancelButton;        // Specific reference to cancelbtn
    public GameObject promptText;          // Specific reference to prompttxt

    private void Start()
    {
        // Ensure default state on start
        ResetUIState();
    }

    // Called when user clicks the initial "Next Phase" button
    public void OnNextPhaseClicked()
    {
        if (nextPhaseButton != null) nextPhaseButton.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(true);

        // Hide clock text and show prompt text alongside confirmation buttons
        if (clockText != null) clockText.SetActive(false);
        if (promptText != null) promptText.SetActive(true);
        if (confirmButton != null) confirmButton.SetActive(true);
        if (cancelButton != null) cancelButton.SetActive(true);
    }

    // Called when user clicks "CANCEL"
    public void OnCancelClicked()
    {
        ResetUIState();
    }

    // Called when user clicks "CONFIRM"
    public void OnConfirmClicked()
    {
        // Advance your game state
        if (DayPhaseManager.Instance != null)
        {
            DayPhaseManager.Instance.ConfirmNextPhase();
        }

        // Return UI to normal state for the next phase
        ResetUIState();
    }

    private void ResetUIState()
    {
        if (nextPhaseButton != null) nextPhaseButton.SetActive(true);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // Show clock text and hide prompt elements
        if (clockText != null) clockText.SetActive(true);
        if (promptText != null) promptText.SetActive(false);
        if (confirmButton != null) confirmButton.SetActive(false);
        if (cancelButton != null) cancelButton.SetActive(false);
    }
}