using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ResetFoodPositionsButton : MonoBehaviour
{
    [Header("3D Button Hierarchy")]
    [Tooltip("The moving top cylinder mesh.")]
    public Transform buttonPressTransform;

    [Header("Press Animation Settings")]
    [Tooltip("Target local Y position when pressed (set to 0.5 for your scale setup).")]
    public float pressedLocalY = 0.5f;

    [Tooltip("Duration of the downward/upward move (in seconds).")]
    public float animationDuration = 0.12f;

    [Tooltip("How long the button stays held down before automatically bouncing back up.")]
    public float autoReleaseDelay = 0.2f;

    [Header("Respawn Configuration")]
    [Tooltip("List of spawn points where food items will be relocated.")]
    public List<Transform> respawnPoints = new List<Transform>();

    [Header("Optional Feedback")]
    public AudioSource buttonAudioSource;
    public AudioClip pressSFX;
    public ParticleSystem resetVFX;

    private Vector3 initialLocalPos;
    private Vector3 pressedLocalPos;
    private Coroutine animateCoroutine;

    /// <summary>
    /// Caches the default and pressed local positions for the 3D button mesh.
    /// </summary>
    private void Start()
    {
        if (buttonPressTransform != null)
        {
            initialLocalPos = buttonPressTransform.localPosition; // Starts at Y = 0.9
            pressedLocalPos = new Vector3(initialLocalPos.x, pressedLocalY, initialLocalPos.z);
        }
    }

    /// <summary>
    /// Event handler for VR interaction; triggers button press audio, mechanical animation sequence, and food repositioning.
    /// </summary>
    public void OnButtonPressed()
    {
        // Plays threedbutton_press_cut SFX via AudioManager
        AudioManager.Instance?.Play3DButtonPressed();

        if (animateCoroutine != null) StopCoroutine(animateCoroutine);
        animateCoroutine = StartCoroutine(FullButtonPressCycle());

        ResetAllFoodPositions();
    }

    /// <summary>
    /// Manual release trigger that animates the button mesh back to its resting state.
    /// </summary>
    public void OnButtonReleased()
    {
        if (buttonPressTransform != null)
        {
            if (animateCoroutine != null) StopCoroutine(animateCoroutine);
            animateCoroutine = StartCoroutine(AnimateToPosition(initialLocalPos));
        }
    }

    /// <summary>
    /// Manages the full physical button press animation cycle including down-stroke, hold delay, and up-stroke.
    /// </summary>
    private IEnumerator FullButtonPressCycle()
    {
        if (buttonPressTransform == null) yield break;

        // 1. Move Down
        yield return AnimateToPosition(pressedLocalPos);

        // 2. Hold momentarily
        yield return new WaitForSeconds(autoReleaseDelay);

        // 3. Move back Up
        yield return AnimateToPosition(initialLocalPos);
    }

    /// <summary>
    /// Smoothly interpolates the button mesh transform toward a targeted local position over time.
    /// </summary>
    /// <param name="targetPos">The target local position vector.</param>
    private IEnumerator AnimateToPosition(Vector3 targetPos)
    {
        Vector3 startPos = buttonPressTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            buttonPressTransform.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        buttonPressTransform.localPosition = targetPos;
    }

    /// <summary>
    /// Distributes all scene food items across defined respawn points, resets their velocity, and triggers feedback effects.
    /// </summary>
    public void ResetAllFoodPositions()
    {
        FoodItem[] foodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);

        if (foodItems == null || foodItems.Length == 0)
        {
            Debug.Log("[ResetButton] No food items found in the scene to reset.", this);
            return;
        }

        if (respawnPoints == null || respawnPoints.Count == 0)
        {
            Debug.LogWarning("[ResetButton] No respawn points assigned!", this);
            return;
        }

        for (int i = 0; i < foodItems.Length; i++)
        {
            FoodItem food = foodItems[i];
            Transform targetSpawn = respawnPoints[i % respawnPoints.Count];

            Rigidbody rb = food.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            food.transform.position = targetSpawn.position + new Vector3(0f, 0.05f * (i / respawnPoints.Count), 0f);
            food.transform.rotation = targetSpawn.rotation;
        }

        // Play SFX directly from AudioManager if local fields are unassigned
        if (buttonAudioSource != null && pressSFX != null)
        {
            buttonAudioSource.PlayOneShot(pressSFX);
        }
        else
        {
            AudioManager.Instance?.Play3DButtonPressed();
        }

        if (resetVFX != null)
        {
            resetVFX.Play();
        }

        Debug.Log($"[ResetButton] Reset {foodItems.Length} food items.", this);
    }
}