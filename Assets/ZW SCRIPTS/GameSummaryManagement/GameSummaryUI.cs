using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameSummaryUI : MonoBehaviour
{
    [Header("Canvases in Main Menu Scene")]
    public GameObject mainMenuCanvas;       // Your default Main Menu UI
    public GameObject gameSummaryCanvas;    // This Summary UI

    [Header("UI Metric Labels")]
    public TextMeshProUGUI moneySpentText;
    public TextMeshProUGUI moneyWastedText;
    public TextMeshProUGUI co2PointsText;
    public TextMeshProUGUI dishesCookedText;
    public TextMeshProUGUI finalGradeText;
    public TextMeshProUGUI performanceMessageText;

    [Header("Grade Images")]
    public Image gradeFeedbackImage;
    public Sprite happySprite;   // Grade A
    public Sprite smileySprite;  // Grade B / C
    public Sprite sadSprite;     // Grade F

    [Header("Confetti VFX")]
    public ParticleSystem leftConfettiVFX;
    public ParticleSystem rightConfettiVFX;

    [Header("Scene Settings")]
    public string kitchenGameplaySceneName = "KitchenScene";

    private void Start()
    {
        // Check if returning from Day 5 completion
        bool shouldShowSummary = PlayerPrefs.GetInt("ShowGameSummaryOnLoad", 0) == 1;

        if (shouldShowSummary)
        {
            // Reset flag
            PlayerPrefs.SetInt("ShowGameSummaryOnLoad", 0);
            PlayerPrefs.Save();

            // Show Summary Canvas & hide standard Main Menu UI
            if (mainMenuCanvas) mainMenuCanvas.SetActive(false);
            if (gameSummaryCanvas) gameSummaryCanvas.SetActive(true);

            CalculateAndDisplaySummary();
        }
        else
        {
            // Standard main menu launch
            if (gameSummaryCanvas) gameSummaryCanvas.SetActive(false);
            if (mainMenuCanvas) mainMenuCanvas.SetActive(true);
        }
    }

    public void CalculateAndDisplaySummary()
    {
        // 1. Retrieve saved totals
        float totalMoneySpent = PlayerPrefs.GetFloat("TotalMoneySpent", 150.0f);
        float totalMoneyWasted = PlayerPrefs.GetFloat("TotalMoneyWasted", 25.0f);
        float totalCO2 = PlayerPrefs.GetFloat("TotalCO2", 12.5f);
        int dishesCooked = PlayerPrefs.GetInt("TotalDishesCooked", 8);

        // 2. Calculate Final Score & Grade
        float baseScore = 100f - (totalMoneyWasted * 1.5f) - (totalCO2 * 0.5f);
        float bonusPoints = dishesCooked * 10f;
        float finalScore = Mathf.Max(0f, baseScore + bonusPoints);

        string grade;
        if (finalScore >= 90f) grade = "A";
        else if (finalScore >= 70f) grade = "B";
        else if (finalScore >= 50f) grade = "C";
        else grade = "F";

        // 3. Update Text Labels
        if (moneySpentText) moneySpentText.text = $"Total Money Spent: ${totalMoneySpent:F2}";
        if (moneyWastedText) moneyWastedText.text = $"Total Money Wasted: ${totalMoneyWasted:F2}";
        if (co2PointsText) co2PointsText.text = $"Total CO2 Points: {totalCO2:F1}";
        if (dishesCookedText) dishesCookedText.text = $"Dishes Cooked (Bonus): {dishesCooked} (+{bonusPoints} pts)";
        if (finalGradeText) finalGradeText.text = $"Grade: {grade}";

        // 4. Update Images & Messages (schedules delayed audio call to respect initialization)
        ApplyGradeFeedback(grade);

        // 5. Trigger Left and Right Confetti
        TriggerConfetti(grade);
    }

    private void ApplyGradeFeedback(string grade)
    {
        switch (grade)
        {
            case "A":
                if (performanceMessageText) performanceMessageText.text = "Excellent Job!";
                if (gradeFeedbackImage && happySprite) gradeFeedbackImage.sprite = happySprite;
                break;

            case "B":
            case "C":
                if (performanceMessageText) performanceMessageText.text = "Great Work!";
                if (gradeFeedbackImage && smileySprite) gradeFeedbackImage.sprite = smileySprite;
                break;

            default: // Grade F
                if (performanceMessageText) performanceMessageText.text = "You can do better!";
                if (gradeFeedbackImage && sadSprite) gradeFeedbackImage.sprite = sadSprite;
                break;
        }

        // Delay audio playback by one frame to prevent muted/skipped audio during scene initialization
        StartCoroutine(PlayGradeAudioDelayed(grade));
    }

    /// <summary>
    /// Waits until the end of the frame before requesting audio playback from AudioManager.
    /// Ensures volume levels and AudioSources are fully ready.
    /// </summary>
    private IEnumerator PlayGradeAudioDelayed(string grade)
    {
        yield return new WaitForEndOfFrame();

        if (AudioManager.Instance == null) yield break;

        switch (grade)
        {
            case "A":
                AudioManager.Instance.PlayGradeA();
                break;
            case "B":
            case "C":
                AudioManager.Instance.PlayGradeBC();
                break;
            default:
                AudioManager.Instance.PlayGradeF();
                break;
        }
    }

    private void TriggerConfetti(string grade)
    {
        if (grade == "A" || grade == "B" || grade == "C")
        {
            if (leftConfettiVFX != null) leftConfettiVFX.Play();
            if (rightConfettiVFX != null) rightConfettiVFX.Play();
        }
    }

    // --- Button Actions ---

    /// <summary>
    /// Replay Button: Closes Summary and opens Main Menu Canvas.
    /// </summary>
    public void OnReplayButtonClicked()
    {
        AudioManager.Instance?.PlayUIClick();
        if (gameSummaryCanvas) gameSummaryCanvas.SetActive(false);
        if (mainMenuCanvas) mainMenuCanvas.SetActive(true);
    }

    /// <summary>
    /// Quit Button: Exits application.
    /// </summary>
    public void OnQuitButtonClicked()
    {
        AudioManager.Instance?.PlayUIClick();
        Debug.Log("[GameSummary] Exiting Application...");
        Application.Quit();
    }
}