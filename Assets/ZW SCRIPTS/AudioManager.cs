using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip uiClickClip;
    public AudioClip ingredientDropClip;
    public AudioClip trashDropClip;
    public AudioClip phaseTransitionClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayUIClick()
    {
        PlaySFX(uiClickClip);
    }

    public void PlayDelayedPhaseAlert(float delaySeconds = 1.0f)
    {
        StartCoroutine(DelayedPhaseAlertRoutine(delaySeconds));
    }

    private IEnumerator DelayedPhaseAlertRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(phaseTransitionClip);
    }
}