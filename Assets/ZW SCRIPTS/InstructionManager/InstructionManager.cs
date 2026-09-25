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
    /// Index 0: Welcome
    /// Index 1: XR Movement & Primary Interactions (xr_grip_trigger_joystick)
    /// Index 2: Height Adjustment Controls (xr_stand_crouch)
    /// Index 3: Shopping Tablet (tablet)
    /// Index 4: Food Storage (storage)
    /// Index 5: TV Dashboard (tvdsbd)
    /// Index 6: Clock & Day Phase (clock)
    /// Index 7: Cooking Stove (stoveck)
    /// Index 8: Trash Bin (trash)
    /// Index 9: Reset Button (resetbtn)
    /// Index 10: Ready to Start / Good Luck
    /// Index 11: Completed (Canvas Closed)
    /// </summary>
    public enum GameStep
    {
        Welcome = 0,
        XRControls = 1,
        XRCrouchStand = 2,
        ShoppingTablet = 3,
        StorageUnit = 4,
        TVDashboard = 5,
        ClockPhaseTransition = 6,
        CookingStove = 7,
        TrashBin = 8,
        ResetButton = 9,
        Step10_GoodLuck = 10,
        Completed = 11
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
    [Tooltip("Must match GameStep order: 0=Welcome, 1=xr_grip_trigger_joystick, 2=xr_stand_crouch, 3=tablet, 4=storage, 5=tvdsbd, 6=clock, 7=stoveck, 8=trash, 9=resetbtn, 10=GoodLuck Sprite")]
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
        if (currentStep < GameStep.Step10_GoodLuck)
        {
            currentStep++;
            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
        else if (currentStep == GameStep.Step10_GoodLuck)
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
                        "Learn how to run a sustainable kitchen!\n\nYour goal is to cook delicious meals while managing your budget, storing food correctly, and preventing spoilage.");
                break;

            case GameStep.XRControls:
                SetText("Movement & Hands",
                        "• Move Around: Use the Left or Right Joystick to walk.\n• Pick Up Items: Hold the Grip button on the side of your controller.\n• Select UI: Press the Trigger button to click buttons.");
                break;

            case GameStep.XRCrouchStand:
                SetText("Adjusting Height",
                        "• Crouch Down: Press Primary Button (A) to reach low shelves or items on the floor.\n• Stand Tall: Press Secondary Button (B) to reach high shelves in the fridge or pantry.");
                break;

            case GameStep.ShoppingTablet:
                SetText("Shopping Tablet Zone",
                        "• Use the Shopping Tablet to purchase fresh ingredients.\n• Keep an eye on your remaining budget while selecting items!");
                break;

            case GameStep.StorageUnit:
                SetText("Smart Food Storage",
                        "• Store groceries in their ideal zones (Fridge, Freezer, or Pantry).\n• Storing items incorrectly doubles their spoilage rate!");
                break;

            case GameStep.TVDashboard:
                SetText("TV Freshness Tracker",
                        "• Check the TV screen to monitor ingredient quality and freshness day by day.");
                break;

            case GameStep.ClockPhaseTransition:
                SetText("Clock & Day Phase",
                        "• Interact with the Clock to advance to the next time of day.\n• Watch how ingredients age and storage conditions change!");
                break;

            case GameStep.CookingStove:
                SetText("Cooking Station",
                        "• Select a target recipe on the stove canvas.\n• Place fresh ingredients into the pot to cook your dish—avoid spoiled items!\n\n• Once cooked, tap the spawned dish to eat it!");
                break;

            case GameStep.TrashBin:
                SetText("Utility Tools (Trash Bin)",
                        "• Throw spoiled, unusable, or incorrect food items into the trash bin to keep your workspace clean.");
                break;

            case GameStep.ResetButton:
                SetText("Utility Tools (Reset Button)",
                        "• Use the reset button to return active ingredients from the stove area back to their original spawn points.");
                break;

            case GameStep.Step10_GoodLuck:
                SetText("Ready to Start!",
                        "You're all set! Every small choice in the kitchen helps build a greener, zero-waste future.\n\nTake your time, plan your meals, and have fun cooking! If you get stuck, click the chalkboard for help.");
                break;
        }

        // Toggle action buttons: Show Close/Start button on final step, Next button on earlier steps
        bool isLastStep = (currentStep == GameStep.Step10_GoodLuck);

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

        // Format step progress text (e.g., "Step 1 / 11")
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