using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShoppingTablet : MonoBehaviour
{
    // Updated category enum: Fruits & Vegetables combined
    public enum FoodCategory { FruitsAndVegetables, Protein, Grains, Others }

    [System.Serializable]
    public struct ShopEntry
    {
        public string itemName;
        public FoodCategory category;
        public GameObject foodPrefab;
    }

    [Header("Budget & Wallet Settings")]
    public float startingBudget = 50.0f;
    public float walletBalance;
    public float totalSpent = 0.0f;

    [Header("UI Header References")]
    public TMP_Text walletBalanceText;
    public TMP_Text totalSpentText;
    public TMP_Text dayText;

    [Header("Spawn Settings")]
    [Tooltip("Assign 3 spawn point transforms located above the kitchen counter.")]
    public Transform[] spawnPoints = new Transform[3];
    public float spawnHeightOffset = 0.15f;
    private int currentSpawnIndex = 0;

    [Header("Categorized Layout References")]
    public Transform mainContentParent;       // Parent for category sections (Vertical Layout)
    public GameObject categorySectionPrefab; // Prefab with Title + ScrollRect for horizontal items
    public GameObject foodCardPrefab;         // UI card prefab for food item

    [Header("Shop Catalog")]
    public List<ShopEntry> availableItems;

    private void Start()
    {
        walletBalance = startingBudget;
        PopulateCategorizedCatalog();
        UpdateUI();
    }

    private void PopulateCategorizedCatalog()
    {
        if (mainContentParent == null || categorySectionPrefab == null || foodCardPrefab == null)
        {
            Debug.LogError("ShoppingTablet: Ensure mainContentParent, categorySectionPrefab, and foodCardPrefab are assigned!");
            return;
        }

        // Clear existing category blocks
        foreach (Transform child in mainContentParent)
        {
            Destroy(child.gameObject);
        }

        // Group items by enum category
        Dictionary<FoodCategory, List<int>> categorizedIndices = new Dictionary<FoodCategory, List<int>>();
        foreach (FoodCategory cat in System.Enum.GetValues(typeof(FoodCategory)))
        {
            categorizedIndices[cat] = new List<int>();
        }

        for (int i = 0; i < availableItems.Count; i++)
        {
            categorizedIndices[availableItems[i].category].Add(i);
        }

        // Build each category section horizontally
        foreach (KeyValuePair<FoodCategory, List<int>> pair in categorizedIndices)
        {
            if (pair.Value.Count == 0) continue; // Skip empty categories

            // 1. Instantiate Category Section
            GameObject sectionObj = Instantiate(categorySectionPrefab, mainContentParent);

            // Set Category Header Title
            TMP_Text headerText = sectionObj.GetComponentInChildren<TMP_Text>();
            if (headerText != null)
            {
                // Format display name (e.g., FruitsAndVegetables -> FRUITS & VEGETABLES)
                if (pair.Key == FoodCategory.FruitsAndVegetables)
                {
                    headerText.text = "FRUITS & VEGETABLES";
                }
                else
                {
                    headerText.text = pair.Key.ToString().ToUpper();
                }
            }

            // Find horizontal content container in section prefab
            ScrollRect scrollRect = sectionObj.GetComponentInChildren<ScrollRect>();
            Transform horizontalContent = (scrollRect != null) ? scrollRect.content : sectionObj.transform;

            // 2. Instantiate Item Cards inside Category horizontal row
            foreach (int index in pair.Value)
            {
                int capturedIndex = index; // Safely capture loop index for closure

                ShopEntry entry = availableItems[capturedIndex];
                GameObject cardObj = Instantiate(foodCardPrefab, horizontalContent);

                string displayName = entry.itemName;
                float displayPrice = 0.0f;

                if (entry.foodPrefab != null)
                {
                    FoodItem foodScript = entry.foodPrefab.GetComponent<FoodItem>();
                    if (foodScript != null)
                    {
                        if (!string.IsNullOrEmpty(foodScript.foodName)) displayName = foodScript.foodName;
                        displayPrice = foodScript.price;
                    }
                }

                // Populate Card UI via FoodItemRow script or direct references
                FoodItemRow rowScript = cardObj.GetComponent<FoodItemRow>();
                if (rowScript != null)
                {
                    if (rowScript.nameText != null) rowScript.nameText.text = displayName;
                    if (rowScript.priceText != null) rowScript.priceText.text = $"${displayPrice:F2}";
                    if (rowScript.buyButton != null)
                    {
                        rowScript.buyButton.onClick.RemoveAllListeners();
                        rowScript.buyButton.onClick.AddListener(() => BuyFoodItem(capturedIndex));
                    }
                }
                else
                {
                    Button buyBtn = cardObj.GetComponent<Button>();
                    if (buyBtn == null) buyBtn = cardObj.GetComponentInChildren<Button>();

                    TMP_Text[] texts = cardObj.GetComponentsInChildren<TMP_Text>();
                    if (texts.Length >= 1) texts[0].text = displayName;
                    if (texts.Length >= 2) texts[1].text = $"${displayPrice:F2}";

                    if (buyBtn != null)
                    {
                        buyBtn.onClick.RemoveAllListeners();
                        buyBtn.onClick.AddListener(() => BuyFoodItem(capturedIndex));
                    }
                }
            }
        }
    }

    public void BuyFoodItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= availableItems.Count) return;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to ShoppingTablet!");
            return;
        }

        GameObject prefabToSpawn = availableItems[itemIndex].foodPrefab;
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"No food prefab assigned to item '{availableItems[itemIndex].itemName}' at index {itemIndex}!");
            return;
        }

        FoodItem foodScript = prefabToSpawn.GetComponent<FoodItem>();
        float price = (foodScript != null) ? foodScript.price : 0.0f;
        string name = (foodScript != null) ? foodScript.foodName : availableItems[itemIndex].itemName;

        if (walletBalance >= price)
        {
            walletBalance -= price;
            totalSpent += price;

            Transform targetPoint = spawnPoints[currentSpawnIndex];
            Vector3 spawnPosition = targetPoint.position + (Vector3.up * spawnHeightOffset);

            Instantiate(prefabToSpawn, spawnPosition, targetPoint.rotation);

            currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;

            UpdateUI();
            Debug.Log($"Purchased {name} for ${price:F2}. Spawned at Spot {currentSpawnIndex + 1}.");
        }
        else
        {
            Debug.LogWarning($"Insufficient Funds! Wallet: ${walletBalance:F2}, Item Price: ${price:F2}");
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
        if (walletBalanceText != null)
            walletBalanceText.text = $"Wallet: ${walletBalance:F2}";

        if (totalSpentText != null)
            totalSpentText.text = $"Spent: ${totalSpent:F2}";

        if (dayText != null && DayManager.Instance != null)
            dayText.text = $"Day {DayManager.Instance.currentDay} / {DayManager.Instance.maxDays}";
    }
}