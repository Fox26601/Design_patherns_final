using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Shared visual settings for menu and HUD screens.
    /// </summary>
    [CreateAssetMenu(fileName = "UiTheme", menuName = "DesignPatterns/UI Theme")]
    public class UiTheme : ScriptableObject
    {
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float titleSize = 32f;
        [SerializeField] private float bodySize = 20f;
        [SerializeField] private float buttonLabelSize = 18f;
        [SerializeField] private float buttonHeight = 40f;
        [SerializeField] private float panelPadding = 24f;
        [SerializeField] private float panelWidth = 560f;
        [SerializeField] private float spacing = 12f;
        [SerializeField] private Vector2 referenceResolution = new(1280f, 720f);
        [SerializeField] private float matchWidthOrHeight = 0.5f;
        [SerializeField] private float cardHorizontalMargin = 0.08f;
        [SerializeField] private float cardVerticalMargin = 0.08f;
        [SerializeField] private Color overlayColor = new(0.05f, 0.06f, 0.09f, 0.72f);
        [SerializeField] private Color panelColor = new(0.12f, 0.14f, 0.2f, 0.96f);
        [SerializeField] private Color primaryButtonColor = new(0.22f, 0.5f, 0.95f, 1f);
        [SerializeField] private Color secondaryButtonColor = new(0.28f, 0.32f, 0.42f, 1f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color mutedTextColor = new(0.75f, 0.78f, 0.85f, 1f);
        [SerializeField] private Color dropdownBackgroundColor = new(0.95f, 0.96f, 0.98f, 1f);
        [SerializeField] private Color dropdownTextColor = new(0.1f, 0.12f, 0.16f, 1f);

        public TMP_FontAsset Font => font;
        public float TitleSize => titleSize;
        public float BodySize => bodySize;
        public float ButtonLabelSize => buttonLabelSize;
        public float ButtonHeight => buttonHeight;
        public float PanelPadding => panelPadding;
        public float PanelWidth => panelWidth;
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
    }
}
