using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShoppingTablet : MonoBehaviour
{
    [Header("Budget & Progression")]
    public float startingBudget = 50.0f;
    public float currentBudget;

    [Header("UI References")]
    public TMP_Text budgetText;
    public TMP_Text dayText;
    public Transform spawnPoint;

    [Header("Scroll View Settings")]
    public Transform contentParent;        // Drag Viewport -> Content here
    public GameObject foodItemRowPrefab;    // Drag your FoodItemRow UI prefab here

    [System.Serializable]
    public struct ShopEntry
    {
        public string itemName;
        public GameObject foodPrefab;
    }

    [Header("Shop Catalog")]
    public List<ShopEntry> availableItems;

    private void Start()
    {
        currentBudget = startingBudget;
        PopulateScrollView();
        UpdateUI();
    }

    private void PopulateScrollView()
    {
        // Clear old children if any exist
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < availableItems.Count; i++)
        {
            int index = i; // Local copy for button listener closure
            ShopEntry entry = availableItems[i];
            FoodItem foodScript = entry.foodPrefab.GetComponent<FoodItem>();

            if (foodScript == null) continue;

            // Spawn row prefab inside Content transform
            GameObject row = Instantiate(foodItemRowPrefab, contentParent);

            // Assign Text values (Assumes order: 0 = Name Text, 1 = Price Text)
            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = foodScript.foodName;
                texts[1].text = $"${foodScript.price:F2}";
            }

            // Hook up Buy Button event dynamically
            Button buyBtn = row.GetComponentInChildren<Button>();
            if (buyBtn != null)
            {
                buyBtn.onClick.AddListener(() => BuyFoodItem(index));
            }
        }
    }

    public void BuyFoodItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= availableItems.Count) return;

        GameObject prefabToSpawn = availableItems[itemIndex].foodPrefab;
        FoodItem foodScript = prefabToSpawn.GetComponent<FoodItem>();

        if (foodScript != null && currentBudget >= foodScript.price)
        {
            currentBudget -= foodScript.price;

            Vector3 randomOffset = new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
            Instantiate(prefabToSpawn, spawnPoint.position + randomOffset, Quaternion.identity);

            UpdateUI();
        }
    }

    public void OnEndDayButtonClicked()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.EndCurrentDay();
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (budgetText != null)
            budgetText.text = $"Budget: ${currentBudget:F2}";

        if (dayText != null && DayManager.Instance != null)
            dayText.text = $"Day {DayManager.Instance.currentDay} / {DayManager.Instance.maxDays}";
    }
}