// FoodItemSetup.cs - Script preview for grabbable food objects
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class FoodItemBase : MonoBehaviour
{
    public string foodName;
    public StorageZone.ZoneType idealStorage;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        // Ensure continuous collision detection to prevent dropping through counters
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
}