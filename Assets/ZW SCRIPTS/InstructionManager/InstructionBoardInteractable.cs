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

    [Header("Board Visual / Canvas Reference")]
    [Tooltip("Drag the Instructions_ChalkBoard GameObject or ChalkboardCanvas here to hide it when the UI appears.")]
    [SerializeField] private GameObject boardVisualObject;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();

        // Fallback: If no visual object assigned, use this gameObject
        if (boardVisualObject == null)
        {
            boardVisualObject = gameObject;
        }
    }

    private void Start()
    {
        // Register this board with the InstructionManager
        if (InstructionManager.Instance != null)
        {
            InstructionManager.Instance.RegisterChalkboard(boardVisualObject);
        }
    }

    private void OnEnable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnBoardClicked);
        }
    }

    private void OnDisable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnBoardClicked);
        }
    }

    #endregion

    #region XR Interaction Handlers

    private void OnBoardClicked(SelectEnterEventArgs args)
    {
        if (InstructionManager.Instance != null)
        {
            InstructionManager.Instance.ToggleOrOpenCanvas();
        }
    }

    #endregion
}