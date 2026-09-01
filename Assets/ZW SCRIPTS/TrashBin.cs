using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public static float TotalWasteCost = 0f;
    public static float TotalCO2Impact = 0f;

    private void OnTriggerEnter(Collider other)
    {
        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            // Accrue financial and environmental impact using existing FoodItem fields
            TotalWasteCost += item.price;
            TotalCO2Impact += item.co2Points;

            AudioManager.Instance?.PlaySFX(AudioManager.Instance.trashDropClip);
            Destroy(item.gameObject);

            Debug.Log($"[Binned {item.foodName}] Cost Lost: ${TotalWasteCost:F2} | CO2 Impact: {TotalCO2Impact:F2}kg");
        }
    }
}