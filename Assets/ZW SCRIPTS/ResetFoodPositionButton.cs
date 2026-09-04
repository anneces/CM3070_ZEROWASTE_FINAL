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
    [Tooltip("Distance the top cylinder moves down along its local Y axis when pressed.")]
    public float pressDistance = 0.015f;
    public float pressSpeed = 10f;

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

    private void Start()
    {
        if (buttonPressTransform != null)
        {
            initialLocalPos = buttonPressTransform.localPosition;
            pressedLocalPos = initialLocalPos - new Vector3(0f, pressDistance, 0f);
        }
    }

    /// <summary>
    /// Hook this method to XR Interactor's Select Entered (or Hover Entered).
    /// </summary>
    public void OnButtonPressed()
    {
        // Animate button moving down
        if (buttonPressTransform != null)
        {
            StartButtonAnimation(pressedLocalPos);
        }

        ResetAllFoodPositions();
    }

    /// <summary>
    /// Hook this method to XR Interactor's Select Exited (or Hover Exited).
    /// </summary>
    public void OnButtonReleased()
    {
        // Animate button returning up
        if (buttonPressTransform != null)
        {
            StartButtonAnimation(initialLocalPos);
        }
    }

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

        if (buttonAudioSource != null && pressSFX != null)
        {
            buttonAudioSource.PlayOneShot(pressSFX);
        }

        if (resetVFX != null)
        {
            resetVFX.Play();
        }

        Debug.Log($"[ResetButton] Reset {foodItems.Length} food items.", this);
    }

    private void StartButtonAnimation(Vector3 targetPos)
    {
        if (animateCoroutine != null) StopCoroutine(animateCoroutine);
        animateCoroutine = StartCoroutine(AnimatePress(targetPos));
    }

    private IEnumerator AnimatePress(Vector3 targetPos)
    {
        while (Vector3.Distance(buttonPressTransform.localPosition, targetPos) > 0.0001f)
        {
            buttonPressTransform.localPosition = Vector3.Lerp(buttonPressTransform.localPosition, targetPos, Time.deltaTime * pressSpeed);
            yield return null;
        }
        buttonPressTransform.localPosition = targetPos;
    }
}