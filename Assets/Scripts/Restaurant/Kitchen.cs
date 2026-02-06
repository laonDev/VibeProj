using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalKitchen
{
    public class Kitchen : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxCookingSlots = 3;
        [SerializeField] private Transform[] cookingStations;

        [Header("State")]
        [SerializeField] private List<CookingOrder> activeOrders = new List<CookingOrder>();

        public int AvailableSlots => maxCookingSlots - activeOrders.Count;
        public IReadOnlyList<CookingOrder> ActiveOrders => activeOrders;

        public event Action<CookingOrder> OnCookingStarted;
        public event Action<CookingOrder> OnCookingCompleted;

        [Serializable]
        public class CookingOrder
        {
            public RecipeData recipe;
            public Customer customer;
            public float progress;
            public float cookTime;
            public bool isCompleted;
            public GameObject foodObject;

            public float ProgressPercent => Mathf.Clamp01(progress / cookTime);
        }

        public bool StartCooking(RecipeData recipe, Customer customer, float efficiencyMultiplier = 1f)
        {
            if (AvailableSlots <= 0) return false;

            // Check if this customer already has any order (including completed ones waiting for waiter)
            foreach (var existingOrder in activeOrders)
            {
                if (existingOrder.customer == customer)
                {
                    Debug.LogWarning($"[Kitchen] Customer already has an order for {existingOrder.recipe.recipeName} (completed: {existingOrder.isCompleted}), cannot start new order");
                    return false;
                }
            }

            var order = new CookingOrder
            {
                recipe = recipe,
                customer = customer,
                progress = 0f,
                cookTime = recipe.cookTime / efficiencyMultiplier,
                isCompleted = false
            };

            activeOrders.Add(order);
            OnCookingStarted?.Invoke(order);

            Debug.Log($"[Kitchen] Started cooking {recipe.recipeName}");
            return true;
        }

        private void Update()
        {
            for (int i = activeOrders.Count - 1; i >= 0; i--)
            {
                var order = activeOrders[i];
                if (order.isCompleted) continue;

                order.progress += Time.deltaTime;

                if (order.progress >= order.cookTime)
                {
                    order.isCompleted = true;

                    // Create food GameObject
                    order.foodObject = CreateFoodObject(order.recipe);

                    OnCookingCompleted?.Invoke(order);
                    Debug.Log($"[Kitchen] Completed cooking {order.recipe.recipeName}");
                }
            }
        }

        public CookingOrder TakeCompletedOrder()
        {
            for (int i = 0; i < activeOrders.Count; i++)
            {
                if (activeOrders[i].isCompleted)
                {
                    var order = activeOrders[i];
                    activeOrders.RemoveAt(i);
                    return order;
                }
            }
            return null;
        }

        public bool HasCompletedOrders()
        {
            foreach (var order in activeOrders)
            {
                if (order.isCompleted) return true;
            }
            return false;
        }

        public void UpgradeSlots(int additionalSlots)
        {
            maxCookingSlots += additionalSlots;
            Debug.Log($"[Kitchen] Upgraded to {maxCookingSlots} cooking slots");
        }

        private GameObject CreateFoodObject(RecipeData recipe)
        {
            // Create food GameObject at kitchen position
            var foodGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foodGO.name = $"Food_{recipe.recipeName}";
            foodGO.transform.position = transform.position + Vector3.up * 0.5f;
            foodGO.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // Set color based on food category
            var renderer = foodGO.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color foodColor = recipe.category switch
                {
                    FoodCategory.Appetizer => new Color(1f, 1f, 0.5f), // Yellow
                    FoodCategory.MainDish => new Color(1f, 0.5f, 0.3f), // Orange
                    FoodCategory.Dessert => new Color(1f, 0.7f, 0.9f), // Pink
                    FoodCategory.Beverage => new Color(0.5f, 0.8f, 1f), // Light blue
                    _ => Color.white
                };
                renderer.material.color = foodColor;
            }

            // Remove collider to avoid physics interactions
            var collider = foodGO.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            Debug.Log($"[Kitchen] Created food object for {recipe.recipeName} at {foodGO.transform.position}");

            return foodGO;
        }
    }
}
