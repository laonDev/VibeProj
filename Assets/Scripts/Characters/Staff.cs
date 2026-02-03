using System;
using UnityEngine;

namespace AnimalKitchen
{
    public abstract class Staff : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] protected StaffData data;
        [SerializeField] protected int level = 1;

        [Header("State")]
        [SerializeField] protected StaffState currentState = StaffState.Idle;

        [Header("Components")]
        [SerializeField] protected SpriteRenderer spriteRenderer;

        public StaffData Data => data;
        public int Level => level;
        public StaffState CurrentState => currentState;
        public float Speed => data.GetSpeed(level);
        public float Efficiency => data.GetEfficiency(level);

        public event Action<Staff> OnStateChanged;
        public event Action<Staff> OnLevelUp;

        protected Vector3 targetPosition;
        protected bool isMoving;

        public virtual void Initialize(StaffData staffData)
        {
            data = staffData;
            level = 1;

            if (spriteRenderer != null && data.portrait != null)
            {
                spriteRenderer.sprite = data.portrait;
            }

            SetState(StaffState.Idle);
        }

        protected virtual void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

            UpdateMovement();
            UpdateBehavior();
        }

        protected virtual void UpdateMovement()
        {
            if (!isMoving) return;

            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > 0.1f)
            {
                transform.position += direction * Speed * Time.deltaTime;

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

        protected abstract void UpdateBehavior();
        protected abstract void OnReachedDestination();

        public virtual void SetState(StaffState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnStateChanged?.Invoke(this);

            Debug.Log($"[{data.staffName}] State changed to: {currentState}");
        }

        public void MoveTo(Vector3 position)
        {
            targetPosition = position;
            isMoving = true;
            SetState(StaffState.Walking);
        }

        public bool TryLevelUp()
        {
            int cost = data.GetLevelUpCost(level);
            if (ResourceManager.Instance.SpendGold(cost))
            {
                level++;
                OnLevelUp?.Invoke(this);
                Debug.Log($"[{data.staffName}] Leveled up to {level}");
                return true;
            }
            return false;
        }

        public abstract void DoWork();
    }
}
