using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodItemRow : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button buyButton;

    private FoodItem currentItem;

    public void SetupRow(FoodItem itemData)
    {
        currentItem = itemData;

        // Display UI values using 'foodName' instead of 'itemName'
        if (nameText != null && itemData != null)
            nameText.text = itemData.foodName;

        if (priceText != null && itemData != null)
            priceText.text = $"${itemData.price:F2}";

        // Clear existing listeners and add purchase action
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    private void OnBuyButtonClicked()
    {
        // Locate the main tablet manager in the scene
        ShoppingTablet tablet = FindFirstObjectByType<ShoppingTablet>();
        if (tablet != null && currentItem != null)
        {
            // Search availableItems list for matching foodPrefab reference to get correct index
            int index = tablet.availableItems.FindIndex(entry => entry.foodPrefab != null && entry.foodPrefab.GetComponent<FoodItem>() == currentItem);

            if (index != -1)
            {
                tablet.BuyFoodItem(index);
            }
            else
            {
                Debug.LogWarning($"Item '{currentItem.foodName}' not found in ShoppingTablet availableItems catalog!");
            }
        }
        else
        {
            Debug.LogError("ShoppingTablet manager or currentItem reference is missing!");
        }
    }
}