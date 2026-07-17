using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Builds readable UI elements from UiTheme (no absolute pixel hacks).
    /// </summary>
    public static class UiFactory
    {
        public static GameObject CreateScaledCanvas(string name, UiTheme theme)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = theme.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = theme.MatchWidthOrHeight;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        public static GameObject CreateFullScreenOverlay(Transform parent, string name, Color color)
        {
            var overlay = new GameObject(name);
            overlay.transform.SetParent(parent, false);
            var image = overlay.AddComponent<Image>();
            image.color = color;
            StretchFull(overlay.GetComponent<RectTransform>());
            return overlay;
        }

        public static GameObject CreateMenuCard(Transform parent, string name, UiTheme theme)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            var image = card.AddComponent<Image>();
            image.color = theme.PanelColor;

            var rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(theme.PanelWidth, 480f);

            var layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                (int)theme.PanelPadding,
                (int)theme.PanelPadding,
                (int)theme.PanelPadding,
                (int)theme.PanelPadding);
            layout.spacing = theme.Spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = card.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return card;
        }

        public static TMP_Text CreateText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            UiTheme theme,
            Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var tmp = textObject.AddComponent<TextMeshProUGUI>();
            if (theme.Font != null)
            {
                tmp.font = theme.Font;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = false;
            tmp.alignment = alignment;
            tmp.color = color ?? theme.TextColor;
            tmp.raycastTarget = false;

            var layout = textObject.AddComponent<LayoutElement>();
            layout.minHeight = fontSize + 12f;
            layout.preferredHeight = fontSize + 16f;

            return tmp;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            UiTheme theme,
            bool primary = true)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.AddComponent<Image>();
            image.color = primary ? theme.PrimaryButtonColor : theme.SecondaryButtonColor;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            var layout = buttonObject.AddComponent<LayoutElement>();
            layout.minHeight = theme.ButtonHeight;
            layout.preferredHeight = theme.ButtonHeight;
            layout.flexibleWidth = 1f;

            var text = CreateText(
                buttonObject.transform,
                "Label",
                label,
                theme.ButtonLabelSize,
                theme,
                theme.TextColor);
            StretchFull(text.rectTransform);
            text.raycastTarget = false;
            Object.DestroyImmediate(text.GetComponent<LayoutElement>());

            return button;
        }

        public static TMP_Dropdown CreateDropdown(Transform parent, string name, UiTheme theme)
        {
            var dropdownObject = new GameObject(name);
            dropdownObject.transform.SetParent(parent, false);
            var image = dropdownObject.AddComponent<Image>();
            image.color = theme.DropdownBackgroundColor;
            var dropdown = dropdownObject.AddComponent<TMP_Dropdown>();

            var layout = dropdownObject.AddComponent<LayoutElement>();
            layout.minHeight = theme.ButtonHeight;
            layout.preferredHeight = theme.ButtonHeight;
            layout.flexibleWidth = 1f;

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(dropdownObject.transform, false);
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            if (theme.Font != null)
            {
                label.font = theme.Font;
            }

            label.fontSize = theme.ButtonLabelSize;
            label.enableAutoSizing = false;
            label.color = theme.DropdownTextColor;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            StretchFull(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(16f, 0f);
            label.rectTransform.offsetMax = new Vector2(-40f, 0f);

            var arrowObject = new GameObject("Arrow");
            arrowObject.transform.SetParent(dropdownObject.transform, false);
            var arrow = arrowObject.AddComponent<TextMeshProUGUI>();
            if (theme.Font != null)
            {
                arrow.font = theme.Font;
            }

            arrow.text = "▼";
            arrow.fontSize = 18f;
            arrow.color = theme.DropdownTextColor;
            arrow.alignment = TextAlignmentOptions.Center;
            arrow.raycastTarget = false;
            var arrowRect = arrow.rectTransform;
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.sizeDelta = new Vector2(36f, 36f);
            arrowRect.anchoredPosition = new Vector2(-8f, 0f);

            var template = new GameObject("Template");
            template.transform.SetParent(dropdownObject.transform, false);
            template.SetActive(false);
            var templateImage = template.AddComponent<Image>();
            templateImage.color = theme.DropdownBackgroundColor;
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0f, 200f);
            templateRect.anchoredPosition = Vector2.zero;

            var scroll = template.AddComponent<ScrollRect>();

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            StretchFull(viewportRect);
            viewport.AddComponent<Image>().color = theme.DropdownBackgroundColor;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, theme.ButtonHeight);

            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var itemToggle = item.AddComponent<Toggle>();
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 48f);

            var itemBg = item.AddComponent<Image>();
            itemBg.color = theme.DropdownBackgroundColor;
            itemToggle.targetGraphic = itemBg;

            var itemLabelObject = new GameObject("Item Label");
            itemLabelObject.transform.SetParent(item.transform, false);
            var itemLabel = itemLabelObject.AddComponent<TextMeshProUGUI>();
            if (theme.Font != null)
            {
                itemLabel.font = theme.Font;
            }

            itemLabel.fontSize = theme.ButtonLabelSize;
            itemLabel.enableAutoSizing = false;
            itemLabel.color = theme.DropdownTextColor;
            itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
            StretchFull(itemLabel.rectTransform);
            itemLabel.rectTransform.offsetMin = new Vector2(16f, 0f);
            itemLabel.rectTransform.offsetMax = new Vector2(-16f, 0f);

            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;

            dropdown.captionText = label;
            dropdown.template = templateRect;
            dropdown.itemText = itemLabel;

            return dropdown;
        }

        public static GameObject CreateVerticalStack(Transform parent, string name, float spacing)
        {
            var stack = new GameObject(name);
            stack.transform.SetParent(parent, false);
            var layout = stack.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var layoutElement = stack.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1f;
            return stack;
        }

        public static GameObject CreateHorizontalStack(Transform parent, string name, float spacing)
        {
            var stack = new GameObject(name);
            stack.transform.SetParent(parent, false);
            var layout = stack.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            var layoutElement = stack.AddComponent<LayoutElement>();
            layoutElement.minHeight = 56f;
            layoutElement.preferredHeight = 56f;
            layoutElement.flexibleWidth = 1f;
            return stack;
        }

        public static TMP_Text CreateHudText(
            Transform parent,
            string name,
            string text,
            UiTheme theme,
            TextAnchor corner,
            Vector2 size)
        {
            var label = CreateText(parent, name, text, theme.BodySize + 4f, theme, theme.TextColor, TextAlignmentOptions.TopLeft);
            Object.DestroyImmediate(label.GetComponent<LayoutElement>());
            var rect = label.rectTransform;
            ApplyCorner(rect, corner, size, new Vector2(24f, 24f));
            return label;
        }

        public static void ApplyCorner(RectTransform rect, TextAnchor corner, Vector2 size, Vector2 margin)
        {
            rect.sizeDelta = size;
            switch (corner)
            {
                case TextAnchor.UpperLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    rect.anchoredPosition = new Vector2(margin.x, -margin.y);
                    break;
                case TextAnchor.UpperRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    rect.anchoredPosition = new Vector2(-margin.x, -margin.y);
                    break;
                default:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                    break;
            }
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
