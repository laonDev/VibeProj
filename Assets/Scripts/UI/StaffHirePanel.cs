using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// UI panel for hiring new staff members
    /// </summary>
    public class StaffHirePanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject staffCardPrefab;
        [SerializeField] private Button closeButton;

        [Header("Filter")]
        [SerializeField] private TMP_Dropdown typeFilter;
        [SerializeField] private TMP_Dropdown rarityFilter;

        [Header("Hired Staff Display")]
        [SerializeField] private Transform hiredStaffContainer;
        [SerializeField] private GameObject staffSlotPrefab;

        private List<StaffCardUI> staffCards = new List<StaffCardUI>();
        private List<StaffSlotUI> staffSlots = new List<StaffSlotUI>();
        private StaffType selectedType = StaffType.Chef;
        private Rarity selectedRarity = Rarity.Common;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (typeFilter != null)
            {
                typeFilter.onValueChanged.AddListener(OnTypeFilterChanged);
            }

            if (rarityFilter != null)
            {
                rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);
            }
        }

        private void OnEnable()
        {
            RefreshAvailableStaff();
            RefreshHiredStaff();

            if (StaffManager.Instance != null)
            {
                StaffManager.Instance.OnStaffHired += OnStaffHired;
            }
        }

        private void OnDisable()
        {
            if (StaffManager.Instance != null)
            {
                StaffManager.Instance.OnStaffHired -= OnStaffHired;
            }
        }

        private void RefreshAvailableStaff()
        {
            // Clear existing cards
            foreach (var card in staffCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            staffCards.Clear();

            if (StaffManager.Instance == null || cardContainer == null || staffCardPrefab == null)
                return;

            // Get filtered staff
            StaffData[] availableStaff = GetFilteredStaff();

            // Create cards for each staff
            foreach (var staff in availableStaff)
            {
                GameObject cardObj = Instantiate(staffCardPrefab, cardContainer);
                StaffCardUI card = cardObj.GetComponent<StaffCardUI>();

                if (card != null)
                {
                    card.Setup(staff);
                    staffCards.Add(card);
                }
            }
        }

        private void RefreshHiredStaff()
        {
            // Clear existing slots
            foreach (var slot in staffSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            staffSlots.Clear();

            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null || hiredStaffContainer == null || staffSlotPrefab == null)
                return;

            // Create slots for hired staff
            foreach (var staff in restaurant.HiredStaff)
            {
                GameObject slotObj = Instantiate(staffSlotPrefab, hiredStaffContainer);
                StaffSlotUI slot = slotObj.GetComponent<StaffSlotUI>();

                if (slot != null)
                {
                    slot.Setup(staff);
                    staffSlots.Add(slot);
                }
            }
        }

        private StaffData[] GetFilteredStaff()
        {
            if (StaffManager.Instance == null)
                return new StaffData[0];

            var allStaff = StaffManager.Instance.AvailableStaff;
            List<StaffData> filtered = new List<StaffData>();

            foreach (var staff in allStaff)
            {
                bool matchesType = typeFilter == null || typeFilter.value == 0 || staff.staffType == selectedType;
                bool matchesRarity = rarityFilter == null || rarityFilter.value == 0 || staff.rarity == selectedRarity;

                if (matchesType && matchesRarity)
                {
                    filtered.Add(staff);
                }
            }

            return filtered.ToArray();
        }

        private void OnTypeFilterChanged(int index)
        {
            // Index 0 = All, 1 = Chef, 2 = Waiter, 3 = Cashier
            selectedType = index switch
            {
                1 => StaffType.Chef,
                2 => StaffType.Waiter,
                3 => StaffType.Cashier,
                _ => StaffType.Chef
            };

            if (index > 0)
            {
                RefreshAvailableStaff();
            }
            else
            {
                RefreshAvailableStaff();
            }
        }

        private void OnRarityFilterChanged(int index)
        {
            // Index 0 = All, 1 = Common, 2 = Rare, 3 = Epic, 4 = Legendary
            selectedRarity = index switch
            {
                1 => Rarity.Common,
                2 => Rarity.Rare,
                3 => Rarity.Epic,
                4 => Rarity.Legendary,
                _ => Rarity.Common
            };

            if (index > 0)
            {
                RefreshAvailableStaff();
            }
            else
            {
                RefreshAvailableStaff();
            }
        }

        private void OnStaffHired(Staff staff)
        {
            RefreshHiredStaff();
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }

        public void OnRefreshButtonClicked()
        {
            RefreshAvailableStaff();
            RefreshHiredStaff();
        }
    }
}
