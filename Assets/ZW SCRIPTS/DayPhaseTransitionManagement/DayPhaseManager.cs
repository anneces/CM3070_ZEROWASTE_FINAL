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

    // Event broadcast for external UI components (Clock, Tablet, TV)
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
                Debug.Log("End of 5-Day Simulation Reached! Loading Main Menu Summary...");

                // 1. Base performance metrics from TrashBinController
                float wastedMoney = TrashBinController.Instance != null ? TrashBinController.Instance.totalMoneyWasted : 0f;
                float totalCO2 = TrashBinController.Instance != null ? TrashBinController.Instance.totalCO2 : 0f;

                // 2. Read total dishes cooked directly from PlayerPrefs saved by StoveController
                int dishesCooked = PlayerPrefs.GetInt("DishesCooked", 0);

                // 3. Uncooked & Spoiled Food Penalty Sweep across the scene
                FoodItem[] remainingItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
                foreach (FoodItem item in remainingItems)
                {
                    if (item != null)
                    {
                        // Penalize spoiled food left untrashed or uncooked food left over at end of Day 5
                        if (item.isSpoiled || !item.isCooked)
                        {
                            wastedMoney += item.price;
                            totalCO2 += item.co2Value;
                        }
                    }
                }

                // 4. Save metrics for Summary UI
                PlayerPrefs.SetFloat("TotalMoneyWasted", wastedMoney);
                PlayerPrefs.SetFloat("TotalCO2", totalCO2);
                PlayerPrefs.SetInt("DishesCooked", dishesCooked);

                // 5. Minimum Meals Requirement Check
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

        // 1. Notify all food items in scene to execute decay tick on every phase change
        FoodItem[] foodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        foreach (FoodItem food in foodItems)
        {
            if (food != null)
            {
                food.OnPhaseTick();
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
            // Directly passes local DayPhase enum to PhaseWarningManager
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