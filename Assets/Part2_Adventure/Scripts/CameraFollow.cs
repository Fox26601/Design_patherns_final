using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Top-down orthographic camera that follows a target.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float height = 22f;
        [SerializeField] private float orthographicSize = 14f;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = orthographicSize;
            }

            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            SnapToTarget();
        }

        private void LateUpdate()
        {
            SnapToTarget();
        }

        private void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            transform.position = new Vector3(target.position.x, height, target.position.z);
        }
    }
}
