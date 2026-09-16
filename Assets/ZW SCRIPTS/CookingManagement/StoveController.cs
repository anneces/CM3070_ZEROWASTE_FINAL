using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoveController : MonoBehaviour
{
    public static StoveController Instance;

    [Header("Recipe & Spawn Settings")]
    public RecipeData activeRecipe;
    public Transform dishSpawnPoint;
    public Transform[] ingredientRespawnPoints;
    public bool isPlateOccupied = false;

    [Header("UI References")]
    public GameObject progressCanvas;
    public GameObject eatMeCanvas;
    public GameObject resetButton;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI statusText;

    [Header("VFX References")]
    public ParticleSystem stoveFireVFX;
    public ParticleSystem dishSpawnVFX;

    private struct ConsumedIngredientData
    {
        public FoodItem prefab;
        public float savedFreshnessDays;
    }

    private List<string> addedIngredients = new List<string>();
    private List<ConsumedIngredientData> consumedIngredientsData = new List<ConsumedIngredientData>();
    private bool isCooking = false;
    private float cookTimer = 0f;
    private GameObject spawnedDish;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (progressCanvas != null) progressCanvas.SetActive(false);
        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
        if (stoveFireVFX != null) stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (dishSpawnVFX != null) dishSpawnVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (ingredientRespawnPoints == null || ingredientRespawnPoints.Length == 0 || ingredientRespawnPoints[0] == null)
        {
            FindCounterSpawnPoints();
        }
    }

    private void Update()
    {
        if (isPlateOccupied && spawnedDish == null)
        {
            ClearPlate();
        }
    }

    private void FindCounterSpawnPoints()
    {
        ingredientRespawnPoints = new Transform[3];

        GameObject sp1 = GameObject.Find("SpawnPoint_1");
        GameObject sp2 = GameObject.Find("SpawnPoint_2");
        GameObject sp3 = GameObject.Find("SpawnPoint_3");

        if (sp1 != null) ingredientRespawnPoints[0] = sp1.transform;
        if (sp2 != null) ingredientRespawnPoints[1] = sp2.transform;
        if (sp3 != null) ingredientRespawnPoints[2] = sp3.transform;
    }

    public void SetActiveRecipe(RecipeData newRecipe)
    {
        if (isPlateOccupied)
        {
            if (progressCanvas != null) progressCanvas.SetActive(true);
            if (headerText != null) headerText.text = "Blocked!";
            if (statusText != null) statusText.text = "Eat the current dish first!";
            return;
        }

        activeRecipe = newRecipe;
        addedIngredients.Clear();
        consumedIngredientsData.Clear();

        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);

        UpdateRecipeUI();

        if (stoveFireVFX != null)
        {
            stoveFireVFX.gameObject.SetActive(true);
            if (!stoveFireVFX.isPlaying)
            {
                stoveFireVFX.Play();
            }
        }

        AudioManager.Instance?.StartCookingSizzle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCooking || activeRecipe == null || isPlateOccupied) return;

        FoodItem item = other.GetComponentInParent<FoodItem>();
        if (item != null)
        {
            if (item.isSpoiled)
            {
                ShowSpoiledFoodWarning();

                Vector3 targetSpawnPos = transform.position + Vector3.up * 0.5f;
                Quaternion targetSpawnRot = Quaternion.identity;

                if (ingredientRespawnPoints != null && ingredientRespawnPoints.Length > 0)
                {
                    Transform spawnPoint = ingredientRespawnPoints[0];
                    if (spawnPoint != null)
                    {
                        targetSpawnPos = spawnPoint.position;
                        targetSpawnRot = spawnPoint.rotation;
                    }
                }

                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                item.transform.position = targetSpawnPos;
                item.transform.rotation = targetSpawnRot;
                return;
            }

            string id = !string.IsNullOrEmpty(item.foodName) ? item.foodName : item.gameObject.name;

            foreach (var req in activeRecipe.ingredients)
            {
                if (req.foodPrefab != null && req.foodPrefab.foodName == id)
                {
                    int maxNeeded = req.requiredAmount;
                    int currentCount = addedIngredients.FindAll(x => x == id).Count;

                    if (currentCount < maxNeeded)
                    {
                        consumedIngredientsData.Add(new ConsumedIngredientData
                        {
                            prefab = req.foodPrefab,
                            savedFreshnessDays = item.currentFreshnessDays
                        });

                        item.isCooked = true;

                        addedIngredients.Add(id);
                        AudioManager.Instance?.PlaySFX(AudioManager.Instance.ingredientDropClip);
                        Destroy(item.gameObject);

                        UpdateRecipeUI();
                        CheckRecipeCompletion();
                        break;
                    }
                }
            }
        }
    }

    public void ResetStove()
    {
        if (isCooking) return;

        if (ingredientRespawnPoints == null || ingredientRespawnPoints.Length == 0 || ingredientRespawnPoints[0] == null)
        {
            FindCounterSpawnPoints();
        }

        AudioManager.Instance?.StopCookingSizzle();

        for (int i = 0; i < consumedIngredientsData.Count; i++)
        {
            ConsumedIngredientData data = consumedIngredientsData[i];
            if (data.prefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                Quaternion spawnRot = Quaternion.identity;

                if (ingredientRespawnPoints != null && ingredientRespawnPoints.Length > 0)
                {
                    Transform targetSpawn = ingredientRespawnPoints[i % ingredientRespawnPoints.Length];
                    if (targetSpawn != null)
                    {
                        spawnPos = targetSpawn.position;
                        spawnRot = targetSpawn.rotation;
                    }
                }

                GameObject spawnedObj = Instantiate(data.prefab.gameObject, spawnPos, spawnRot);
                FoodItem spawnedItem = spawnedObj.GetComponent<FoodItem>();
                if (spawnedItem != null)
                {
                    spawnedItem.SetFreshnessDays(data.savedFreshnessDays);
                    spawnedItem.isCooked = false;
                }
            }
        }

        addedIngredients.Clear();
        consumedIngredientsData.Clear();

        if (stoveFireVFX != null)
        {
            stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        UpdateRecipeUI();
        AudioManager.Instance?.PlayUIClick();
    }

    private void ShowSpoiledFoodWarning()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);
        if (headerText != null) headerText.text = "Warning!";
        if (statusText != null) statusText.text = "Do not add spoiled food inside!";

        AudioManager.Instance?.PlayAlert();
    }

    private void UpdateRecipeUI()
    {
        if (progressCanvas != null) progressCanvas.SetActive(true);

        if (activeRecipe != null)
        {
            if (headerText != null)
            {
                headerText.text = activeRecipe.recipeName;
            }

            if (statusText != null)
            {
                string progressList = "";
                foreach (var req in activeRecipe.ingredients)
                {
                    if (req.foodPrefab != null)
                    {
                        string id = req.foodPrefab.foodName;
                        int currentCount = addedIngredients.FindAll(x => x == id).Count;
                        int maxNeeded = req.requiredAmount;

                        progressList += $"{id} {currentCount}/{maxNeeded}\n";
                    }
                }
                statusText.text = progressList.TrimEnd();
            }

            if (resetButton != null)
            {
                resetButton.SetActive(addedIngredients.Count > 0 && !isCooking);
            }
        }
    }

    private void CheckRecipeCompletion()
    {
        int totalRequiredCount = GetTotalRequiredIngredientsCount();
        if (addedIngredients.Count >= totalRequiredCount && totalRequiredCount > 0)
        {
            StartCoroutine(StartCookingProcess());
        }
    }

    private System.Collections.IEnumerator StartCookingProcess()
    {
        isCooking = true;
        if (resetButton != null) resetButton.SetActive(false);
        cookTimer = 0f;

        AudioManager.Instance?.StartCookingSizzle();

        float duration = 5f;

        while (cookTimer < duration)
        {
            cookTimer += Time.deltaTime;
            yield return null;
        }

        SpawnDish();
    }

    private void SpawnDish()
    {
        AudioManager.Instance?.StopCookingSizzle();
        AudioManager.Instance?.PlayPoofCloud();

        if (stoveFireVFX != null)
        {
            stoveFireVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (dishSpawnVFX != null)
        {
            dishSpawnVFX.gameObject.SetActive(true);
            dishSpawnVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dishSpawnVFX.Play();
        }

        if (activeRecipe.cookedDishPrefab != null && dishSpawnPoint != null)
        {
            spawnedDish = Instantiate(activeRecipe.cookedDishPrefab, dishSpawnPoint.position, dishSpawnPoint.rotation);
            isPlateOccupied = true;

            // FIX: Notify CookingManager to increment and save TotalDishesCooked
            if (CookingManager.Instance != null)
            {
                CookingManager.Instance.RegisterDishCooked();
            }
            else
            {
                // Fallback direct save if CookingManager instance isn't in scene
                int currentCooked = PlayerPrefs.GetInt("TotalDishesCooked", 0) + 1;
                PlayerPrefs.SetInt("TotalDishesCooked", currentCooked);
                PlayerPrefs.Save();
            }

            if (eatMeCanvas != null)
            {
                eatMeCanvas.SetActive(true);
            }
        }

        addedIngredients.Clear();
        consumedIngredientsData.Clear();
        isCooking = false;
        if (progressCanvas != null) progressCanvas.SetActive(false);
    }

    public void ClearPlate()
    {
        isPlateOccupied = false;
        spawnedDish = null;
        activeRecipe = null;

        if (eatMeCanvas != null) eatMeCanvas.SetActive(false);
        if (progressCanvas != null) progressCanvas.SetActive(false);
        if (resetButton != null) resetButton.SetActive(false);
    }

    private int GetTotalRequiredIngredientsCount()
    {
        if (activeRecipe == null || activeRecipe.ingredients == null) return 0;

        int total = 0;
        foreach (var req in activeRecipe.ingredients)
        {
            total += req.requiredAmount;
        }
        return total;
    }
}