using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class EatableDish : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Assign platebkg directly in inspector or let script search child transforms.")]
    [SerializeField] private GameObject plateBkgObj;

    private XRBaseInteractable interactable;

    /// <summary>
    /// Sets up references to the XR interactable component, locates the world-space UI background,
    /// and activates the dish prompt UI as soon as the cooked dish object spawns on the stove plate.
    /// </summary>
    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        // Fallback search: If not assigned in Inspector, search inactive child objects safely
        if (plateBkgObj == null)
        {
            Transform foundChild = transform.Find("PlateCanvas/platebkg");
            if (foundChild != null)
            {
                plateBkgObj = foundChild.gameObject;
            }
        }

        // Enable the "platebkg" UI panel when this dish spawns
        if (plateBkgObj != null)
        {
            plateBkgObj.SetActive(true);
        }
    }

    /// <summary>
    /// Registers event listeners to detect when the VR player grabs or interacts with the dish.
    /// </summary>
    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnDishEaten);
        }
    }

    /// <summary>
    /// Cleans up XR event listeners when the dish object is disabled/destroyed to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnDishEaten);
        }
    }

    /// <summary>
    /// Event callback triggered by the XR Interaction Toolkit when a player selects/grabs the dish.
    /// Passes control directly to the core EatDish logic.
    /// </summary>
    private void OnDishEaten(SelectEnterEventArgs args)
    {
        EatDish();
    }

    /// <summary>
    /// Handles dish consumption logic—hides UI elements, resets the stove plate state 
    /// via StoveController so new recipes can be cooked, plays audio feedback, and destroys the dish GameObject.
    /// </summary>
    public void EatDish()
    {
        // Hide the "platebkg" UI panel when the dish is consumed
        if (plateBkgObj != null)
        {
            plateBkgObj.SetActive(false);
        }

        // Reset stove state, UI canvases, and recipe book interactions
        if (StoveController.Instance != null)
        {
            StoveController.Instance.ClearPlate();
        }

        // Play SFX
        AudioManager.Instance?.PlayUIClick();

        // Destroy cooked dish object
        Destroy(gameObject);
    }
}