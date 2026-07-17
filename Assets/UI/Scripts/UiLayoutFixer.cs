using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Runtime normalizer for PersistentUI built with oversized UiFactory theme values.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class UiLayoutFixer : MonoBehaviour
    {
        private void Awake()
        {
            EnsureCanvasScaler();
            NormalizeAllCards();
            NormalizeButtons();
            NormalizeTexts();
            NormalizeDropdowns();
        }

        private void EnsureCanvasScaler()
        {
            if (GetComponent<AdaptiveCanvasGuard>() == null)
            {
                gameObject.AddComponent<AdaptiveCanvasGuard>();
            }

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void NormalizeAllCards()
        {
            foreach (Transform child in transform)
            {
                var card = child.Find("Card");
                if (card == null)
                {
                    continue;
                }

                var rect = card as RectTransform;
                if (rect == null)
                {
                    continue;
                }

                // Compact centered card instead of almost-fullscreen stretch.
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = child.name switch
                {
                    "LevelSelectScreen" => new Vector2(420f, 420f),
                    "GameOverScreen" => new Vector2(380f, 240f),
                    "PauseScreen" => new Vector2(360f, 320f),
                    _ => new Vector2(420f, 360f)
                };

                var vertical = card.GetComponent<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    vertical.padding = new RectOffset(24, 24, 24, 24);
                    vertical.spacing = 12f;
                    vertical.childAlignment = TextAnchor.MiddleCenter;
                    vertical.childControlWidth = true;
                    vertical.childControlHeight = true;
                    vertical.childForceExpandWidth = true;
                    vertical.childForceExpandHeight = false;
                }

                var fitter = card.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                {
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                }

                foreach (Transform cardChild in card)
                {
                    if (cardChild.name == "Spacer")
                    {
                        cardChild.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void NormalizeButtons()
        {
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                var rect = button.transform as RectTransform;
                if (rect == null)
                {
                    continue;
                }

                var layout = button.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    layout.minHeight = 40f;
                    layout.preferredHeight = 40f;
                    layout.preferredWidth = 150f;
                    layout.flexibleWidth = 0f;
                    layout.flexibleHeight = 0f;
                }
                else
                {
                    rect.sizeDelta = new Vector2(150f, 40f);
                }

                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.enableAutoSizing = false;
                    label.fontSize = 18f;
                    label.textWrappingMode = TextWrappingModes.NoWrap;
                    label.overflowMode = TextOverflowModes.Truncate;
                }
            }

            // Button rows should not force children to fill width.
            foreach (var horizontal in GetComponentsInChildren<HorizontalLayoutGroup>(true))
            {
                if (horizontal.gameObject.name != "Buttons" &&
                    horizontal.gameObject.name != "ButtonRow")
                {
                    continue;
                }

                horizontal.childForceExpandWidth = false;
                horizontal.childForceExpandHeight = false;
                horizontal.childControlWidth = true;
                horizontal.childControlHeight = true;
                horizontal.spacing = 16f;
                horizontal.childAlignment = TextAnchor.MiddleCenter;

                var rowLayout = horizontal.GetComponent<LayoutElement>();
                if (rowLayout != null)
                {
                    rowLayout.minHeight = 40f;
                    rowLayout.preferredHeight = 44f;
                    rowLayout.flexibleWidth = 0f;
                }
            }
        }

        private void NormalizeTexts()
        {
            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
            {
                if (text == null)
                {
                    continue;
                }

                text.enableAutoSizing = false;

                if (text.name.Contains("Title"))
                {
                    text.fontSize = 28f;
                    text.textWrappingMode = TextWrappingModes.Normal;
                    text.overflowMode = TextOverflowModes.Truncate;
                }
                else if (text.name.Contains("Message") || text.name.Contains("Progress"))
                {
                    text.fontSize = 22f;
                    text.textWrappingMode = TextWrappingModes.Normal;
                    text.overflowMode = TextOverflowModes.Truncate;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 16f;
                    text.fontSizeMax = 24f;
                }
                else if (text.fontSize > 24f)
                {
                    text.fontSize = 18f;
                }

                var layout = text.GetComponent<LayoutElement>();
                if (layout != null && text.name.Contains("Title"))
                {
                    layout.minHeight = 36f;
                    layout.preferredHeight = 40f;
                }
            }
        }

        private void NormalizeDropdowns()
        {
            foreach (var dropdown in GetComponentsInChildren<TMP_Dropdown>(true))
            {
                var rect = dropdown.transform as RectTransform;
                if (rect != null)
                {
                    var layout = dropdown.GetComponent<LayoutElement>();
                    if (layout != null)
                    {
                        layout.minHeight = 40f;
                        layout.preferredHeight = 40f;
                        layout.flexibleWidth = 1f;
                    }
                    else
                    {
                        rect.sizeDelta = new Vector2(320f, 40f);
                    }
                }

                if (dropdown.captionText != null)
                {
                    dropdown.captionText.enableAutoSizing = false;
                    dropdown.captionText.fontSize = 18f;
                    dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap;
                    dropdown.captionText.overflowMode = TextOverflowModes.Truncate;

                    var captionRect = dropdown.captionText.rectTransform;
                    captionRect.offsetMin = new Vector2(12f, 4f);
                    captionRect.offsetMax = new Vector2(-28f, -4f);
                }

                if (dropdown.itemText != null)
                {
                    dropdown.itemText.fontSize = 16f;
                }
            }
        }
    }
}
