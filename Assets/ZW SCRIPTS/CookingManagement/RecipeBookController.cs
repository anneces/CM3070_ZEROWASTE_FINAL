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

    /// <summary>
    /// Subscribes to time/phase transition events from DayPhaseManager to automatically update UI visibility.
    /// </summary>
    private void OnEnable()
    {
        DayPhaseManager.OnPhaseChanged += OnPhaseChangedHandler;
    }

    /// <summary>
    /// Unsubscribes from phase transition events when the script is disabled to avoid unhandled object callbacks.
    /// </summary>
    private void OnDisable()
    {
        DayPhaseManager.OnPhaseChanged -= OnPhaseChangedHandler;
    }

    /// <summary>
    /// Initializes default component states on start (clears VFX particle systems, ensures the 
    /// recipe UI canvas is active, updates button visibility, and draws the first recipe details).
    /// </summary>
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
    /// Resets recipe index back to zero and updates UI controls whenever the game shifts day phases
    /// (e.g Morning -> Evening), ensuring the confirm button only appears when cooking is allowed.
    /// </summary>
    private void OnPhaseChangedHandler()
    {
        currentRecipeIndex = 0;
        UpdateConfirmButtonVisibility();
        UpdateBookUI();
    }

    /// <summary>
    /// Enforces phase-based game rules by ensuring cooking recipes can only be confirmed during 
    /// designated phases (Evening phase).
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

    /// <summary>
    /// Provides visual affordance in VR by playing a particle sparkle effect when the user's VR ray or hand hovers over the recipe book.
    /// </summary>
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.gameObject.SetActive(true);
            sparkleVFX.Play();
        }
    }

    /// <summary>
    /// Stops the sparkle particle effect as soon as the player looks or points away from the recipe book.
    /// </summary>
    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    /// <summary>
    /// Responds to direct VR pointer clicks on the physical book object to open the selection display interface.
    /// </summary>
    public void OnBookClicked(SelectEnterEventArgs args)
    {
        Debug.Log($"[RecipeBookController] Book Clicked by Interactor: {args.interactorObject?.transform.name}", this);
        OpenRecipeBook();
    }

    #endregion

    #region UI & Recipe Functions

    /// <summary>
    /// Displays the main recipe canvas and validates that valid recipes exist in CookingManager before opening.
    /// </summary>
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

    /// <summary>
    /// Hides the recipe book UI canvas from the user's view.
    /// </summary>
    public void CloseRecipeBook()
    {
        if (recipeBookCanvas != null)
            recipeBookCanvas.SetActive(false);
    }

    /// <summary>
    /// Cycles forward through available recipes list and plays UI click audio for responsive navigation.
    /// </summary>
    public void NextRecipe()
    {
        List<RecipeData> recipes = CookingManager.Instance?.allRecipes;
        if (recipes == null || recipes.Count == 0) return;

        currentRecipeIndex = (currentRecipeIndex + 1) % recipes.Count;
        UpdateBookUI();
        AudioManager.Instance?.PlayUIClick();
    }

    /// <summary>
    /// Cycles backward through available recipes list with loop-around logic and UI click feedback.
    /// </summary>
    public void PreviousRecipe()
    {
        List<RecipeData> recipes = CookingManager.Instance?.allRecipes;
        if (recipes == null || recipes.Count == 0) return;

        currentRecipeIndex--;
        if (currentRecipeIndex < 0) currentRecipeIndex = recipes.Count - 1;

        UpdateBookUI();
        AudioManager.Instance?.PlayUIClick();
    }

    /// <summary>
    /// Sends the selected recipe scriptable object to the StoveController script so the stove 
    /// knows which ingredients to accept for cooking.
    /// </summary>
    public void ConfirmRecipeSelection()
    {
        if (selectedRecipe == null) return;

        StoveController.Instance?.SetActiveRecipe(selectedRecipe);

        // Canvas is kept open continuously after selecting/confirming recipes
        AudioManager.Instance?.PlayUIClick();
    }

    /// <summary>
    /// Formats and displays the current recipe title and required ingredient quantities onto text components.
    /// </summary>
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