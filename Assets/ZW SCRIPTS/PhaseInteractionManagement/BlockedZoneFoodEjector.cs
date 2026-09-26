using UnityEngine;

/// <summary>
/// Attach to zone trigger GameObjects (e.g., PantryZones, StoveCookZone).
/// Detects incoming food items during blocked phase and ejects them to counter spawn points.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BlockedZoneFoodEjector : MonoBehaviour
{
    [Header("Blocker Reference")]
    [Tooltip("The PhaseInteractionBlocker controlling this station area.")]
    public PhaseInteractionBlocker phaseBlocker;

    [Header("Respawn Points")]
    [Tooltip("Assign SpawnPoint_1, SpawnPoint_2, SpawnPoint_3 here.")]
    public Transform[] counterSpawnPoints;

    [Header("Item Tag Filter")]
    [Tooltip("Tag used to identify food or grabbable kitchen items.")]
    public string foodItemTag = "FoodItem";

    private void OnTriggerEnter(Collider other)
    {
        // Only process if the station is currently blocked
        if (phaseBlocker != null && phaseBlocker.IsBlocked)
        {
            // Check if the object entering is a food item or has a Rigidbody
            if (other.CompareTag(foodItemTag) || other.GetComponent<Rigidbody>() != null)
            {
                EjectItemToCounter(other.gameObject);
            }
        }
    }

    private void EjectItemToCounter(GameObject item)
    {
        if (counterSpawnPoints == null || counterSpawnPoints.Length == 0) return;

        // Choose a random spawn point from your counter list
        Transform targetSpawn = counterSpawnPoints[Random.Range(0, counterSpawnPoints.Length)];

        // Stop physics movement
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Teleport item to the chosen counter spawn point
        item.transform.position = targetSpawn.position;
        item.transform.rotation = targetSpawn.rotation;
    }
}