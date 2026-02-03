using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// UI card displaying hireable staff information
    /// </summary>
    public class StaffCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI efficiencyText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button hireButton;

        [Header("Visual")]
        [SerializeField] private Image rarityBorder;
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = Color.magenta;
        [SerializeField] private Color legendaryColor = Color.yellow;

        private StaffData staffData;

        private void Awake()
        {
            if (hireButton != null)
            {
                hireButton.onClick.AddListener(OnHireClicked);
            }
        }

        public void Setup(StaffData data)
        {
            staffData = data;

            if (portrait != null && data.portrait != null)
            {
                portrait.sprite = data.portrait;
            }

            if (nameText != null)
            {
                nameText.text = data.staffName;
            }

            if (typeText != null)
            {
                typeText.text = GetStaffTypeDisplayName(data.staffType);
            }

            if (rarityText != null)
            {
                rarityText.text = GetRarityDisplayName(data.rarity);
            }

            if (speedText != null)
            {
                speedText.text = $"Speed: {data.baseSpeed:F1}x";
            }

            if (efficiencyText != null)
            {
                efficiencyText.text = $"Efficiency: {data.baseEfficiency:F1}x";
            }

            if (costText != null)
            {
                costText.text = $"{data.hireCost}G";
            }

            // Set rarity color
            Color rarityColor = GetRarityColor(data.rarity);
            if (rarityBorder != null)
            {
                rarityBorder.color = rarityColor;
            }
            if (rarityText != null)
            {
                rarityText.color = rarityColor;
            }

            UpdateHireButton();
        }

        private void Update()
        {
            UpdateHireButton();
        }

        private void UpdateHireButton()
        {
            if (hireButton == null || staffData == null) return;

            bool canAfford = ResourceManager.Instance != null && ResourceManager.Instance.Gold >= staffData.hireCost;
            hireButton.interactable = canAfford;
        }

        private void OnHireClicked()
        {
            if (staffData == null) return;

            StaffManager staffManager = FindObjectOfType<StaffManager>();
            if (staffManager != null)
            {
                staffManager.TryHireStaff(staffData);
            }
        }

        private string GetStaffTypeDisplayName(StaffType type)
        {
            return type switch
            {
                StaffType.Chef => "Chef",
                StaffType.Waiter => "Waiter",
                StaffType.Cashier => "Cashier",
                _ => type.ToString()
            };
        }

        private string GetRarityDisplayName(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => "Common",
                Rarity.Rare => "Rare",
                Rarity.Epic => "Epic",
                Rarity.Legendary => "Legendary",
                _ => rarity.ToString()
            };
        }

        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => commonColor,
                Rarity.Rare => rareColor,
                Rarity.Epic => epicColor,
                Rarity.Legendary => legendaryColor,
                _ => Color.white
            };
        }
    }
}
