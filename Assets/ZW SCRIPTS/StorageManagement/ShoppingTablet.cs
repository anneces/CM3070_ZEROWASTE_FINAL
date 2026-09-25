using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the virtual shopping tablet UI, catalog generation, food purchasing logic,
/// budget tracking, and item spawning in the kitchen.
/// </summary>
public class ShoppingTablet : MonoBehaviour
{
    // ENUMS & STRUCTS

    /// <summary>
    /// Food categories used for organizing items in the shop interface.
    /// </summary>
    public enum FoodCategory { FruitsAndVegetables, Protein, Grains, Others }

    /// <summary>
    /// Struct defining an entry in the shop catalog.
    /// </summary>
    [System.Serializable]
    public struct ShopEntry
    {
        public string itemName;
        public FoodCategory category;
        public GameObject foodPrefab;
    }

    // BUDGET & WALLET SETTINGS

    [Header("Budget & Wallet Settings")]
    [Tooltip("Starting funds assigned to the player at the beginning of a run.")]
    public float startingBudget = 50.0f;

    [Tooltip("Current remaining money in the wallet.")]
    public float walletBalance;

    [Tooltip("Total money spent across purchases in the current run.")]
    public float totalSpent = 0.0f;

    // PURCHASE COOLDOWN SETTINGS

    [Header("Purchase Cooldown Settings")]
    [Tooltip("Time in seconds to wait between purchases to prevent accidental double clicks.")]
    public float buyCooldown = 0.5f;

    /// <summary>
    /// Timestamp of the last successful purchase.
    /// </summary>
    private float lastBuyTime = -10.0f;

    // UI HEADER REFERENCES

    [Header("UI Header References")]
    [Tooltip("Text component displaying current wallet balance.")]
    public TMP_Text walletBalanceText;

    [Tooltip("Text component displaying total money spent.")]
    public TMP_Text totalSpentText;

    [Tooltip("UI Container/Panel object displayed when funds are insufficient.")]
    public GameObject warningLabel;

    [Tooltip("How long in seconds the warning label remains visible.")]
    public float warningDuration = 2.0f;

    private Coroutine activeWarningCoroutine;

    // SPAWN SETTINGS

    [Header("Spawn Settings")]
    [Tooltip("Assign 3 spawn point transforms located above the kitchen counter.")]
    public Transform[] spawnPoints = new Transform[3];

    [Tooltip("Vertical height offset applied above spawn points to prevent clipping.")]
    public float spawnHeightOffset = 0.15f;

    /// <summary>
    /// Index tracking which spawn point to use next in a round-robin rotation.
    /// </summary>
    private int currentSpawnIndex = 0;

    // CATEGORIZED LAYOUT REFERENCES

    [Header("Categorized Layout References")]
    [Tooltip("Parent transform holding all category sections (Vertical Layout Group).")]
    public Transform mainContentParent;

    [Tooltip("Prefab containing a category title and horizontal ScrollRect.")]
    public GameObject categorySectionPrefab;

    [Tooltip("UI card prefab representing an individual food item.")]
    public GameObject foodCardPrefab;

    // SHOP CATALOG

    [Header("Shop Catalog")]
    [Tooltip("List of all purchasable food entries.")]
    public List<ShopEntry> availableItems;

    // MONOBEHAVIOUR LIFECYCLE

    private void Start()
    {
        // Hide warning label on start
        if (warningLabel != null) warningLabel.SetActive(false);

        // Reset saved spent amount on new run start
        totalSpent = 0.0f;
        PlayerPrefs.SetFloat("TotalMoneySpent", 0.0f);
        PlayerPrefs.Save();

        // Initialize wallet and build shop UI
        walletBalance = startingBudget;
        PopulateCategorizedCatalog();
        UpdateUI();
    }

    // CATALOG POPULATION

