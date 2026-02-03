using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// UI panel for restaurant expansion (adding tables, upgrading kitchen, etc.)
    /// </summary>
    public class RestaurantExpansionPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button addTableButton;
        [SerializeField] private Button upgradeKitchenButton;
        [SerializeField] private TextMeshProUGUI tableCountText;
        [SerializeField] private TextMeshProUGUI tableCostText;
        [SerializeField] private TextMeshProUGUI kitchenSlotsText;
        [SerializeField] private TextMeshProUGUI kitchenCostText;

        [Header("Costs")]
        [SerializeField] private int baseTableCost = 500;
        [SerializeField] private float tableCostMultiplier = 1.5f;
        [SerializeField] private int baseKitchenCost = 1000;
        [SerializeField] private float kitchenCostMultiplier = 2f;

        [Header("Placement")]
        [SerializeField] private GameObject tablePrefab;
        [SerializeField] private bool isPlacementMode;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (addTableButton != null)
            {
                addTableButton.onClick.AddListener(OnAddTableClicked);
            }

            if (upgradeKitchenButton != null)
            {
                upgradeKitchenButton.onClick.AddListener(OnUpgradeKitchenClicked);
            }
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        private void Update()
        {
            UpdateButtonStates();
        }

        private void RefreshUI()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            // Update table count
            if (tableCountText != null)
            {
                tableCountText.text = $"Tables: {restaurant.Tables.Count} / 20";
            }

            // Update table cost
            if (tableCostText != null)
            {
                int cost = CalculateTableCost(restaurant.Tables.Count);
                tableCostText.text = $"{cost}G";
            }

            // Update kitchen slots
            if (kitchenSlotsText != null)
            {
                kitchenSlotsText.text = $"Cooking Slots: {restaurant.Kitchen?.AvailableSlots ?? 0} + Max";
            }

            // Update kitchen cost
            if (kitchenCostText != null)
            {
                int cost = CalculateKitchenUpgradeCost();
                kitchenCostText.text = $"{cost}G";
            }
        }

        private void UpdateButtonStates()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            // Table button
            if (addTableButton != null)
            {
                int tableCost = CalculateTableCost(restaurant.Tables.Count);
                bool canAfford = ResourceManager.Instance.Gold >= tableCost;
                bool notMaxed = restaurant.Tables.Count < 20;
                addTableButton.interactable = canAfford && notMaxed;
            }

            // Kitchen button
            if (upgradeKitchenButton != null)
            {
                int kitchenCost = CalculateKitchenUpgradeCost();
                bool canAfford = ResourceManager.Instance.Gold >= kitchenCost;
                upgradeKitchenButton.interactable = canAfford;
            }
        }

        private int CalculateTableCost(int currentTableCount)
        {
            return Mathf.RoundToInt(baseTableCost * Mathf.Pow(tableCostMultiplier, currentTableCount));
        }

        private int CalculateKitchenUpgradeCost()
        {
            var kitchen = GameManager.Instance?.CurrentRestaurant?.Kitchen;
            if (kitchen == null) return baseKitchenCost;

            int currentSlots = kitchen.AvailableSlots;
            return Mathf.RoundToInt(baseKitchenCost * Mathf.Pow(kitchenCostMultiplier, currentSlots - 3));
        }

        private void OnAddTableClicked()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            int cost = CalculateTableCost(restaurant.Tables.Count);

            if (ResourceManager.Instance.SpendGold(cost))
            {
                // Enter placement mode or auto-place
                if (tablePrefab != null)
                {
                    // Auto-place for now (can add manual placement later)
                    Vector3 position = GetNextTablePosition(restaurant);
                    GameObject tableObj = Instantiate(tablePrefab, position, Quaternion.identity, restaurant.transform);
                    Table table = tableObj.GetComponent<Table>();

                    if (table != null)
                    {
                        restaurant.AddTable(table);
                        Debug.Log($"[Expansion] Added table at {position}");
                    }
                }

                RefreshUI();
            }
        }

        private void OnUpgradeKitchenClicked()
        {
            var kitchen = GameManager.Instance?.CurrentRestaurant?.Kitchen;
            if (kitchen == null) return;

            int cost = CalculateKitchenUpgradeCost();

            if (ResourceManager.Instance.SpendGold(cost))
            {
                kitchen.UpgradeSlots(1);
                Debug.Log("[Expansion] Upgraded kitchen slots");
                RefreshUI();
            }
        }

        private Vector3 GetNextTablePosition(Restaurant restaurant)
        {
            // Simple grid placement logic
            int tableCount = restaurant.Tables.Count;
            int row = tableCount / 4;
            int col = tableCount % 4;

            return new Vector3(col * 2f - 3f, -row * 2f, 0);
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
