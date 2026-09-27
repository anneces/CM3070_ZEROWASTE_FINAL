using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Singleton manager handling the step-by-step tutorial instructional overlay canvas.
/// Controls tutorial progression, UI text updates, step counter formatting, visual sprite illustrations,
/// and physical chalkboard object visibility synchronization.
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

    [Header("Chalkboard Reference")]
    [Tooltip("Reference to the physical instruction chalkboard GameObject in the scene.")]
    [SerializeField] private GameObject chalkboardObject;

    [Header("Step Visual Assets")]
    [Tooltip("Must match GameStep order: 0=Welcome, 1=xr_grip_trigger_joystick, 2=xr_stand_crouch, 3=tablet, 4=storage, 5=tvdsbd, 6=clock, 7=stoveck, 8=trash, 9=resetbtn, 10=GoodLuck Sprite")]
    [SerializeField] private Sprite[] stepSprites; // Array of tutorial illustrations/sprites

    [Header("Current Progress")]
    [Tooltip("The current active step in the tutorial sequence.")]
    public GameStep currentStep = GameStep.Welcome;

    [Header("Input Cooldown (VR Fix)")]
    [Tooltip("Prevents XR raycast input double-triggering in VR headset.")]
    [SerializeField] private float buttonClickCooldown = 0.25f;
    private float lastClickTime = 0f;

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

        // Synchronize board visibility on startup
        SyncChalkboardVisibility();

        // Safely bind button click listeners by clearing any pre-existing listeners first
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseCanvas);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextStep);
        }

        // Initialize UI content for the starting step
        UpdateInstructionUI();
    }

    #endregion

    #region Canvas & Board Visibility Controls

    /// <summary>
    /// Registers the physical chalkboard object dynamically from the interactable script.
    /// </summary>
    public void RegisterChalkboard(GameObject board)
    {
        chalkboardObject = board;
        SyncChalkboardVisibility();
    }

    /// <summary>
    /// Shows or hides the physical chalkboard based on whether the instruction canvas is active.
    /// </summary>
    private void SyncChalkboardVisibility()
    {
        if (chalkboardObject != null && instructionCanvas != null)
        {
            chalkboardObject.SetActive(!instructionCanvas.activeSelf);
        }
    }

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
                if (currentStep == GameStep.Completed)
                {
                    currentStep = GameStep.Welcome;
                }

                UpdateInstructionUI();
                AudioManager.Instance?.PlayUIClick();
            }

            SyncChalkboardVisibility();
        }
    }

    /// <summary>
    /// Closes the tutorial canvas and sets the current state to Completed.
    /// </summary>
    public void CloseCanvas()
    {
        currentStep = GameStep.Completed;

        if (instructionCanvas != null)
        {
            instructionCanvas.SetActive(false);
        }

        AudioManager.Instance?.PlayUIClick();
        SyncChalkboardVisibility();
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

            if (instructionCanvas != null && !instructionCanvas.activeSelf && currentStep != GameStep.Completed)
            {
                instructionCanvas.SetActive(true);
            }

            SyncChalkboardVisibility();
            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
    }

    /// <summary>
    /// Advances the tutorial to the next chronological step or closes the canvas at final step.
    /// </summary>
    public void NextStep()
    {
        // Ignore clicks if they occur faster than the cooldown threshold (VR Raycast fix)
        if (Time.time - lastClickTime < buttonClickCooldown)
        {
            return;
        }

        lastClickTime = Time.time;

        if (currentStep < GameStep.Step10_GoodLuck)
        {
            currentStep++;
            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
        else if (currentStep == GameStep.Step10_GoodLuck)
        {
            CloseCanvas();
        }
    }

    /// <summary>
    /// Reverts the tutorial to the previous step.
    /// </summary>
    public void PreviousStep()
    {
        if (Time.time - lastClickTime < buttonClickCooldown)
        {
            return;
        }

        lastClickTime = Time.time;

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
        // Prevent execution if the tutorial is closed or completed
        if (currentStep == GameStep.Completed)
        {
            return;
        }

        // Set title and body copy depending on current step
        switch (currentStep)
        {
            case GameStep.Welcome:
                SetText("Welcome to ZeroWaste!",
                        "Goal: Plan meals on a budget across 5 days while minimizing food waste.\n" +
                        "Each day consists of 3 phases:\n" +
                        "1. Morning (Procurement)\n" +
                        "2. Afternoon (Sorting)\n" +
                        "3. Evening (Cooking)\n" +
                        "Pay attention to Orange Canvas blockers—they act as game hints and restrict stations per phase!");
                break;

            case GameStep.XRControls:
                SetText("Movement & Grabbing",
                        "• Joystick: Move around the kitchen.\n" +
                        "• Grip Button: Grab food items and objects.\n" +
                        "• Trigger Button: Interact with UI buttons.\n\n" +
                        "Note: If you have trouble grabbing food items, use the 3D Reset Button to return them to the kitchen counter.");
                break;

            case GameStep.XRCrouchStand:
                SetText("Height Adjustments",
                        "• Left Controller Button A: Crouch down to reach lower places and floor items.\n" +
                        "• Left Controller Button B: Stand tall to reach high shelves in the fridge or pantry.");
                break;

            case GameStep.ShoppingTablet:
                SetText("Morning: Procurement Phase",
                        "• Go to the Shopping Tablet to purchase food items within your budget.\n" +
                        "• Refer to the Blue Recipe Panel next to the stove if you want to buy specific ingredients for planned dishes.");
                break;

            case GameStep.StorageUnit:
                SetText("Afternoon: Food Storage",
                        "• Pick up food items from the counter and sort them into the Fridge, Pantry, or Freezer.\n" +
                        "• Proper placement is critical—improperly stored food will spoil rapidly!");
                break;

            case GameStep.TVDashboard:
                SetText("TV Freshness & Placement Tracker",
                        "• Check placement status: 'Optimal' means correct storage; 'Sub-Optimal' means wrong storage.\n" +
                        "• Monitor fresh status and freshness percentage.\n" +
                        "• Watch for visual cues directly on ingredients, such as dark spots or spoilage signs.");
                break;

            case GameStep.ClockPhaseTransition:
                SetText("Clock Phase Transition",
                        "• Once you complete your tasks for a phase, click the Clock to advance to the next phase.\n" +
                        "• Complete Day 5 Evening to finish the game session!");
                break;

            case GameStep.CookingStove:
                SetText("Evening: Cooking Phase",
                        "1. Go to the stove station and select a dish from the blue recipe panel.\n" +
                        "2. Confirm to start cooking—a progress bar will appear.\n" +
                        "3. Place required items into the pan. (Cooking spoiled food is forbidden!)\n" +
                        "4. Missing an item? Click to reset progress—food will respawn on the counter for future phases.");
                break;

            case GameStep.TrashBin:
                SetText("Trash Bin & Penalties",
                        "• Place spoiled or unusable food into the Trash Bin.\n" +
                        "• Warning: Items thrown into the trash, along with total CO2 points from spoiled food, will penalize your final score!");
                break;

            case GameStep.ResetButton:
                SetText("3D Food Respawn Button",
                        "• If food items become hard to grab or get stuck in awkward locations, press the 3D Reset Button.\n" +
                        "• Ingredients will safely return to the main kitchen counter.");
                break;

            case GameStep.Step10_GoodLuck:
                SetText("Ready to Cook!",
                        "Your score will be calculated by:\n" +
                        "• Points earned for dishes cooked\n" +
                        "• Total CO2 points from spoiled items\n" +
                        "• Items discarded in the trash\n\n" +
                        "Need to re-read instructions later? Click the Chalkboard anytime. Good luck!");
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
                stepImageDisplay.gameObject.SetActive(false);
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