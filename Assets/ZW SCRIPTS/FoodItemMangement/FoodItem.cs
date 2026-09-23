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

    /// <summary>
    /// Helper property to map co2Points for DayPhaseManager metrics calculations.
    /// </summary>
    public float co2Value => co2Points;

    [Header("Cooking & Spoiled State")]
    public bool isCooked = false;

    /// <summary>
    /// Helper property that evaluates whether the food item is considered spoiled based on expiration, remaining days, or freshness threshold.
    /// </summary>
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
    /// Calculates the remaining freshness percentage based on max vs current freshness days.
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
    /// Returns the descriptive freshness stage text: Fresh (100%-66%), Spotting (65%-16%), or Spoiled (15% or less).
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
    /// Returns hex color codes for TextMeshPro UI formatting matching the current freshness stage.
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

    /// <summary>
    /// Caches spawn transforms, initializes references, instantiates unique material instances, and defaults freshness capacity.
    /// </summary>
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

    /// <summary>
    /// Updates mold shader parameters on initialization to reflect starting freshness.
    /// </summary>
    private void Start()
    {
        UpdateMoldVisuals();
    }

    /// <summary>
    /// Registers grab and drop event listeners on the XR grab interactable.
    /// </summary>
    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnDrop);
        }
    }

    /// <summary>
    /// Unsubscribes grab and drop event listeners when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnDrop);
        }
    }

    /// <summary>
    /// Triggers appropriate audio feedback when the player grabs the item.
    /// </summary>
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

    /// <summary>
    /// Triggers appropriate audio feedback when the player releases the item.
    /// </summary>
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

    /// <summary>
    /// Plays sound effect specific to handling rotten or spoiled food items.
    /// </summary>
    private void PlaySpoiledAudio()
    {
        AudioManager.Instance?.PlaySpoiledFood();
    }

    /// <summary>
    /// Cleans up dynamically instantiated material clones to prevent memory leaks upon destruction.
    /// </summary>
    private void OnDestroy()
    {
        if (foodMaterialInstance != null)
        {
            Destroy(foodMaterialInstance);
        }
    }

    /// <summary>
    /// Teleports the food item back to its initial spawn position and resets linear and angular velocities.
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
    /// Sets specific freshness days without resetting to maximum capacity and updates corresponding visuals.
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
    /// Called by DayPhaseManager during phase shift ticks to advance shelf life degradation and update mold visuals.
    /// </summary>
    public void OnPhaseTick()
    {
        // Prevent decay if called during Afternoon phase transition
        if (DayPhaseManager.Instance != null && DayPhaseManager.Instance.CurrentPhase == DayPhase.Afternoon)
        {
            return;
        }

        AdvancePhase();
        UpdateMoldVisuals();
    }

    /// <summary>
    /// Updates the _DecayAmount shader graph property based on the current freshness stage.
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
    /// Advances shelf life degradation per phase shift, applying accelerated decay if stored in an improper storage zone.
    /// </summary>
    public void AdvancePhase()
    {
        if (isExpired) return;

        // Base decay per phase (3 phases per full day)
        float daysToDeduct = 0.333f;

        // Apply 2x Spoil Multiplier if stored outside ideal storage location
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
    /// Alias method to advance a single phase shift tick.
    /// </summary>
    public void AdvanceDay()
    {
        AdvancePhase();
    }

    /// <summary>
    /// Adjusts the local transform scale between true serving dimensions and distorted serving dimensions.
    /// </summary>
    public void ApplyPortionDistortion(bool enableDistortion)
    {
        transform.localScale = enableDistortion ? distortedServingScale : trueServingScale;
    }

    /// <summary>
    /// Marks the item as expired, logs economic and environmental penalties, and darkens material color.
    /// </summary>
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