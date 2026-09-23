using System.Collections.Generic;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance;

    [Header("Recipes Data")]
    public List<RecipeData> allRecipes = new List<RecipeData>();

    [Header("Cooked Metrics")]
    public int totalDishesCooked = 0;

    /// <summary>
    /// Implements the Singleton pattern to guarantee only one instance 
    /// of CookingManager exists in the scene so other scripts can easily query recipe data.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads the persistent dish count from PlayerPrefs upon initialization.
    /// Ensures cumulative cooking metrics are retained across different day/phase scene reloads for final grading.
    /// </summary>
    private void Start()
    {
        // Load persistent dish count across phases instead of overwriting to 0 on scene reload
        totalDishesCooked = PlayerPrefs.GetInt("TotalDishesCooked", 0);
    }

    /// <summary>
    /// Increments the global cooked dish counter and immediately saves it to disk (PlayerPrefs).
    /// Called when a dish finishes cooking so end-of-simulation summary screens accurately reflect productivity.
    /// </summary>
    public void RegisterDishCooked()
    {
        totalDishesCooked++;
        PlayerPrefs.SetInt("TotalDishesCooked", totalDishesCooked);
        PlayerPrefs.Save();
        Debug.Log($"[CookingManager] Dish cooked! Total dishes cooked: {totalDishesCooked}");
    }
}