using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Keeps a CanvasScaler configured for readable UI across resolutions.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    [DefaultExecutionOrder(-200)]
    public class AdaptiveCanvasGuard : MonoBehaviour
    {
        [SerializeField] private Vector2 referenceResolution = new(1280f, 720f);
        [SerializeField] private float matchWidthOrHeight = 0.5f;

        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            if (size != _lastScreenSize)
            {
                _lastScreenSize = size;
                Apply();
            }
        }

        private void Apply()
        {
            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            var match = matchWidthOrHeight;
            var reference = referenceResolution;

            if (Screen.height > 0 && Screen.height < 700)
            {
                match = 0.75f;
            }

            scaler.referenceResolution = reference;
            scaler.matchWidthOrHeight = Mathf.Clamp01(match);
        }
    }
}
