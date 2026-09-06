using UnityEngine;

public class PhaseWarningManager : MonoBehaviour
{
    public static PhaseWarningManager Instance { get; private set; }

    [Header("Station Blockers")]
    public PhaseInteractionBlocker tabletBlocker;
    public PhaseInteractionBlocker storageUnitsBlocker;
    public PhaseInteractionBlocker tvDisplayBlocker;
    public PhaseInteractionBlocker cookingStoveBlocker;

    [Header("Warning Messages")]
    public string morningMessage = "Procurement Phase! Go to the tablet and buy food";
    public string afternoonMessage = "Sorting Phase! Go sort the food into the storage zones";
    public string eveningMessage = "Cooking Phase! Go to the cooking zone to cook food";

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    /// <summary>
    /// Updates station blocking according to the current phase.
    /// </summary>
    public void UpdatePhaseRestrictions(DayManager.DayPhase currentPhase)
    {
        switch (currentPhase)
        {
            case DayManager.DayPhase.Morning:
                // Morning: Tablet active | Storage, TV, Stove blocked
                if (tabletBlocker) tabletBlocker.Unblock();
                if (storageUnitsBlocker) storageUnitsBlocker.Block(morningMessage);
                if (tvDisplayBlocker) tvDisplayBlocker.Block(morningMessage);
                if (cookingStoveBlocker) cookingStoveBlocker.Block(morningMessage);
                break;

            case DayManager.DayPhase.Afternoon:
                // Afternoon: Storage & TV active | Tablet, Stove blocked
                if (tabletBlocker) tabletBlocker.Block(afternoonMessage);
                if (storageUnitsBlocker) storageUnitsBlocker.Unblock();
                if (tvDisplayBlocker) tvDisplayBlocker.Unblock();
                if (cookingStoveBlocker) cookingStoveBlocker.Block(afternoonMessage);
                break;

            case DayManager.DayPhase.Evening:
                // Evening: Stove active | Tablet, Storage, TV blocked
                if (tabletBlocker) tabletBlocker.Block(eveningMessage);
                if (storageUnitsBlocker) storageUnitsBlocker.Block(eveningMessage);
                if (tvDisplayBlocker) tvDisplayBlocker.Block(eveningMessage);
                if (cookingStoveBlocker) cookingStoveBlocker.Unblock();
                break;
        }
    }
}