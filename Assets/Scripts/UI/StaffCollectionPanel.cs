using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// UI panel displaying collection of all available staff (encyclopedia/codex)
    /// </summary>
    public class StaffCollectionPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform collectionContainer;
        [SerializeField] private GameObject collectionCardPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI collectionStatsText;

        [Header("Filter")]
        [SerializeField] private TMP_Dropdown typeFilter;
        [SerializeField] private TMP_Dropdown rarityFilter;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailPortrait;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailStatsText;

        private List<StaffCollectionCardUI> collectionCards = new List<StaffCollectionCardUI>();
        private StaffData[] allStaff;
        private HashSet<string> hiredStaffNames = new HashSet<string>();

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (typeFilter != null)
            {
                typeFilter.onValueChanged.AddListener(OnFilterChanged);
            }

            if (rarityFilter != null)
            {
                rarityFilter.onValueChanged.AddListener(OnFilterChanged);
            }

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            LoadAllStaff();
            UpdateHiredStaffList();
            RefreshCollection();
        }

        private void LoadAllStaff()
        {
            if (StaffManager.Instance != null)
            {
                allStaff = StaffManager.Instance.AvailableStaff as StaffData[];
            }

            if (allStaff == null || allStaff.Length == 0)
            {
                allStaff = Resources.LoadAll<StaffData>("Staff");
            }
        }

        private void UpdateHiredStaffList()
        {
            hiredStaffNames.Clear();

            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant != null)
            {
                foreach (var staff in restaurant.HiredStaff)
                {
                    if (staff != null && staff.Data != null)
                    {
                        hiredStaffNames.Add(staff.Data.staffName);
                    }
                }
            }
        }

        private void RefreshCollection()
        {
            // Clear existing cards
            foreach (var card in collectionCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            collectionCards.Clear();

            if (collectionContainer == null || collectionCardPrefab == null) return;

            int totalStaff = allStaff.Length;
            int hiredCount = hiredStaffNames.Count;

            // Update collection stats
            if (collectionStatsText != null)
            {
                collectionStatsText.text = $"Collection: {hiredCount} / {totalStaff}";
            }

            // Create cards for all staff
            foreach (var staff in allStaff)
            {
                GameObject cardObj = Instantiate(collectionCardPrefab, collectionContainer);
                StaffCollectionCardUI card = cardObj.GetComponent<StaffCollectionCardUI>();

                if (card != null)
                {
                    bool isHired = hiredStaffNames.Contains(staff.staffName);
                    card.Setup(staff, isHired, this);
                    collectionCards.Add(card);
                }
            }
        }

        private void OnFilterChanged(int value)
        {
            // TODO: Implement filtering logic
            RefreshCollection();
        }

        public void ShowStaffDetail(StaffData staff, bool isHired)
        {
            if (detailPanel == null) return;

            detailPanel.SetActive(true);

            if (detailPortrait != null && staff.portrait != null)
            {
                detailPortrait.sprite = staff.portrait;
            }

            if (detailNameText != null)
            {
                detailNameText.text = staff.staffName;
            }

            if (detailDescriptionText != null)
            {
                string description = $"{staff.animalType} - {staff.staffType}\n";
                description += $"Rarity: {staff.rarity}\n";
                description += isHired ? "Status: Hired" : "Status: Not Hired";
                detailDescriptionText.text = description;
            }

            if (detailStatsText != null)
            {
                string stats = $"Base Speed: {staff.baseSpeed:F2}x\n";
                stats += $"Base Efficiency: {staff.baseEfficiency:F2}x\n";
                stats += $"Hire Cost: {staff.hireCost}G";
                detailStatsText.text = stats;
            }
        }

        public void HideStaffDetail()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        private void OnCloseClicked()
        {
            HideStaffDetail();
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Individual staff collection card
    /// </summary>
    public class StaffCollectionCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject hiredIndicator;
        [SerializeField] private Button cardButton;

        private StaffData staffData;
        private bool isHired;
        private StaffCollectionPanel parentPanel;

        private void Awake()
        {
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnCardClicked);
            }
        }

        public void Setup(StaffData data, bool hired, StaffCollectionPanel panel)
        {
            staffData = data;
            isHired = hired;
            parentPanel = panel;

            if (portrait != null)
            {
                if (data.portrait != null && isHired)
                {
                    portrait.sprite = data.portrait;
                }
                else
                {
                    // Show silhouette for unhired staff
                    portrait.color = Color.black;
                }
            }

            if (nameText != null)
            {
                nameText.text = isHired ? data.staffName : "???";
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!isHired);
            }

            if (hiredIndicator != null)
            {
                hiredIndicator.SetActive(isHired);
            }
        }

        private void OnCardClicked()
        {
            if (parentPanel != null && staffData != null)
            {
                parentPanel.ShowStaffDetail(staffData, isHired);
            }
        }
    }
}
