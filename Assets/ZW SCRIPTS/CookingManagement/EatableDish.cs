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

        // Find "platebkg" in the scene hierarchy even if it is currently disabled
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Ensure object matches "platebkg", is in the active scene, and is not a prefab asset
            if (obj.name == "platebkg" && obj.scene.isLoaded)
            {
                plateBkgObj = obj;
                break;
            }
        }

        // Enable the "platebkg" UI panel (and child eatmetxt) when this dish spawns
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
        // Hide the "platebkg" UI panel when the dish is consumed
        if (plateBkgObj != null)
        {
            plateBkgObj.SetActive(false);
        }

        // Properly reset stove state, UI canvases, and recipe book interactions
        if (StoveController.Instance != null)
        {
            StoveController.Instance.ClearPlate();
        }

        // Play SFX if available
        AudioManager.Instance?.PlayUIClick();

        // Destroy cooked dish object
        Destroy(gameObject);
    }
}