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

            public float ProgressPercent => Mathf.Clamp01(progress / cookTime);
        }

        public bool StartCooking(RecipeData recipe, Customer customer, float efficiencyMultiplier = 1f)
        {
            if (AvailableSlots <= 0) return false;

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
    }
}
