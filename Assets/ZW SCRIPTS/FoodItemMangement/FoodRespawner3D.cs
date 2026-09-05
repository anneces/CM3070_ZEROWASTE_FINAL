using UnityEngine;

public class FoodRespawner3D : MonoBehaviour
{
    [Header("Respawn Configuration")]
    public Transform respawnZonePoint;

    private void OnTriggerEnter(Collider other)
    {
        ResetAllFoodPositions();
    }

    public void ResetAllFoodPositions()
    {
        // Modernized Unity API calls
        FoodItem[] allItems = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);

        foreach (FoodItem item in allItems)
        {
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (respawnZonePoint != null)
            {
                // Slight position jitter prevents items from occupying identical physics space
                Vector3 randomOffset = new Vector3(Random.Range(-0.15f, 0.15f), 0.05f, Random.Range(-0.15f, 0.15f));
                item.transform.position = respawnZonePoint.position + randomOffset;
                item.transform.rotation = Quaternion.identity;
            }
        }

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.uiClickClip);
    }
}