using System;
using System.Collections;
using UnityEngine;

namespace AnimalKitchen
{
    public class Customer : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CustomerData data;

        [Header("State")]
        [SerializeField] private CustomerState currentState = CustomerState.Entering;
        [SerializeField] private float remainingPatience;

        [Header("Order")]
        [SerializeField] private RecipeData orderedRecipe;
        [SerializeField] private Table assignedTable;

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private SpeechBubble speechBubble;

        public CustomerData Data => data;
        public CustomerState CurrentState => currentState;
        public float RemainingPatience => remainingPatience;
        public float PatiencePercent => remainingPatience / data.patience;
        public RecipeData OrderedRecipe => orderedRecipe;
        public Table AssignedTable => assignedTable;

        public event Action<Customer> OnStateChanged;
        public event Action<Customer> OnOrderPlaced;
        public event Action<Customer, int, int> OnPaymentComplete; // customer, bill, tip

        private Vector3 targetPosition;
        private bool isMoving;
        private float eatTimer;

        public void Initialize(CustomerData customerData)
        {
            data = customerData;
            remainingPatience = data.patience;

            if (spriteRenderer != null && data.portrait != null)
            {
                spriteRenderer.sprite = data.portrait;
            }

            // Create speech bubble
            if (speechBubble == null)
            {
                speechBubble = SpeechBubble.Create(transform);
            }

            SetState(CustomerState.Entering);
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

            UpdateMovement();
            UpdateState();
        }

        private void UpdateMovement()
        {
            if (!isMoving) return;

            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > 0.1f)
            {
                transform.position += direction * data.moveSpeed * Time.deltaTime;

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }
            else
            {
                transform.position = targetPosition;
                isMoving = false;
                OnReachedDestination();
            }
        }

        private void UpdateState()
        {
            switch (currentState)
            {
                case CustomerState.WaitingForSeat:
                case CustomerState.WaitingForFood:
                    remainingPatience -= Time.deltaTime;
                    if (remainingPatience <= 0)
                    {
                        LeaveAngry();
                    }
                    break;

                case CustomerState.Eating:
                    eatTimer -= Time.deltaTime;
                    if (eatTimer <= 0)
                    {
                        SetState(CustomerState.Paying);
                    }
                    break;
            }
        }

        private void OnReachedDestination()
        {
            switch (currentState)
            {
                case CustomerState.WalkingToSeat:
                    SetState(CustomerState.Ordering);
                    break;

                case CustomerState.Leaving:
                    Destroy(gameObject);
                    break;
            }
        }

        public void SetState(CustomerState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnStateChanged?.Invoke(this);

            switch (newState)
            {
                case CustomerState.Ordering:
                    StartCoroutine(PlaceOrderRoutine());
                    break;

                case CustomerState.Eating:
                    eatTimer = data.eatDuration;
                    break;

                case CustomerState.Paying:
                    ProcessPayment();
                    break;
            }

            Debug.Log($"[Customer] State changed to: {currentState}");
        }

        public void MoveTo(Vector3 position)
        {
            targetPosition = position;
            isMoving = true;
        }

        public void AssignTable(Table table)
        {
            assignedTable = table;
            table.TryOccupy(this);
            MoveTo(table.SeatPosition.position);
            SetState(CustomerState.WalkingToSeat);
        }

        private IEnumerator PlaceOrderRoutine()
        {
            yield return new WaitForSeconds(1f);

            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant != null)
            {
                orderedRecipe = restaurant.GetRandomRecipe();
                if (orderedRecipe != null)
                {
                    OnOrderPlaced?.Invoke(this);
                    ShowSpeechBubble($"I want {orderedRecipe.recipeName}!", 3f);
                    SetState(CustomerState.WaitingForFood);
                    Debug.Log($"[Customer] Ordered: {orderedRecipe.recipeName}");
                }
            }
        }

        public void ReceiveFood()
        {
            if (currentState != CustomerState.WaitingForFood) return;

            ShowSpeechBubble("Yummy!", 2f);
            SetState(CustomerState.Eating);
            Debug.Log("[Customer] Received food, starting to eat");
        }

        private void ProcessPayment()
        {
            if (orderedRecipe == null) return;

            int bill = orderedRecipe.sellPrice;
            int tip = data.CalculateTip(bill, PatiencePercent);
            int total = bill + tip;

            ResourceManager.Instance?.AddGold(total);
            OnPaymentComplete?.Invoke(this, bill, tip);

            ShowSpeechBubble($"Here's ${total}!", 2f);
            Debug.Log($"[Customer] Paid {bill} + {tip} tip = {total} gold");

            LeaveHappy();
        }

        private void LeaveHappy()
        {
            assignedTable?.Free();
            SetState(CustomerState.Leaving);
            MoveTo(GameManager.Instance.CurrentRestaurant.Exit.position);
        }

        private void LeaveAngry()
        {
            assignedTable?.Free();
            ShowSpeechBubble("Too slow!", 2f);
            SetState(CustomerState.Leaving);
            MoveTo(GameManager.Instance.CurrentRestaurant.Exit.position);
            Debug.Log("[Customer] Left angry due to timeout!");
        }

        public void WaitForSeat()
        {
            SetState(CustomerState.WaitingForSeat);
        }

        private void ShowSpeechBubble(string message, float duration = 2f)
        {
            if (speechBubble == null)
            {
                speechBubble = SpeechBubble.Create(transform);
            }

            speechBubble.Show(message, transform, duration);
        }

        private void HideSpeechBubble()
        {
            if (speechBubble != null)
            {
                speechBubble.Hide();
            }
        }
    }
}
