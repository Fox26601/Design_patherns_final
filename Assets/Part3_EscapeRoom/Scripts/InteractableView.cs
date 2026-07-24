using UnityEngine;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// World object view that reacts to RoomState changes (Observer).
    /// </summary>
    public class InteractableView : MonoBehaviour, IInteractable, IRoomItem
    {
        [SerializeField] private string interactableId;
        [SerializeField] private string examineText = "Nothing special.";
        [SerializeField] private Color closedColor = new(0.45f, 0.35f, 0.3f, 1f);
        [SerializeField] private Color openColor = new(0.55f, 0.5f, 0.35f, 1f);
        [SerializeField] private Color lockedColor = new(0.35f, 0.35f, 0.4f, 1f);
        [SerializeField] private Color unlockedColor = new(0.25f, 0.65f, 0.35f, 1f);
        [SerializeField] private bool hideWhenLocked;
        [SerializeField] private bool hideUntilRevealed;

        private Renderer _renderer;
        private EscapeRoomController _controller;
        private Color _defaultColor;
        private string _displayLabel;

        public string InteractableId => interactableId;
        public string ExamineText => examineText;
        public string DisplayLabel => string.IsNullOrEmpty(_displayLabel) ? gameObject.name : _displayLabel;

        public void Configure(string id, string examine, EscapeRoomController controller, bool hideUntilRevealed = false)
        {
            interactableId = id;
            examineText = examine;
            this.hideUntilRevealed = hideUntilRevealed;
            if (hideUntilRevealed && controller == null)
            {
                gameObject.SetActive(false);
            }

            Bind(controller);
        }

        public void SetDisplayLabel(string label)
        {
            _displayLabel = label;
        }

        public void Bind(EscapeRoomController controller)
        {
            if (_controller != null)
            {
                _controller.RoomState.OnStateChanged -= HandleStateChanged;
            }

            _controller = controller;
            if (_controller != null)
            {
                _controller.RoomState.OnStateChanged += HandleStateChanged;
                ApplyVisual(_controller.RoomState.GetState(interactableId));
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _defaultColor = _renderer.material.color;
            }
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.RoomState.OnStateChanged -= HandleStateChanged;
            }
        }

        public virtual bool CanInteract(InteractionContext context) => true;

        public virtual InteractionResult Interact(InteractionContext context)
        {
            return new InteractionResult(true, examineText);
        }

        public virtual void Accept(IRoomItemVisitor visitor)
        {
            // Concrete subclasses override with typed Visit* calls.
        }

        private void HandleStateChanged(string objectId, RoomObjectState newState)
        {
            if (objectId != interactableId)
            {
                // Note visibility depends on drawer open.
                if (interactableId == "note" && objectId == "drawer")
                {
                    ApplyVisual(_controller.RoomState.GetState(interactableId));
                }

                return;
            }

            ApplyVisual(newState);
        }

        protected virtual void ApplyVisual(RoomObjectState state)
        {
            if (hideUntilRevealed)
            {
                var visible = state == RoomObjectState.Revealed ||
                              state == RoomObjectState.Open ||
                              state == RoomObjectState.Solved;
                // Also show note when drawer opens even if note state not yet Revealed.
                if (!visible && interactableId == "note" && _controller != null &&
                    _controller.RoomState.GetState("drawer") == RoomObjectState.Open)
                {
                    visible = true;
                }

                gameObject.SetActive(visible);
                if (!visible)
                {
                    return;
                }
            }

            if (hideWhenLocked && state == RoomObjectState.Locked)
            {
                gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf && !hideUntilRevealed)
            {
                gameObject.SetActive(true);
            }

            if (_renderer == null)
            {
                return;
            }

            _renderer.material.color = state switch
            {
                RoomObjectState.Open => openColor,
                RoomObjectState.Unlocked => unlockedColor,
                RoomObjectState.Locked => lockedColor,
                RoomObjectState.Closed => closedColor,
                RoomObjectState.Revealed => openColor,
                _ => _defaultColor
            };

            GetComponent<EscapeRoomInteractableHighlight>()?.RefreshBaseColor();
        }
    }
}
