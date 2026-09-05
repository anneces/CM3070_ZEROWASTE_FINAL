using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class EatableDish : MonoBehaviour
{
    private XRBaseInteractable interactable;
    private GameObject plateBkgObj;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        // Find "bkg" in the scene hierarchy even if it is currently disabled
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Ensure object matches "bkg", is in the active scene, and is not a prefab asset
            if (obj.name == "bkg" && obj.scene.isLoaded)
            {
                plateBkgObj = obj;
                break;
            }
        }

        // Enable the "bkg" UI panel (and child eatmetxt) when this dish spawns
        if (plateBkgObj != null)
        {
            plateBkgObj.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnDishEaten);
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnDishEaten);
        }
    }

    private void OnDishEaten(SelectEnterEventArgs args)
    {
        EatDish();
    }

    public void EatDish()
    {
        // Hide the "bkg" UI panel when the dish is consumed
        if (plateBkgObj != null)
        {
            plateBkgObj.SetActive(false);
        }

        // Reset plate state on StoveController so new dishes can be cooked
        if (StoveController.Instance != null)
        {
            StoveController.Instance.isPlateOccupied = false;
        }

        // Play SFX if available
        AudioManager.Instance?.PlayUIClick();

        // Destroy cooked dish object
        Destroy(gameObject);
    }
}