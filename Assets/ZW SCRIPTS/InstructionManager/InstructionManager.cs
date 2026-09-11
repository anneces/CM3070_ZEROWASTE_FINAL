using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InstructionManager : MonoBehaviour
{
    public static InstructionManager Instance;

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
        Completed = 9
    }

    [Header("UI Canvas References")]
    [SerializeField] private GameObject instructionCanvas;
    [SerializeField] private TextMeshProUGUI stepTitleText;
    [SerializeField] private TextMeshProUGUI stepDescriptionText;
    [SerializeField] private TextMeshProUGUI stepProgressText;
    [SerializeField] private Image stepImageDisplay; // Image UI element on canvas
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nextButton;

    [Header("Step Visual Assets")]
    [Tooltip("Order must match GameStep enum: 0=Welcome, 1=XR Controls, 2=Shopping, 3=Storage, 4=TV, 5=Clock, 6=Stove, 7=Trash, 8=Reset")]
    [SerializeField] private Sprite[] stepSprites; // Array of visual sprites

    [Header("Current Progress")]
    public GameStep currentStep = GameStep.Welcome;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (instructionCanvas != null) instructionCanvas.SetActive(true);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCanvas);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextStep);
        }

        UpdateInstructionUI();
    }

    public void ToggleOrOpenCanvas()
    {
        if (instructionCanvas != null)
        {
            bool isActive = instructionCanvas.activeSelf;
            instructionCanvas.SetActive(!isActive);

            if (!isActive)
            {
                UpdateInstructionUI();
                AudioManager.Instance?.PlayUIClick();
            }
        }
    }

    public void CloseCanvas()
    {
        if (instructionCanvas != null)
        {
            instructionCanvas.SetActive(false);
            AudioManager.Instance?.PlayUIClick();
        }
    }

    public void SetStep(GameStep newStep)
    {
        if ((int)newStep > (int)currentStep)
        {
            currentStep = newStep;

            if (instructionCanvas != null && !instructionCanvas.activeSelf && currentStep != GameStep.Completed)
            {
                instructionCanvas.SetActive(true);
            }

            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
    }

    public void NextStep()
    {
        if (currentStep < GameStep.Completed)
        {
            currentStep++;
            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
    }

    public void PreviousStep()
    {
        if (currentStep > GameStep.Welcome)
        {
            currentStep--;
            UpdateInstructionUI();
            AudioManager.Instance?.PlayUIClick();
        }
    }

    private void UpdateInstructionUI()
    {
        if (currentStep == GameStep.Completed)
        {
            CloseCanvas();
            return;
        }

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
        }

        // Update Step Image Visual
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
                stepImageDisplay.gameObject.SetActive(false); // Hide if sprite is missing
            }
        }

        if (stepProgressText != null)
        {
            stepProgressText.text = $"Step {stepIndex + 1} / 9";
        }
    }

    private void SetText(string title, string description)
    {
        if (stepTitleText != null) stepTitleText.text = title;
        if (stepDescriptionText != null) stepDescriptionText.text = description;
    }
}