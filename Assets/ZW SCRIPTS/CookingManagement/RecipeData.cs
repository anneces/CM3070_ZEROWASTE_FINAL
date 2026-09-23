using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure linking a specific FoodItem prefab to an integer quantity requirement.
/// Used inside RecipeData to specify exact meal proportions.
/// </summary>
[System.Serializable]
public struct IngredientRequirement
{
    public FoodItem foodPrefab; // Drag prefab directly from Project folder
    public int requiredAmount; // Set quantity (e.g., 2 for 2x Carrot)
}

/// <summary>
/// Serves as a modular data container for creating custom recipes directly inside the Unity Inspector
/// without hardcoding values in scripts. Stores target dish prefabs and required ingredients lists.
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe Data")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<IngredientRequirement> ingredients;
    public GameObject cookedDishPrefab;
}