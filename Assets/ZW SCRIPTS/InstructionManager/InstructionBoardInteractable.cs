using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// XR Interactable component attached to the physical instruction chalkboard in the scene.
/// Listens for XR selection events (ray click/grab) and toggles the main tutorial instruction UI.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class InstructionBoardInteractable : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        // Cache reference to the simple interactable component
        simpleInteractable = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        // Subscribe to XR select event when enabled
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnBoardClicked);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from XR select event when disabled to prevent memory leaks
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnBoardClicked);
        }
    }

    #endregion

    #region XR Interaction Handlers

    /// <summary>
    /// Event callback triggered when an XR controller selects/clicks this chalkboard interactable.
    /// Opens or toggles the Instruction Manager UI canvas.
    /// </summary>
    /// <param name="args">Arguments containing XR interactor event data.</param>
    private void OnBoardClicked(SelectEnterEventArgs args)
    {
        if (InstructionManager.Instance != null)
        {
            InstructionManager.Instance.ToggleOrOpenCanvas();
        }
    }

    #endregion
}