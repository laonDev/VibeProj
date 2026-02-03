using System;
using UnityEngine;

namespace AnimalKitchen
{
    public class Table : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int tableId;
        [SerializeField] private int capacity = 1;
        [SerializeField] private Transform seatPosition;
        [SerializeField] private Transform foodPosition;

        [Header("State")]
        [SerializeField] private bool isOccupied;
        [SerializeField] private Customer currentCustomer;

        public int TableId => tableId;
        public int Capacity => capacity;
        public bool IsOccupied => isOccupied;
        public Customer CurrentCustomer => currentCustomer;
        public Transform SeatPosition => seatPosition;
        public Transform FoodPosition => foodPosition;

        public event Action<Table> OnTableOccupied;
        public event Action<Table> OnTableFreed;

        private void Awake()
        {
            AutoFindPositions();
        }

        private void AutoFindPositions()
        {
            // 자식에서 자동으로 위치 찾기
            if (seatPosition == null)
            {
                var seat = transform.Find("SeatPosition");
                if (seat != null)
                    seatPosition = seat;
                else
                    seatPosition = transform; // fallback to table position
            }

            if (foodPosition == null)
            {
                var food = transform.Find("FoodPosition");
                if (food != null)
                    foodPosition = food;
                else
                    foodPosition = transform; // fallback to table position
            }
        }

        public void SetPositions(Transform seat, Transform food)
        {
            seatPosition = seat;
            foodPosition = food;
        }

        public bool TryOccupy(Customer customer)
        {
            if (isOccupied) return false;

            isOccupied = true;
            currentCustomer = customer;
            OnTableOccupied?.Invoke(this);

            Debug.Log($"[Table {tableId}] Occupied by customer");
            return true;
        }

        public void Free()
        {
            if (!isOccupied) return;

            isOccupied = false;
            currentCustomer = null;
            OnTableFreed?.Invoke(this);

            Debug.Log($"[Table {tableId}] Freed");
        }

        public void SetTableId(int id)
        {
            tableId = id;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.5f, 1f));

            if (seatPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(seatPosition.position, 0.2f);
            }
        }
    }
}
