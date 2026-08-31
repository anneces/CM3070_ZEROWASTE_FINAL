// FoodItemSetup.cs - Script preview for grabbable food objects
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class FoodItemBase : MonoBehaviour
{
    public string foodName = "Apple";
    public StorageZone.ZoneType idealStorage = StorageZone.ZoneType.Fridge;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Ensure collision detection mode is set safely to prevent passing through counters/tables
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Sync with FoodItem component if present on the same GameObject
        FoodItem foodItem = GetComponent<FoodItem>();
        if (foodItem != null)
        {
            if (!string.IsNullOrEmpty(foodName)) foodItem.foodName = foodName;
            foodItem.idealStorage = idealStorage;
        }
    }
}