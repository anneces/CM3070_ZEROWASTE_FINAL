using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RecipeBookHover : MonoBehaviour
{
    [Header("VFX Setup")]
    public ParticleSystem sparkleVFX;

    private void Start()
    {
        if (sparkleVFX != null) sparkleVFX.Stop();
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (sparkleVFX != null && !sparkleVFX.isPlaying)
        {
            sparkleVFX.Play();
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (sparkleVFX != null)
        {
            sparkleVFX.Stop();
        }
    }

    public void OnBookClicked(SelectEnterEventArgs args)
    {
        CookingManager.Instance?.OpenRecipeBook();
    }
}