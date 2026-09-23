using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    public static event Action OnPhaseChanged;

    [Header("Day & Phase Tracking")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int maxDays = 5;
    [SerializeField] private DayPhase currentPhase = DayPhase.Morning;

    [Header("UI References")]
    [SerializeField] private TMP_Text clockDisplayText;
    [SerializeField] private GameObject confirmationPopupModal;

    [Header("Scene Transition Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

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

    /// <summary>
    /// Initializes the singleton instance for global access and destroys duplicate manager instances.
    /// </summary>
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

    /// <summary>
    /// Resets persistent run metrics, hides modal popups, and initializes environment lighting and UI.
    /// </summary>
    private void Start()
    {
        // Reset counters at the start of a new run session
        PlayerPrefs.SetInt("TotalDishesCooked", 0);
        PlayerPrefs.Save();

        if (TrashBinController.Instance != null)
        {
            TrashBinController.Instance.ResetCounters();
        }

        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);

        UpdateEnvironment();
        UpdateClockUI();
        UpdatePhaseRestrictions();

        OnPhaseChanged?.Invoke();
    }

    /// <summary>
    /// Public helper wrapper that executes phase advancement directly.
    /// </summary>
    public void ConfirmNextPhase()
    {
        ConfirmAdvancePhase();
    }

    /// <summary>
    /// Plays UI interaction audio and opens the phase confirmation modal.
    /// </summary>
    public void OpenPhaseChangeConfirmation()
    {
        AudioManager.Instance?.PlayUIClick();
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(true);
    }

    /// <summary>
    /// Closes the confirmation modal and executes phase progression logic.
    /// </summary>
    public void ConfirmAdvancePhase()
    {
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);

        AdvancePhase();
    }

    /// <summary>
    /// Dismisses the phase transition confirmation modal and plays button click audio.
    /// </summary>
    public void CancelPhaseChange()
    {
        AudioManager.Instance?.PlayUIClick();
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);
    }

    /// <summary>
    /// Manages the full cycle between day phases, updates food freshness, resets cooking equipment,
    /// and handles game-over summary calculations when completing the 5-day simulation.
    /// </summary>
    private void AdvancePhase()
    {
        if (StoveController.Instance != null)
        {
            StoveController.Instance.ResetStove();
        }

        DayPhase previousPhase = currentPhase;
        AudioManager.Instance?.PlayPhaseTransition();

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

                StoveController.Instance?.ResetStoveToDefault();
            }
            else
            {
                Debug.Log("End of 5-Day Simulation Reached! Loading Main Menu Summary...");

                float wastedMoney = TrashBinController.Instance != null ? TrashBinController.Instance.totalMoneyWasted : 0f;
                float totalCO2 = TrashBinController.Instance != null ? TrashBinController.Instance.totalCO2 : 0f;

                int dishesCooked = PlayerPrefs.GetInt("TotalDishesCooked", 0);

                // Check un-trashed items remaining on counters at simulation end
                FoodItem[] remainingItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
                foreach (FoodItem item in remainingItems)
                {
                    if (item != null)
                    {
                        if (item.isSpoiled || item.isExpired || item.currentFreshnessDays <= 0)
                        {
                            wastedMoney += item.price;
                            totalCO2 += item.co2Points;
                        }
                    }
                }

                PlayerPrefs.SetFloat("TotalMoneyWasted", wastedMoney);
                PlayerPrefs.SetFloat("TotalCO2", totalCO2);
                PlayerPrefs.SetInt("TotalDishesCooked", dishesCooked);

                if (dishesCooked == 0)
                {
                    PlayerPrefs.SetString("FinalGrade", "F");
                    PlayerPrefs.SetString("GradeFeedback", "Failed: No meals were cooked during the 5 days!");
                }
                else
                {
                    PlayerPrefs.DeleteKey("FinalGrade");
                    PlayerPrefs.DeleteKey("GradeFeedback");
                }

                PlayerPrefs.SetInt("ShowGameSummaryOnLoad", 1);
                PlayerPrefs.Save();

                Time.timeScale = 1f;
                SceneManager.LoadScene(mainMenuSceneName);
                return;
            }
        }

        UpdateEnvironment();
        UpdateClockUI();
        UpdatePhaseRestrictions();

        if (previousPhase == DayPhase.Morning && currentPhase == DayPhase.Afternoon)
        {
            Debug.Log("[DayPhaseManager] Transitioning Morning -> Afternoon: Freshness degradation skipped as storage was locked.");
        }
        else
        {
            FoodItem[] foodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            foreach (FoodItem food in foodItems)
            {
                if (food != null)
                {
                    food.OnPhaseTick();
                }
            }
        }

        if (TVDisplayController.Instance != null)
        {
            TVDisplayController.Instance.RefreshDisplay();
        }

        OnPhaseChanged?.Invoke();
    }

    /// <summary>
    /// Notifies the PhaseWarningManager to update active interaction limits based on the new phase.
    /// </summary>
    private void UpdatePhaseRestrictions()
    {
        if (PhaseWarningManager.Instance != null)
        {
            PhaseWarningManager.Instance.UpdatePhaseRestrictions(currentPhase);
        }
    }

    /// <summary>
    /// Formats and updates the HUD clock text with current day and phase information.
    /// </summary>
    private void UpdateClockUI()
    {
        if (clockDisplayText != null)
        {
            clockDisplayText.text = $"DAY {currentDay} / {maxDays}\n<size=80%>{currentPhase}</size>";
        }
    }

    /// <summary>
    /// Adjusts scene skyboxes, directional light color, and intensity to reflect the current phase of the day.
    /// </summary>
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