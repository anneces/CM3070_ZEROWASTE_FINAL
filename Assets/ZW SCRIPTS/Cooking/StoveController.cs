using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoveController : MonoBehaviour
{
    public static StoveController Instance;

    [Header("Recipe & Spawn Settings")]
    public RecipeData activeRecipe;
    public Transform dishSpawnPoint;

    [Header("UI References")]
    public GameObject progressCanvas;
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
        if (stoveFireVFX != null) stoveFireVFX.Stop();
        if (dishSpawnVFX != null) dishSpawnVFX.Stop();
    }

    public void SetActiveRecipe(RecipeData newRecipe)
    {
        activeRecipe = newRecipe;
        addedIngredients.Clear();
        UpdateRecipeUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCooking || activeRecipe == null) return;

        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            string id = !string.IsNullOrEmpty(item.foodName) ? item.foodName : item.gameObject.name;

            foreach (var req in activeRecipe.ingredients)
            {
                if (req.foodPrefab != null && req.foodPrefab.foodName == id)
                {
                    int maxNeeded = req.requiredAmount;
                    int currentCount = addedIngredients.FindAll(x => x == id).Count;

                    if (currentCount < maxNeeded)
                    {
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

        if (stoveFireVFX != null && !stoveFireVFX.isPlaying) stoveFireVFX.Play();

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
        if (stoveFireVFX != null) stoveFireVFX.Stop();
        if (dishSpawnVFX != null) dishSpawnVFX.Play();

        if (activeRecipe.cookedDishPrefab != null && dishSpawnPoint != null)
        {
            Instantiate(activeRecipe.cookedDishPrefab, dishSpawnPoint.position, dishSpawnPoint.rotation);
        }

        addedIngredients.Clear();
        isCooking = false;
        if (progressCanvas != null) progressCanvas.SetActive(false);
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