using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalKitchen
{
    /// <summary>
    /// Manages kitchen UI displaying cooking slots and completion notifications
    /// </summary>
    public class KitchenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Kitchen kitchen;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject cookingSlotPrefab;

        [Header("Notification")]
        [SerializeField] private GameObject completionNotification;
        [SerializeField] private AudioClip completionSound;
        [SerializeField] private float notificationDuration = 2f;

        private List<CookingSlotUI> slotUIs = new List<CookingSlotUI>();
        private AudioSource audioSource;
        private float notificationTimer;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (completionNotification != null)
            {
                completionNotification.SetActive(false);
            }
        }

        private void Start()
        {
            if (kitchen == null)
            {
                kitchen = FindObjectOfType<Kitchen>();
            }

            if (kitchen != null)
            {
                kitchen.OnCookingStarted += OnCookingStarted;
                kitchen.OnCookingCompleted += OnCookingCompleted;
            }
        }

        private void OnDestroy()
        {
            if (kitchen != null)
            {
                kitchen.OnCookingStarted -= OnCookingStarted;
                kitchen.OnCookingCompleted -= OnCookingCompleted;
            }
        }

        private void Update()
        {
            // Hide notification after timer expires
            if (notificationTimer > 0)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0 && completionNotification != null)
                {
                    completionNotification.SetActive(false);
                }
            }

            // Sync slot UIs with kitchen orders
            SyncSlots();
        }

        private void OnCookingStarted(Kitchen.CookingOrder order)
        {
            // Find or create a slot UI
            CookingSlotUI slotUI = GetAvailableSlot();
            if (slotUI != null)
            {
                slotUI.Setup(order);
            }
        }

        private void OnCookingCompleted(Kitchen.CookingOrder order)
        {
            // Show notification
            if (completionNotification != null)
            {
                completionNotification.SetActive(true);
                notificationTimer = notificationDuration;
            }

            // Play sound
            if (audioSource != null && completionSound != null)
            {
                audioSource.PlayOneShot(completionSound);
            }

            Debug.Log($"[KitchenUI] Order completed: {order.recipe.recipeName}");
        }

        private CookingSlotUI GetAvailableSlot()
        {
            // Try to find an inactive slot
            foreach (var slot in slotUIs)
            {
                if (!slot.gameObject.activeSelf)
                {
                    return slot;
                }
            }

            // Create a new slot if needed
            if (cookingSlotPrefab != null && slotContainer != null)
            {
                GameObject slotObj = Instantiate(cookingSlotPrefab, slotContainer);
                CookingSlotUI newSlot = slotObj.GetComponent<CookingSlotUI>();
                if (newSlot != null)
                {
                    slotUIs.Add(newSlot);
                    return newSlot;
                }
            }

            return null;
        }

        private void SyncSlots()
        {
            if (kitchen == null) return;

            var activeOrders = kitchen.ActiveOrders;

            // Clear slots that no longer have matching orders
            foreach (var slotUI in slotUIs)
            {
                if (!slotUI.gameObject.activeSelf) continue;

                var order = slotUI.GetOrder();
                if (order != null && !activeOrders.Contains(order))
                {
                    slotUI.Clear();
                }
            }
        }

        /// <summary>
        /// Manually trigger serve action (for tap-to-serve feature)
        /// </summary>
        public void OnServeButtonClicked()
        {
            if (kitchen == null || !kitchen.HasCompletedOrders()) return;

            var order = kitchen.TakeCompletedOrder();
            if (order != null && order.customer != null)
            {
                order.customer.ReceiveFood();
                Debug.Log($"[KitchenUI] Manually served: {order.recipe.recipeName}");
            }
        }
    }
}
