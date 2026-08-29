using UnityEngine;
using TMPro;
using System.Text;

public class TVDisplayController : MonoBehaviour
{
    public static TVDisplayController Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TMP_Text tvTextDisplay;

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

    private void Start()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        // Refreshes display dynamically every frame so zone changes reflect immediately on TV
        RefreshDisplay();
    }

    /// <summary>
    /// Reads dynamic food item states and updates formatted text on the World-Space TV Canvas.
    /// </summary>
    public void RefreshDisplay()
    {
        if (tvTextDisplay == null) return;

        FoodItem[] allFood = FindObjectsOfType<FoodItem>();

        if (allFood.Length == 0)
        {
            tvTextDisplay.text = "<color=#888888>No food items detected in kitchen.</color>";
            return;
        }

        StringBuilder sb = new StringBuilder();

        // Standard header with spaces matched to plain text padding below
        sb.AppendLine("<b><color=#555555>FOOD            STATUS       FRESH(%)   CO2pts   STORAGE         PLACEMENT</color></b>");

        foreach (FoodItem food in allFood)
        {
            // Determine Placement Status (Optimal / Sub-Optimal)
            bool isOptimal = (food.currentStorage == food.idealStorage);
            string placementText = isOptimal ? "Optimal" : "Sub-Optimal";
            string placementColor = isOptimal ? "#22C55E" : "#EF4444"; // Green vs Red

            // Plain text padding for clean column alignment
            string foodNameCol = food.foodName.PadRight(16);
            string statusText = food.FreshnessStatus.PadRight(13);
            string freshPctText = $"{Mathf.RoundToInt(food.FreshnessPercentage)}%".PadRight(11);
            string co2Col = $"{food.co2Points:F1} kg".PadRight(9);
            string storageCol = food.currentStorage.ToString().PadRight(16);

            // Apply Rich Text colors to padded string variables
            string coloredStatus = $"<color={food.FreshnessStatusColor}>{statusText}</color>";
            string coloredFreshPct = $"<color={food.FreshnessStatusColor}>{freshPctText}</color>";
            string coloredPlacement = $"<color={placementColor}>{placementText}</color>";

            // Append row without mspace tag so font isn't horizontally compressed
            sb.AppendLine($"{foodNameCol}{coloredStatus}{coloredFreshPct}{co2Col}{storageCol}{coloredPlacement}");
        }

        tvTextDisplay.text = sb.ToString();
    }
}