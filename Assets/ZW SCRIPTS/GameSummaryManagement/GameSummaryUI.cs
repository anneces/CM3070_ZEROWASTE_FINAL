using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the end-of-game summary user interface in the Main Menu scene.
/// Calculates performance metrics (money spent/wasted, CO2 footprint, dishes cooked),
/// assigns grades, updates feedback visuals, and plays audio/VFX celebrations.
/// </summary>
public class GameSummaryUI : MonoBehaviour
{
    [Header("Canvases in Main Menu Scene")]
    [Tooltip("Reference to the standard main menu UI canvas.")]
    public GameObject mainMenuCanvas;

    [Tooltip("Reference to the end-game summary UI canvas.")]
    public GameObject gameSummaryCanvas;

    [Header("UI Metric Labels")]
    public TextMeshProUGUI moneySpentText;
    public TextMeshProUGUI moneyWastedText;
    public TextMeshProUGUI co2PointsText;
    public TextMeshProUGUI dishesCookedText;
    public TextMeshProUGUI finalGradeText;
    public TextMeshProUGUI performanceMessageText;

    [Header("Grade Images")]
    public Image gradeFeedbackImage;

    [Tooltip("Sprite displayed for Grade A performance.")]
    public Sprite happySprite;

    [Tooltip("Sprite displayed for Grade B or C performance.")]
    public Sprite smileySprite;

    [Tooltip("Sprite displayed for Grade F performance.")]
    public Sprite sadSprite;

    [Header("Confetti VFX")]
    public ParticleSystem leftConfettiVFX;
    public ParticleSystem rightConfettiVFX;

    [Header("Scene Settings")]
    [Tooltip("Name of the kitchen gameplay scene for reload/replay functionality.")]
    public string kitchenGameplaySceneName = "KitchenScene";

    #region Unity Lifecycle Methods

    private void Start()
    {
        // Check if returning from completing Day 5 in gameplay
        bool shouldShowSummary = PlayerPrefs.GetInt("ShowGameSummaryOnLoad", 0) == 1;

        if (shouldShowSummary)
        {
            // Reset the load flag to prevent summary from opening on normal main menu launches
            PlayerPrefs.SetInt("ShowGameSummaryOnLoad", 0);
            PlayerPrefs.Save();

            // Display the Summary Canvas and hide standard Main Menu UI
            if (mainMenuCanvas) mainMenuCanvas.SetActive(false);
            if (gameSummaryCanvas) gameSummaryCanvas.SetActive(true);

            // Compute metrics and update UI display
            CalculateAndDisplaySummary();
        }
        else
        {
            // Standard main menu startup
            if (gameSummaryCanvas) gameSummaryCanvas.SetActive(false);
            if (mainMenuCanvas) mainMenuCanvas.SetActive(true);
        }
    }

    #endregion

    #region Summary Evaluation & Display

    /// <summary>
    /// Fetches saved metrics from PlayerPrefs, calculates the final score and grade,
    /// and updates all relevant UI components and visual feedback.
    /// </summary>
    public void CalculateAndDisplaySummary()
    {
        // Step 1: Retrieve cumulative player stats from PlayerPrefs
        float totalMoneySpent = PlayerPrefs.GetFloat("TotalMoneySpent", 0.0f);
        float totalMoneyWasted = PlayerPrefs.GetFloat("TotalMoneyWasted", 0.0f);
        float totalCO2 = PlayerPrefs.GetFloat("TotalCO2", 0.0f);
        int dishesCooked = PlayerPrefs.GetInt("TotalDishesCooked", 0);

        // Step 2: Calculate overall performance score (0 to 100 scale)
        float wastePenalty = totalMoneyWasted * 1.5f;
        float co2Penalty = totalCO2 * 0.05f;

        float baseScore = 100f - wastePenalty - co2Penalty;
        float bonusPoints = dishesCooked * 15f; // Award 15 bonus points per completed meal
        float finalScore = Mathf.Clamp(baseScore + bonusPoints, 0f, 100f);

        // Log diagnostic values for debugging end-game calculations
        Debug.Log($"[SUMMARY EVALUATION] Dishes Cooked: {dishesCooked} | Spent: ${totalMoneySpent:F2} | Wasted: ${totalMoneyWasted:F2} | CO2: {totalCO2:F1} | Final Score: {finalScore}");

        // Step 3: Determine final letter grade
        string grade;

        // Check for explicit grade override (e.g., immediate failure triggers during gameplay)
        if (PlayerPrefs.HasKey("FinalGrade"))
        {
            grade = PlayerPrefs.GetString("FinalGrade", "F");
        }
        // Automatic failure if no meals were cooked
        else if (dishesCooked == 0)
        {
            grade = "F";
        }
        // Grade A: Cooked at least 3 meals with minimal waste or achieved high score
        else if (dishesCooked >= 3 && (totalMoneyWasted <= 5.0f || finalScore >= 80f))
        {
            grade = "A";
        }
        // Grade B: Moderate score or good productivity with low overall waste
        else if (finalScore >= 60f || (dishesCooked >= 2 && totalMoneyWasted < 10f))
        {
            grade = "B";
        }
        // Grade C: Minimum passing score or at least 1 dish cooked
        else if (finalScore >= 40f || dishesCooked >= 1)
        {
            grade = "C";
        }
        // Grade F: Excess waste or low overall performance score
        else
        {
            grade = "F";
        }

        // Step 4: Update text labels on the summary UI
        if (moneySpentText) moneySpentText.text = $"Total Money Spent: ${totalMoneySpent:F2}";
        if (moneyWastedText) moneyWastedText.text = $"Total Money Wasted: ${totalMoneyWasted:F2}";
        if (co2PointsText) co2PointsText.text = $"Total CO2 Points: {totalCO2:F1}";
        if (dishesCookedText) dishesCookedText.text = $"Dishes Cooked (Bonus): {dishesCooked} (+{bonusPoints} pts)";
        if (finalGradeText) finalGradeText.text = $"Grade: {grade}";

        // Step 5: Update feedback image sprites and performance messages
        ApplyGradeFeedback(grade);

        // Step 6: Trigger particle effects based on final grade
        TriggerConfetti(grade);
    }

    /// <summary>
    /// Sets performance message strings, feedback sprites, and schedules sound effects according to grade.
    /// </summary>
    /// <param name="grade">The calculated grade string ("A", "B", "C", or "F").</param>
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

    /// <summary>
    /// Plays left and right confetti particle effects for passing grades (A, B, C).
    /// </summary>
    private void TriggerConfetti(string grade)
    {
        if (grade == "A" || grade == "B" || grade == "C")
        {
            if (leftConfettiVFX != null) leftConfettiVFX.Play();
            if (rightConfettiVFX != null) rightConfettiVFX.Play();
        }
    }

    #endregion

    #region UI Button Handlers

    /// <summary>
    /// Replay Button Action: Closes the summary canvas and re-enables the standard main menu canvas.
    /// </summary>
    public void OnReplayButtonClicked()
    {
        AudioManager.Instance?.PlayUIClick();
        if (gameSummaryCanvas) gameSummaryCanvas.SetActive(false);
        if (mainMenuCanvas) mainMenuCanvas.SetActive(true);
    }

    /// <summary>
    /// Quit Button Action: Exits the application build.
    /// </summary>
    public void OnQuitButtonClicked()
    {
        AudioManager.Instance?.PlayUIClick();
        Debug.Log("[GameSummary] Exiting Application...");
        Application.Quit();
    }

    #endregion
}