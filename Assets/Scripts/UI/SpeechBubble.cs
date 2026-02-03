using System.Collections;
using UnityEngine;
using TMPro;

namespace AnimalKitchen
{
    public class SpeechBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float heightOffset = 1f;

        private Transform target;
        private Canvas canvas;
        private Coroutine hideCoroutine;

        private void Awake()
        {
            // Auto-setup if not assigned
            if (messageText == null)
            {
                messageText = GetComponentInChildren<TextMeshProUGUI>();
            }

            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;

                // Scale down for world space
                transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }

            // Ensure canvas sorts on top
            canvas.sortingOrder = 100;

            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                // Follow target position
                transform.position = target.position + Vector3.up * heightOffset;

                // Always face camera
                if (Camera.main != null)
                {
                    transform.rotation = Camera.main.transform.rotation;
                }
            }
        }

        public void Show(string message, Transform targetTransform, float duration = -1)
        {
            if (string.IsNullOrEmpty(message)) return;

            target = targetTransform;

            if (messageText != null)
            {
                messageText.text = message;
            }

            gameObject.SetActive(true);

            // Stop previous hide coroutine if any
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            // Use custom duration or default
            float actualDuration = duration > 0 ? duration : displayDuration;
            hideCoroutine = StartCoroutine(HideAfterDelay(actualDuration));
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            target = null;
        }

        public static SpeechBubble Create(Transform parent, string initialMessage = "")
        {
            // Create speech bubble GameObject
            var bubbleGO = new GameObject("SpeechBubble");
            bubbleGO.transform.SetParent(parent);
            bubbleGO.transform.localPosition = Vector3.up * 1f;

            // Add Canvas
            var canvas = bubbleGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Add CanvasScaler for better text rendering
            var scaler = bubbleGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            // Create background (optional)
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(bubbleGO.transform);
            bgGO.transform.localPosition = Vector3.zero;
            bgGO.transform.localScale = Vector3.one;

            var bgImage = bgGO.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.9f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(200, 60);

            // Create text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(bgGO.transform);
            textGO.transform.localPosition = Vector3.zero;
            textGO.transform.localScale = Vector3.one;

            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = initialMessage;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Add SpeechBubble component
            var bubble = bubbleGO.AddComponent<SpeechBubble>();
            bubble.messageText = text;

            // Scale for world space
            bubbleGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            return bubble;
        }
    }
}
