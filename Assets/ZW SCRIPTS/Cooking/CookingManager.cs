using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance;

    [Header("Recipes Data")]
    public List<RecipeData> allRecipes;
    private int currentRecipeIndex = 0;
    private RecipeData selectedRecipe;

    [Header("UI References - Recipe Book")]
    public GameObject recipeBookCanvas;
    public TextMeshProUGUI bookRecipeTitleText;
    public TextMeshProUGUI bookIngredientsText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (recipeBookCanvas != null) recipeBookCanvas.SetActive(false);
        UpdateBookUI();
    }

    public void OpenRecipeBook()
    {
        if (recipeBookCanvas != null) recipeBookCanvas.SetActive(true);
        UpdateBookUI();
    }

    public void NextRecipe()
    {
        if (allRecipes == null || allRecipes.Count == 0) return;
        currentRecipeIndex = (currentRecipeIndex + 1) % allRecipes.Count;
        UpdateBookUI();
        AudioManager.Instance?.PlayUIClick();
    }

    public void PreviousRecipe()
    {
        if (allRecipes == null || allRecipes.Count == 0) return;
        currentRecipeIndex--;
        if (currentRecipeIndex < 0) currentRecipeIndex = allRecipes.Count - 1;
        UpdateBookUI();
        AudioManager.Instance?.PlayUIClick();
    }

    private void UpdateBookUI()
    {
        if (allRecipes == null || allRecipes.Count == 0) return;
        selectedRecipe = allRecipes[currentRecipeIndex];

        if (bookRecipeTitleText != null) bookRecipeTitleText.text = selectedRecipe.recipeName;

        if (bookIngredientsText != null)
        {
            string list = "";
            foreach (var req in selectedRecipe.ingredients)
            {
                if (req.foodPrefab != null)
                {
                    string ingredientName = req.foodPrefab.foodName;
                    list += $"• {ingredientName} x{req.requiredAmount}\n";
                }
            }
            bookIngredientsText.text = list;
        }
    }

    public void ConfirmRecipeSelection()
    {
        if (selectedRecipe == null) return;

        // Route selection to StoveController
        StoveController.Instance?.SetActiveRecipe(selectedRecipe);

        if (recipeBookCanvas != null) recipeBookCanvas.SetActive(false);
        AudioManager.Instance?.PlayUIClick();
    }
}