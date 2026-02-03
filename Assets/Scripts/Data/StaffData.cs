using UnityEngine;

namespace AnimalKitchen
{
    [CreateAssetMenu(fileName = "NewStaff", menuName = "Animal Kitchen/Staff Data")]
    public class StaffData : ScriptableObject
    {
        [Header("Basic Info")]
        public string staffName;
        public AnimalType animalType;
        public StaffType staffType;
        public Rarity rarity;
        public Sprite portrait;
        public RuntimeAnimatorController animator;

        [Header("Base Stats")]
        [Range(0.5f, 2f)]
        public float baseSpeed = 1f;
        [Range(0.5f, 2f)]
        public float baseEfficiency = 1f;

        [Header("Level Up")]
        public float speedPerLevel = 0.05f;
        public float efficiencyPerLevel = 0.05f;

        [Header("Economy")]
        public int hireCost = 100;
        public int levelUpBaseCost = 50;

        public float GetSpeed(int level)
        {
            return baseSpeed + (speedPerLevel * (level - 1));
        }

        public float GetEfficiency(int level)
        {
            return baseEfficiency + (efficiencyPerLevel * (level - 1));
        }

        public int GetLevelUpCost(int currentLevel)
        {
            return levelUpBaseCost * currentLevel;
        }
    }
}
