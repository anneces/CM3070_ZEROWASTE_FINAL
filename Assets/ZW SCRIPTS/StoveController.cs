using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoveController : MonoBehaviour
{
    [Header("Recipe Settings")]
    public RecipeData activeRecipe;
    public Transform dishSpawnPoint;

    [Header("UI References")]
    public GameObject progressCanvas;
    public Image progressBar;
    public TextMeshProUGUI statusText;

    private List<string> addedIngredients = new List<string>();
    private bool isCooking = false;
    private float cookTimer = 0f;

    private void Start()
    {
        if (progressCanvas != null) progressCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCooking || activeRecipe == null) return;

        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            string id = !string.IsNullOrEmpty(item.foodName) ? item.foodName : item.gameObject.name;

            // Check if dropped item matches any ingredient requirement in the new RecipeData format
            foreach (var req in activeRecipe.ingredients)
            {
                if (req.foodPrefab != null && req.foodPrefab.foodName == id)
                {
                    // Calculate total quantity required for this specific ingredient
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
        if (statusText != null && activeRecipe != null)
        {
            int totalRequiredCount = GetTotalRequiredIngredientsCount();
            statusText.text = $"{activeRecipe.recipeName}: {addedIngredients.Count}/{totalRequiredCount}";
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

        // Uses a 5-second default timer fallback if cookDuration was removed from RecipeData
        float duration = 5f;

        while (cookTimer < duration)
        {
            cookTimer += Time.deltaTime;
            if (progressBar != null) progressBar.fillAmount = cookTimer / duration;
            yield return null;
        }

        SpawnDish();
    }

    private void SpawnDish()
    {
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