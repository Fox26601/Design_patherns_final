using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Ensures CanvasScaler produces a readable scale even if the scene was saved with scale 0
    /// (common after headless/batchmode scene generation).
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class AdaptiveCanvasGuard : MonoBehaviour
    {
        [SerializeField] private Vector2 referenceResolution = new(1280f, 720f);
        [SerializeField] private float matchWidthOrHeight = 0.5f;
        [SerializeField] private float minimumScale = 0.75f;

        private CanvasScaler _scaler;
        private RectTransform _rect;

        private void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void LateUpdate()
        {
            if (_rect != null && _rect.localScale.x < minimumScale * 0.5f)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if (_scaler == null)
            {
                _scaler = GetComponent<CanvasScaler>();
            }

            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }

            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = referenceResolution;
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _scaler.matchWidthOrHeight = matchWidthOrHeight;

            // Never keep a zero scale baked by batchmode saves.
            if (_rect.localScale.sqrMagnitude < 0.01f)
            {
                _rect.localScale = Vector3.one;
            }

            Canvas.ForceUpdateCanvases();
        }
    }
}
