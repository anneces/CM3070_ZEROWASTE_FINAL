using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShoppingTablet : MonoBehaviour
{
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

    [Header("Scroll View Settings")]
    public Transform contentParent;
    public GameObject foodItemRowPrefab;

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
        walletBalance = startingBudget;
        PopulateScrollView();
        UpdateUI();
    }

    private void PopulateScrollView()
    {
        if (contentParent == null || foodItemRowPrefab == null)
        {
            Debug.LogError("ShoppingTablet: Missing contentParent or foodItemRowPrefab references!");
            return;
        }

        // Clear existing children inside Content
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < availableItems.Count; i++)
        {
            int index = i;
            ShopEntry entry = availableItems[i];

            // 1. Instantiate the UI Row Prefab
            GameObject rowObj = Instantiate(foodItemRowPrefab, contentParent);
            FoodItemRow rowScript = rowObj.GetComponent<FoodItemRow>();

            // 2. Fetch price/name safely from 3D FoodPrefab component if available
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

            // 3. Populate via FoodItemRow component (Best Practice)
            if (rowScript != null)
            {
                if (rowScript.nameText != null) rowScript.nameText.text = displayName;
                if (rowScript.priceText != null) rowScript.priceText.text = $"${displayPrice:F2}";
                if (rowScript.buyButton != null)
                {
                    rowScript.buyButton.onClick.RemoveAllListeners();
                    rowScript.buyButton.onClick.AddListener(() => BuyFoodItem(index));
                }
            }
            // Fallback: Component search if FoodItemRow script isn't used directly
            else
            {
                Button buyBtn = rowObj.GetComponentInChildren<Button>();
                TMP_Text[] allTexts = rowObj.GetComponentsInChildren<TMP_Text>();

                List<TMP_Text> labelTexts = new List<TMP_Text>();
                foreach (TMP_Text t in allTexts)
                {
                    if (buyBtn != null && t.transform.IsChildOf(buyBtn.transform)) continue;
                    labelTexts.Add(t);
                }

                if (labelTexts.Count >= 1) labelTexts[0].text = displayName;
                if (labelTexts.Count >= 2) labelTexts[1].text = $"${displayPrice:F2}";

                if (buyBtn != null)
                {
                    buyBtn.onClick.RemoveAllListeners();
                    buyBtn.onClick.AddListener(() => BuyFoodItem(index));
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
            Debug.LogWarning($"No food prefab assigned to item at index {itemIndex}!");
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