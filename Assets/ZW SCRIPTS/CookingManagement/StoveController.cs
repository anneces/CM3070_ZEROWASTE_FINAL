using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoveController : MonoBehaviour
{
    public static StoveController Instance;

    [Header("Recipe & Spawn Settings")]
    public RecipeData activeRecipe;
    public Transform dishSpawnPoint;
    public bool isPlateOccupied = false; // Tracks if plate already has a cooked dish

    [Header("UI References")]
    public GameObject progressCanvas;
    public GameObject eatMeCanvas; // "Eat Me!" prompt UI
    public TextMeshProUGUI headerText; // Header for dish name
    public TextMeshProUGUI statusText; // Ingredients progress list

    [Header("VFX References")]
    public ParticleSystem stoveFireVFX;
    public ParticleSystem dishSpawnVFX;

    private List<string> addedIngredients = new List<string>();
    private bool isCooking = false;
    private float cookTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (progressCanvas != null) progressCanvas.SetActive(false);
        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (stoveFireVFX != null) stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (dishSpawnVFX != null) dishSpawnVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void SetActiveRecipe(RecipeData newRecipe)
    {
        // Don't start a new recipe if a dish is currently on the plate
        if (isPlateOccupied)
        {
            if (progressCanvas != null) progressCanvas.SetActive(true);
            if (headerText != null) headerText.text = "Blocked!";
            if (statusText != null) statusText.text = "Eat the current dish first!";
            return;
        }

        activeRecipe = newRecipe;
        addedIngredients.Clear();

        // Hide Eat Me prompt when setting up a new recipe
        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);

        UpdateRecipeUI();

        // Trigger stove fire VFX as soon as recipe confirmation happens
        if (stoveFireVFX != null)
        {
            stoveFireVFX.gameObject.SetActive(true);
            if (!stoveFireVFX.isPlaying)
            {
                stoveFireVFX.Play();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Block ingredient processing if plate is occupied or currently cooking
        if (isCooking || activeRecipe == null || isPlateOccupied) return;

        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            // Reject expired/spoiled ingredients using isSpoiled/isExpired check
            if (item.isSpoiled)
            {
                ShowSpoiledFoodWarning();
                return;
            }

            string id = !string.IsNullOrEmpty(item.foodName) ? item.foodName : item.gameObject.name;

            foreach (var req in activeRecipe.ingredients)
            {
                if (req.foodPrefab != null && req.foodPrefab.foodName == id)
                {
                    int maxNeeded = req.requiredAmount;
                    int currentCount = addedIngredients.FindAll(x => x == id).Count;

                    if (currentCount < maxNeeded)
                    {
                        // Mark as cooked before destroying to preserve item state consistency
                        item.isCooked = true;

                        addedIngredients.Add(id);
                        AudioManager.Instance?.PlaySFX(AudioManager.Instance.ingredientDropClip);
                        Destroy(item.gameObject);

                        UpdateRecipeUI();
                        CheckRecipeCompletion();
                        break;
                    }
                }
            }
        }
    }

    private void ShowSpoiledFoodWarning()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);
        if (headerText != null) headerText.text = "Warning!";
        if (statusText != null) statusText.text = "Please do not put spoiled food inside!";
    }

    private void UpdateRecipeUI()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);

        if (activeRecipe != null)
        {
            // Set Header Text (Dish Name)
            if (headerText != null)
            {
                headerText.text = activeRecipe.recipeName;
            }

            // Set Progress Text (Individual ingredient counts e.g. Carrot 0/2)
            if (statusText != null)
            {
                string progressList = "";
                foreach (var req in activeRecipe.ingredients)
                {
                    if (req.foodPrefab != null)
                    {
                        string id = req.foodPrefab.foodName;
                        int currentCount = addedIngredients.FindAll(x => x == id).Count;
                        int maxNeeded = req.requiredAmount;

                        progressList += $"{id} {currentCount}/{maxNeeded}\n";
                    }
                }
                statusText.text = progressList.TrimEnd();
            }
        }
    }

    private void CheckRecipeCompletion()
    {
        int totalRequiredCount = GetTotalRequiredIngredientsCount();
        if (addedIngredients.Count >= totalRequiredCount && totalRequiredCount > 0)
        {
            StartCoroutine(StartCookingProcess());
        }
    }

    private System.Collections.IEnumerator StartCookingProcess()
    {
        isCooking = true;
        cookTimer = 0f;

        float duration = 5f;

        while (cookTimer < duration)
        {
            cookTimer += Time.deltaTime;
            yield return null;
        }

        SpawnDish();
    }

    private void SpawnDish()
    {
        // Stop stove fire VFX when food spawns
        if (stoveFireVFX != null)
        {
            stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // Trigger cloud / poof VFX when progress finishes and food spawns
        if (dishSpawnVFX != null)
        {
            dishSpawnVFX.gameObject.SetActive(true);
            dishSpawnVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dishSpawnVFX.Play();
        }

        if (activeRecipe.cookedDishPrefab != null && dishSpawnPoint != null)
        {
            Instantiate(activeRecipe.cookedDishPrefab, dishSpawnPoint.position, dishSpawnPoint.rotation);
            isPlateOccupied = true; // Set plate occupied status when spawned

            // Track completed dish count for DayPhaseManager evaluation
            int currentCooked = PlayerPrefs.GetInt("DishesCooked", 0);
            PlayerPrefs.SetInt("DishesCooked", currentCooked + 1);
            PlayerPrefs.Save();

            // Show "Eat Me!" prompt once food is ready on the plate
            if (eatMeCanvas != null)
            {
                eatMeCanvas.SetActive(true);
            }
        }

        addedIngredients.Clear();
        isCooking = false;
        if (progressCanvas != null) progressCanvas.SetActive(false);
    }

    // Helper method to call when the player consumes the dish
    public void ClearPlate()
    {
        isPlateOccupied = false;
        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
    }

    private int GetTotalRequiredIngredientsCount()
    {
        if (activeRecipe == null || activeRecipe.ingredients == null) return 0;

        int total = 0;
        foreach (var req in activeRecipe.ingredients)
        {
            total += req.requiredAmount;
        }
        return total;
    }
}