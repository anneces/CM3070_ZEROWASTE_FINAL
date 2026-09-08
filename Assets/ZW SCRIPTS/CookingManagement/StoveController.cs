using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoveController : MonoBehaviour
{
    public static StoveController Instance;

    [Header("Recipe & Spawn Settings")]
    public RecipeData activeRecipe;
    public Transform dishSpawnPoint;
    public Transform[] ingredientRespawnPoints; // Drag your SpawnPoint_1, SpawnPoint_2, SpawnPoint_3 here!
    public bool isPlateOccupied = false; // Tracks if plate already has a cooked dish

    [Header("UI References")]
    public GameObject progressCanvas;
    public GameObject eatMeCanvas; // "Eat Me!" prompt UI
    public GameObject resetButton; // Drag your Reset UI Button here
    public TextMeshProUGUI headerText; // Header for dish name
    public TextMeshProUGUI statusText; // Ingredients progress list

    [Header("VFX References")]
    public ParticleSystem stoveFireVFX;
    public ParticleSystem dishSpawnVFX;

    private List<string> addedIngredients = new List<string>();
    private List<FoodItem> consumedIngredientPrefabs = new List<FoodItem>(); // Tracks consumed prefabs for respawning
    private bool isCooking = false;
    private float cookTimer = 0f;
    private GameObject spawnedDish; // Reference to track the instantiated cooked dish

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (progressCanvas != null) progressCanvas.SetActive(false);
        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
        if (stoveFireVFX != null) stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (dishSpawnVFX != null) dishSpawnVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        // Auto-detect when the cooked dish is destroyed/eaten and clear the plate UI state
        if (isPlateOccupied && spawnedDish == null)
        {
            ClearPlate();
        }
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
        consumedIngredientPrefabs.Clear();

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
                        // Save reference to prefab before destroying item instance
                        consumedIngredientPrefabs.Add(req.foodPrefab);

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

    /// <summary>
    /// Resets current cooking progress, respawns added ingredients back at spawn points,
    /// and resets the stove state.
    /// </summary>
    public void ResetStove()
    {
        if (isCooking) return; // Prevent reset mid-cook routine

        // Respawn consumed ingredients across assigned spawn points
        for (int i = 0; i < consumedIngredientPrefabs.Count; i++)
        {
            FoodItem prefab = consumedIngredientPrefabs[i];
            if (prefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                Quaternion spawnRot = Quaternion.identity;

                if (ingredientRespawnPoints != null && ingredientRespawnPoints.Length > 0)
                {
                    // Pick matching index, or wrap around if ingredients exceed available spawn points
                    Transform targetSpawn = ingredientRespawnPoints[i % ingredientRespawnPoints.Length];
                    if (targetSpawn != null)
                    {
                        spawnPos = targetSpawn.position;
                        spawnRot = targetSpawn.rotation;
                    }
                }

                Instantiate(prefab, spawnPos, spawnRot);
            }
        }

        // Clear tracked ingredient data
        addedIngredients.Clear();
        consumedIngredientPrefabs.Clear();

        // Turn off fire VFX if resetting before cooking
        if (stoveFireVFX != null)
        {
            stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        UpdateRecipeUI();
        AudioManager.Instance?.PlayUIClick();
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

            // Display reset button only when at least one ingredient has been added and not cooking
            if (resetButton != null)
            {
                resetButton.SetActive(addedIngredients.Count > 0 && !isCooking);
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
        if (resetButton != null) resetButton.SetActive(false);
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
            spawnedDish = Instantiate(activeRecipe.cookedDishPrefab, dishSpawnPoint.position, dishSpawnPoint.rotation);
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
        consumedIngredientPrefabs.Clear();
        isCooking = false;
        if (progressCanvas != null) progressCanvas.SetActive(false);
    }

    /// <summary>
    /// Helper method to call when the player consumes the dish.
    /// Resets stove state variables while keeping the Recipe Book canvas visible.
    /// </summary>
    public void ClearPlate()
    {
        isPlateOccupied = false;
        spawnedDish = null;
        activeRecipe = null; // Reset active recipe reference

        if (eatMeCanvas != null)
            eatMeCanvas.SetActive(false);

        if (progressCanvas != null)
            progressCanvas.SetActive(false);

        if (resetButton != null)
            resetButton.SetActive(false);

        // Recipe Book canvas is no longer closed here and stays enabled throughout gameplay
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