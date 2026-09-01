using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<string> requiredIngredientIDs; // Matches FoodItem.itemID or name
    public GameObject cookedDishPrefab;
    public float cookDuration = 5f;
}