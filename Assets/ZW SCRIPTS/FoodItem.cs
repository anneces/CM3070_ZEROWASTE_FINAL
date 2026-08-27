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

    private void Awake()
    {
        currentFreshnessDays = maxFreshnessDays;
        itemRenderer = GetComponentInChildren<Renderer>();
        if (itemRenderer != null)
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

        // Apply decay penalties based on storage location
        if (currentStorage == StorageZone.ZoneType.KitchenCounter)
        {
            daysToDeduct = 1; // Standard 1-day decay on counter
        }
        else if (currentStorage != idealStorage)
        {
            daysToDeduct = 2; // Spoil Multiplier: 2x decay for improper storage
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
        Debug.Log($"[SPOIL WARNING] {foodName} has spoiled! Financial loss: ${price}, Carbon penalty: {co2Points} kg CO2.");

        // Darkens material to indicate rot
        if (itemRenderer != null)
        {
            itemRenderer.material.color = originalColor * 0.3f;
        }
    }
}