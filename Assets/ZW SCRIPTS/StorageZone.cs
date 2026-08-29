using UnityEngine;

public class StorageZone : MonoBehaviour
{
    public enum ZoneType { Pantry, Fridge, Freezer, KitchenCounter }
    public ZoneType zoneType;

    private void OnTriggerEnter(Collider other)
    {
        FoodItem item = GetFoodItemFromCollider(other);
        if (item != null)
        {
            item.currentStorage = zoneType;
            Debug.Log($"[StorageZone] {item.foodName} placed in {zoneType}. Ideal: {item.idealStorage}");
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FoodItem item = GetFoodItemFromCollider(other);
        // Only revert to KitchenCounter if the item is exiting the EXACT zone it is currently assigned to
        if (item != null && item.currentStorage == zoneType)
        {
            // Revert back to KitchenCounter status when removed from this zone
            item.currentStorage = ZoneType.KitchenCounter;
            Debug.Log($"[StorageZone] {item.foodName} removed from {zoneType}. Defaulted to KitchenCounter.");
        }
    }

    /// <summary>
    /// Helper method to search child colliders, parent objects, and root objects for the FoodItem component.
    /// </summary>
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