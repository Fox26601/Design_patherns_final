using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Scene entry for the point-and-click escape room.
    /// </summary>
    public class EscapeRoomGameController : MonoBehaviour
    {
        [SerializeField] private EscapeRoomSetupSO setup;
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private MessageLogUI messageLog;
        [SerializeField] private CodeInputPopup codePopup;
        [SerializeField] private ExaminePanel examinePanel;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private LayerMask interactableMask = ~0;

        private EscapeRoomController _controller;
        private bool _won;
        private InteractableView _hoveredView;
        private RectTransform _hoverTooltipRoot;
        private TMP_Text _hoverTooltipText;

        public EscapeRoomController Controller => _controller;

        private void Start()
        {
            if (FindFirstObjectByType<GamePauseHandler>() == null)
            {
                var pauseObject = new GameObject("GamePauseHandler");
                pauseObject.AddComponent<GamePauseHandler>();
            }

            if (raycastCamera == null)
            {
                raycastCamera = Camera.main;
            }

            if (setup == null)
            {
                setup = Resources.Load<EscapeRoomSetupSO>("EscapeRoomSetup");
            }

            BuildWorldIfMissing();
            StripLegacyWorldLabels();
            BuildUiIfMissing();
            FindSceneUi();
            EnsureHoverTooltip();

            _controller = new EscapeRoomController();
            _controller.Configure(setup);
            _controller.OnWinRequested += HandleWin;

            BindInteractables();
            inventoryUI?.Bind(_controller);
            messageLog?.AddMessage("You wake up in a locked room.");
            messageLog?.AddMessage("Look around. Collect items. Escape.");

            if (hintText != null)
            {
                hintText.text = setup != null
                    ? $"{setup.HintText}\nI = inspect all | R = room report | item+click = use."
                    : "Click objects. Keys 1-6 select inventory. I/R = reports. Esc pauses.";
                hintText.raycastTarget = false;
            }

            if (codePopup != null)
            {
                codePopup.OnCodeSubmitted += HandleCodeSubmitted;
            }
        }

        private void LateUpdate()
        {
            if (_won || GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused)
            {
                ClearHover();
                return;
            }

            if (codePopup != null && codePopup.IsVisible)
            {
                ClearHover();
                return;
            }

            if (examinePanel != null && examinePanel.IsVisible)
            {
                ClearHover();
                return;
            }

            UpdateHover();
        }

        private void UpdateHover()
        {
            if (raycastCamera == null || Mouse.current == null)
            {
                ClearHover();
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                ClearHover();
                return;
            }

            var ray = raycastCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            InteractableView next = RaycastBestInteractable(ray);

            if (_hoveredView == next)
            {
                UpdateHoverTooltipPosition();
                return;
            }

            ClearHover();
            _hoveredView = next;
            if (_hoveredView != null)
            {
                _hoveredView.GetComponent<EscapeRoomInteractableHighlight>()?.SetHighlighted(true);
                ShowHoverTooltip();
            }
        }

        private void ClearHover()
        {
            if (_hoveredView != null)
            {
                _hoveredView.GetComponent<EscapeRoomInteractableHighlight>()?.SetHighlighted(false);
                _hoveredView = null;
            }

            if (_hoverTooltipRoot != null)
            {
                _hoverTooltipRoot.gameObject.SetActive(false);
            }
        }

        private void ShowHoverTooltip()
        {
            if (_hoveredView == null || _hoverTooltipRoot == null || _hoverTooltipText == null)
            {
                return;
            }

            _hoverTooltipText.text = _hoveredView.DisplayLabel;
            _hoverTooltipRoot.gameObject.SetActive(true);
            _hoverTooltipRoot.SetAsLastSibling();
            UpdateHoverTooltipPosition();
        }

        private void UpdateHoverTooltipPosition()
        {
            if (_hoveredView == null || _hoverTooltipRoot == null || raycastCamera == null)
            {
                return;
            }

            var renderer = _hoveredView.GetComponent<Renderer>();
            var worldPosition = renderer != null
                ? renderer.bounds.center + Vector3.up * (renderer.bounds.extents.y + 0.25f)
                : _hoveredView.transform.position + Vector3.up;
            var screenPosition = raycastCamera.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f)
            {
                _hoverTooltipRoot.gameObject.SetActive(false);
                return;
            }

            const float halfWidth = 100f;
            const float marginY = 28f;
            screenPosition.x = Mathf.Clamp(screenPosition.x, halfWidth, Screen.width - halfWidth);
            screenPosition.y = Mathf.Clamp(screenPosition.y + 18f, marginY, Screen.height - marginY);
            _hoverTooltipRoot.position = screenPosition;
        }

        private void BuildWorldIfMissing()
        {
            if (FindFirstObjectByType<InteractableView>() != null)
            {
                return;
            }

            var roomRoot = new GameObject("Room");
            var floorColor = new Color(0.35f, 0.12f, 0.12f, 1f);
            var wallColor = new Color(0.28f, 0.1f, 0.1f, 1f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Floor";
            ground.transform.SetParent(roomRoot.transform, false);
            ground.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            ground.GetComponent<Renderer>().material.color = floorColor;
            Destroy(ground.GetComponent<Collider>());

            CreateWall(roomRoot.transform, "WallNorth", new Vector3(0f, 1.5f, 6f), new Vector3(12f, 3f, 0.3f), wallColor);
            CreateWall(roomRoot.transform, "WallSouth", new Vector3(0f, 1.5f, -6f), new Vector3(12f, 3f, 0.3f), wallColor);
            CreateWall(roomRoot.transform, "WallEast", new Vector3(6f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f), wallColor);
            CreateWall(roomRoot.transform, "WallWest", new Vector3(-6f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f), wallColor);

            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(roomRoot.transform, false);
            desk.transform.position = new Vector3(-3.5f, 0.45f, 3.2f);
            desk.transform.localScale = new Vector3(1.8f, 0.9f, 1.1f);
            desk.GetComponent<Renderer>().material.color = new Color(0.32f, 0.22f, 0.14f, 1f);
            Destroy(desk.GetComponent<Collider>());

            var spawns = setup != null && setup.Spawns != null && setup.Spawns.Length > 0
                ? setup.Spawns
                : RoomObjectFactory.CreateDefaultSpawns();

            foreach (var spawn in spawns)
            {
                RoomObjectFactory.Spawn(roomRoot.transform, spawn);
            }

            if (raycastCamera == null)
            {
                raycastCamera = Camera.main;
            }

            if (raycastCamera != null)
            {
                raycastCamera.clearFlags = CameraClearFlags.SolidColor;
                raycastCamera.backgroundColor = new Color(0.08f, 0.07f, 0.09f, 1f);
                raycastCamera.fieldOfView = 50f;
                raycastCamera.transform.position = new Vector3(0f, 6.8f, -5.8f);
                raycastCamera.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            }
        }

        private static void StripLegacyWorldLabels()
        {
            foreach (var view in FindObjectsByType<InteractableView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                for (var i = view.transform.childCount - 1; i >= 0; i--)
                {
                    var child = view.transform.GetChild(i);
                    if (child != null && child.name == "Label")
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = color;
            Destroy(wall.GetComponent<Collider>());
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.OnWinRequested -= HandleWin;
            }

            if (codePopup != null)
            {
                codePopup.OnCodeSubmitted -= HandleCodeSubmitted;
            }
        }

        private void Update()
        {
            if (_won || GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused)
            {
                return;
            }

            if (codePopup != null && codePopup.IsVisible)
            {
                return;
            }

            if (examinePanel != null && examinePanel.IsVisible)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    examinePanel.Hide();
                }

                return;
            }

            inventoryUI?.HandleHotkeys();

            if (Keyboard.current != null &&
                (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame))
            {
                if (Keyboard.current.iKey.wasPressedThisFrame)
                {
                    RunInspectAllVisitor();
                }
                else
                {
                    RunInventoryReportVisitor();
                }

                return;
            }

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            HandleClick();
        }

        private void HandleClick()
        {
            if (_controller == null || raycastCamera == null || Mouse.current == null)
            {
                return;
            }

            var ray = raycastCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            var view = RaycastBestInteractable(ray);
            if (view == null)
            {
                return;
            }

            var selected = inventoryUI != null ? inventoryUI.SelectedItemId : null;
            if (!string.IsNullOrEmpty(selected))
            {
                var useVisitor = new UseOnTargetVisitor(_controller, selected);
                view.Accept(useVisitor);
                var usage = useVisitor.Result;
                if (usage.Success)
                {
                    messageLog?.AddMessage(usage.Message);
                    if (_controller.Inventory.WasConsumed(selected))
                    {
                        messageLog?.AddMessage($"{_controller.GetDisplayName(selected)} used.");
                    }

                    inventoryUI?.ClearSelection();
                    inventoryUI?.Refresh();
                    if (usage.Message.IndexOf("note", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        examinePanel?.Show(usage.Message);
                    }

                    if (_controller.CheckWin())
                    {
                        HandleWin();
                    }

                    return;
                }

                // Wrong target: interact with the object instead.
                if (!string.Equals(usage.Message, "Nothing happens.", System.StringComparison.Ordinal))
                {
                    messageLog?.AddMessage(usage.Message);
                    inventoryUI?.ClearSelection();
                    return;
                }

                inventoryUI?.ClearSelection();
            }

            if (view is SafeInteractable safe)
            {
                safe.OnCodeInputRequested -= HandleSafeCodeRequest;
                safe.OnCodeInputRequested += HandleSafeCodeRequest;
            }

            var result = _controller.TryInteract(view);
            messageLog?.AddMessage(result.Message);

            if (result.Success)
            {
                // Examine popup only for notes and pickups.
                if (view is NoteInteractable or PickupInteractable)
                {
                    examinePanel?.Show(result.Message);
                }
            }

            inventoryUI?.Refresh();
        }

        private void RunInventoryReportVisitor()
        {
            if (_controller == null)
            {
                return;
            }

            var report = new InventoryReportVisitor(_controller);
            AcceptAllRoomItems(report);
            messageLog?.AddMessage(report.Result);
            examinePanel?.Show(report.Result);
        }

        private void RunInspectAllVisitor()
        {
            if (_controller == null)
            {
                return;
            }

            var inspect = new InspectVisitor(_controller);
            AcceptAllRoomItems(inspect);
            messageLog?.AddMessage(inspect.Result);
            examinePanel?.Show(inspect.Result);
        }

        private static void AcceptAllRoomItems(IRoomItemVisitor visitor)
        {
            var views = FindObjectsByType<InteractableView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var view in views)
            {
                if (view != null)
                {
                    view.Accept(visitor);
                }
            }
        }

        private InteractableView RaycastBestInteractable(Ray ray)
        {
            var hits = Physics.RaycastAll(ray, 100f, interactableMask);
            if (hits == null || hits.Length == 0)
            {
                return null;
            }

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            InteractableView best = null;
            var bestScore = int.MinValue;
            foreach (var hit in hits)
            {
                var view = hit.collider.GetComponentInParent<InteractableView>();
                if (view == null || !view.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var score = view switch
                {
                    NoteInteractable => 100,
                    PickupInteractable => 90,
                    SafeInteractable => 80,
                    DoorInteractable => 70,
                    DecoyInteractable => 60,
                    DrawerInteractable => 40,
                    _ => 10
                };

                // Prefer nearer hits with a small score bias.
                score -= Mathf.RoundToInt(hit.distance);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = view;
                }
            }

            return best;
        }

        private void HandleSafeCodeRequest()
        {
            codePopup?.Show("Enter safe code");
        }

        private void HandleCodeSubmitted(string code)
        {
            if (_controller == null)
            {
                return;
            }

            var result = _controller.TryOpenSafe(code);
            messageLog?.AddMessage(result.Message);
            if (result.Success)
            {
                examinePanel?.Show(result.Message);
                inventoryUI?.Refresh();
            }
        }

        private void HandleWin()
        {
            if (_won)
            {
                return;
            }

            _won = true;
            messageLog?.AddMessage("You escaped the room!");
            examinePanel?.Show("You escaped the room!");
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.ShowGameOver("You escaped the room!");
            }
        }

        private void BindInteractables()
        {
            foreach (var view in FindObjectsByType<InteractableView>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                view.Bind(_controller);
                if (view is SafeInteractable safe)
                {
                    safe.OnCodeInputRequested -= HandleSafeCodeRequest;
                    safe.OnCodeInputRequested += HandleSafeCodeRequest;
                }
            }
        }

        private void FindSceneUi()
        {
            if (inventoryUI == null)
            {
                inventoryUI = FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
            }

            if (messageLog == null)
            {
                messageLog = FindFirstObjectByType<MessageLogUI>(FindObjectsInactive.Include);
            }

            if (codePopup == null)
            {
                codePopup = FindFirstObjectByType<CodeInputPopup>(FindObjectsInactive.Include);
            }

            if (examinePanel == null)
            {
                examinePanel = FindFirstObjectByType<ExaminePanel>(FindObjectsInactive.Include);
            }
        }

        private void EnsureHoverTooltip()
        {
            if (_hoverTooltipRoot != null && _hoverTooltipText != null)
            {
                return;
            }

            var canvas = FindEscapeRoomCanvas();
            if (canvas == null)
            {
                return;
            }

            var existing = canvas.transform.Find("HoverTooltip");
            if (existing != null)
            {
                _hoverTooltipRoot = existing as RectTransform;
                _hoverTooltipText = existing.GetComponentInChildren<TMP_Text>(true);
                if (_hoverTooltipRoot != null)
                {
                    _hoverTooltipRoot.gameObject.SetActive(false);
                }

                return;
            }

            CreateHoverTooltip(canvas.transform);
        }

        private static Canvas FindEscapeRoomCanvas()
        {
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas != null && canvas.name == "EscapeRoomCanvas")
                {
                    return canvas;
                }
            }

            return null;
        }

        private void CreateHoverTooltip(Transform canvasTransform)
        {
            var tooltipObject = new GameObject("HoverTooltip", typeof(RectTransform), typeof(Image));
            tooltipObject.transform.SetParent(canvasTransform, false);
            _hoverTooltipRoot = tooltipObject.GetComponent<RectTransform>();
            _hoverTooltipRoot.anchorMin = new Vector2(0.5f, 0.5f);
            _hoverTooltipRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _hoverTooltipRoot.pivot = new Vector2(0.5f, 0f);
            _hoverTooltipRoot.sizeDelta = new Vector2(220f, 42f);
            var tooltipBackground = tooltipObject.GetComponent<Image>();
            tooltipBackground.color = new Color(0.035f, 0.045f, 0.075f, 0.96f);
            tooltipBackground.raycastTarget = false;

            var tooltipTextObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            tooltipTextObject.transform.SetParent(tooltipObject.transform, false);
            _hoverTooltipText = tooltipTextObject.GetComponent<TextMeshProUGUI>();
            _hoverTooltipText.fontSize = 18f;
            _hoverTooltipText.fontStyle = FontStyles.Bold;
            _hoverTooltipText.alignment = TextAlignmentOptions.Center;
            _hoverTooltipText.color = Color.white;
            _hoverTooltipText.textWrappingMode = TextWrappingModes.NoWrap;
            _hoverTooltipText.overflowMode = TextOverflowModes.Truncate;
            _hoverTooltipText.raycastTarget = false;
            var tooltipTextRect = _hoverTooltipText.rectTransform;
            tooltipTextRect.anchorMin = Vector2.zero;
            tooltipTextRect.anchorMax = Vector2.one;
            tooltipTextRect.offsetMin = new Vector2(10f, 4f);
            tooltipTextRect.offsetMax = new Vector2(-10f, -4f);
            tooltipObject.SetActive(false);
        }

        private void BuildUiIfMissing()
        {
            if (HasEscapeRoomCanvas())
            {
                return;
            }

            var canvasObject = new GameObject("EscapeRoomCanvas", typeof(RectTransform));
            var canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            canvasRect.localScale = Vector3.one;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<UI.AdaptiveCanvasGuard>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EscapeRoomEventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Top title (leave top-right free for Pause)
            var title = CreateHudText(
                canvasObject.transform,
                "Title",
                "Crimson Mini Room",
                22,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -14f),
                new Vector2(480f, 34f));
            title.alignment = TextAlignmentOptions.Center;

            hintText = CreateHudText(
                canvasObject.transform,
                "Hint",
                setup != null ? setup.HintText : "Click objects. Keys 1-6 select inventory slots.",
                14,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -48f),
                new Vector2(560f, 44f));
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.color = new Color(0.85f, 0.88f, 0.95f, 1f);

            // Message log — bottom left
            var logPanel = CreatePanel(
                canvasObject.transform,
                "MessageLog",
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(16f, 16f),
                new Vector2(340f, 210f));
            var logBody = CreateHudText(
                logPanel.transform,
                "LogText",
                string.Empty,
                14,
                new Vector2(0f, 1f),
                new Vector2(12f, -12f),
                new Vector2(316f, 186f));
            messageLog = logPanel.AddComponent<MessageLogUI>();
            messageLog.Bind(logBody);

            // Inventory hotbar — bottom center
            var invPanel = CreatePanel(
                canvasObject.transform,
                "Inventory",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 18f),
                new Vector2(640f, 132f));

            var selected = CreateHudText(
                invPanel.transform,
                "Selected",
                "Inventory — 1–6 or click, then click a target",
                13,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -8f),
                new Vector2(600f, 22f));
            selected.alignment = TextAlignmentOptions.Center;

            var slotRootGo = new GameObject("Slots", typeof(RectTransform));
            slotRootGo.transform.SetParent(invPanel.transform, false);
            var slotRect = slotRootGo.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0f);
            slotRect.anchorMax = new Vector2(0.5f, 0f);
            slotRect.pivot = new Vector2(0.5f, 0f);
            slotRect.anchoredPosition = new Vector2(0f, 28f);
            slotRect.sizeDelta = new Vector2(600f, 56f);

            var hlg = slotRootGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.padding = new RectOffset(8, 8, 2, 2);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var used = CreateHudText(
                invPanel.transform,
                "Used",
                "Used: —",
                12,
                new Vector2(0.5f, 0f),
                new Vector2(0f, 6f),
                new Vector2(600f, 20f));
            used.alignment = TextAlignmentOptions.Center;
            used.color = new Color(0.72f, 0.76f, 0.84f, 1f);

            inventoryUI = invPanel.AddComponent<InventoryUI>();
            inventoryUI.Configure(slotRootGo.transform, selected, used);

            CreateHoverTooltip(canvasObject.transform);

            // Examine panel
            var examineRoot = CreatePanel(
                canvasObject.transform,
                "ExaminePanel",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(440f, 170f));
            var examineBody = CreateHudText(
                examineRoot.transform,
                "Body",
                string.Empty,
                17,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 18f),
                new Vector2(400f, 80f));
            examineBody.alignment = TextAlignmentOptions.Center;
            var examineClose = CreateUiButton(examineRoot.transform, "Close", "OK", new Vector2(0f, -52f));
            examinePanel = examineRoot.AddComponent<ExaminePanel>();
            examinePanel.Bind(examineRoot, examineBody, examineClose);

            // Code popup
            var codeRoot = CreatePanel(
                canvasObject.transform,
                "CodePopup",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(380f, 220f));
            var codeTitle = CreateHudText(
                codeRoot.transform,
                "Title",
                "Enter safe code",
                20,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -18f),
                new Vector2(340f, 32f));
            codeTitle.alignment = TextAlignmentOptions.Center;

            var inputGo = new GameObject("Input", typeof(RectTransform), typeof(Image));
            inputGo.transform.SetParent(codeRoot.transform, false);
            var inputImage = inputGo.GetComponent<Image>();
            inputImage.color = new Color(0.1f, 0.12f, 0.16f, 1f);
            var inputRect = inputGo.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.sizeDelta = new Vector2(240f, 42f);
            inputRect.anchoredPosition = new Vector2(0f, 16f);

            var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(inputGo.transform, false);
            var textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10f, 6f);
            textAreaRect.offsetMax = new Vector2(-10f, -6f);

            var inputTextGo = new GameObject("Text", typeof(RectTransform));
            inputTextGo.transform.SetParent(textArea.transform, false);
            var inputTmp = inputTextGo.AddComponent<TextMeshProUGUI>();
            inputTmp.fontSize = 22;
            inputTmp.color = Color.white;
            inputTmp.alignment = TextAlignmentOptions.Center;
            var inputTextRect = inputTextGo.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;

            var input = inputGo.AddComponent<TMP_InputField>();
            input.textViewport = textAreaRect;
            input.textComponent = inputTmp;
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.characterLimit = 4;
            input.caretColor = Color.white;
            input.selectionColor = new Color(0.25f, 0.5f, 0.9f, 0.45f);

            var ok = CreateUiButton(codeRoot.transform, "OK", "OK", new Vector2(-70f, -70f));
            var cancel = CreateUiButton(codeRoot.transform, "Cancel", "Cancel", new Vector2(70f, -70f));
            codePopup = codeRoot.AddComponent<CodeInputPopup>();
            codePopup.Bind(codeRoot, input, ok, cancel, codeTitle);
        }

        private static bool HasEscapeRoomCanvas()
        {
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas != null && canvas.name == "EscapeRoomCanvas")
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject CreatePanel(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.04f, 0.05f, 0.08f, 0.92f);
            // Decorative panels should not steal world clicks; interactive panels keep raycasts.
            image.raycastTarget = name is "Inventory" or "ExaminePanel" or "CodePopup";
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return go;
        }

        private static TMP_Text CreateHudText(
            Transform parent,
            string name,
            string text,
            int fontSize,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = new Color(0.94f, 0.96f, 1f, 1f);
            tmp.outlineWidth = 0.18f;
            tmp.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(
                Mathf.Approximately(anchor.x, 0f) ? 0f : Mathf.Approximately(anchor.x, 1f) ? 1f : 0.5f,
                Mathf.Approximately(anchor.y, 0f) ? 0f : Mathf.Approximately(anchor.y, 1f) ? 1f : 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return tmp;
        }

        private static Button CreateUiButton(Transform parent, string name, string label, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.45f, 0.85f, 1f);
            var button = go.GetComponent<Button>();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(110f, 38f);
            rect.anchoredPosition = position;

            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            field?.SetValue(target, value);
        }
    }
}
