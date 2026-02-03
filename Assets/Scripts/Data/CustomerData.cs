using UnityEngine;

namespace AnimalKitchen
{
    [CreateAssetMenu(fileName = "NewCustomer", menuName = "Animal Kitchen/Customer Data")]
    public class CustomerData : ScriptableObject
    {
        [Header("Basic Info")]
        public string customerName;
        public AnimalType animalType;
        public Sprite portrait;
        public RuntimeAnimatorController animator;

        [Header("Behavior")]
        [Tooltip("How long the customer will wait before leaving (in seconds)")]
        [Range(30f, 180f)]
        public float patience = 60f;

        [Tooltip("Movement speed")]
        [Range(1f, 5f)]
        public float moveSpeed = 2f;

        [Tooltip("Time to eat (in seconds)")]
        [Range(5f, 30f)]
        public float eatDuration = 10f;

        [Header("Preferences")]
        public FoodCategory[] preferredCategories;

        [Header("Tipping")]
        [Range(0f, 0.5f)]
        public float baseTipRate = 0.1f;
        [Range(0f, 0.3f)]
        public float maxBonusTipRate = 0.2f;

        public int CalculateTip(int billAmount, float satisfactionRate)
        {
            float tipRate = baseTipRate + (maxBonusTipRate * satisfactionRate);
            return Mathf.RoundToInt(billAmount * tipRate);
        }
    }
}
