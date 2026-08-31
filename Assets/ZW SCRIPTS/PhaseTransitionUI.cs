using UnityEngine;

public class PhaseTransitionUI : MonoBehaviour
{
    [Header("UI Panels & Buttons")]
    public GameObject nextPhaseButton;     // The main 'Next Phase' button
    public GameObject confirmationPanel;   // Container holding Text, CONFIRM, CANCEL

    private void Start()
    {
        // Ensure default state on start
        ResetUIState();
    }

    // Called when user clicks the initial "Next Phase" button
    public void OnNextPhaseClicked()
    {
        nextPhaseButton.SetActive(false);
        confirmationPanel.SetActive(true);
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
        nextPhaseButton.SetActive(true);
        confirmationPanel.SetActive(false);
    }
}