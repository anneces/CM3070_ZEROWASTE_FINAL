using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IngredientRequirement
{
    public FoodItem foodPrefab; // Drag prefab directly from Project folder
    public int requiredAmount; // Set quantity (e.g., 2 for 2x Carrot)
}

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe Data")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<IngredientRequirement> ingredients;
    public GameObject cookedDishPrefab;
}