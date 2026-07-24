using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Shows Part 3 architecture diagrams (not a gameplay scene).
    /// </summary>
    public class EscapeRoomArchitectureViewer : MonoBehaviour
    {
        private void Start()
        {
            // Always rebuild so Play Mode after script recompile cannot keep a broken empty canvas.
            DestroyExistingViewerCanvas();
            BuildViewerUi();
        }

        private static void DestroyExistingViewerCanvas()
        {
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas != null && canvas.name == "ArchitectureCanvas")
                {
                    Destroy(canvas.gameObject);
                }
            }
        }

        private static void BuildViewerUi()
        {
            var canvasObject = new GameObject("ArchitectureCanvas", typeof(RectTransform));
            Stretch(canvasObject.GetComponent<RectTransform>());

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            var root = CreateRect("Root", canvasObject.transform, withImage: true);
            Stretch(root);
            root.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f);

            const float headerHeight = 56f;
            var header = CreateRect("Header", root, withImage: true);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, headerHeight);
            header.anchoredPosition = Vector2.zero;
            header.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.98f);

            var title = CreateLabel(
                header,
                "Title",
                "Part 3 — Escape Room Architecture",
                22,
                TextAlignmentOptions.Center);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(16f, 8f);
            titleRect.offsetMax = new Vector2(-150f, -8f);

            var backButton = CreateButton(header, "Back", new Vector2(140f, 36f));
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(1f, 0.5f);
            backRect.anchorMax = new Vector2(1f, 0.5f);
            backRect.pivot = new Vector2(1f, 0.5f);
            backRect.anchoredPosition = new Vector2(-16f, 0f);
            backButton.onClick.AddListener(() =>
            {
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.ReturnToMainMenu();
                }
                else
                {
                    SceneManager.LoadScene("MainMenu");
                }
            });

            var scrollObject = CreateRect("Scroll", root, withImage: true);
            scrollObject.anchorMin = new Vector2(0f, 0f);
            scrollObject.anchorMax = new Vector2(1f, 1f);
            scrollObject.offsetMin = new Vector2(16f, 16f);
            scrollObject.offsetMax = new Vector2(-16f, -(headerHeight + 8f));
            scrollObject.GetComponent<Image>().color = new Color(0.1f, 0.11f, 0.15f, 0.95f);

            var viewport = CreateRect("Viewport", scrollObject, withImage: true);
            Stretch(viewport);
            viewport.GetComponent<Image>().color = new Color(0.1f, 0.11f, 0.15f, 1f);
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content: VerticalLayoutGroup ONLY (no ContentSizeFitter on this GO).
            var content = CreateRect("Content", viewport, withImage: false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 100f);

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 16, 24);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var scrollRect = scrollObject.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 40f;

            CreateTextBlock(
                content,
                "Assignment note",
                "Playable scene: Part 3 → Crimson Mini Room.\n" +
                "This screen shows architecture diagrams for submission.\n" +
                "Visitor (Unseen pattern) is used in Crimson Mini Room (I / R / item use).");

            CreateTextBlock(
                content,
                "Patterns",
                "Visitor · State + Observer · Strategy · Composite · ScriptableObject · Factory");

            CreateTextBlock(
                content,
                "Visitor",
                "IRoomItem.Accept(IRoomItemVisitor) on Pickup / Note / Drawer / Door / Safe / Decoy.\n" +
                "• InspectVisitor — I (inspect all)\n" +
                "• InventoryReportVisitor — R (room report)\n" +
                "• UseOnTargetVisitor — inventory item + click target\n\n" +
                "Play: Part 3 Escape Room → Crimson Mini Room.");

            AddDiagram(content, "Class Diagram — Visitor", "visitor_class_diagram");

            CreateTextBlock(
                content,
                "Room content (6 kinds)",
                "Pickup · Drawer · Note · Safe · Door · Decoy\n" +
                "Objects: Rusty Key, Drawer, Note, Safe, Gold Key, Door, Red Box (decoy)\n\n" +
                "1) Use Rusty Key on Drawer\n" +
                "2) Read Note\n" +
                "3) Enter code on Safe → Gold Key\n" +
                "4) Use Gold Key on Door → escape");

            AddDiagram(content, "Class Diagram", "class_diagram");
            AddDiagram(content, "Sequence — Use Item On Item", "sequence_item_on_item");
            AddDiagram(content, "Sequence — Solve Puzzle / Open Door", "sequence_puzzle_door");

            CreateTextBlock(
                content,
                "How to extend the room",
                "1) Add a RoomObjectSpawn on EscapeRoomSetup (and ItemDefinition if it is a pickup)\n" +
                "2) Add or reorder InteractionRuleSO (source to target + actions)\n" +
                "3) Update PuzzleDefinition prerequisites / code if needed\n" +
                "4) For a new report-style operation, add an IRoomItemVisitor\n" +
                "5) Press Play\n\n" +
                "Inventory slots show found items; Used shows consumed items.\n\n" +
                "Details: Docs/Part3_EscapeRoom/Architecture.md");

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            var preferredHeight = LayoutUtility.GetPreferredHeight(content);
            if (preferredHeight < 200f)
            {
                // Fallback if layout utility still returns near-zero: sum child preferred heights.
                preferredHeight = layout.padding.top + layout.padding.bottom;
                for (var i = 0; i < content.childCount; i++)
                {
                    var child = content.GetChild(i) as RectTransform;
                    if (child == null)
                    {
                        continue;
                    }

                    var le = child.GetComponent<LayoutElement>();
                    preferredHeight += le != null ? Mathf.Max(le.preferredHeight, le.minHeight) : 40f;
                    if (i < content.childCount - 1)
                    {
                        preferredHeight += layout.spacing;
                    }
                }
            }

            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(preferredHeight, 800f));
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private static void AddDiagram(RectTransform parent, string title, string resourceName)
        {
            CreateTextBlock(parent, title, null);

            var imageObject = CreateRect(title + "Image", parent, withImage: true);
            var layout = imageObject.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 480f;
            layout.minHeight = 480f;
            layout.flexibleWidth = 1f;
            layout.flexibleHeight = 0f;

            var image = imageObject.GetComponent<Image>();
            image.preserveAspect = true;
            image.color = Color.white;

            var sprite = Resources.Load<Sprite>($"Diagrams/{resourceName}");
            if (sprite == null)
            {
                var texture = Resources.Load<Texture2D>($"Diagrams/{resourceName}");
                if (texture != null)
                {
                    sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }
            }

            if (sprite != null)
            {
                image.sprite = sprite;
            }
            else
            {
                image.color = new Color(0.25f, 0.28f, 0.35f, 1f);
                var missing = CreateLabel(
                    imageObject,
                    "Missing",
                    $"Missing: Resources/Diagrams/{resourceName}.png",
                    14,
                    TextAlignmentOptions.Center);
                Stretch(missing.rectTransform);
            }
        }

        private static void CreateTextBlock(RectTransform parent, string title, string body)
        {
            var lineCount = string.IsNullOrEmpty(body) ? 0 : body.Split('\n').Length;
            var bodyHeight = string.IsNullOrEmpty(body)
                ? 0f
                : Mathf.Clamp(24f + lineCount * 20f, 40f, 400f);
            var blockHeight = 26f + (string.IsNullOrEmpty(body) ? 0f : 4f + bodyHeight);

            var block = CreateRect(title + "Block", parent, withImage: false);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.flexibleWidth = 1f;
            blockLayout.flexibleHeight = 0f;
            blockLayout.minHeight = blockHeight;
            blockLayout.preferredHeight = blockHeight;

            var titleLabel = CreateLabel(block, "Title", title, 18, TextAlignmentOptions.TopLeft);
            titleLabel.fontStyle = FontStyles.Bold;
            var titleRect = titleLabel.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(0f, 26f);

            if (!string.IsNullOrEmpty(body))
            {
                var bodyLabel = CreateLabel(block, "Body", body, 15, TextAlignmentOptions.TopLeft);
                bodyLabel.color = new Color(0.86f, 0.89f, 0.95f, 1f);
                var bodyRect = bodyLabel.rectTransform;
                bodyRect.anchorMin = new Vector2(0f, 1f);
                bodyRect.anchorMax = new Vector2(1f, 1f);
                bodyRect.pivot = new Vector2(0.5f, 1f);
                bodyRect.anchoredPosition = new Vector2(0f, -30f);
                bodyRect.sizeDelta = new Vector2(0f, bodyHeight);
            }
        }

        private static TMP_Text CreateLabel(
            Transform parent,
            string name,
            string text,
            int fontSize,
            TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = new Color(0.94f, 0.96f, 1f, 1f);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 size)
        {
            var buttonObject = CreateRect(label + "Button", parent, withImage: true);
            buttonObject.sizeDelta = size;
            buttonObject.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.85f, 1f);

            var button = buttonObject.gameObject.AddComponent<Button>();
            var labelTmp = CreateLabel(buttonObject, "Label", label, 16, TextAlignmentOptions.Center);
            Stretch(labelTmp.rectTransform);
            return button;
        }

        private static RectTransform CreateRect(string name, Transform parent, bool withImage)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            if (withImage)
            {
                go.AddComponent<CanvasRenderer>();
                go.AddComponent<Image>();
            }

            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }
}
