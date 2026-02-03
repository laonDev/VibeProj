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
            if (carryingFood != null && currentCustomer != null)
            {
                ServeFood();
            }
            else if (currentCustomer != null)
            {
                PickUpFood();
            }
        }

        private void TryFindWork()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

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

                // Take the urgent order
                if (urgentOrder != null)
                {
                    var completedOrder = restaurant.Kitchen.TakeCompletedOrder();
                    if (completedOrder != null)
                    {
                        currentCustomer = completedOrder.customer;
                        carryingFood = completedOrder.recipe;

                        if (currentCustomer != null && currentCustomer.AssignedTable != null)
                        {
                            MoveTo(currentCustomer.AssignedTable.FoodPosition.position);
                        }
                        return;
                    }
                }
            }
        }

        private void PickUpFood()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            var completedOrder = restaurant.Kitchen.TakeCompletedOrder();
            if (completedOrder != null)
            {
                carryingFood = completedOrder.recipe;
                currentCustomer = completedOrder.customer;

                if (currentCustomer != null && currentCustomer.AssignedTable != null)
                {
                    MoveTo(currentCustomer.AssignedTable.FoodPosition.position);
                }
            }
        }

        private void ServeFood()
        {
            if (currentCustomer != null)
            {
                currentCustomer.ReceiveFood();
                Debug.Log($"[{data.staffName}] Served food to customer");
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
