using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance;

    [Header("Recipes Data")]
    public List<RecipeData> allRecipes;
    private int currentRecipeIndex = 0;
    private RecipeData selectedRecipe;
    private Dictionary<string, int> currentIngredients = new Dictionary<string, int>();

    [Header("UI References - Recipe Book")]
    public GameObject recipeBookCanvas;
    public TextMeshProUGUI bookRecipeTitleText;
    public TextMeshProUGUI bookIngredientsText;

    [Header("UI References - Stove")]
    public GameObject stoveCanvas;
    public TextMeshProUGUI stoveHeaderText;
    public TextMeshProUGUI stoveProgressText;

    [Header("Plate & Dish References")]
    public Transform plateSpawnPoint;
    public GameObject eatMeCanvas; // World-space UI canvas above plate containing 'EAT ME' button
    private GameObject currentActiveDish;

    [Header("VFX References (Cartoon FX)")]
    public ParticleSystem bookHoverVFX;
    public ParticleSystem stoveFireVFX;
    public ParticleSystem plateSpawnVFX;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (recipeBookCanvas != null) recipeBookCanvas.SetActive(false);
        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (stoveFireVFX != null) stoveFireVFX.Stop();

        UpdateBookUI();
    }

    #region Recipe Book Methods

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
        if (currentActiveDish != null)
        {
            if (stoveHeaderText != null) stoveHeaderText.text = "Eat the current dish first!";
            if (stoveProgressText != null) stoveProgressText.text = "Plate is occupied.";
            if (recipeBookCanvas != null) recipeBookCanvas.SetActive(false);
            return;
        }

        currentIngredients.Clear();
        foreach (var req in selectedRecipe.ingredients)
        {
            if (req.foodPrefab != null)
            {
                currentIngredients[req.foodPrefab.foodName] = 0;
            }
        }

        if (recipeBookCanvas != null) recipeBookCanvas.SetActive(false);
        if (stoveFireVFX != null) stoveFireVFX.Play();

        UpdateStoveUI();
        AudioManager.Instance?.PlayUIClick();
    }

    #endregion

    #region Stove & Cooking Logic

    public void OnIngredientDropped(FoodItem item)
    {
        if (currentActiveDish != null)
        {
            if (stoveHeaderText != null) stoveHeaderText.text = "Eat the current dish first!";
            return;
        }

        if (selectedRecipe == null || item == null) return;

        string droppedItemName = item.foodName;
        if (currentIngredients.ContainsKey(droppedItemName))
        {
            int maxNeeded = 0;
            foreach (var req in selectedRecipe.ingredients)
            {
                if (req.foodPrefab != null && req.foodPrefab.foodName == droppedItemName)
                {
                    maxNeeded = req.requiredAmount;
                    break;
                }
            }

            if (currentIngredients[droppedItemName] < maxNeeded)
            {
                currentIngredients[droppedItemName]++;
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.ingredientDropClip);
                Destroy(item.gameObject);

                UpdateStoveUI();
                CheckCookingCompletion();
            }
        }
    }

    private void UpdateStoveUI()
    {
        if (selectedRecipe == null) return;

        if (stoveHeaderText != null)
            stoveHeaderText.text = $"Cooking [{selectedRecipe.recipeName}] in progress!";

        string statusStr = "";
        foreach (var req in selectedRecipe.ingredients)
        {
            if (req.foodPrefab != null)
            {
                string ingredientName = req.foodPrefab.foodName;
                int current = currentIngredients.ContainsKey(ingredientName) ? currentIngredients[ingredientName] : 0;
                statusStr += $"{ingredientName} {current}/{req.requiredAmount}\n";
            }
        }

        if (stoveProgressText != null) stoveProgressText.text = statusStr;
    }

    private void CheckCookingCompletion()
    {
        foreach (var req in selectedRecipe.ingredients)
        {
            if (req.foodPrefab != null)
            {
                string ingredientName = req.foodPrefab.foodName;
                if (!currentIngredients.ContainsKey(ingredientName) || currentIngredients[ingredientName] < req.requiredAmount)
                    return;
            }
        }

        CompleteDish();
    }

    private void CompleteDish()
    {
        if (stoveFireVFX != null) stoveFireVFX.Stop();

        if (selectedRecipe.cookedDishPrefab != null && plateSpawnPoint != null)
        {
            currentActiveDish = Instantiate(selectedRecipe.cookedDishPrefab, plateSpawnPoint.position, plateSpawnPoint.rotation, plateSpawnPoint);

            if (plateSpawnVFX != null)
            {
                plateSpawnVFX.transform.position = plateSpawnPoint.position;
                plateSpawnVFX.Play();
            }
        }

        if (eatMeCanvas != null) eatMeCanvas.SetActive(true);
        if (stoveHeaderText != null) stoveHeaderText.text = "Dish Ready!";
        if (stoveProgressText != null) stoveProgressText.text = "Serve or Eat the dish.";

        selectedRecipe = null;
    }

    #endregion

    #region Plate & Dish Consumption

    public void OnEatMeButtonClicked()
    {
        if (currentActiveDish != null)
        {
            if (plateSpawnVFX != null)
            {
                plateSpawnVFX.transform.position = plateSpawnPoint.position;
                plateSpawnVFX.Play();
            }

            Destroy(currentActiveDish);
            currentActiveDish = null;

            if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
            if (stoveHeaderText != null) stoveHeaderText.text = "Stove Idle";
            if (stoveProgressText != null) stoveProgressText.text = "Select a recipe from book.";

            AudioManager.Instance?.PlayUIClick();
        }
    }

    #endregion
}