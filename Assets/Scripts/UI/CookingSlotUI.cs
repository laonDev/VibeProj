using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// Individual cooking slot UI showing recipe icon, progress bar, and completion status
    /// </summary>
    public class CookingSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image recipeIcon;
        [SerializeField] private Image progressBar;
        [SerializeField] private GameObject completedIndicator;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string completedAnimationTrigger = "Complete";

        private Kitchen.CookingOrder currentOrder;
        private bool isCompleted;

        public void Setup(Kitchen.CookingOrder order)
        {
            currentOrder = order;
            isCompleted = false;

            if (recipeIcon != null && order.recipe.icon != null)
            {
                recipeIcon.sprite = order.recipe.icon;
                recipeIcon.enabled = true;
            }

            if (completedIndicator != null)
            {
                completedIndicator.SetActive(false);
            }

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (currentOrder == null) return;

            // Update progress bar
            if (progressBar != null)
            {
                progressBar.fillAmount = currentOrder.ProgressPercent;
            }

            // Update timer text
            if (timerText != null)
            {
                float remaining = currentOrder.cookTime - currentOrder.progress;
                timerText.text = $"{Mathf.CeilToInt(remaining)}s";
            }

            // Show completed indicator
            if (currentOrder.isCompleted && !isCompleted)
            {
                OnCookingCompleted();
            }
        }

        private void OnCookingCompleted()
        {
            isCompleted = true;

            if (completedIndicator != null)
            {
                completedIndicator.SetActive(true);
            }

            if (animator != null)
            {
                animator.SetTrigger(completedAnimationTrigger);
            }

            if (timerText != null)
            {
                timerText.text = "Done!";
            }

            Debug.Log($"[CookingSlotUI] Cooking completed: {currentOrder.recipe.recipeName}");
        }

        public void Clear()
        {
            currentOrder = null;
            isCompleted = false;
            gameObject.SetActive(false);
        }

        public Kitchen.CookingOrder GetOrder()
        {
            return currentOrder;
        }
    }
}
