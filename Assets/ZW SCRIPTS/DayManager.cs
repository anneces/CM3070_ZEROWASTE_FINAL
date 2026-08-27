using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Tracker")]
    public int currentDay = 1;
    public int maxDays = 5;

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

    /// <summary>
    /// Call this via UI Button or Event to advance to the next day.
    /// </summary>
    public void EndCurrentDay()
    {
        if (currentDay >= maxDays)
        {
            Debug.Log("End of 5-Day Simulation Reached!");
            // Add end-game summary screen trigger here
            return;
        }

        currentDay++;
        Debug.Log($"================ ADVANCING TO DAY {currentDay} ================");

        // Find all food items currently in the scene and degrade them by 1 day cycle
        FoodItem[] allFoodItems = FindObjectsOfType<FoodItem>();
        foreach (FoodItem item in allFoodItems)
        {
            item.AdvanceDay();
        }
    }
}