using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Keeps the gameplay camera in a readable top-down position.
    /// </summary>
    public class AdventureCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 14f, -8f);
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private bool lookAtTarget = true;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                followSpeed * Time.deltaTime);

            if (lookAtTarget)
            {
                var lookTarget = target.position + Vector3.up * 1.5f;
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookTarget - transform.position),
                    followSpeed * Time.deltaTime);
            }
        }
    }
}
