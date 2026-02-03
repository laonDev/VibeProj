using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI gemsText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Panels")]
        [SerializeField] private GameObject expansionPanel;
        [SerializeField] private GameObject recipePanel;
        [SerializeField] private GameObject staffPanel;
        [SerializeField] private GameObject collectionPanel;

        [Header("Popup")]
        [SerializeField] private GameObject earningsPopupPrefab;
        [SerializeField] private Transform popupParent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            AutoFindReferences();

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged += UpdateGoldDisplay;
                ResourceManager.Instance.OnGemsChanged += UpdateGemsDisplay;
                UpdateGoldDisplay(ResourceManager.Instance.Gold);
                UpdateGemsDisplay(ResourceManager.Instance.Gems);
            }

            if (GameManager.Instance?.CurrentRestaurant != null)
            {
                UpdateLevelDisplay(GameManager.Instance.CurrentRestaurant.Level);
            }

            CloseAllPanels();
        }

        private void AutoFindReferences()
        {
            // Find HUD texts by name if not assigned
            if (goldText == null)
            {
                var goldDisplay = GameObject.Find("GoldDisplay");
                if (goldDisplay != null)
                {
                    goldText = goldDisplay.GetComponent<TextMeshProUGUI>();
                }
            }

            if (gemsText == null)
            {
                var gemsDisplay = GameObject.Find("GemsDisplay");
                if (gemsDisplay != null)
                {
                    gemsText = gemsDisplay.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
                ResourceManager.Instance.OnGemsChanged -= UpdateGemsDisplay;
            }
        }

        private void UpdateGoldDisplay(int gold)
        {
            if (goldText != null)
            {
                goldText.text = FormatNumber(gold);
            }
        }

        private void UpdateGemsDisplay(int gems)
        {
            if (gemsText != null)
            {
                gemsText.text = FormatNumber(gems);
            }
        }

        private void UpdateLevelDisplay(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000)
                return $"{number / 1000000f:F1}M";
            if (number >= 1000)
                return $"{number / 1000f:F1}K";
            return number.ToString();
        }

        public void ShowEarningsPopup(Vector3 worldPosition, int amount)
        {
            if (earningsPopupPrefab == null || popupParent == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            GameObject popup = Instantiate(earningsPopupPrefab, screenPos, Quaternion.identity, popupParent);

            var text = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"+{amount}";
            }

            Destroy(popup, 1.5f);
        }

        public void OpenExpansionPanel()
        {
            CloseAllPanels();
            if (expansionPanel != null)
            {
                expansionPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[UIManager] Expansion panel not assigned");
            }
        }

        public void OpenRecipePanel()
        {
            CloseAllPanels();
            if (recipePanel != null)
            {
                recipePanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[UIManager] Recipe panel not assigned");
            }
        }

        public void OpenStaffPanel()
        {
            CloseAllPanels();
            if (staffPanel != null)
            {
                staffPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[UIManager] Staff panel not assigned");
            }
        }

        public void OpenCollectionPanel()
        {
            CloseAllPanels();
            if (collectionPanel != null)
            {
                collectionPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[UIManager] Collection panel not assigned");
            }
        }

        public void CloseAllPanels()
        {
            if (expansionPanel != null) expansionPanel.SetActive(false);
            if (recipePanel != null) recipePanel.SetActive(false);
            if (staffPanel != null) staffPanel.SetActive(false);
            if (collectionPanel != null) collectionPanel.SetActive(false);
        }

        public void OnPauseButtonClicked()
        {
            GameManager.Instance?.PauseGame();
        }

        public void OnResumeButtonClicked()
        {
            GameManager.Instance?.ResumeGame();
        }
    }
}
