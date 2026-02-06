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

                        // Check in active orders (including completed orders waiting for waiter)
                        foreach (var order in restaurant.Kitchen.ActiveOrders)
                        {
                            if (order.customer == customer)
                            {
                                alreadyCooking = true;
                                break;
                            }
                        }

                        // Also check if any chef is already assigned to this customer
                        if (!alreadyCooking)
                        {
                            foreach (var staff in restaurant.HiredStaff)
                            {
                                if (staff is Chef chef && chef != this && chef.GetCurrentCustomer() == customer)
                                {
                                    alreadyCooking = true;
                                    break;
                                }
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

            // CRITICAL: Re-check if this customer already has any order (including completed ones waiting for waiter)
            // This prevents race conditions when multiple chefs move to kitchen simultaneously
            foreach (var order in restaurant.Kitchen.ActiveOrders)
            {
                if (order.customer == currentCustomer)
                {
                    Debug.LogWarning($"[{data?.staffName ?? "Chef"}] Customer already has an order in kitchen, aborting");
                    currentCustomer = null;
                    SetState(StaffState.Idle);
                    return;
                }
            }

            bool started = restaurant.Kitchen.StartCooking(
                currentCustomer.OrderedRecipe,
                currentCustomer,
                Efficiency
            );

            if (started)
            {
                SetState(StaffState.Working);
                ShowSpeechBubble($"Cooking: {currentCustomer.OrderedRecipe.recipeName}", 3f);
                StartCoroutine(WaitForCookingComplete());
            }
            else
            {
                // Failed to start cooking (no slots available), reset and go idle
                Debug.LogWarning($"[{data?.staffName ?? "Chef"}] Failed to start cooking, no slots available");
                currentCustomer = null;
                SetState(StaffState.Idle);
            }
        }

        private IEnumerator WaitForCookingComplete()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;

            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                // Check if our order is completed
                bool orderCompleted = false;
                foreach (var order in restaurant.Kitchen.ActiveOrders)
                {
                    if (order.customer == currentCustomer && order.isCompleted)
                    {
                        orderCompleted = true;
                        break;
                    }
                }

                if (orderCompleted)
                {
                    // Food is ready, stay at kitchen and go back to idle
                    Debug.Log($"[{data?.staffName ?? "Chef"}] Finished cooking for customer, staying at kitchen");
                    ShowSpeechBubble("Food ready!", 2f);
                    currentCustomer = null;
                    SetState(StaffState.Idle);
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
