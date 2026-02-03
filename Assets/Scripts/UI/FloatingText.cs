using UnityEngine;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// Floating text animation for earnings, tips, and notifications
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float lifetime = 1.5f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float fadeSpeed = 1f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1f);

        [Header("Movement")]
        [SerializeField] private Vector3 moveDirection = Vector3.up;
        [SerializeField] private bool randomizeDirection = true;
        [SerializeField] private float randomAngle = 30f;

        private TextMeshPro textMesh;
        private float elapsedTime;
        private Color initialColor;
        private Vector3 startPosition;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh == null)
            {
                textMesh = gameObject.AddComponent<TextMeshPro>();
            }

            startPosition = transform.position;
        }

        public void Setup(string text, Color color)
        {
            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
                initialColor = color;
            }

            // Randomize direction slightly
            if (randomizeDirection)
            {
                float angle = Random.Range(-randomAngle, randomAngle);
                moveDirection = Quaternion.Euler(0, 0, angle) * moveDirection;
            }

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / lifetime;

            // Move upward
            transform.position = startPosition + moveDirection * moveSpeed * elapsedTime;

            // Scale animation
            float scale = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.one * scale;

            // Fade out
            if (textMesh != null)
            {
                Color color = initialColor;
                color.a = Mathf.Lerp(1f, 0f, normalizedTime * fadeSpeed);
                textMesh.color = color;
            }
        }

        /// <summary>
        /// Quick setup with default yellow color for tips
        /// </summary>
        public void SetupTip(int amount)
        {
            Setup($"+{amount} Tip!", Color.yellow);
        }

        /// <summary>
        /// Quick setup with default white color for regular earnings
        /// </summary>
        public void SetupEarnings(int amount)
        {
            Setup($"+{amount}", Color.white);
        }
    }
}
