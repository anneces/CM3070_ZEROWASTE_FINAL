using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Singleton manager handling the step-by-step tutorial instructional overlay canvas.
/// Controls tutorial progression, UI text updates, step counter formatting, and visual sprite illustrations.
/// </summary>
public class InstructionManager : MonoBehaviour
{
    public static InstructionManager Instance;

    /// <summary>
    /// Tutorial workflow steps representing each gameplay mechanic introduction.
    /// </summary>
    public enum GameStep
    {
        Welcome = 0,
        XRControls = 1,
        ShoppingTablet = 2,
        StorageUnit = 3,
        TVDashboard = 4,
        ClockPhaseTransition = 5,
        CookingStove = 6,
        TrashBin = 7,
        ResetButton = 8,
        Step9_GoodLuck = 9,
        Completed = 10
    }

    [Header("UI Canvas References")]
    [SerializeField] private GameObject instructionCanvas;
    [SerializeField] private TextMeshProUGUI stepTitleText;
    [SerializeField] private TextMeshProUGUI stepDescriptionText;
    [SerializeField] private TextMeshProUGUI stepProgressText;
    [SerializeField] private Image stepImageDisplay; // Image display on the instruction canvas
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nextButton;

    [Header("Step Visual Assets")]
    [Tooltip("Order must match GameStep enum indices: 0=Welcome through 9=GoodLuck")]
    [SerializeField] private Sprite[] stepSprites; // Array of tutorial illustrations/sprites

    [Header("Current Progress")]
    [Tooltip("The current active step in the tutorial sequence.")]
    public GameStep currentStep = GameStep.Welcome;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        // Enforce Singleton instance pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ensure instruction canvas is active at game start
        if (instructionCanvas != null) instructionCanvas.SetActive(true);

