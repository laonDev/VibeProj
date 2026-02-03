using System.Collections;
using UnityEngine;

namespace AnimalKitchen
{
    public class Waiter : Staff
    {
        [Header("Waiter Settings")]
        [SerializeField] private float serviceCheckInterval = 1f;
        [SerializeField] private float tipBonusMultiplier = 1.1f;

        private Customer currentCustomer;
        private RecipeData carryingFood;
        private float checkTimer;

        protected override void UpdateBehavior()
        {
            if (currentState == StaffState.Idle)
            {
                checkTimer -= Time.deltaTime;
                if (checkTimer <= 0)
                {
                    checkTimer = serviceCheckInterval;
                    TryFindWork();
                }
            }
        }

        protected override void OnReachedDestination()
        {
            // If carrying food, we're at the table - serve it
            if (carryingFood != null && currentCustomer != null)
            {
                ServeFood();
            }
            // If not carrying food but have a customer, we're at the kitchen - pick up food
            else if (currentCustomer != null && carryingFood == null)
            {
                PickUpFood();
            }
        }

        private void TryFindWork()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            // Check if there are completed orders
            if (restaurant.Kitchen.HasCompletedOrders())
            {
                // Find most urgent completed order (lowest patience)
                Kitchen.CookingOrder urgentOrder = null;
                float lowestPatience = float.MaxValue;

                foreach (var order in restaurant.Kitchen.ActiveOrders)
                {
                    if (order.isCompleted && order.customer != null)
                    {
                        if (order.customer.PatiencePercent < lowestPatience)
                        {
                            lowestPatience = order.customer.PatiencePercent;
                            urgentOrder = order;
                        }
                    }
                }

                // If found urgent order, go to kitchen to pick it up
                if (urgentOrder != null)
                {
                    currentCustomer = urgentOrder.customer;
                    Debug.Log($"[{data?.staffName ?? "Waiter"}] Going to kitchen to pick up food for customer");
                    MoveTo(restaurant.Kitchen.transform.position);
                    SetState(StaffState.Walking);
                }
            }
        }

        private void PickUpFood()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            // Take completed order from kitchen
            var completedOrder = restaurant.Kitchen.TakeCompletedOrder();
            if (completedOrder != null)
            {
                carryingFood = completedOrder.recipe;
                currentCustomer = completedOrder.customer;

                Debug.Log($"[{data?.staffName ?? "Waiter"}] Picked up food from kitchen, going to customer table");
                ShowSpeechBubble($"Delivering: {carryingFood.recipeName}", 2f);

                if (currentCustomer != null && currentCustomer.AssignedTable != null)
                {
                    MoveTo(currentCustomer.AssignedTable.FoodPosition.position);
                    SetState(StaffState.Walking);
                }
            }
            else
            {
                // No food to pick up, go back to idle
                Debug.LogWarning($"[{data?.staffName ?? "Waiter"}] Reached kitchen but no food to pick up");
                currentCustomer = null;
                SetState(StaffState.Idle);
            }
        }

        private void ServeFood()
        {
            if (currentCustomer != null)
            {
                currentCustomer.ReceiveFood();
                Debug.Log($"[{data.staffName}] Served food to customer");
                ShowSpeechBubble("Bon appetit!", 2f);
            }

            carryingFood = null;
            currentCustomer = null;
            SetState(StaffState.Idle);
        }

        public override void DoWork()
        {
            TryFindWork();
        }
    }
}
