using UnityEngine;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Simple hover feedback for point-and-click targets.
    /// </summary>
    public class EscapeRoomInteractableHighlight : MonoBehaviour
    {
        private Renderer _renderer;
        private Color _baseColor;
        private bool _highlighted;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _baseColor = _renderer.material.color;
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (_renderer == null || _highlighted == highlighted)
            {
                return;
            }

            _highlighted = highlighted;
            _renderer.material.color = highlighted
                ? Color.Lerp(_baseColor, Color.white, 0.35f)
                : _baseColor;
        }

        public void RefreshBaseColor()
        {
            if (_renderer != null)
            {
                _baseColor = _renderer.material.color;
                if (_highlighted)
                {
                    _renderer.material.color = Color.Lerp(_baseColor, Color.white, 0.35f);
                }
            }
        }
    }
}
