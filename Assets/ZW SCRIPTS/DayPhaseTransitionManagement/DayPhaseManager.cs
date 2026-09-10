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

    // Event broadcast for external UI components (Clock, Tablet, TV, Recipe Book)
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
        // Reset dishes cooked counter at the beginning of a game session
        PlayerPrefs.SetInt("DishesCooked", 0);
        PlayerPrefs.Save();

        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);

        UpdateEnvironment();
        UpdateClockUI();
        UpdatePhaseRestrictions();

        // Broadcast phase update on initial start so all subscribers sync correctly
        OnPhaseChanged?.Invoke();
    }

    public void ConfirmNextPhase()
    {
        ConfirmAdvancePhase();
    }

    public void OpenPhaseChangeConfirmation()
    {
        AudioManager.Instance?.PlayUIClick();
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(true);
    }

    public void ConfirmAdvancePhase()
    {
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);

        AdvancePhase();
    }

    public void CancelPhaseChange()
    {
        AudioManager.Instance?.PlayUIClick();
        if (confirmationPopupModal != null)
            confirmationPopupModal.SetActive(false);
    }

    private void AdvancePhase()
    {
        // Track previous phase to handle specific transition rules
        DayPhase previousPhase = currentPhase;

        // Play phase transition audio
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
            }
            else
            {
                Debug.Log("End of 5-Day Simulation Reached! Loading Main Menu Summary...");

                float wastedMoney = TrashBinController.Instance != null ? TrashBinController.Instance.totalMoneyWasted : 0f;
                float totalCO2 = TrashBinController.Instance != null ? TrashBinController.Instance.totalCO2 : 0f;

                int dishesCooked = PlayerPrefs.GetInt("DishesCooked", 0);

                FoodItem[] remainingItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
                foreach (FoodItem item in remainingItems)
                {
                    if (item != null)
                    {
                        if (item.isSpoiled || !item.isCooked)
                        {
                            wastedMoney += item.price;
                            totalCO2 += item.co2Value;
                        }
                    }
                }

                PlayerPrefs.SetFloat("TotalMoneyWasted", wastedMoney);
                PlayerPrefs.SetFloat("TotalCO2", totalCO2);
                PlayerPrefs.SetInt("DishesCooked", dishesCooked);

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

        // 1. Notify all food items in scene to execute decay tick
        // FIX: Skip freshness decay when transitioning from Morning -> Afternoon
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

        // 2. Refresh TV Display UI values
        if (TVDisplayController.Instance != null)
        {
            TVDisplayController.Instance.RefreshDisplay();
        }

        // 3. Broadcast phase update event to all subscribed canvases
        OnPhaseChanged?.Invoke();
    }

    private void UpdatePhaseRestrictions()
    {
        if (PhaseWarningManager.Instance != null)
        {
            PhaseWarningManager.Instance.UpdatePhaseRestrictions(currentPhase);
        }
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