        // Bind button click listeners safely
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseCanvas);
            closeButton.onClick.AddListener(CloseCanvas);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextStep);
            nextButton.onClick.AddListener(NextStep);
        }

        // Initialize UI content for the starting step
        UpdateInstructionUI();
    }

    #endregion

    #region Canvas & Visibility Controls

    /// <summary>
    /// Toggles the instruction canvas visibility. Restarts tutorial from Step 0 if re-opened after completion.
    /// </summary>
    public void ToggleOrOpenCanvas()
    {
        if (instructionCanvas != null)
        {
            bool isActive = instructionCanvas.activeSelf;
            instructionCanvas.SetActive(!isActive);

            if (!isActive)
            {
                // Reset back to Welcome step if re-opening after full completion
                if (currentStep == GameStep.Completed)
                {
                    currentStep = GameStep.Welcome;
                }

                UpdateInstructionUI();
                AudioManager.Instance?.PlayUIClick();
            }
        }
    }

    /// <summary>
    /// Closes the tutorial canvas and sets the current state to Completed.
    /// </summary>
    public void CloseCanvas()
    {
        if (instructionCanvas != null)
        {
            instructionCanvas.SetActive(false);
            currentStep = GameStep.Completed;
            AudioManager.Instance?.PlayUIClick();
        }
    }

    #endregion

    #region Tutorial Step Navigation

    /// <summary>
    /// Sets the tutorial to a specific step if it represents forward progression.
    /// </summary>
    /// <param name="newStep">The target GameStep enum value.</param>
    public void SetStep(GameStep newStep)
    {
        if ((int)newStep > (int)currentStep)
        {
            currentStep = newStep;

            // Automatically reveal canvas if it was closed during phase transitions
            if (instructionCanvas != null && !instructionCanvas.activeSelf && currentStep != GameStep.Completed)
            {
                instructionCanvas.SetActive(true);
            }

            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
    }

    /// <summary>
    /// Advances the tutorial to the next chronological step or closes the canvas at final step.
    /// </summary>
    public void NextStep()
    {
        if (currentStep < GameStep.Step9_GoodLuck)
        {
            currentStep++;
            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
        else if (currentStep == GameStep.Step9_GoodLuck)
        {
            currentStep = GameStep.Completed;
            CloseCanvas();
        }
    }

    /// <summary>
    /// Reverts the tutorial to the previous step.
    /// </summary>
    public void PreviousStep()
    {
        if (currentStep > GameStep.Welcome)
        {
            currentStep--;
            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
    }

    #endregion

    #region UI Rendering & Updating

    /// <summary>
    /// Refreshes title text, body copy, images, and button states matching the current GameStep.
    /// </summary>
    private void UpdateInstructionUI()
    {
        if (currentStep == GameStep.Completed)
        {
            CloseCanvas();
            return;
        }

        // Set title and body copy depending on current step
        switch (currentStep)
        {
            case GameStep.Welcome:
                SetText("Welcome to ZeroWaste Kitchen!",
                        "Learn to manage food sustainably! Your goal is to prepare delicious recipes while properly storing ingredients, minimizing food waste, and staying within budget.");
                break;

            case GameStep.XRControls:
                SetText("Step 1: XR Movement & Controls",
                        "• Navigation: Use the Joystick to move around the kitchen.\n• Interact UI: Press the Trigger button to click UI buttons.\n• Grab Items: Use the Grip button to grab and hold ingredients.\n• Crouch (Right Hand Primary Button - A/X): Crouch down to reach items on the floor.\n• Stand Tall (Right Hand Secondary Button - B/Y): Gain extra height to reach high fridge or pantry shelves.");
                break;

            case GameStep.ShoppingTablet:
                SetText("Step 2: Shopping Tablet Zone",
                        "Order fresh food items to prepare your meals. Keep an eye on your budget while selecting ingredients.");
                break;

            case GameStep.StorageUnit:
                SetText("Step 3: Food Storage",
                        "Place your purchased items into their ideal storage zones (Fridge, Freezer, or Pantry). Storing items incorrectly doubles their spoilage rate!");
                break;

            case GameStep.TVDashboard:
                SetText("Step 4: TV Dashboard",
                        "Check the TV screen to monitor the food items freshness as they change day by day.");
                break;

            case GameStep.ClockPhaseTransition:
                SetText("Step 5: Clock & Day Phase",
                        "Interact with the clock to advance to the next day phase. Watch how food freshness and storage conditions progress over time.");
                break;

            case GameStep.CookingStove:
                SetText("Step 6: Cooking Stove",
                        "Select a target recipe at the stove station. Place fresh ingredients into the pot to cook your dish—be careful not to add spoiled items!\n\nOnce cooked, click on the spawned dish to eat it!");
                break;

            case GameStep.TrashBin:
                SetText("Step 7: Utility Tools (Trash Bin)",
                        "Dispose of spoiled, unusable, or incorrect food items in the trash bin to keep your workspace clear.");
                break;

            case GameStep.ResetButton:
                SetText("Step 8: Utility Tools (Reset Button)",
                        "Use the reset button to return active ingredients from the stove area back to their original spawn points.");
                break;

            case GameStep.Step9_GoodLuck:
                SetText("Step 9: Ready to Start!",
                        "You're all set! Every small choice in the kitchen helps build a greener, zero-waste future.\n\nTake your time, plan your meals wisely, and most importantly—have fun cooking! If you are still unsure, click the chalkboard for help!");
                break;
        }

        // Toggle action buttons: Show Close/Start button on final step, Next button on earlier steps
        bool isLastStep = (currentStep == GameStep.Step9_GoodLuck);

        if (nextButton != null) nextButton.gameObject.SetActive(!isLastStep);
        if (closeButton != null) closeButton.gameObject.SetActive(isLastStep);

        // Update corresponding step illustration sprite
        int stepIndex = (int)currentStep;
        if (stepImageDisplay != null && stepSprites != null && stepIndex < stepSprites.Length)
        {
            if (stepSprites[stepIndex] != null)
            {
                stepImageDisplay.sprite = stepSprites[stepIndex];
                stepImageDisplay.gameObject.SetActive(true);
            }
            else
            {
                stepImageDisplay.gameObject.SetActive(false); // Hide image container if sprite is unassigned
            }
        }

        // Format step progress text (e.g., "Step 1 / 10")
        int totalVisibleSteps = Enum.GetValues(typeof(GameStep)).Length - 1; // Excludes Completed state
        if (stepProgressText != null)
        {
            stepProgressText.text = $"Step {stepIndex + 1} / {totalVisibleSteps}";
        }
    }

    /// <summary>
    /// Helper method to assign header and body text components.
    /// </summary>
    private void SetText(string title, string description)
    {
        if (stepTitleText != null) stepTitleText.text = title;
        if (stepDescriptionText != null) stepDescriptionText.text = description;
    }

    #endregion
}