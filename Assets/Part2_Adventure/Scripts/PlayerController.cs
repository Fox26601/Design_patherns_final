using UnityEngine;
using UnityEngine.InputSystem;

namespace Part2_Adventure
{
    /// <summary>
    /// Simple top-down player movement on a plane.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;

        private Rigidbody _rigidbody;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        private void Update()
        {
            _moveInput = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) _moveInput.y += 1f;
                if (Keyboard.current.sKey.isPressed) _moveInput.y -= 1f;
                if (Keyboard.current.aKey.isPressed) _moveInput.x -= 1f;
                if (Keyboard.current.dKey.isPressed) _moveInput.x += 1f;
            }
        }

        private void FixedUpdate()
        {
            var direction = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            _rigidbody.MovePosition(_rigidbody.position + direction * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}
