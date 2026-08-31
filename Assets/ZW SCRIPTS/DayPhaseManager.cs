using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DayPhase
{
    Morning,
    Afternoon,
    Evening
}

public class DayPhaseManager : MonoBehaviour
{
    public static DayPhaseManager Instance { get; private set; }

    // Event broadcast for external UI components (Clock, Tablet, TV)
    public static event Action OnPhaseChanged;

    [Header("Day & Phase Tracking")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int maxDays = 5;
    [SerializeField] private DayPhase currentPhase = DayPhase.Morning;

    [Header("UI References")]
    [SerializeField] private TMP_Text clockDisplayText;
    [SerializeField] private GameObject confirmationPopupModal;

    [Header("Lighting References")]
    [SerializeField] private Light mainDirectionalLight;

    [Header("Morning Preset")]
    [SerializeField] private Color morningLightColor = new Color(1f, 0.85f, 0.7f);
    [SerializeField] private float morningIntensity = 1.0f;
    [SerializeField] private Material morningSkybox;

    [Header("Afternoon Preset")]
    [SerializeField] private Color afternoonLightColor = new Color(1f, 0.95f, 0.85f);
    [SerializeField] private float afternoonIntensity = 1.3f;
    [SerializeField] private Material afternoonSkybox;

    [Header("Evening Preset")]
    [SerializeField] private Color eveningLightColor = new Color(0.85f, 0.45f, 0.25f);
    [SerializeField] private float eveningIntensity = 0.5f;
    [SerializeField] private Material eveningSkybox;

    public int CurrentDay => currentDay;
    public DayPhase CurrentPhase => currentPhase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);

        UpdateEnvironment();
        UpdateClockUI();
    }

    /// <summary>
    /// Alias method to prevent CS1061 errors from external scripts calling ConfirmNextPhase.
    /// </summary>
    public void ConfirmNextPhase()
    {
        ConfirmAdvancePhase();
    }

    /// <summary>
    /// Opens the confirmation modal ("Move to next phase?").
    /// </summary>
    public void OpenPhaseChangeConfirmation()
    {
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(true);
    }

    /// <summary>
    /// Called when player confirms "Yes" on the confirmation popup modal.
    /// </summary>
    public void ConfirmAdvancePhase()
    {
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);

        AdvancePhase();
    }

    /// <summary>
    /// Called when player clicks "No / Cancel" on the confirmation popup modal.
    /// </summary>
    public void CancelPhaseChange()
    {
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);
    }

    private void AdvancePhase()
    {
        if (currentPhase == DayPhase.Morning)
        {
            currentPhase = DayPhase.Afternoon;
        }
        else if (currentPhase == DayPhase.Afternoon)
        {
            currentPhase = DayPhase.Evening;
        }
        else if (currentPhase == DayPhase.Evening)
        {
            if (currentDay < maxDays)
            {
                currentDay++;
                currentPhase = DayPhase.Morning;
            }
            else
            {
                Debug.Log("5-Day Simulation Complete!");
                // Add end of simulation / score logic here
                return;
            }
        }

        UpdateEnvironment();
        UpdateClockUI();

        // 1. Notify all food items in scene to execute decay tick
        FoodItem[] foodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        foreach (FoodItem food in foodItems)
        {
            food.OnPhaseTick();
        }

        // 2. Refresh TV Display UI values
        if (TVDisplayController.Instance != null)
        {
            TVDisplayController.Instance.RefreshDisplay();
        }

        // 3. Broadcast phase update event to all subscribed canvases
        OnPhaseChanged?.Invoke();
    }

    private void UpdateClockUI()
    {
        if (clockDisplayText != null)
        {
            clockDisplayText.text = $"DAY {currentDay} / {maxDays}\n<size=80%>{currentPhase}</size>";
        }
    }

    private void UpdateEnvironment()
    {
        if (mainDirectionalLight == null) return;

        switch (currentPhase)
        {
            case DayPhase.Morning:
                mainDirectionalLight.color = morningLightColor;
                mainDirectionalLight.intensity = morningIntensity;
                if (morningSkybox != null) RenderSettings.skybox = morningSkybox;
                break;

            case DayPhase.Afternoon:
                mainDirectionalLight.color = afternoonLightColor;
                mainDirectionalLight.intensity = afternoonIntensity;
                if (afternoonSkybox != null) RenderSettings.skybox = afternoonSkybox;
                break;

            case DayPhase.Evening:
                mainDirectionalLight.color = eveningLightColor;
                mainDirectionalLight.intensity = eveningIntensity;
                if (eveningSkybox != null) RenderSettings.skybox = eveningSkybox;
                break;
        }

        DynamicGI.UpdateEnvironment();
    }
}