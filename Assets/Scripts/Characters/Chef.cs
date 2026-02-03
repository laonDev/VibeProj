using System.Collections;
using UnityEngine;

namespace AnimalKitchen
{
    public class Chef : Staff
    {
        [Header("Chef Settings")]
        [SerializeField] private float cookingCheckInterval = 1f;

        private Customer currentCustomer;
        private float checkTimer;

        protected override void UpdateBehavior()
        {
            if (currentState == StaffState.Idle)
            {
                checkTimer -= Time.deltaTime;
                if (checkTimer <= 0)
                {
                    checkTimer = cookingCheckInterval;
                    TryFindWork();
                }
            }
        }

        protected override void OnReachedDestination()
        {
            if (currentCustomer != null)
            {
                StartCooking();
            }
        }

        private void TryFindWork()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            // Find customer with highest priority (lowest patience)
            Customer urgentCustomer = null;
            float lowestPatience = float.MaxValue;

            foreach (var table in restaurant.Tables)
            {
                if (table.IsOccupied && table.CurrentCustomer != null)
                {
                    var customer = table.CurrentCustomer;
                    if (customer.CurrentState == CustomerState.WaitingForFood && customer.OrderedRecipe != null)
                    {
                        // Check if this customer is already being cooked for
                        bool alreadyCooking = false;
                        foreach (var order in restaurant.Kitchen.ActiveOrders)
                        {
                            if (order.customer == customer)
                            {
                                alreadyCooking = true;
                                break;
                            }
                        }

                        if (!alreadyCooking && customer.PatiencePercent < lowestPatience)
                        {
                            lowestPatience = customer.PatiencePercent;
                            urgentCustomer = customer;
                        }
                    }
                }
            }

            // Process urgent customer if kitchen has slots
            if (urgentCustomer != null && restaurant.Kitchen.AvailableSlots > 0)
            {
                currentCustomer = urgentCustomer;
                MoveTo(restaurant.Kitchen.transform.position);
            }
        }

        private void StartCooking()
        {
            if (currentCustomer == null || currentCustomer.OrderedRecipe == null) return;

            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            bool started = restaurant.Kitchen.StartCooking(
                currentCustomer.OrderedRecipe,
                currentCustomer,
                Efficiency
            );

            if (started)
            {
                SetState(StaffState.Working);
                StartCoroutine(WaitForCookingComplete());
            }
        }

        private IEnumerator WaitForCookingComplete()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;

            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                var completedOrder = restaurant.Kitchen.TakeCompletedOrder();
                if (completedOrder != null && completedOrder.customer == currentCustomer)
                {
                    DeliverFood();
                    yield break;
                }

                if (currentCustomer == null || currentCustomer.CurrentState == CustomerState.Leaving)
                {
                    SetState(StaffState.Idle);
                    currentCustomer = null;
                    yield break;
                }
            }
        }

        private void DeliverFood()
        {
            if (currentCustomer != null && currentCustomer.AssignedTable != null)
            {
                MoveTo(currentCustomer.AssignedTable.FoodPosition.position);
                StartCoroutine(DeliverAndReturn());
            }
        }

        private IEnumerator DeliverAndReturn()
        {
            while (isMoving)
            {
                yield return null;
            }

            currentCustomer?.ReceiveFood();
            Debug.Log($"[{data.staffName}] Delivered food to customer");

            currentCustomer = null;
            SetState(StaffState.Idle);
        }

        public override void DoWork()
        {
            TryFindWork();
        }
    }
}
