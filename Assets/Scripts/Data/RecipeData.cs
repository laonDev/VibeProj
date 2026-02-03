using UnityEngine;

namespace AnimalKitchen
{
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Animal Kitchen/Recipe Data")]
    public class RecipeData : ScriptableObject
    {
        [Header("Basic Info")]
        public string recipeName;
        public string description;
        public Sprite icon;
        public FoodCategory category;

        [Header("Cooking")]
        [Tooltip("Time in seconds to cook this dish")]
        public float cookTime = 5f;

        [Header("Economy")]
        public int sellPrice = 50;
        public int unlockCost = 500;
        public int unlockLevel = 1;

        [Header("Customer")]
        [Range(1, 100)]
        public int popularity = 50;
        [Range(1, 5)]
        public int satisfactionBonus = 3;
    }
}
