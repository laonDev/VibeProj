using System.Collections;
using UnityEngine;

namespace AnimalKitchen
{
    /// <summary>
    /// Cashier staff that speeds up customer payment process
    /// </summary>
    public class Cashier : Staff
    {
        [Header("Cashier Settings")]
        [SerializeField] private float checkInterval = 1f;
        [SerializeField] private Transform cashRegisterPosition;

        private Customer currentCustomer;
        private float checkTimer;

        protected override void UpdateBehavior()
        {
            if (currentState == StaffState.Idle)
            {
                checkTimer -= Time.deltaTime;
                if (checkTimer <= 0)
                {
                    checkTimer = checkInterval;
                    TryFindWork();
                }
            }
        }

        protected override void OnReachedDestination()
        {
            if (currentCustomer != null)
            {
                ProcessPayment();
            }
        }

        private void TryFindWork()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            // Find customer who is paying
            Customer payingCustomer = null;
            float lowestPatience = float.MaxValue;

            foreach (var table in restaurant.Tables)
            {
                if (table.IsOccupied && table.CurrentCustomer != null)
                {
                    var customer = table.CurrentCustomer;
                    if (customer.CurrentState == CustomerState.Paying)
                    {
                        if (customer.PatiencePercent < lowestPatience)
                        {
                            lowestPatience = customer.PatiencePercent;
                            payingCustomer = customer;
                        }
                    }
                }
            }

            if (payingCustomer != null)
            {
                currentCustomer = payingCustomer;

                // Move to cash register or customer table
                Vector3 targetPos = cashRegisterPosition != null
                    ? cashRegisterPosition.position
                    : payingCustomer.AssignedTable.SeatPosition.position;

                MoveTo(targetPos);
            }
        }

        private void ProcessPayment()
        {
            if (currentCustomer == null) return;

            SetState(StaffState.Working);
            StartCoroutine(ProcessPaymentRoutine());
        }

        private IEnumerator ProcessPaymentRoutine()
        {
            // Cashier speeds up payment with efficiency multiplier
            float processingTime = 0.5f / Efficiency;
            yield return new WaitForSeconds(processingTime);

            if (currentCustomer != null)
            {
                Debug.Log($"[{data.staffName}] Processed payment for customer");
            }

            currentCustomer = null;
            SetState(StaffState.Idle);
        }

        public override void DoWork()
        {
            TryFindWork();
        }

        public void SetCashRegisterPosition(Transform position)
        {
            cashRegisterPosition = position;
        }
    }
}
