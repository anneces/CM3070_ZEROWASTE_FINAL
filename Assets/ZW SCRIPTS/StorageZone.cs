using UnityEngine;

public class StorageZone : MonoBehaviour
{
    public enum ZoneType { Pantry, Fridge, Freezer, KitchenCounter }
    public ZoneType zoneType;

    private void OnTriggerEnter(Collider other)
    {
        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            item.currentStorage = zoneType;
            Debug.Log($"{item.foodName} placed in {zoneType}. Ideal: {item.idealStorage}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null && item.currentStorage == zoneType)
        {
            // Revert back to KitchenCounter status when picked up or moved outside zones
            item.currentStorage = ZoneType.KitchenCounter;
            Debug.Log($"{item.foodName} removed from {zoneType}. Defaulted to KitchenCounter.");
        }
    }
}