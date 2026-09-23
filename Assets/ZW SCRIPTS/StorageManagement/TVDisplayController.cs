using UnityEngine;
using TMPro;
using System.Text;

/// <summary>
/// Singleton manager for the World-Space TV display UI canvas in the kitchen.
/// Aggregates all FoodItem instances in real-time to render detailed status columns.
/// </summary>
public class TVDisplayController : MonoBehaviour
{
    // SINGLETON INSTANCE

    public static TVDisplayController Instance { get; private set; }

    // UI HEADER REFERENCES

    [Header("Headers")]
    [SerializeField] private TMP_Text foodHeader;
    [SerializeField] private TMP_Text placementHeader;
    [SerializeField] private TMP_Text statusHeader;
    [SerializeField] private TMP_Text freshHeader;
    [SerializeField] private TMP_Text co2Header;
    [SerializeField] private TMP_Text storageHeader;

    // DATA COLUMN REFERENCES

    [Header("Data Columns")]
    [SerializeField] private TMP_Text foodColumn;
    [SerializeField] private TMP_Text placementColumn;
    [SerializeField] private TMP_Text statusColumn;
    [SerializeField] private TMP_Text freshColumn;
    [SerializeField] private TMP_Text co2Column;
    [SerializeField] private TMP_Text storageColumn;

    // UPDATE TIMING

    /// <summary>
    /// Accumulator timer for throttling UI updates.
    /// </summary>
    private float updateTimer = 0f;

    /// <summary>
    /// Refresh interval (0.2s = 5 updates/sec) to optimize performance in VR.
    /// </summary>
    private const float UPDATE_INTERVAL = 0.2f;

    // MONOBEHAVIOUR LIFECYCLE

    private void Awake()
    {
        // Enforce Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        // Subscribe to phase change events
        DayPhaseManager.OnPhaseChanged += RefreshDisplay;
    }

    private void OnDisable()
    {
        // Unsubscribe from phase change events
        DayPhaseManager.OnPhaseChanged -= RefreshDisplay;
    }

    private void Start()
    {
        SetHeaderLabels();
        RefreshDisplay();
    }

    private void Update()
    {
        // Throttle updates slightly to optimize VR performance while maintaining real-time feel
        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;
            RefreshDisplay();
        }
    }

    // DISPLAY LOGIC

    /// <summary>
    /// Sets default static header strings for the TV UI columns.
    /// </summary>
    private void SetHeaderLabels()
    {
        if (foodHeader != null) foodHeader.text = "FOOD";
        if (placementHeader != null) placementHeader.text = "PLACEMENT";
        if (statusHeader != null) statusHeader.text = "STATUS";
        if (freshHeader != null) freshHeader.text = "FRESH(%)";
        if (co2Header != null) co2Header.text = "CO2pts";
        if (storageHeader != null) storageHeader.text = "STORAGE";
    }

    /// <summary>
    /// Reads dynamic food item states and updates separate column UI components on the World-Space TV Canvas.
    /// </summary>
    public void RefreshDisplay()
    {
        FoodItem[] allFood = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);

        // Clear display if no food items are present in the scene
        if (allFood.Length == 0)
        {
            ClearColumns("<color=#888888>None</color>");
            return;
        }

        // String builders for each column to avoid string allocation overhead
        StringBuilder foodSb = new StringBuilder();
        StringBuilder placementSb = new StringBuilder();
        StringBuilder statusSb = new StringBuilder();
        StringBuilder freshSb = new StringBuilder();
        StringBuilder co2Sb = new StringBuilder();
        StringBuilder storageSb = new StringBuilder();

        foreach (FoodItem food in allFood)
        {
            // Determine Placement Status (Optimal / Sub-Optimal)
            bool isOptimal = (food.currentStorage == food.idealStorage);
            string placementText = isOptimal ? "Optimal" : "Sub-Optimal";
            string placementColor = isOptimal ? "#22C55E" : "#EF4444"; // Green vs Red

            // Format raw values
            string foodNameText = food.foodName;
            string statusText = food.FreshnessStatus;
            string freshPctText = $"{Mathf.RoundToInt(food.FreshnessPercentage)}%";
            string co2Text = $"{food.co2Points:F1} kg";
            string storageText = food.currentStorage.ToString();

            // Append each field to its respective column string builder
            foodSb.AppendLine(foodNameText);
            placementSb.AppendLine($"<color={placementColor}>{placementText}</color>");
            statusSb.AppendLine($"<color={food.FreshnessStatusColor}>{statusText}</color>");
            freshSb.AppendLine($"<color={food.FreshnessStatusColor}>{freshPctText}</color>");
            co2Sb.AppendLine(co2Text);
            storageSb.AppendLine(storageText);
        }

        // Apply updated strings to individual UI text references
        if (foodColumn != null) foodColumn.text = foodSb.ToString();
        if (placementColumn != null) placementColumn.text = placementSb.ToString();
        if (statusColumn != null) statusColumn.text = statusSb.ToString();
        if (freshColumn != null) freshColumn.text = freshSb.ToString();
        if (co2Column != null) co2Column.text = co2Sb.ToString();
        if (storageColumn != null) storageColumn.text = storageSb.ToString();
    }

    /// <summary>
    /// Clears text content from all columns and displays default empty message.
    /// </summary>
    /// <param name="defaultText">Text string to display in the main food column.</param>
    private void ClearColumns(string defaultText)
    {
        if (foodColumn != null) foodColumn.text = defaultText;
        if (placementColumn != null) placementColumn.text = "";
        if (statusColumn != null) statusColumn.text = "";
        if (freshColumn != null) freshColumn.text = "";
        if (co2Column != null) co2Column.text = "";
        if (storageColumn != null) storageColumn.text = "";
    }
}