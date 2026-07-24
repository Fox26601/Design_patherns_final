using Core;
using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Enemy that moves toward the player and triggers game over on contact.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private Transform target;
        [SerializeField] private float catchDistance = 1.1f;

        private Rigidbody _rigidbody;
        private bool _caught;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;

            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        public void Configure(Transform player, float speed)
        {
            target = player;
            moveSpeed = speed;
        }

        private void Update()
        {
            if (_caught || target == null)
            {
                return;
            }

            var direction = (target.position - transform.position);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            direction.Normalize();
            transform.position += direction * (moveSpeed * Time.deltaTime);

            var flatDistance = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(target.position.x, 0f, target.position.z));

            if (flatDistance <= catchDistance)
            {
                TriggerCatch();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                TriggerCatch();
            }
        }

        private void TriggerCatch()
        {
            if (_caught)
            {
                return;
            }

            _caught = true;
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.ShowGameOver("Caught by enemy!");
            }
        }
    }
}
