using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class FoodItem : MonoBehaviour
{
    [Header("Food Configuration")]
    public string foodName = "Apple";
    public StorageZone.ZoneType idealStorage = StorageZone.ZoneType.Fridge;
    public StorageZone.ZoneType currentStorage = StorageZone.ZoneType.KitchenCounter;

    [Header("Economic & Environmental Data")]
    public float price = 0.80f;
    public float co2Points = 0.4f;

    [Header("Day-Based Expiration Settings")]
    [Tooltip("Maximum shelf life of the item in days when stored correctly.")]
    public int maxFreshnessDays = 3;
    public int currentFreshnessDays;
    public bool isExpired = false;

    [Header("Portion-Distortion Visuals")]
    public Vector3 trueServingScale = Vector3.one;
    public Vector3 distortedServingScale = new Vector3(1.4f, 1.4f, 1.4f);

    private Renderer itemRenderer;
    private Color originalColor;

    /// <summary>
    /// Calculates the remaining freshness percentage.
    /// </summary>
    public float FreshnessPercentage
    {
        get
        {
            if (maxFreshnessDays <= 0) return 0f;
            float pct = ((float)currentFreshnessDays / maxFreshnessDays) * 100f;
            return Mathf.Clamp(pct, 0f, 100f);
        }
    }

    /// <summary>
    /// Returns Status text: Fresh (>50%), Spotting (1%-50%), or Spoiled (<=0%).
    /// </summary>
    public string FreshnessStatus
    {
        get
        {
            if (isExpired || currentFreshnessDays <= 0) return "Spoiled";
            if (FreshnessPercentage <= 50f) return "Spotting";
            return "Fresh";
        }
    }

    /// <summary>
    /// Returns color codes for TextMeshPro UI formatting matching the freshness stage.
    /// </summary>
    public string FreshnessStatusColor
    {
        get
        {
            if (isExpired || currentFreshnessDays <= 0) return "#EF4444"; // Red (Spoiled)
            if (FreshnessPercentage <= 50f) return "#EAB308";             // Yellow/Orange (Spotting)
            return "#22C55E";                                            // Green (Fresh)
        }
    }

    private void Awake()
    {
        currentFreshnessDays = maxFreshnessDays;
        itemRenderer = GetComponentInChildren<Renderer>();
        if (itemRenderer != null && itemRenderer.material.HasProperty("_Color"))
        {
            originalColor = itemRenderer.material.color;
        }
    }

    /// <summary>
    /// Advances the item's shelf life based on its current storage location.
    /// Called directly by DayManager during day transitions.
    /// </summary>
    public void AdvanceDay()
    {
        if (isExpired) return;

        int daysToDeduct = 1;

        // Corrected Priority: 
        // 1. If stored in its ideal location -> Standard 1-day decay.
        // 2. If stored improperly anywhere else (counter, wrong zone) -> 2x Spoil Multiplier.
        if (currentStorage == idealStorage)
        {
            daysToDeduct = 1;
        }
        else
        {
            daysToDeduct = 2; // Spoil Multiplier for improper storage (e.g., milk on counter)
        }

        currentFreshnessDays -= daysToDeduct;

        if (currentFreshnessDays <= 0)
        {
            ExpireItem();
        }
    }

    public void ApplyPortionDistortion(bool enableDistortion)
    {
        transform.localScale = enableDistortion ? distortedServingScale : trueServingScale;
    }

    private void ExpireItem()
    {
        isExpired = true;
        currentFreshnessDays = 0;
        Debug.Log($"[SPOIL WARNING] {foodName} has spoiled! Financial loss: ${price:F2}, Carbon penalty: {co2Points} kg CO2.");

        // Darkens material to indicate rot
        if (itemRenderer != null && itemRenderer.material.HasProperty("_Color"))
        {
            itemRenderer.material.color = originalColor * 0.3f;
        }
    }
}