using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AnimalKitchen
{
    /// <summary>
    /// Simple test panel to verify UI functionality
    /// </summary>
    public class SimplePanelTest : MonoBehaviour
    {
        public static GameObject CreateSimplePanel(string title, Transform parent, TMPro.TMP_FontAsset font)
        {
            // Panel container
            var panelGO = new GameObject($"{title}Panel");
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            // Background
            var bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            // Content area
            var contentGO = new GameObject("Content");
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.SetParent(panelRect, false);
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(800, 600);
            contentRect.anchoredPosition = Vector2.zero;

            var contentBg = contentGO.AddComponent<Image>();
            contentBg.color = new Color(0.2f, 0.2f, 0.2f);

            // Title
            var titleGO = new GameObject("Title");
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.SetParent(contentRect, false);
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 80);
            titleRect.anchoredPosition = Vector2.zero;

            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 36;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.font = font;

            // Info text
            var infoGO = new GameObject("Info");
            var infoRect = infoGO.AddComponent<RectTransform>();
            infoRect.SetParent(contentRect, false);
            infoRect.anchorMin = new Vector2(0.1f, 0.1f);
            infoRect.anchorMax = new Vector2(0.9f, 0.7f);
            infoRect.sizeDelta = Vector2.zero;
            infoRect.anchoredPosition = Vector2.zero;

            var infoText = infoGO.AddComponent<TextMeshProUGUI>();
            infoText.text = $"This is the {title} panel.\n\nUI implementation coming soon!";
            infoText.fontSize = 24;
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.color = Color.gray;
            infoText.font = font;

            // Close button
            var closeButtonGO = new GameObject("CloseButton");
            var closeButtonRect = closeButtonGO.AddComponent<RectTransform>();
            closeButtonRect.SetParent(contentRect, false);
            closeButtonRect.anchorMin = new Vector2(0.5f, 0);
            closeButtonRect.anchorMax = new Vector2(0.5f, 0);
            closeButtonRect.pivot = new Vector2(0.5f, 0);
            closeButtonRect.sizeDelta = new Vector2(200, 60);
            closeButtonRect.anchoredPosition = new Vector2(0, 20);

            var closeButton = closeButtonGO.AddComponent<Button>();
            var closeButtonImage = closeButtonGO.AddComponent<Image>();
            closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f);

            // Close button text
            var closeTextGO = new GameObject("Text");
            var closeTextRect = closeTextGO.AddComponent<RectTransform>();
            closeTextRect.SetParent(closeButtonRect, false);
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.sizeDelta = Vector2.zero;

            var closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
            closeText.text = "Close";
            closeText.fontSize = 24;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;
            closeText.font = font;

            // Add close functionality
            closeButton.onClick.AddListener(() => panelGO.SetActive(false));

            // Start deactivated
            panelGO.SetActive(false);

            return panelGO;
        }
    }
}
