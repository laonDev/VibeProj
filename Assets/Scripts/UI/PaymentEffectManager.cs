using UnityEngine;

namespace AnimalKitchen
{
    /// <summary>
    /// Manages payment visual effects including gold particles and tip displays
    /// </summary>
    public class PaymentEffectManager : MonoBehaviour
    {
        public static PaymentEffectManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject floatingTextPrefab;
        [SerializeField] private ParticleSystem goldParticlePrefab;

        [Header("Settings")]
        [SerializeField] private Transform effectParent;
        [SerializeField] private Color billColor = Color.white;
        [SerializeField] private Color tipColor = Color.yellow;

        [Header("Audio")]
        [SerializeField] private AudioClip paymentSound;
        [SerializeField] private AudioClip tipSound;

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (effectParent == null)
            {
                effectParent = transform;
            }
        }

        private void Start()
        {
            // Subscribe to all customer payment events
            var spawner = FindObjectOfType<CustomerSpawner>();
            if (spawner != null)
            {
                spawner.OnCustomerSpawned += SubscribeToCustomer;
            }
        }

        private void SubscribeToCustomer(Customer customer)
        {
            if (customer != null)
            {
                customer.OnPaymentComplete += OnCustomerPayment;
            }
        }

        private void OnCustomerPayment(Customer customer, int bill, int tip)
        {
            Vector3 worldPos = customer.transform.position;

            // Show bill amount
            ShowFloatingText(worldPos, $"+{bill}", billColor);

            // Show tip amount if any
            if (tip > 0)
            {
                ShowFloatingText(worldPos + Vector3.up * 0.5f, $"+{tip} Tip!", tipColor);

                if (audioSource != null && tipSound != null)
                {
                    audioSource.PlayOneShot(tipSound);
                }
            }
            else if (audioSource != null && paymentSound != null)
            {
                audioSource.PlayOneShot(paymentSound);
            }

            // Spawn gold particles
            SpawnGoldParticles(worldPos);

            Debug.Log($"[PaymentEffect] Displayed: Bill={bill}, Tip={tip}");
        }

        public void ShowFloatingText(Vector3 worldPosition, string text, Color color)
        {
            if (floatingTextPrefab == null) return;

            GameObject textObj = Instantiate(floatingTextPrefab, worldPosition, Quaternion.identity, effectParent);
            FloatingText floatingText = textObj.GetComponent<FloatingText>();

            if (floatingText != null)
            {
                floatingText.Setup(text, color);
            }
        }

        public void SpawnGoldParticles(Vector3 worldPosition)
        {
            if (goldParticlePrefab == null) return;

            ParticleSystem particles = Instantiate(goldParticlePrefab, worldPosition, Quaternion.identity, effectParent);
            particles.Play();

            Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }

        /// <summary>
        /// Show generic earning popup (used by other systems)
        /// </summary>
        public void ShowEarnings(Vector3 worldPosition, int amount, bool isTip = false)
        {
            Color color = isTip ? tipColor : billColor;
            string text = isTip ? $"+{amount} Tip!" : $"+{amount}";

            ShowFloatingText(worldPosition, text, color);
            SpawnGoldParticles(worldPosition);
        }
    }
}
