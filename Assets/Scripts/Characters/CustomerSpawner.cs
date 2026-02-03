using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalKitchen
{
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float baseSpawnInterval = 10f;
        [SerializeField] private float minSpawnInterval = 3f;
        [SerializeField] private int maxCustomers = 10;

        [Header("References")]
        [SerializeField] private GameObject customerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private CustomerData[] customerTypes;

        [Header("State")]
        [SerializeField] private List<Customer> activeCustomers = new List<Customer>();

        private float spawnTimer;
        private bool isSpawning = true;

        public int ActiveCustomerCount => activeCustomers.Count;
        public event Action<Customer> OnCustomerSpawned;

        private void Start()
        {
            ResetSpawnTimer();
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (!isSpawning) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0 && activeCustomers.Count < maxCustomers)
            {
                SpawnCustomer();
                ResetSpawnTimer();
            }
        }

        private void ResetSpawnTimer()
        {
            float restaurantLevel = GameManager.Instance?.CurrentRestaurant?.Level ?? 1;
            float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (restaurantLevel * 0.5f));
            spawnTimer = interval * UnityEngine.Random.Range(0.8f, 1.2f);
        }

        public void SpawnCustomer()
        {
            if (customerPrefab == null || customerTypes.Length == 0) return;

            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject customerObj = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
            Customer customer = customerObj.GetComponent<Customer>();

            if (customer != null)
            {
                CustomerData randomType = customerTypes[UnityEngine.Random.Range(0, customerTypes.Length)];
                customer.Initialize(randomType);
                customer.OnStateChanged += OnCustomerStateChanged;

                activeCustomers.Add(customer);

                OnCustomerSpawned?.Invoke(customer);

                TryAssignTable(customer);

                Debug.Log($"[CustomerSpawner] Spawned customer ({activeCustomers.Count}/{maxCustomers})");
            }
        }

        private void TryAssignTable(Customer customer)
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            Table availableTable = restaurant.GetAvailableTable();
            if (availableTable != null)
            {
                customer.AssignTable(availableTable);
            }
            else
            {
                customer.WaitForSeat();
                Debug.Log("[CustomerSpawner] No table available, customer waiting");
            }
        }

        private void OnCustomerStateChanged(Customer customer)
        {
            if (customer.CurrentState == CustomerState.Leaving)
            {
                activeCustomers.Remove(customer);
                customer.OnStateChanged -= OnCustomerStateChanged;
            }
        }

        public void SetSpawning(bool enabled)
        {
            isSpawning = enabled;
        }

        public void IncreaseMaxCustomers(int amount)
        {
            maxCustomers += amount;
        }
    }
}
