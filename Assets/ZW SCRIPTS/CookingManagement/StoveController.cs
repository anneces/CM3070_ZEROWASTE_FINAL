using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoveController : MonoBehaviour
{
    public static StoveController Instance;

    [Header("Recipe & Spawn Settings")]
    public RecipeData activeRecipe;
    public Transform dishSpawnPoint;
    public Transform[] ingredientRespawnPoints;
    public bool isPlateOccupied = false;

    [Header("UI References")]
    public GameObject progressCanvas;
    public GameObject eatMeCanvas;
    public GameObject resetButton;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI statusText;

    [Header("UI Default Messages")]
    [TextArea(2, 4)]
    public string defaultInstructionMessage = "Select a recipe and Click on the confirm button to begin cooking";

    [Header("VFX References")]
    public ParticleSystem stoveFireVFX;
    public ParticleSystem dishSpawnVFX;

    private struct ConsumedIngredientData
    {
        public FoodItem prefab;
        public float savedFreshnessDays;
    }

    private List<string> addedIngredients = new List<string>();
    private List<ConsumedIngredientData> consumedIngredientsData = new List<ConsumedIngredientData>();
    private bool isCooking = false;
    private float cookTimer = 0f;
    private GameObject spawnedDish;

    /// <summary>
    /// Sets up the Singleton instance for global access by stove triggers and recipe books.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Initializes initial stove UI states, stops active fire/smoke VFX, locates fallback ingredient
    /// respawn transforms, and displays default setup instructions.
    /// </summary>
    private void Start()
    {
        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
        if (stoveFireVFX != null) stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (dishSpawnVFX != null) dishSpawnVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (ingredientRespawnPoints == null || ingredientRespawnPoints.Length == 0 || ingredientRespawnPoints[0] == null)
        {
            FindCounterSpawnPoints();
        }

        // Show default instruction message on startup
        ShowDefaultInstruction();
    }

    /// <summary>
    /// Constantly monitors stove plate state—if a cooked dish was consumed or deleted externally,
    /// it triggers ClearPlate() to unblock the stove for the next meal.
    /// </summary>
    private void Update()
    {
        if (isPlateOccupied && spawnedDish == null)
        {
            ClearPlate();
        }
    }

    /// <summary>
    /// Automatic scene recovery fallback that searches for counter spawn points by name if references were lost in the Inspector.
    /// </summary>
    private void FindCounterSpawnPoints()
    {
        ingredientRespawnPoints = new Transform[3];

        GameObject sp1 = GameObject.Find("SpawnPoint_1");
        GameObject sp2 = GameObject.Find("SpawnPoint_2");
        GameObject sp3 = GameObject.Find("SpawnPoint_3");

        if (sp1 != null) ingredientRespawnPoints[0] = sp1.transform;
        if (sp2 != null) ingredientRespawnPoints[1] = sp2.transform;
        if (sp3 != null) ingredientRespawnPoints[2] = sp3.transform;
    }

    /// <summary>
    /// Resets stove header and body UI text to prompt the player to choose a recipe from the recipe book.
    /// </summary>
    public void ShowDefaultInstruction()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);
        if (headerText != null) headerText.text = "Stove";
        if (statusText != null) statusText.text = defaultInstructionMessage;
    }

    /// <summary>
    /// Binds a chosen RecipeData instance from the RecipeBook to the stove. Prevents selection if an unconsumed meal 
    /// occupies the plate, resets ingredient lists, updates HUD UI, and plays cooking sizzle audio with fire VFX.
    /// </summary>
    public void SetActiveRecipe(RecipeData newRecipe)
    {
        if (isPlateOccupied)
        {
            if (progressCanvas != null) progressCanvas.SetActive(true);
            if (headerText != null) headerText.text = "Blocked!";
            if (statusText != null) statusText.text = "Eat the current dish first!";
            return;
        }

        activeRecipe = newRecipe;
        addedIngredients.Clear();
        consumedIngredientsData.Clear();

        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);

        UpdateRecipeUI();

        if (stoveFireVFX != null)
        {
            stoveFireVFX.gameObject.SetActive(true);
            if (!stoveFireVFX.isPlaying)
            {
                stoveFireVFX.Play();
            }
        }

        AudioManager.Instance?.StartCookingSizzle();
    }

    /// <summary>
    /// Detects physical VR food ingredients entering the stove's trigger volume. Rejects spoiled food with a warning,
    /// validates required ingredients against active recipe requirements, updates ingredient counts, saves freshness stats, and triggers cooking checks.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isCooking || activeRecipe == null || isPlateOccupied) return;

        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            // Reject spoiled food to reinforce ZeroWaste educational goals
            if (item.isSpoiled)
            {
                ShowSpoiledFoodWarning();

                Vector3 targetSpawnPos = transform.position + Vector3.up * 0.5f;
                Quaternion targetSpawnRot = Quaternion.identity;

                if (ingredientRespawnPoints != null && ingredientRespawnPoints.Length > 0)
                {
                    Transform spawnPoint = ingredientRespawnPoints[0];
                    if (spawnPoint != null)
                    {
                        targetSpawnPos = spawnPoint.position;
                        targetSpawnRot = spawnPoint.rotation;
                    }
                }

                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                item.transform.position = targetSpawnPos;
                item.transform.rotation = targetSpawnRot;
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
                        // Record consumed food state for potential stove resets
                        consumedIngredientsData.Add(new ConsumedIngredientData
                        {
                            prefab = req.foodPrefab,
                            savedFreshnessDays = item.currentFreshnessDays
                        });

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
    /// Cancels ingredient collection, restores pre-added raw food items back onto the kitchen counter 
    /// with their exact original freshness levels retained, and stops stove audio/VFX.
    /// </summary>
    public void ResetStove()
    {
        if (isCooking) return;

        if (ingredientRespawnPoints == null || ingredientRespawnPoints.Length == 0 || ingredientRespawnPoints[0] == null)
        {
            FindCounterSpawnPoints();
        }

        AudioManager.Instance?.StopCookingSizzle();

        for (int i = 0; i < consumedIngredientsData.Count; i++)
        {
            ConsumedIngredientData data = consumedIngredientsData[i];
            if (data.prefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                Quaternion spawnRot = Quaternion.identity;

                if (ingredientRespawnPoints != null && ingredientRespawnPoints.Length > 0)
                {
                    Transform targetSpawn = ingredientRespawnPoints[i % ingredientRespawnPoints.Length];
                    if (targetSpawn != null)
                    {
                        spawnPos = targetSpawn.position;
                        spawnRot = targetSpawn.rotation;
                    }
                }

                GameObject spawnedObj = Instantiate(data.prefab.gameObject, spawnPos, spawnRot);
                FoodItem spawnedItem = spawnedObj.GetComponent<FoodItem>();
                if (spawnedItem != null)
                {
                    spawnedItem.SetFreshnessDays(data.savedFreshnessDays);
                    spawnedItem.isCooked = false;
                }
            }
        }

        addedIngredients.Clear();
        consumedIngredientsData.Clear();

        if (stoveFireVFX != null)
        {
            stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (activeRecipe != null)
        {
            UpdateRecipeUI();
        }
        else
        {
            ShowDefaultInstruction();
        }

        AudioManager.Instance?.PlayUIClick();
    }

    /// <summary>
    /// Called by DayPhaseManager when shifting to new days or phases (e.g. Morning). 
    /// Fully clears active dishes, recipes, and UI alerts to return stove to pristine default state.
    /// </summary>
    public void ResetStoveToDefault()
    {
        isCooking = false;
        isPlateOccupied = false;
        activeRecipe = null;
        addedIngredients.Clear();
        consumedIngredientsData.Clear();

        if (spawnedDish != null)
        {
            Destroy(spawnedDish);
            spawnedDish = null;
        }

        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);

        if (stoveFireVFX != null)
        {
            stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ShowDefaultInstruction();
    }

    /// <summary>
    /// Displays an alert on the stove's UI panel and plays an audio warning when a player attempts
    /// to drop rotten/spoiled ingredients into the pot.
    /// </summary>
    private void ShowSpoiledFoodWarning()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);
        if (headerText != null) headerText.text = "Warning!";
        if (statusText != null) statusText.text = "Do not add spoiled food inside!";

        AudioManager.Instance?.PlayAlert();
    }

    /// <summary>
    /// Updates the text screen mounted on the stove with current ingredient counts (e.g., "Tomato 1/2"),
    /// giving real-time feedback on remaining items needed.
    /// </summary>
    private void UpdateRecipeUI()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);

        if (activeRecipe != null)
        {
            if (headerText != null)
            {
                headerText.text = activeRecipe.recipeName;
            }

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

            if (resetButton != null)
            {
                resetButton.SetActive(addedIngredients.Count > 0 && !isCooking);
            }
        }
        else
        {
            ShowDefaultInstruction();
        }
    }

    /// <summary>
    /// Evaluates whether all required quantities for the active recipe have been dropped into the stove.
    /// If complete, triggers the timed cooking process coroutine.
    /// </summary>
    private void CheckRecipeCompletion()
    {
        int totalRequiredCount = GetTotalRequiredIngredientsCount();
        if (addedIngredients.Count >= totalRequiredCount && totalRequiredCount > 0)
        {
            StartCoroutine(StartCookingProcess());
        }
    }

    /// <summary>
    /// Simulates a timed cooking duration (5 seconds) with sizzle audio, locking ingredient entry until complete.
    /// </summary>
    private System.Collections.IEnumerator StartCookingProcess()
    {
        isCooking = true;
        if (resetButton != null) resetButton.SetActive(false);
        cookTimer = 0f;

        AudioManager.Instance?.StartCookingSizzle();

        float duration = 5f;

        while (cookTimer < duration)
        {
            cookTimer += Time.deltaTime;
            yield return null;
        }

        SpawnDish();
    }

    /// <summary>
    /// Handles final meal creation—instantiates the cooked dish prefab on the stove plate,
    /// triggers poof particle/VFX, logs cooked metrics via CookingManager, and prompts the user with "Eat Me" UI.
    /// </summary>
    private void SpawnDish()
    {
        AudioManager.Instance?.StopCookingSizzle();
        AudioManager.Instance?.PlayPoofCloud();

        if (stoveFireVFX != null)
        {
            stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (dishSpawnVFX != null)
        {
            dishSpawnVFX.gameObject.SetActive(true);
            dishSpawnVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dishSpawnVFX.Play();
        }

        if (activeRecipe.cookedDishPrefab != null && dishSpawnPoint != null)
        {
            spawnedDish = Instantiate(activeRecipe.cookedDishPrefab, dishSpawnPoint.position, dishSpawnPoint.rotation);
            isPlateOccupied = true;

            // Notify CookingManager to increment and save TotalDishesCooked
            if (CookingManager.Instance != null)
            {
                CookingManager.Instance.RegisterDishCooked();
            }
            else
            {
                // Fallback direct save if CookingManager instance isn't in scene
                int currentCooked = PlayerPrefs.GetInt("TotalDishesCooked", 0) + 1;
                PlayerPrefs.SetInt("TotalDishesCooked", currentCooked);
                PlayerPrefs.Save();
            }

            if (eatMeCanvas != null)
            {
                eatMeCanvas.SetActive(true);
            }
        }

        addedIngredients.Clear();
        consumedIngredientsData.Clear();
        isCooking = false;

        ShowDefaultInstruction();
    }

    /// <summary>
    /// Resets stove occupancy flags when a cooked dish is eaten or removed, clearing UI popups
    /// and enabling new recipe selection.
    /// </summary>
    public void ClearPlate()
    {
        isPlateOccupied = false;
        spawnedDish = null;
        activeRecipe = null;

        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);

        ShowDefaultInstruction();
    }

    /// <summary>
    /// Helper function that calculates the total sum of all individual ingredient amounts required for the active recipe.
    /// </summary>
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