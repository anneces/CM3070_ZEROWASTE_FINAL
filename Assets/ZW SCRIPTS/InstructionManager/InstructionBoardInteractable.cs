using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class InstructionBoardInteractable : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
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

    private void OnBoardClicked(SelectEnterEventArgs args)
    {
        if (InstructionManager.Instance != null)
        {
            InstructionManager.Instance.ToggleOrOpenCanvas();
        }
    }
}