    /// <summary>
    /// Dynamically instantiates and populates category headers and item cards based on the catalog.
    /// </summary>
    private void PopulateCategorizedCatalog()
    {
        // Validate UI container references
        if (mainContentParent == null || categorySectionPrefab == null || foodCardPrefab == null)
        {
            Debug.LogError("ShoppingTablet: Ensure mainContentParent, categorySectionPrefab, and foodCardPrefab are assigned!");
            return;
        }

        // Clear existing category blocks from UI
        foreach (Transform child in mainContentParent)
        {
            Destroy(child.gameObject);
        }

        // Initialize category grouping dictionary
        Dictionary<FoodCategory, List<int>> categorizedIndices = new Dictionary<FoodCategory, List<int>>();
        foreach (FoodCategory cat in System.Enum.GetValues(typeof(FoodCategory)))
        {
            categorizedIndices[cat] = new List<int>();
        }

        // Group item catalog indices by category
        for (int i = 0; i < availableItems.Count; i++)
        {
            categorizedIndices[availableItems[i].category].Add(i);
        }

        // Build UI sections for each non-empty category
        foreach (KeyValuePair<FoodCategory, List<int>> pair in categorizedIndices)
        {
            if (pair.Value.Count == 0) continue; // Skip empty categories

            // 1. Instantiate Category Section Prefab
            GameObject sectionObj = Instantiate(categorySectionPrefab, mainContentParent);

            // Set Category Header Title
            TMP_Text headerText = sectionObj.GetComponentInChildren<TMP_Text>();
            if (headerText != null)
            {
                if (pair.Key == FoodCategory.FruitsAndVegetables)
                {
                    headerText.text = "FRUITS & VEGETABLES";
                }
                else
                {
                    headerText.text = pair.Key.ToString().ToUpper();
                }
            }

            // Locate horizontal content container within the section prefab
            ScrollRect scrollRect = sectionObj.GetComponentInChildren<ScrollRect>();
            Transform horizontalContent = (scrollRect != null) ? scrollRect.content : sectionObj.transform;

            // 2. Instantiate Item Cards into category horizontal layout
            foreach (int index in pair.Value)
            {
                int capturedIndex = index; // Safely capture loop index for lambda closures

                ShopEntry entry = availableItems[capturedIndex];
                GameObject cardObj = Instantiate(foodCardPrefab, horizontalContent);

                string displayName = entry.itemName;
                float displayPrice = 0.0f;

                // Extract name and price details directly from the food prefab script if present
                if (entry.foodPrefab != null)
                {
                    FoodItem foodScript = entry.foodPrefab.GetComponent<FoodItem>();
                    if (foodScript != null)
                    {
                        if (!string.IsNullOrEmpty(foodScript.foodName)) displayName = foodScript.foodName;
                        displayPrice = foodScript.price;
                    }
                }

                // Bind UI references using FoodItemRow component if present, otherwise fallback to generic components
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

    // PURCHASE LOGIC

    /// <summary>
    /// Processes purchasing an item, deducting funds, spawning the 3D item in the kitchen, and updating UI.
    /// </summary>
    /// <param name="itemIndex">Index of the item in the availableItems list.</param>
    public void BuyFoodItem(int itemIndex)
    {
        // Enforce input cooldown to prevent rapid double-clicking
        if (Time.time < lastBuyTime + buyCooldown)
        {
            Debug.Log("Purchase requested too quickly; click ignored for cooldown.");
            return;
        }

        // Validate index bounds
        if (itemIndex < 0 || itemIndex >= availableItems.Count) return;

        // Ensure valid spawn points exist
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

        // Read price and name properties
        FoodItem foodScript = prefabToSpawn.GetComponent<FoodItem>();
        float price = (foodScript != null) ? foodScript.price : 0.0f;
        string name = (foodScript != null) ? foodScript.foodName : availableItems[itemIndex].itemName;

        // Process purchase if wallet balance is sufficient
        if (walletBalance >= price)
        {
            lastBuyTime = Time.time;

            walletBalance -= price;
            totalSpent += price;

            // Save running total spent to PlayerPrefs
            PlayerPrefs.SetFloat("TotalMoneySpent", totalSpent);
            PlayerPrefs.Save();

            // Calculate spawn position and rotation using rotating spawn index
            Transform targetPoint = spawnPoints[currentSpawnIndex];
            Vector3 spawnPosition = targetPoint.position + (Vector3.up * spawnHeightOffset);

            // Instantiate food prefab into scene
            Instantiate(prefabToSpawn, spawnPosition, targetPoint.rotation);

            // Cycle spawn point index for next purchase
            currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;

            // Play purchase audio feedback
            AudioManager.Instance?.PlayPurchase();

            // Refresh UI and output log
            UpdateUI();
            Debug.Log($"Purchased {name} for ${price:F2}. Spawned at Spot {currentSpawnIndex + 1}.");
        }
        else
        {
            // Play alert SFX
            AudioManager.Instance?.PlayAlert();

            // Trigger warning label feedback
            ShowInsufficientFundsWarning();
            Debug.LogWarning($"Insufficient Funds! Wallet: ${walletBalance:F2}, Item Price: ${price:F2}");
        }
    }

    /// <summary>
    /// Displays the warning label for a temporary duration.
    /// </summary>
    private void ShowInsufficientFundsWarning()
    {
        if (warningLabel != null)
        {
            if (activeWarningCoroutine != null)
            {
                StopCoroutine(activeWarningCoroutine);
            }
            activeWarningCoroutine = StartCoroutine(ShowWarningRoutine());
        }
    }

    private IEnumerator ShowWarningRoutine()
    {
        warningLabel.SetActive(true);
        yield return new WaitForSeconds(warningDuration);
        warningLabel.SetActive(false);
        activeWarningCoroutine = null;
    }

    // UI HANDLERS & REFRESH

    /// <summary>
    /// Handler for the "End Day" button click; notifies DayPhaseManager to advance phases.
    /// </summary>
    public void OnEndDayButtonClicked()
    {
        AudioManager.Instance?.PlayUIClick();
        if (DayPhaseManager.Instance != null)
        {
            DayPhaseManager.Instance.ConfirmNextPhase();
            UpdateUI();
        }
    }

    /// <summary>
    /// Updates all text components in the tablet header with current values.
    /// </summary>
    public void UpdateUI()
    {
        if (walletBalanceText != null)
            walletBalanceText.text = $"Wallet: ${walletBalance:F2}";

        if (totalSpentText != null)
            totalSpentText.text = $"Spent: ${totalSpent:F2}";
    }
}