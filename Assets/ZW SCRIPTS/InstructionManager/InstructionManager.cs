using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InstructionManager : MonoBehaviour
{
    public static InstructionManager Instance;

    public enum GameStep
    {
        Welcome = 0,
        ShoppingTablet = 1,
        StorageUnit = 2,
        TVDashboard = 3,
        ClockPhaseTransition = 4,
        CookingStove = 5,
        TrashBin = 6,
        ResetButton = 7,
        Completed = 8
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
    [Tooltip("Order must match GameStep enum: 0=Welcome, 1=Shopping, 2=Storage, 3=TV, 4=Clock, 5=Stove, 6=Trash, 7=Reset")]
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

            case GameStep.ShoppingTablet:
                SetText("Step 1: Shopping Tablet Zone",
                        "Order fresh food items to prepare your meals. Keep an eye on your budget while selecting ingredients.");
                break;

            case GameStep.StorageUnit:
                SetText("Step 2: Food Storage",
                        "Place your purchased items into their ideal storage zones (Fridge, Freezer, or Pantry). Storing items incorrectly doubles their spoilage rate!");
                break;

            case GameStep.TVDashboard:
                SetText("Step 3: TV Dashboard",
                        "Check the TV screen to monitor the food items freshness as they change day by day.");
                break;

            case GameStep.ClockPhaseTransition:
                SetText("Step 4: Clock & Day Phase",
                        "Interact with the clock to advance to the next day phase. Watch how food freshness and storage conditions progress over time.");
                break;

            case GameStep.CookingStove:
                SetText("Step 5: Cooking Stove",
                        "Select a target recipe at the stove station. Place fresh ingredients into the pot to cook your dish—be careful not to add spoiled items!");
                break;

            case GameStep.TrashBin:
                SetText("Step 6: Utility Tools (Trash Bin)",
                        "Dispose of spoiled, unusable, or incorrect food items in the trash bin to keep your workspace clear.");
                break;

            case GameStep.ResetButton:
                SetText("Step 7: Utility Tools (Reset Button)",
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
            stepProgressText.text = $"Step {stepIndex + 1} / 8";
        }
    }

    private void SetText(string title, string description)
    {
        if (stepTitleText != null) stepTitleText.text = title;
        if (stepDescriptionText != null) stepDescriptionText.text = description;
    }
}