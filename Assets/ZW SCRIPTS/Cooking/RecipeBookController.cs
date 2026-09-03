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

    private int currentRecipeIndex = 0;
    private RecipeData selectedRecipe;

    private void Start()
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Debug.Log("[RecipeBookController] Sparkle VFX assigned and initialized.", this);
        }
        else
        {
            Debug.LogError("[RecipeBookController] Sparkle VFX reference is MISSING in Inspector!", this);
        }

        if (recipeBookCanvas != null)
        {
            recipeBookCanvas.SetActive(false);
        }

        UpdateBookUI();
    }

    #region XR Interaction Handlers

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log($"[RecipeBookController] Hover Enter triggered by Interactor: {args.interactorObject?.transform.name}", this);

        if (sparkleVFX != null)
        {
            sparkleVFX.Play();
            Debug.Log("[RecipeBookController] Sparkle VFX playing.", this);
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        Debug.Log($"[RecipeBookController] Hover Exit triggered by Interactor: {args.interactorObject?.transform.name}", this);

        if (sparkleVFX != null)
        {
            sparkleVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Debug.Log("[RecipeBookController] Sparkle VFX stopped.", this);
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
        if (recipeBookCanvas != null) recipeBookCanvas.SetActive(true);
        UpdateBookUI();
    }

    public void CloseRecipeBook()
    {
        if (recipeBookCanvas != null) recipeBookCanvas.SetActive(false);
        AudioManager.Instance?.PlayUIClick();
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

        // Route selected recipe to StoveController
        StoveController.Instance?.SetActiveRecipe(selectedRecipe);

        CloseRecipeBook();
    }

    private void UpdateBookUI()
    {
        List<RecipeData> recipes = CookingManager.Instance?.allRecipes;
        if (recipes == null || recipes.Count == 0) return;

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