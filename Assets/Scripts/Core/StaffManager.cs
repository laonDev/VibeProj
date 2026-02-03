using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalKitchen
{
    /// <summary>
    /// Manages staff hiring, spawning, and lifecycle
    /// </summary>
    public class StaffManager : MonoBehaviour
    {
        public static StaffManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject chefPrefab;
        [SerializeField] private GameObject waiterPrefab;
        [SerializeField] private GameObject cashierPrefab;

        [Header("Available Staff")]
        [SerializeField] private StaffData[] availableStaff;

        [Header("Spawn Settings")]
        [SerializeField] private Transform staffSpawnPoint;
        [SerializeField] private Transform staffContainer;

        public IReadOnlyList<StaffData> AvailableStaff => availableStaff;
        public event Action<Staff> OnStaffHired;
        public event Action<StaffData[]> OnAvailableStaffChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (staffContainer == null)
            {
                GameObject container = new GameObject("Staff Container");
                staffContainer = container.transform;
                staffContainer.SetParent(transform);
            }
        }

        private void Start()
        {
            LoadAvailableStaff();
        }

        private void LoadAvailableStaff()
        {
            // Load staff from Resources folder
            if (availableStaff == null || availableStaff.Length == 0)
            {
                availableStaff = Resources.LoadAll<StaffData>("Staff");
            }

            OnAvailableStaffChanged?.Invoke(availableStaff);

            #if UNITY_EDITOR
            if (availableStaff.Length == 0)
            {
                Debug.LogWarning("[StaffManager] No staff data found. Create staff in Resources/Staff/ or ScriptableObjects/Staff/");
            }
            #endif
        }

        public bool TryHireStaff(StaffData data)
        {
            if (data == null) return false;

            // Check if player can afford
            if (!ResourceManager.Instance.SpendGold(data.hireCost))
            {
                Debug.Log($"[StaffManager] Cannot afford {data.staffName} (Cost: {data.hireCost}G)");
                return false;
            }

            // Spawn staff
            Staff newStaff = SpawnStaff(data);
            if (newStaff == null)
            {
                // Refund if spawning fails
                ResourceManager.Instance.AddGold(data.hireCost);
                return false;
            }

            // Register with restaurant
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant != null)
            {
                restaurant.HireStaff(newStaff);
            }

            OnStaffHired?.Invoke(newStaff);
            Debug.Log($"[StaffManager] Hired {data.staffName} for {data.hireCost}G");

            return true;
        }

        private Staff SpawnStaff(StaffData data)
        {
            GameObject prefab = GetPrefabForStaffType(data.staffType);
            if (prefab == null)
            {
                Debug.LogError($"[StaffManager] No prefab found for {data.staffType}");
                return null;
            }

            Vector3 spawnPos = staffSpawnPoint != null ? staffSpawnPoint.position : Vector3.zero;
            GameObject staffObj = Instantiate(prefab, spawnPos, Quaternion.identity, staffContainer);
            Staff staff = staffObj.GetComponent<Staff>();

            if (staff != null)
            {
                staff.Initialize(data);
            }

            return staff;
        }

        private GameObject GetPrefabForStaffType(StaffType type)
        {
            return type switch
            {
                StaffType.Chef => chefPrefab,
                StaffType.Waiter => waiterPrefab,
                StaffType.Cashier => cashierPrefab,
                _ => null
            };
        }

        public void RefreshAvailableStaff()
        {
            LoadAvailableStaff();
        }

        public StaffData[] GetStaffByType(StaffType type)
        {
            List<StaffData> result = new List<StaffData>();
            foreach (var staff in availableStaff)
            {
                if (staff.staffType == type)
                {
                    result.Add(staff);
                }
            }
            return result.ToArray();
        }

        public StaffData[] GetStaffByRarity(Rarity rarity)
        {
            List<StaffData> result = new List<StaffData>();
            foreach (var staff in availableStaff)
            {
                if (staff.rarity == rarity)
                {
                    result.Add(staff);
                }
            }
            return result.ToArray();
        }
    }
}
