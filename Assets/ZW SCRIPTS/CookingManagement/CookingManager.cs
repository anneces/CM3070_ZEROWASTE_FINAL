using System.Collections.Generic;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance;

    [Header("Recipes Data")]
    public List<RecipeData> allRecipes = new List<RecipeData>();

    [Header("Cooked Metrics")]
    public int totalDishesCooked = 0;

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

    private void Start()
    {
        // Load persistent dish count across phases instead of overwriting to 0 on scene reload
        totalDishesCooked = PlayerPrefs.GetInt("TotalDishesCooked", 0);
    }

    /// <summary>
    /// Call this method in your StoveController/EatableDish script right when a cooked dish is spawned!
    /// </summary>
    public void RegisterDishCooked()
    {
        totalDishesCooked++;
        PlayerPrefs.SetInt("TotalDishesCooked", totalDishesCooked);
        PlayerPrefs.Save();
        Debug.Log($"[CookingManager] Dish cooked! Total dishes cooked: {totalDishesCooked}");
    }
}