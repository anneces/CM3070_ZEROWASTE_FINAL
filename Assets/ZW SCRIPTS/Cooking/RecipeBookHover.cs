using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RecipeBookHover : MonoBehaviour
{
    [Header("VFX & Audio Setup")]
    public ParticleSystem sparkleVFX;

    private void Start()
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.Stop();
        }
    }

    /// <summary>
    /// Accepts HoverEnterEventArgs for Unity's XR Interaction Toolkit event system
    /// </summary>
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (sparkleVFX != null && !sparkleVFX.isPlaying)
        {
            sparkleVFX.Play();
        }
    }

    /// <summary>
    /// Accepts HoverExitEventArgs for Unity's XR Interaction Toolkit event system
    /// </summary>
    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.Stop();
        }
    }

    /// <summary>
    /// Accepts SelectEnterEventArgs for Unity's XR Interaction Toolkit event system
    /// </summary>
    public void OnBookClicked(SelectEnterEventArgs args)
    {
        CookingManager.Instance?.OpenRecipeBook();
    }
}