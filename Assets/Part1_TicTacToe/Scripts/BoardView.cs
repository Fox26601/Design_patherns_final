using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Part1_TicTacToe
{
    /// <summary>
    /// UI-only board representation.
    /// Pattern: MVP View (https://www.unitydesignpatterns.com/patterns/mvp)
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private Button[] cellButtons;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private Button restartButton;

        public event Action<int> OnCellClicked;
        public event Action OnUndoClicked;
        public event Action OnRedoClicked;
        public event Action OnRestartClicked;

        private void Awake()
        {
            for (var i = 0; i < cellButtons.Length; i++)
            {
                var index = i;
                cellButtons[i].onClick.AddListener(() => OnCellClicked?.Invoke(index));
            }

            undoButton.onClick.AddListener(() => OnUndoClicked?.Invoke());
            redoButton.onClick.AddListener(() => OnRedoClicked?.Invoke());
            restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
        }

        private void Start()
        {
            StartCoroutine(ApplyResponsiveLayoutWhenReady());
        }

        private IEnumerator ApplyResponsiveLayoutWhenReady()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            ApplyResponsiveLayout();
        }

        private void ApplyResponsiveLayout()
        {
            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;

            var canvasRect = root.parent as RectTransform;
            var canvasHeight = canvasRect != null ? canvasRect.rect.height : 720f;
            var canvasWidth = canvasRect != null ? canvasRect.rect.width : 1280f;
            root.sizeDelta = new Vector2(
                Mathf.Min(500f, canvasWidth * 0.9f),
                Mathf.Min(560f, canvasHeight * 0.86f));

            var vertical = GetComponent<VerticalLayoutGroup>();
            if (vertical != null)
            {
                vertical.padding = new RectOffset(20, 20, 20, 20);
                vertical.spacing = 10f;
                vertical.childAlignment = TextAnchor.MiddleCenter;
                vertical.childForceExpandWidth = true;
                vertical.childForceExpandHeight = false;
            }

            ConfigureHeaderLayout(statusText, 34f);
            ConfigureHeaderLayout(scoreText, 30f);
            ConfigureActionsLayout();

            var gridTransform = transform.Find("Grid");
            if (gridTransform == null)
            {
                return;
            }

            var gridLayout = gridTransform.GetComponent<GridLayoutGroup>();
            var gridRect = gridTransform as RectTransform;
            if (gridLayout == null || gridRect == null)
            {
                return;
            }

            const float gridSpacing = 8f;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.spacing = new Vector2(gridSpacing, gridSpacing);

            LayoutRebuilder.ForceRebuildLayoutImmediate(root);

            var innerWidth = root.rect.width - (vertical != null ? vertical.padding.horizontal : 40f);
            var innerHeight = root.rect.height - (vertical != null ? vertical.padding.vertical : 40f);
            var fixedHeight = 34f + 30f + 44f + 30f;
            var gridHeight = Mathf.Max(168f, innerHeight - fixedHeight);
            var gridWidth = innerWidth;

            var cellByWidth = (gridWidth - gridSpacing * 2f) / 3f;
            var cellByHeight = (gridHeight - gridSpacing * 2f) / 3f;
            var cellSize = Mathf.Floor(Mathf.Min(cellByWidth, cellByHeight, 96f));
            cellSize = Mathf.Max(cellSize, 52f);

            gridLayout.cellSize = new Vector2(cellSize, cellSize);

            var gridHeightExact = cellSize * 3f + gridSpacing * 2f;
            var gridLayoutElement = gridTransform.GetComponent<LayoutElement>();
            if (gridLayoutElement != null)
            {
                gridLayoutElement.minHeight = gridHeightExact;
                gridLayoutElement.preferredHeight = gridHeightExact;
                gridLayoutElement.flexibleHeight = 0f;
            }

            if (statusText != null)
            {
                statusText.fontSize = 22f;
            }

            if (scoreText != null)
            {
                scoreText.fontSize = 18f;
            }

            foreach (var button in cellButtons)
            {
                if (button == null)
                {
                    continue;
                }

                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.fontSize = Mathf.RoundToInt(cellSize * 0.52f);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        }

        private static void ConfigureHeaderLayout(TMP_Text text, float height)
        {
            if (text == null)
            {
                return;
            }

            var layoutElement = text.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = text.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleHeight = 0f;
        }

        private void ConfigureActionsLayout()
        {
            var actions = transform.Find("Actions");
            if (actions == null)
            {
                ConfigureButtonLayout(undoButton, 44f);
                ConfigureButtonLayout(redoButton, 44f);
                ConfigureButtonLayout(restartButton, 44f);
                return;
            }

            var actionsLayout = actions.GetComponent<HorizontalLayoutGroup>();
            if (actionsLayout != null)
            {
                actionsLayout.spacing = 12f;
                actionsLayout.childForceExpandWidth = true;
                actionsLayout.childForceExpandHeight = true;
            }

            var actionsElement = actions.GetComponent<LayoutElement>();
            if (actionsElement == null)
            {
                actionsElement = actions.gameObject.AddComponent<LayoutElement>();
            }

            actionsElement.minHeight = 44f;
            actionsElement.preferredHeight = 44f;
            actionsElement.flexibleHeight = 0f;

            foreach (Transform child in actions)
            {
                var button = child.GetComponent<Button>();
                if (button != null)
                {
                    ConfigureButtonLayout(button, 44f);
                }
            }
        }

        private static void ConfigureButtonLayout(Button button, float height)
        {
            if (button == null)
            {
                return;
            }

            var layoutElement = button.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = button.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 1f;
        }

        public void RenderCell(int index, PlayerMark mark)
        {
            var label = cellButtons[index].GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = mark switch
                {
                    PlayerMark.X => "X",
                    PlayerMark.O => "O",
                    _ => string.Empty
                };
            }

            cellButtons[index].interactable = mark == PlayerMark.None;
        }

        public void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void SetScore(int xScore, int oScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"X: {xScore}   O: {oScore}";
            }
        }

        public void SetUndoAvailable(bool available)
        {
            undoButton.interactable = available;
        }

        public void SetRedoAvailable(bool available)
        {
            redoButton.interactable = available;
        }

        public void ResetBoard()
        {
            for (var i = 0; i < cellButtons.Length; i++)
            {
                RenderCell(i, PlayerMark.None);
            }
        }
    }
}
