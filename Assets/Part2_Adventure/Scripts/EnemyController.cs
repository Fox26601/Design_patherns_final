using Core;
using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Enemy that moves in a straight line toward the player.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private Transform target;

        public void Configure(Transform player, float speed)
        {
            target = player;
            moveSpeed = speed;
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            var direction = (target.position - transform.position).normalized;
            direction.y = 0f;
            transform.position += direction * (moveSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                if (GameFlowManager.Instance.IsPaused)
                {
                    return;
                }

                GameFlowManager.Instance.SetPaused(true);
                ScreenManager.Instance.ShowGameOver("Defeat! An enemy caught you.");
            }
        }
    }
}
