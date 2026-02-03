using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// UI slot displaying hired staff with level and upgrade options
    /// </summary>
    public class StaffSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI efficiencyText;
        [SerializeField] private TextMeshProUGUI levelUpCostText;
        [SerializeField] private Button levelUpButton;

        [Header("State Indicator")]
        [SerializeField] private Image stateIndicator;
        [SerializeField] private Color idleColor = Color.green;
        [SerializeField] private Color workingColor = Color.yellow;

        private Staff staff;

        private void Awake()
        {
            if (levelUpButton != null)
            {
                levelUpButton.onClick.AddListener(OnLevelUpClicked);
            }
        }

        public void Setup(Staff staffMember)
        {
            staff = staffMember;

            if (staff != null)
            {
                staff.OnLevelUp += OnStaffLevelUp;
                staff.OnStateChanged += OnStaffStateChanged;
            }

            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (staff != null)
            {
                staff.OnLevelUp -= OnStaffLevelUp;
                staff.OnStateChanged -= OnStaffStateChanged;
            }
        }

        private void Update()
        {
            UpdateLevelUpButton();
        }

        private void RefreshDisplay()
        {
            if (staff == null || staff.Data == null) return;

            if (portrait != null && staff.Data.portrait != null)
            {
                portrait.sprite = staff.Data.portrait;
            }

            if (nameText != null)
            {
                nameText.text = staff.Data.staffName;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{staff.Level}";
            }

            if (speedText != null)
            {
                speedText.text = $"Speed: {staff.Speed:F2}x";
            }

            if (efficiencyText != null)
            {
                efficiencyText.text = $"Eff: {staff.Efficiency:F2}x";
            }

            if (levelUpCostText != null)
            {
                int cost = staff.Data.GetLevelUpCost(staff.Level);
                levelUpCostText.text = $"{cost}G";
            }

            UpdateStateIndicator();
        }

        private void UpdateLevelUpButton()
        {
            if (levelUpButton == null || staff == null) return;

            int cost = staff.Data.GetLevelUpCost(staff.Level);
            bool canAfford = ResourceManager.Instance != null && ResourceManager.Instance.Gold >= cost;
            levelUpButton.interactable = canAfford;
        }

        private void UpdateStateIndicator()
        {
            if (stateIndicator == null || staff == null) return;

            Color color = staff.CurrentState switch
            {
                StaffState.Idle => idleColor,
                StaffState.Walking => workingColor,
                StaffState.Working => workingColor,
                _ => Color.white
            };

            stateIndicator.color = color;
        }

        private void OnLevelUpClicked()
        {
            if (staff != null && staff.TryLevelUp())
            {
                RefreshDisplay();
            }
        }

        private void OnStaffLevelUp(Staff staffMember)
        {
            RefreshDisplay();
        }

        private void OnStaffStateChanged(Staff staffMember)
        {
            UpdateStateIndicator();
        }

        public void Clear()
        {
            if (staff != null)
            {
                staff.OnLevelUp -= OnStaffLevelUp;
                staff.OnStateChanged -= OnStaffStateChanged;
            }

            staff = null;
            gameObject.SetActive(false);
        }
    }
}
