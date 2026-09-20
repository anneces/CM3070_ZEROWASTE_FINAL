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
        float totalMoneySpent = PlayerPrefs.GetFloat("TotalMoneySpent", 0.0f);
        float totalMoneyWasted = PlayerPrefs.GetFloat("TotalMoneyWasted", 0.0f);
        float totalCO2 = PlayerPrefs.GetFloat("TotalCO2", 0.0f);
        int dishesCooked = PlayerPrefs.GetInt("TotalDishesCooked", 0);

        // 2. Adjust CO2 Weighting Local to Summary Calculation
        float wastePenalty = totalMoneyWasted * 1.5f;
        float co2Penalty = totalCO2 * 0.05f;

        float baseScore = 100f - wastePenalty - co2Penalty;
        float bonusPoints = dishesCooked * 15f; // Award 15 pts per cooked dish
        float finalScore = Mathf.Clamp(baseScore + bonusPoints, 0f, 100f);

        // Debug diagnostic log to pinpoint exact values during end-game summary evaluation
        Debug.Log($"[SUMMARY EVALUATION] Dishes Cooked: {dishesCooked} | Spent: ${totalMoneySpent:F2} | Wasted: ${totalMoneyWasted:F2} | CO2: {totalCO2:F1} | Final Score: {finalScore}");

        // 3. Clean Grade Evaluation
        string grade;

        // Check explicit override first (e.g. Failure setting from DayPhaseManager)
        if (PlayerPrefs.HasKey("FinalGrade"))
        {
            grade = PlayerPrefs.GetString("FinalGrade", "F");
        }
        // Fail if no meals were cooked
        else if (dishesCooked == 0)
        {
            grade = "F";
        }
        // Grade A: Cooking at least 3 dishes with minimal waste or achieving high overall score
        else if (dishesCooked >= 3 && (totalMoneyWasted <= 5.0f || finalScore >= 80f))
        {
            grade = "A";
        }
        // Grade B: Moderate score or good productivity with low waste
        else if (finalScore >= 60f || (dishesCooked >= 2 && totalMoneyWasted < 10f))
        {
            grade = "B";
        }
        // Grade C: Minimal passing score
        else if (finalScore >= 40f || dishesCooked >= 1)
        {
            grade = "C";
        }
        // Grade F: High waste or severe penalty
        else
        {
            grade = "F";
        }

        // 4. Update Text Labels
        if (moneySpentText) moneySpentText.text = $"Total Money Spent: ${totalMoneySpent:F2}";
        if (moneyWastedText) moneyWastedText.text = $"Total Money Wasted: ${totalMoneyWasted:F2}";
        if (co2PointsText) co2PointsText.text = $"Total CO2 Points: {totalCO2:F1}";
        if (dishesCookedText) dishesCookedText.text = $"Dishes Cooked (Bonus): {dishesCooked} (+{bonusPoints} pts)";
        if (finalGradeText) finalGradeText.text = $"Grade: {grade}";

        // 5. Update Images & Messages
        ApplyGradeFeedback(grade);

        // 6. Trigger Left and Right Confetti
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