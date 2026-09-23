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

    /// <summary>
    /// Restores default UI panel visibility states on initialization.
    /// </summary>
    private void Start()
    {
        ResetUIState();
    }

    /// <summary>
    /// Hides the main phase transition button and displays the confirmation modal controls.
    /// </summary>
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

    /// <summary>
    /// Resets the UI back to default state when the user cancels phase advancement.
    /// </summary>
    public void OnCancelClicked()
    {
        ResetUIState();
    }

    /// <summary>
    /// Triggers phase confirmation logic on DayPhaseManager and restores standard UI layout.
    /// </summary>
    public void OnConfirmClicked()
    {
        if (DayPhaseManager.Instance != null)
        {
            DayPhaseManager.Instance.ConfirmNextPhase();
        }

        ResetUIState();
    }

    /// <summary>
    /// Restores default canvas visibility by showing clock/main button and hiding popup dialog elements.
    /// </summary>
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