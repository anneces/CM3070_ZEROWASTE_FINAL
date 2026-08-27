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
    public float spawnHeightOffset = 0.15f; // Extra height so item drops onto the counter
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
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < availableItems.Count; i++)
        {
            int index = i;
            ShopEntry entry = availableItems[i];
            if (entry.foodPrefab == null) continue;

            FoodItem foodScript = entry.foodPrefab.GetComponent<FoodItem>();
            if (foodScript == null) continue;

            GameObject row = Instantiate(foodItemRowPrefab, contentParent);

            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = foodScript.foodName;
                texts[1].text = $"${foodScript.price:F2}";
            }

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
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to ShoppingTablet!");
            return;
        }

        GameObject prefabToSpawn = availableItems[itemIndex].foodPrefab;
        FoodItem foodScript = prefabToSpawn.GetComponent<FoodItem>();

        if (foodScript != null)
        {
            if (walletBalance >= foodScript.price)
            {
                walletBalance -= foodScript.price;
                totalSpent += foodScript.price;

                // Get target transform from current index
                Transform targetPoint = spawnPoints[currentSpawnIndex];
                Vector3 spawnPosition = targetPoint.position + (Vector3.up * spawnHeightOffset);

                // Instantiate item above the counter spawn position
                Instantiate(prefabToSpawn, spawnPosition, targetPoint.rotation);

                // Cycle to the next spawn point (0, 1, 2)
                currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;

                UpdateUI();
                Debug.Log($"Purchased {foodScript.foodName} for ${foodScript.price:F2}. Spawned at Spot {currentSpawnIndex + 1}.");
            }
            else
            {
                Debug.LogWarning($"Insufficient Funds! Wallet: ${walletBalance:F2}, Item Price: ${foodScript.price:F2}");
            }
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