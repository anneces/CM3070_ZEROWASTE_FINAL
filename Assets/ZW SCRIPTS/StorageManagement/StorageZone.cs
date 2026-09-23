using UnityEngine;

/// <summary>
/// Handles trigger detection for storage areas (Pantry, Fridge, Freezer, Counter),
/// updating the current storage location state on entering food items.
/// </summary>
public class StorageZone : MonoBehaviour
{
    // CONFIGURATION

    /// <summary>
    /// Defines the specific type of storage location this zone represents.
    /// </summary>
    public enum ZoneType { Pantry, Fridge, Freezer, KitchenCounter }

    [Tooltip("The storage zone type assigned to this trigger area.")]
    public ZoneType zoneType;

    // TRIGGER EVENTS

    private void OnTriggerEnter(Collider other)
    {
        FoodItem item = GetFoodItemFromCollider(other);
        if (item != null)
        {
            // Update storage location on entry
            item.currentStorage = zoneType;
            Debug.Log($"[StorageZone] {item.foodName} placed in {zoneType}. Ideal: {item.idealStorage}");

            // Refresh TV UI display when item is stored
            if (TVDisplayController.Instance != null)
            {
                TVDisplayController.Instance.RefreshDisplay();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Continuous check to ensure currentStorage stays correct while sitting inside the zone
        FoodItem item = GetFoodItemFromCollider(other);
        if (item != null && item.currentStorage != zoneType)
        {
            item.currentStorage = zoneType;
            Debug.Log($"[StorageZone] {item.foodName} updated to {zoneType} via Stay check.");

            // Refresh TV UI display on state change
            if (TVDisplayController.Instance != null)
            {
                TVDisplayController.Instance.RefreshDisplay();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FoodItem item = GetFoodItemFromCollider(other);

        // Only revert to KitchenCounter if the item is exiting the EXACT zone it is currently assigned to
        if (item != null && item.currentStorage == zoneType)
        {
            // Verify if any collider of this item is still inside this storage trigger volume
            // (Prevents VR hand/grab interactions from causing false exit resets)
            if (IsItemStillInZone(item))
            {
                return;
            }

            // Revert back to KitchenCounter status when removed from this zone
            item.currentStorage = ZoneType.KitchenCounter;
            Debug.Log($"[StorageZone] {item.foodName} removed from {zoneType}. Defaulted to KitchenCounter.");

            // Refresh TV UI display when item is picked up / removed
            if (TVDisplayController.Instance != null)
            {
                TVDisplayController.Instance.RefreshDisplay();
            }
        }
    }

    // HELPER METHODS

    /// <summary>
    /// Checks if any colliders attached to the FoodItem are still inside this zone's trigger bounds.
    /// </summary>
    /// <param name="item">FoodItem component being checked.</param>
    /// <returns>True if any collider intersects zone bounds; false otherwise.</returns>
    private bool IsItemStillInZone(FoodItem item)
    {
        Collider zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null) return false;

        Collider[] itemColliders = item.GetComponentsInChildren<Collider>();
        foreach (var col in itemColliders)
        {
            if (col.enabled && zoneCollider.bounds.Intersects(col.bounds))
            {
                return true; // Item is still inside the zone
            }
        }
        return false;
    }

    /// <summary>
    /// Helper method to search child colliders, parent objects, and root objects for the FoodItem component.
    /// </summary>
    /// <param name="col">Collider triggering the event.</param>
    /// <returns>The FoodItem reference if found; null otherwise.</returns>
    private FoodItem GetFoodItemFromCollider(Collider col)
    {
        if (col == null) return null;

        // 1. Try finding FoodItem on the direct collider object
        FoodItem item = col.GetComponent<FoodItem>();

        // 2. Try finding FoodItem in parent hierarchy
        if (item == null)
            item = col.GetComponentInParent<FoodItem>();

        // 3. Try finding FoodItem in root object
        if (item == null && col.transform.root != null)
            item = col.transform.root.GetComponent<FoodItem>();

        // 4. Try finding FoodItem in child components
        if (item == null)
            item = col.GetComponentInChildren<FoodItem>();

        return item;
    }
}