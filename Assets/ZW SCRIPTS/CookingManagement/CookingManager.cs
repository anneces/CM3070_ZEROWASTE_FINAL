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
        // Reset dishes cooked count on new run start
        totalDishesCooked = 0;
        PlayerPrefs.SetInt("TotalDishesCooked", 0);
        PlayerPrefs.Save();
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