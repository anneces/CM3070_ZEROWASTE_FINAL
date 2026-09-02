using UnityEngine;

public class StovePanTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Search for FoodItem on the collider or its root parent (handles grabbed objects)
        FoodItem item = other.GetComponentInParent<FoodItem>();

        if (item != null)
        {
            // Pass the detected food item directly to the central manager
            CookingManager.Instance?.OnIngredientDropped(item);
        }
    }
}