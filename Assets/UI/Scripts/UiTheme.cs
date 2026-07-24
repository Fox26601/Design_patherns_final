using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Shared visual tokens for all menus and HUDs.
    /// Layout uses screen-relative anchors; sizes are designed for 1920x1080 reference.
    /// </summary>
    [CreateAssetMenu(fileName = "UiTheme", menuName = "DesignPatterns/UI Theme")]
    public class UiTheme : ScriptableObject
    {
        [Header("Typography")]
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float titleSize = 64f;
        [SerializeField] private float bodySize = 36f;
        [SerializeField] private float buttonLabelSize = 32f;

        [Header("Layout")]
        [SerializeField] private float buttonHeight = 80f;
        [SerializeField] private float panelPadding = 48f;
        [SerializeField] private float spacing = 24f;
        [SerializeField] private Vector2 referenceResolution = new(1280f, 720f);
        [SerializeField] private float matchWidthOrHeight = 0.5f;
        [Tooltip("Card horizontal inset as fraction of screen (0.1 = 10% margin each side)")]
        [SerializeField] private float cardHorizontalMargin = 0.08f;
        [Tooltip("Card vertical inset as fraction of screen")]
        [SerializeField] private float cardVerticalMargin = 0.08f;

        [Header("Colors")]
        [SerializeField] private Color overlayColor = new(0.04f, 0.05f, 0.08f, 0.85f);
        [SerializeField] private Color panelColor = new(0.11f, 0.13f, 0.19f, 0.98f);
        [SerializeField] private Color primaryButtonColor = new(0.22f, 0.5f, 0.95f, 1f);
        [SerializeField] private Color secondaryButtonColor = new(0.28f, 0.32f, 0.42f, 1f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color mutedTextColor = new(0.78f, 0.82f, 0.9f, 1f);
        [SerializeField] private Color dropdownBackgroundColor = new(0.95f, 0.96f, 0.98f, 1f);
        [SerializeField] private Color dropdownTextColor = new(0.08f, 0.1f, 0.14f, 1f);

        public TMP_FontAsset Font => font;
        public float TitleSize => titleSize;
        public float BodySize => bodySize;
        public float ButtonLabelSize => buttonLabelSize;
        public float ButtonHeight => buttonHeight;
        public float PanelPadding => panelPadding;
        public float Spacing => spacing;
        public Vector2 ReferenceResolution => referenceResolution;
        public float MatchWidthOrHeight => matchWidthOrHeight;
        public float CardHorizontalMargin => cardHorizontalMargin;
        public float CardVerticalMargin => cardVerticalMargin;
        public Color OverlayColor => overlayColor;
        public Color PanelColor => panelColor;
        public Color PrimaryButtonColor => primaryButtonColor;
        public Color SecondaryButtonColor => secondaryButtonColor;
        public Color TextColor => textColor;
        public Color MutedTextColor => mutedTextColor;
        public Color DropdownBackgroundColor => dropdownBackgroundColor;
        public Color DropdownTextColor => dropdownTextColor;

        public void ApplyFont(TMP_FontAsset fontAsset)
        {
            font = fontAsset;
        }

        public void ApplyReadableDefaults(TMP_FontAsset fontAsset)
        {
            font = fontAsset;
            titleSize = 72f;
            bodySize = 40f;
            buttonLabelSize = 36f;
            buttonHeight = 96f;
            panelPadding = 56f;
            spacing = 28f;
            // Lower reference = larger on-screen UI in Free Aspect Game View.
            referenceResolution = new Vector2(1280f, 720f);
            matchWidthOrHeight = 0.5f;
            cardHorizontalMargin = 0.08f;
            cardVerticalMargin = 0.08f;
        }
    }
}
