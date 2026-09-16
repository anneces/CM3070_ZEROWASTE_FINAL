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

    // Helper property to map co2Points for DayPhaseManager calculations
    public float co2Value => co2Points;

    [Header("Cooking & Spoiled State")]
    public bool isCooked = false;

    // Updated: Helper property that maps isExpired or <= 15% freshness to isSpoiled
    public bool isSpoiled => isExpired || currentFreshnessDays <= 0f || FreshnessPercentage <= 15f;

    [Header("Day-Based Expiration Settings")]
    [Tooltip("Maximum shelf life of the item in days when stored correctly.")]
    public float maxFreshnessDays = 3f;
    public float currentFreshnessDays;
    public bool isExpired = false;

    [Header("Portion-Distortion Visuals")]
    public Vector3 trueServingScale = Vector3.one;
    public Vector3 distortedServingScale = new Vector3(1.4f, 1.4f, 1.4f);

    private Renderer itemRenderer;
    private Color originalColor;
    private Material foodMaterialInstance;
    private static readonly int DecayAmountProperty = Shader.PropertyToID("_DecayAmount");

    private XRGrabInteractable grabInteractable;

    // Spawn tracking for resetting/respawning position
    private Vector3 initialSpawnPosition;
    private Quaternion initialSpawnRotation;
    private Rigidbody rb;

    /// <summary>
    /// Calculates the remaining freshness percentage.
    /// </summary>
    public float FreshnessPercentage
    {
        get
        {
            if (maxFreshnessDays <= 0f) return 0f;
            float pct = (currentFreshnessDays / maxFreshnessDays) * 100f;
            return Mathf.Clamp(pct, 0f, 100f);
        }
    }

    /// <summary>
    /// Updated: Returns Status text: Fresh (100%-66%), Spotting (65%-16%), or Spoiled (15% or less).
    /// </summary>
    public string FreshnessStatus
    {
        get
        {
            if (isExpired || currentFreshnessDays <= 0f) return "Spoiled";

            float pct = FreshnessPercentage;
            if (pct >= 66f) return "Fresh";
            if (pct > 15f) return "Spotting";
            return "Spoiled";
        }
    }

    /// <summary>
    /// Updated: Returns color codes for TextMeshPro UI formatting matching the freshness stage.
    /// </summary>
    public string FreshnessStatusColor
    {
        get
        {
            if (isExpired || currentFreshnessDays <= 0f) return "#EF4444"; // Red (Spoiled)

            float pct = FreshnessPercentage;
            if (pct >= 66f) return "#22C55E"; // Green (Fresh)
            if (pct > 15f) return "#EAB308";  // Yellow/Orange (Spotting)
            return "#EF4444";                  // Red (Spoiled: 15% or less)
        }
    }

    private void Awake()
    {
        // Cache initial spawn position and rotation
        initialSpawnPosition = transform.position;
        initialSpawnRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();

        // Only set default freshness if it hasn't been set prior to Awake execution
        if (currentFreshnessDays <= 0f && !isExpired)
        {
            currentFreshnessDays = maxFreshnessDays;
        }

        itemRenderer = GetComponentInChildren<Renderer>();
        if (itemRenderer != null)
        {
            foodMaterialInstance = itemRenderer.material; // Unique material instance clone

            if (foodMaterialInstance.HasProperty("_Color"))
            {
                originalColor = foodMaterialInstance.color;
            }
        }

        // Get the attached XRGrabInteractable component automatically
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        UpdateMoldVisuals();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnDrop);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnDrop);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (isSpoiled)
        {
            PlaySpoiledAudio();
        }
        else
        {
            AudioManager.Instance?.PlayGrabDrop();
        }
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        if (isSpoiled)
        {
            PlaySpoiledAudio();
        }
        else
        {
            AudioManager.Instance?.PlayGrabDrop();
        }
    }

    private void PlaySpoiledAudio()
    {
        AudioManager.Instance?.PlaySpoiledFood();
    }

    private void OnDestroy()
    {
        // Clean up material clone instance to prevent leaks when food items are instantiated/destroyed
        if (foodMaterialInstance != null)
        {
            Destroy(foodMaterialInstance);
        }
    }

    /// <summary>
    /// Teleports the food item back to its original spawn point and resets physics velocities.
    /// </summary>
    public void RespawnToOriginalPoint()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = initialSpawnPosition;
        transform.rotation = initialSpawnRotation;
    }

    /// <summary>
    /// Explicitly sets freshness state without resetting back to max capacity. Updates visuals.
    /// </summary>
    public void SetFreshnessDays(float savedDays)
    {
        currentFreshnessDays = Mathf.Clamp(savedDays, 0f, maxFreshnessDays);
        if (currentFreshnessDays <= 0f || FreshnessPercentage <= 15f)
        {
            ExpireItem();
        }
        else
        {
            isExpired = false;
        }
        UpdateMoldVisuals();
    }

    /// <summary>
    /// Called by DayPhaseManager during phase shift ticks (Morning -> Afternoon -> Evening).
    /// Drives both fractional shelf life deduction and visual mold shader progression.
    /// </summary>
    public void OnPhaseTick()
    {
        // FIX: Prevent decay if called during Afternoon phase (transitioned from Morning)
        if (DayPhaseManager.Instance != null && DayPhaseManager.Instance.CurrentPhase == DayPhase.Afternoon)
        {
            return;
        }

        AdvancePhase();
        UpdateMoldVisuals();
    }

    /// <summary>
    /// Updated: Updates the _DecayAmount property on the custom Shader Graph material 
    /// (0.0 = Fresh, scales 0.0 to 1.0 during Spotting 65%-16%, 1.0 = Fully Moldy at <= 15%).
    /// </summary>
    public void UpdateMoldVisuals()
    {
        if (foodMaterialInstance == null) return;

        float pct = FreshnessPercentage;
        float decayNormalized = 0f;

        if (pct >= 66f)
        {
            // Fresh phase: Texture clean, Decay set to 0
            decayNormalized = 0f;
        }
        else if (pct > 15f)
        {
            // Spotting phase: Remap 65%..16% to 0.0..1.0 linearly
            decayNormalized = Mathf.InverseLerp(65f, 16f, pct);
        }
        else
        {
            // Spoiled phase (<= 15%): Maximum mold & darkened surface (1.0)
            decayNormalized = 1.0f;
        }

        if (foodMaterialInstance.HasProperty(DecayAmountProperty))
        {
            foodMaterialInstance.SetFloat(DecayAmountProperty, decayNormalized);
        }
    }

    /// <summary>
    /// Advances the item's shelf life per phase shift (1/3 of a day per phase).
    /// Applies 2x multiplier if stored in an improper zone.
    /// </summary>
    public void AdvancePhase()
    {
        if (isExpired) return;

        // Base decay per phase (3 phases per full day)
        float daysToDeduct = 0.333f;

        // 1. Stored in ideal location -> Normal 0.333 decay per phase (1 full day per 3 phases)
        // 2. Stored improperly -> 2x Spoil Multiplier (~0.666 decay per phase)
        if (currentStorage != idealStorage)
        {
            daysToDeduct *= 2.0f;
        }

        currentFreshnessDays -= daysToDeduct;

        if (currentFreshnessDays <= 0f || FreshnessPercentage <= 15f)
        {
            ExpireItem();
        }
    }

    /// <summary>
    /// Legacy alias retained for external day-tick calls.
    /// </summary>
    public void AdvanceDay()
    {
        AdvancePhase();
    }

    public void ApplyPortionDistortion(bool enableDistortion)
    {
        transform.localScale = enableDistortion ? distortedServingScale : trueServingScale;
    }

    private void ExpireItem()
    {
        isExpired = true;
        Debug.Log($"[SPOIL WARNING] {foodName} has spoiled! Financial loss: ${price:F2}, Carbon penalty: {co2Points} kg CO2.");

        // Darkens material to indicate rot
        if (foodMaterialInstance != null && foodMaterialInstance.HasProperty("_Color"))
        {
            foodMaterialInstance.color = originalColor * 0.3f;
        }
    }
}