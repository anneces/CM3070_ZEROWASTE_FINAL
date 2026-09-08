using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class RecipeBookController : MonoBehaviour
{
    [Header("VFX Setup")]
    public ParticleSystem sparkleVFX;

    [Header("UI References")]
    public GameObject recipeBookCanvas;
    public TextMeshProUGUI bookRecipeTitleText;
    public TextMeshProUGUI bookIngredientsText;
    public GameObject confirmButton;

    private int currentRecipeIndex = 0;
    private RecipeData selectedRecipe;

    private void OnEnable()
    {
        DayPhaseManager.OnPhaseChanged += OnPhaseChangedHandler;
    }

    private void OnDisable()
    {
        DayPhaseManager.OnPhaseChanged -= OnPhaseChangedHandler;
    }

    private void Start()
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else
        {
            Debug.LogError("[RecipeBookController] Sparkle VFX reference is MISSING in Inspector!", this);
        }

        // Keep UI visible on Start
        if (recipeBookCanvas != null)
        {
            recipeBookCanvas.SetActive(true);
        }

        UpdateConfirmButtonVisibility();
        UpdateBookUI();
    }

    /// <summary>
    /// Updates confirm button visibility when phase shifts without closing the book canvas.
    /// </summary>
    private void OnPhaseChangedHandler()
    {
        currentRecipeIndex = 0;
        UpdateConfirmButtonVisibility();
        UpdateBookUI();
    }

    /// <summary>
    /// Shows the Confirm button ONLY during Evening phase.
    /// </summary>
    private void UpdateConfirmButtonVisibility()
    {
        if (confirmButton == null) return;

        if (DayPhaseManager.Instance != null)
        {
            bool isEvening = DayPhaseManager.Instance.CurrentPhase == DayPhase.Evening;
            confirmButton.SetActive(isEvening);
        }
    }

    #region XR Interaction Handlers

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.gameObject.SetActive(true);
            sparkleVFX.Play();
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void OnBookClicked(SelectEnterEventArgs args)
    {
        Debug.Log($"[RecipeBookController] Book Clicked by Interactor: {args.interactorObject?.transform.name}", this);
        OpenRecipeBook();
    }

    #endregion

    #region UI & Recipe Functions

    public void OpenRecipeBook()
    {
        if (CookingManager.Instance == null || CookingManager.Instance.allRecipes == null || CookingManager.Instance.allRecipes.Count == 0)
        {
            Debug.LogWarning("[RecipeBookController] Cannot open book: CookingManager has no active recipes!", this);
            return;
        }

        if (recipeBookCanvas != null)
            recipeBookCanvas.SetActive(true);

        UpdateBookUI();
        UpdateConfirmButtonVisibility();
    }

    public void CloseRecipeBook()
    {
        if (recipeBookCanvas != null)
            recipeBookCanvas.SetActive(false);
    }

    public void NextRecipe()
    {
        List<RecipeData> recipes = CookingManager.Instance?.allRecipes;
        if (recipes == null || recipes.Count == 0) return;

        currentRecipeIndex = (currentRecipeIndex + 1) % recipes.Count;
        UpdateBookUI();
        AudioManager.Instance?.PlayUIClick();
    }

    public void PreviousRecipe()
    {
        List<RecipeData> recipes = CookingManager.Instance?.allRecipes;
        if (recipes == null || recipes.Count == 0) return;

        currentRecipeIndex--;
        if (currentRecipeIndex < 0) currentRecipeIndex = recipes.Count - 1;

        UpdateBookUI();
        AudioManager.Instance?.PlayUIClick();
    }

    public void ConfirmRecipeSelection()
    {
        if (selectedRecipe == null) return;

        StoveController.Instance?.SetActiveRecipe(selectedRecipe);

        // Canvas is kept open continuously after selecting/confirming recipes
        AudioManager.Instance?.PlayUIClick();
    }

    private void UpdateBookUI()
    {
        List<RecipeData> recipes = CookingManager.Instance?.allRecipes;
        if (recipes == null || recipes.Count == 0) return;

        if (currentRecipeIndex < 0 || currentRecipeIndex >= recipes.Count)
            currentRecipeIndex = 0;

        selectedRecipe = recipes[currentRecipeIndex];

        if (bookRecipeTitleText != null)
            bookRecipeTitleText.text = selectedRecipe.recipeName;

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

    #endregion
}