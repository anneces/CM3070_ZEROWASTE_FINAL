using UnityEngine;
using TMPro;

public class TrashBinController : MonoBehaviour
{
    public static TrashBinController Instance;

    [Header("Running Totals")]
    [Tooltip("Total money lost from discarded items.")]
    public float totalFinancialLoss = 0f;

    [Tooltip("Total CO2 penalty in kg from discarded items.")]
    public float totalCO2Penalty = 0f;

    [Tooltip("Total count of discarded food items.")]
    public int totalItemsDiscarded = 0;

    // Property Getters for DayManager compatibility
    public float totalMoneyWasted => totalFinancialLoss;
    public float totalCO2 => totalCO2Penalty;

    [Header("UI Display (Optional)")]
    public TextMeshProUGUI financialLossText;
    public TextMeshProUGUI co2PenaltyText;

    [Header("VFX & Audio (Optional)")]
    public ParticleSystem trashVFX;
    public AudioClip trashDropSFX;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Load persistent totals or fallback to 0
        totalFinancialLoss = PlayerPrefs.GetFloat("TotalMoneyWasted", 0f);
        totalCO2Penalty = PlayerPrefs.GetFloat("TotalCO2", 0f);
        totalItemsDiscarded = PlayerPrefs.GetInt("TotalItemsDiscarded", 0);

        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            ProcessDiscardedItem(item);
        }
    }

    private void ProcessDiscardedItem(FoodItem item)
    {
        // Add item values to running totals
        totalFinancialLoss += item.price;
        totalCO2Penalty += item.co2Points;
        totalItemsDiscarded++;

        // Save totals for GameSummaryUI evaluation
        PlayerPrefs.SetFloat("TotalMoneyWasted", totalFinancialLoss);
        PlayerPrefs.SetFloat("TotalCO2", totalCO2Penalty);
        PlayerPrefs.SetInt("TotalItemsDiscarded", totalItemsDiscarded);
        PlayerPrefs.Save();

        // Log waste type for analytics/debugging
        string status = (item.isExpired || item.currentFreshnessDays <= 0) ? "Rotted/Expired" : "Fresh Leftover";
        Debug.Log($"[TRASH] Discarded {status} {item.foodName} | Lost: ${item.price:F2} | CO2 Impact: {item.co2Points} kg | Total Discarded: {totalItemsDiscarded}");

        // Play feedback effects
        if (trashVFX != null)
        {
            trashVFX.transform.position = item.transform.position;
            trashVFX.Play();
        }

        // Play Trash Can SFX via AudioManager
        if (trashDropSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(trashDropSFX);
        }
        else
        {
            AudioManager.Instance?.PlayTrashCan();
        }

        // Update UI counters
        UpdateUI();

        // Destroy discarded food item
        Destroy(item.gameObject);
    }

    public void UpdateUI()
    {
        if (financialLossText != null)
        {
            financialLossText.text = $"Total Cost Lost: ${totalFinancialLoss:F2}";
        }

        if (co2PenaltyText != null)
        {
            co2PenaltyText.text = $"Total CO2 Waste: {totalCO2Penalty:F2} kg";
        }
    }

    /// <summary>
    /// Resets running totals (useful when restarting a level or day phase).
    /// </summary>
    public void ResetCounters()
    {
        totalFinancialLoss = 0f;
        totalCO2Penalty = 0f;
        totalItemsDiscarded = 0;

        PlayerPrefs.SetFloat("TotalMoneyWasted", 0f);
        PlayerPrefs.SetFloat("TotalCO2", 0f);
        PlayerPrefs.SetInt("TotalItemsDiscarded", 0);
        PlayerPrefs.Save();

        UpdateUI();
    }
}