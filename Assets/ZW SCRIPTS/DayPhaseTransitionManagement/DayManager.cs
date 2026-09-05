using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Tracker")]
    public int currentDay = 1;
    public int maxDays = 5;

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

    /// <summary>
    /// Call this via UI Button or Event to advance to the next day.
    /// </summary>
    public void EndCurrentDay()
    {
        if (currentDay >= maxDays)
        {
            Debug.Log("End of 5-Day Simulation Reached! Loading Main Menu Summary...");

            // 1. Save performance metrics for the summary board
            float wastedMoney = TrashBinController.Instance != null ? TrashBinController.Instance.totalMoneyWasted : 0f;
            float totalCO2 = TrashBinController.Instance != null ? TrashBinController.Instance.totalCO2 : 0f;

            PlayerPrefs.SetFloat("TotalMoneyWasted", wastedMoney);
            PlayerPrefs.SetFloat("TotalCO2", totalCO2);

            // Set flag so Main Menu knows to activate the Game Summary UI Canvas
            PlayerPrefs.SetInt("ShowGameSummaryOnLoad", 1);
            PlayerPrefs.Save();

            // 2. Load Main Menu Scene
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        currentDay++;
        Debug.Log($"================ ADVANCING TO DAY {currentDay} ================");

        // Find all food items currently in the scene and trigger their phase tick (decay + shader update)
        FoodItem[] allFoodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        foreach (FoodItem item in allFoodItems)
        {
            item.OnPhaseTick();
        }
    }
}