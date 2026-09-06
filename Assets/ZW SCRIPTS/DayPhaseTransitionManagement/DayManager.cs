using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    public enum DayPhase { Morning, Afternoon, Evening }

    [Header("Day & Phase Tracker")]
    public int currentDay = 1;
    public int maxDays = 5;
    public DayPhase currentPhase = DayPhase.Morning;

    [Header("Scene Transition Settings")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Apply phase restrictions for the initial phase (Morning Day 1)
        UpdatePhaseRestrictions();
    }

    /// <summary>
    /// Call this via UI Button or Event to advance to the next phase/day.
    /// </summary>
    public void EndCurrentDay()
    {
        // 1. Phase Advancement Progression
        if (currentPhase == DayPhase.Morning)
        {
            currentPhase = DayPhase.Afternoon;
            Debug.Log($"================ DAY {currentDay}: AFTERNOON PHASE ================");
            UpdatePhaseRestrictions();
            return;
        }
        else if (currentPhase == DayPhase.Afternoon)
        {
            currentPhase = DayPhase.Evening;
            Debug.Log($"================ DAY {currentDay}: EVENING PHASE ================");
            UpdatePhaseRestrictions();
            return;
        }

        // 2. Evening Phase Completed -> Check for end of Day 5 simulation
        if (currentDay >= maxDays)
        {
            Debug.Log("End of 5-Day Simulation Reached! Loading Main Menu Summary...");

            // Save performance metrics for the summary board
            float wastedMoney = TrashBinController.Instance != null ? TrashBinController.Instance.totalMoneyWasted : 0f;
            float totalCO2 = TrashBinController.Instance != null ? TrashBinController.Instance.totalCO2 : 0f;

            PlayerPrefs.SetFloat("TotalMoneyWasted", wastedMoney);
            PlayerPrefs.SetFloat("TotalCO2", totalCO2);

            // Set flag so Main Menu knows to activate the Game Summary UI Canvas
            PlayerPrefs.SetInt("ShowGameSummaryOnLoad", 1);
            PlayerPrefs.Save();

            // Load Main Menu Scene
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        // 3. Advance to the next day's Morning phase
        currentDay++;
        currentPhase = DayPhase.Morning;
        Debug.Log($"================ ADVANCING TO DAY {currentDay}: MORNING PHASE ================");

        // Find all food items currently in the scene and trigger their phase tick (decay + shader update)
        FoodItem[] allFoodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        foreach (FoodItem item in allFoodItems)
        {
            item.OnPhaseTick();
        }

        UpdatePhaseRestrictions();
    }

    private void UpdatePhaseRestrictions()
    {
        if (PhaseWarningManager.Instance != null)
        {
            PhaseWarningManager.Instance.UpdatePhaseRestrictions(currentPhase);
        }
    }
}