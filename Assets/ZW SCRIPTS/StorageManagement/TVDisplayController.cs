using UnityEngine;
using TMPro;
using System.Text;

public class TVDisplayController : MonoBehaviour
{
    public static TVDisplayController Instance { get; private set; }

    [Header("Headers")]
    [SerializeField] private TMP_Text foodHeader;
    [SerializeField] private TMP_Text placementHeader;
    [SerializeField] private TMP_Text statusHeader;
    [SerializeField] private TMP_Text freshHeader;
    [SerializeField] private TMP_Text co2Header;
    [SerializeField] private TMP_Text storageHeader;

    [Header("Data Columns")]
    [SerializeField] private TMP_Text foodColumn;
    [SerializeField] private TMP_Text placementColumn;
    [SerializeField] private TMP_Text statusColumn;
    [SerializeField] private TMP_Text freshColumn;
    [SerializeField] private TMP_Text co2Column;
    [SerializeField] private TMP_Text storageColumn;

    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 0.2f; // Refreshes 5 times per second instead of every frame

    private void Awake()
    {
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
        DayPhaseManager.OnPhaseChanged += RefreshDisplay;
    }

    private void OnDisable()
    {
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

        if (allFood.Length == 0)
        {
            ClearColumns("<color=#888888>None</color>");
            return;
        }

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

            // Raw values
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