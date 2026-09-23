using UnityEngine;

/// <summary>
/// Singleton manager responsible for evaluating day phases (Morning, Afternoon, Evening) 
/// and updating station-level <see cref="PhaseInteractionBlocker"/> components across the kitchen.
/// </summary>
public class PhaseWarningManager : MonoBehaviour
{
    public static PhaseWarningManager Instance { get; private set; }

    [Header("Station Blockers")]
    [Tooltip("Blocker overlay on the Shopping Tablet station.")]
    public PhaseInteractionBlocker tabletBlocker;

    [Tooltip("Blocker overlay on the Food Storage units (Fridge, Freezer, Pantry).")]
    public PhaseInteractionBlocker storageUnitsBlocker;

    [Tooltip("Blocker overlay on the TV display screen.")]
    public PhaseInteractionBlocker tvDisplayBlocker;

    [Tooltip("Blocker overlay on the Cooking Stove station.")]
    public PhaseInteractionBlocker cookingStoveBlocker;

    [Header("Warning Messages")]
    public string morningMessage = "Procurement Phase! Go to the tablet and buy food";
    public string afternoonMessage = "Sorting Phase! Go sort the food into the storage zones";
    public string eveningMessage = "Cooking Phase! Go to the cooking zone to cook food";

    #region Unity Lifecycle Methods

    private void Awake()
    {
        // Enforce Singleton instance pattern
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    #endregion

    #region Phase Restriction Management

    /// <summary>
    /// Updates station blocking state and warning text based on the active <see cref="DayPhase"/>.
    /// </summary>
    /// <param name="currentPhase">The active phase of the day.</param>
    public void UpdatePhaseRestrictions(DayPhase currentPhase)
    {
        switch (currentPhase)
        {
            case DayPhase.Morning:
                // Morning (Procurement): Tablet active | Storage & Stove blocked | TV accessible
                if (tabletBlocker) tabletBlocker.Unblock();
                if (storageUnitsBlocker) storageUnitsBlocker.Block(morningMessage);
                if (tvDisplayBlocker) tvDisplayBlocker.Unblock();
                if (cookingStoveBlocker) cookingStoveBlocker.Block(morningMessage);
                break;

            case DayPhase.Afternoon:
                // Afternoon (Sorting): Storage & TV active | Tablet & Stove blocked
                if (tabletBlocker) tabletBlocker.Block(afternoonMessage);
                if (storageUnitsBlocker) storageUnitsBlocker.Unblock();
                if (tvDisplayBlocker) tvDisplayBlocker.Unblock();
                if (cookingStoveBlocker) cookingStoveBlocker.Block(afternoonMessage);
                break;

            case DayPhase.Evening:
                // Evening (Cooking): Stove, Storage & TV active | Tablet blocked
                if (tabletBlocker) tabletBlocker.Block(eveningMessage);
                if (storageUnitsBlocker) storageUnitsBlocker.Unblock();
                if (tvDisplayBlocker) tvDisplayBlocker.Unblock();
                if (cookingStoveBlocker) cookingStoveBlocker.Unblock();
                break;
        }
    }

    /// <summary>
    /// Triggers an alert/warning audio feedback clip via <see cref="AudioManager"/> when an illegal action is attempted.
    /// </summary>
    public void PlayWarningAlertSound()
    {
        AudioManager.Instance?.PlayAlert();
    }

    #endregion
}