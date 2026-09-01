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
            // Maps directly to foodName (e.g. "Apple")
            string id = !string.IsNullOrEmpty(item.foodName) ? item.foodName : item.gameObject.name;

            if (activeRecipe.requiredIngredientIDs.Contains(id))
            {
                addedIngredients.Add(id);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.ingredientDropClip);
                Destroy(item.gameObject);

                UpdateRecipeUI();
                CheckRecipeCompletion();
            }
        }
    }

    private void UpdateRecipeUI()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);
        if (statusText != null)
            statusText.text = $"{activeRecipe.recipeName}: {addedIngredients.Count}/{activeRecipe.requiredIngredientIDs.Count}";
    }

    private void CheckRecipeCompletion()
    {
        if (addedIngredients.Count >= activeRecipe.requiredIngredientIDs.Count)
        {
            StartCoroutine(StartCookingProcess());
        }
    }

    private System.Collections.IEnumerator StartCookingProcess()
    {
        isCooking = true;
        cookTimer = 0f;

        while (cookTimer < activeRecipe.cookDuration)
        {
            cookTimer += Time.deltaTime;
            if (progressBar != null) progressBar.fillAmount = cookTimer / activeRecipe.cookDuration;
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
}