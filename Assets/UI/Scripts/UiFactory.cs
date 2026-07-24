using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Builds adaptive UI from UiTheme using stretch anchors and layout groups.
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
            canvasObject.AddComponent<AdaptiveCanvasGuard>();

            var rect = canvasObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
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

        /// <summary>
        /// Screen-relative card: stretches with margins so it stays large on any aspect.
        /// </summary>
        public static GameObject CreateMenuCard(Transform parent, string name, UiTheme theme)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            var image = card.AddComponent<Image>();
            image.color = theme.PanelColor;

            var rect = card.GetComponent<RectTransform>();
            var h = Mathf.Clamp01(theme.CardHorizontalMargin);
            var v = Mathf.Clamp01(theme.CardVerticalMargin);
            rect.anchorMin = new Vector2(h, v);
            rect.anchorMax = new Vector2(1f - h, 1f - v);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            var layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                (int)theme.PanelPadding,
                (int)theme.PanelPadding,
                (int)theme.PanelPadding,
                (int)theme.PanelPadding);
            layout.spacing = theme.Spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return card;
        }

        public static GameObject CreateFlexibleSpacer(Transform parent, float flexibleHeight = 1f)
        {
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);
            var layout = spacer.AddComponent<LayoutElement>();
            layout.flexibleHeight = flexibleHeight;
            layout.flexibleWidth = 1f;
            layout.minHeight = 0f;
            return spacer;
        }

        public static TMP_Text CreateText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            UiTheme theme,
            Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            bool addLayoutElement = true)
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
            tmp.overflowMode = TextOverflowModes.Overflow;

            if (addLayoutElement)
            {
                var layout = textObject.AddComponent<LayoutElement>();
                layout.minHeight = fontSize * 1.35f;
                layout.preferredHeight = fontSize * 1.5f;
                layout.flexibleWidth = 1f;
            }

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
                theme.TextColor,
                TextAlignmentOptions.Center,
                addLayoutElement: false);
            StretchFull(text.rectTransform);
            text.raycastTarget = false;

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
            label.overflowMode = TextOverflowModes.Ellipsis;
            StretchFull(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(24f, 8f);
            label.rectTransform.offsetMax = new Vector2(-56f, -8f);

            var arrowObject = new GameObject("Arrow");
            arrowObject.transform.SetParent(dropdownObject.transform, false);
            var arrow = arrowObject.AddComponent<TextMeshProUGUI>();
            if (theme.Font != null)
            {
                arrow.font = theme.Font;
            }

            arrow.text = "▼";
            arrow.fontSize = theme.ButtonLabelSize * 0.7f;
            arrow.color = theme.DropdownTextColor;
            arrow.alignment = TextAlignmentOptions.Center;
            arrow.raycastTarget = false;
            var arrowRect = arrow.rectTransform;
            arrowRect.anchorMin = new Vector2(1f, 0f);
            arrowRect.anchorMax = new Vector2(1f, 1f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.offsetMin = new Vector2(-52f, 0f);
            arrowRect.offsetMax = new Vector2(-8f, 0f);

            var template = new GameObject("Template");
            template.transform.SetParent(dropdownObject.transform, false);
            template.SetActive(false);
            var templateImage = template.AddComponent<Image>();
            templateImage.color = theme.DropdownBackgroundColor;
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0f, theme.ButtonHeight * 4.5f);
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
            itemRect.sizeDelta = new Vector2(0f, theme.ButtonHeight);

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
            itemLabel.rectTransform.offsetMin = new Vector2(24f, 0f);
            itemLabel.rectTransform.offsetMax = new Vector2(-24f, 0f);

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
            layoutElement.flexibleHeight = 1f;
            return stack;
        }

        public static GameObject CreateHorizontalStack(Transform parent, string name, float spacing, float height)
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
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 1f;
            return stack;
        }

        public static GameObject CreateHorizontalStack(Transform parent, string name, float spacing)
        {
            return CreateHorizontalStack(parent, name, spacing, 80f);
        }

        public static TMP_Text CreateHudText(
            Transform parent,
            string name,
            string text,
            UiTheme theme,
            TextAnchor corner,
            Vector2 size)
        {
            var label = CreateText(
                parent,
                name,
                text,
                theme.BodySize,
                theme,
                theme.TextColor,
                TextAlignmentOptions.TopLeft,
                addLayoutElement: false);
            ApplyCorner(label.rectTransform, corner, size, new Vector2(32f, 32f));
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
