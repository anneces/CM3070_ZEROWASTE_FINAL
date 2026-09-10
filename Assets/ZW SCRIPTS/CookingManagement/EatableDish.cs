using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class EatableDish : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Assign platebkg directly in inspector or let script search child transforms.")]
    [SerializeField] private GameObject plateBkgObj;

    private XRBaseInteractable interactable;

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