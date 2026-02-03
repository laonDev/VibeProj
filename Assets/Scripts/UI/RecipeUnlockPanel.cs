using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// UI panel for unlocking new recipes
    /// </summary>
    public class RecipeUnlockPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform recipeContainer;
        [SerializeField] private GameObject recipeCardPrefab;
        [SerializeField] private Button closeButton;

        [Header("Recipe Database")]
        [SerializeField] private RecipeData[] allRecipes;

        private List<RecipeCardUI> recipeCards = new List<RecipeCardUI>();

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void OnEnable()
        {
            LoadAllRecipes();
            RefreshRecipes();

            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant != null)
            {
                restaurant.OnRecipeUnlocked += OnRecipeUnlocked;
            }
        }

        private void OnDisable()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant != null)
            {
                restaurant.OnRecipeUnlocked -= OnRecipeUnlocked;
            }
        }

        private void LoadAllRecipes()
        {
            if (allRecipes == null || allRecipes.Length == 0)
            {
                allRecipes = Resources.LoadAll<RecipeData>("Recipes");
            }

            #if UNITY_EDITOR
            if (allRecipes.Length == 0)
            {
                Debug.LogWarning("[RecipeUnlockPanel] No recipes found. Create recipes in Resources/Recipes/");
            }
            #endif
        }

        private void RefreshRecipes()
        {
            // Clear existing cards
            foreach (var card in recipeCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            recipeCards.Clear();

            if (recipeContainer == null || recipeCardPrefab == null) return;

            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant == null) return;

            // Create cards for all recipes
            foreach (var recipe in allRecipes)
            {
                GameObject cardObj = Instantiate(recipeCardPrefab, recipeContainer);
                RecipeCardUI card = cardObj.GetComponent<RecipeCardUI>();

                if (card != null)
                {
                    bool isUnlocked = restaurant.UnlockedRecipes.Contains(recipe);
                    card.Setup(recipe, isUnlocked);
                    recipeCards.Add(card);
                }
            }
        }

        private void OnRecipeUnlocked(RecipeData recipe)
        {
            RefreshRecipes();
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Individual recipe card UI
    /// </summary>
    public class RecipeCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI cookTimeText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI unlockCostText;
        [SerializeField] private Button unlockButton;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject unlockedIndicator;

        private RecipeData recipeData;
        private bool isUnlocked;

        private void Awake()
        {
            if (unlockButton != null)
            {
                unlockButton.onClick.AddListener(OnUnlockClicked);
            }
        }

        public void Setup(RecipeData recipe, bool unlocked)
        {
            recipeData = recipe;
            isUnlocked = unlocked;

            if (icon != null && recipe.icon != null)
            {
                icon.sprite = recipe.icon;
            }

            if (nameText != null)
            {
                nameText.text = recipe.recipeName;
            }

            if (categoryText != null)
            {
                categoryText.text = recipe.category.ToString();
            }

            if (cookTimeText != null)
            {
                cookTimeText.text = $"{recipe.cookTime}s";
            }

            if (priceText != null)
            {
                priceText.text = $"{recipe.sellPrice}G";
            }

            if (unlockCostText != null)
            {
                if (recipe.unlockCost == 0)
                {
                    unlockCostText.text = "Free";
                }
                else
                {
                    unlockCostText.text = $"{recipe.unlockCost}G";
                }
            }

            // Update locked/unlocked state
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!isUnlocked);
            }

            if (unlockedIndicator != null)
            {
                unlockedIndicator.SetActive(isUnlocked);
            }

            UpdateUnlockButton();
        }

        private void Update()
        {
            UpdateUnlockButton();
        }

        private void UpdateUnlockButton()
        {
            if (unlockButton == null || recipeData == null) return;

            if (isUnlocked)
            {
                unlockButton.gameObject.SetActive(false);
            }
            else
            {
                unlockButton.gameObject.SetActive(true);
                bool canAfford = ResourceManager.Instance.Gold >= recipeData.unlockCost;
                unlockButton.interactable = canAfford;
            }
        }

        private void OnUnlockClicked()
        {
            if (recipeData == null || isUnlocked) return;

            var restaurant = GameManager.Instance?.CurrentRestaurant;
            if (restaurant != null && restaurant.UnlockRecipe(recipeData))
            {
                isUnlocked = true;
                Setup(recipeData, true);
            }
        }
    }
}
