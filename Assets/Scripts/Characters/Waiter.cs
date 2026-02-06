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
        private GameObject carryingFoodObject;
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
                        // Check if another waiter is already handling this order
                        bool alreadyHandled = false;
                        foreach (var staff in restaurant.HiredStaff)
                        {
                            if (staff is Waiter waiter && waiter != this && waiter.GetCurrentCustomer() == order.customer)
                            {
                                alreadyHandled = true;
                                break;
                            }
                        }

                        if (!alreadyHandled && order.customer.PatiencePercent < lowestPatience)
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
                carryingFoodObject = completedOrder.foodObject;

                // Attach food object to waiter
                if (carryingFoodObject != null)
                {
                    carryingFoodObject.transform.SetParent(transform);
                    carryingFoodObject.transform.localPosition = Vector3.up * 0.5f; // Above waiter
                    Debug.Log($"[{data?.staffName ?? "Waiter"}] Attached food object to waiter");
                }

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
                // Place food on table
                if (carryingFoodObject != null && currentCustomer.AssignedTable != null)
                {
                    carryingFoodObject.transform.SetParent(null); // Detach from waiter
                    carryingFoodObject.transform.position = currentCustomer.AssignedTable.FoodPosition.position;
                    Debug.Log($"[{data.staffName}] Placed food on table at {carryingFoodObject.transform.position}");
                }

                currentCustomer.ReceiveFood(carryingFoodObject);
                Debug.Log($"[{data.staffName}] Served food to customer");
                ShowSpeechBubble("Bon appetit!", 2f);
            }

            carryingFood = null;
            carryingFoodObject = null;
            currentCustomer = null;
            SetState(StaffState.Idle);
        }

        public override void DoWork()
        {
            TryFindWork();
        }

        public Customer GetCurrentCustomer()
        {
            return currentCustomer;
        }
    }
}
