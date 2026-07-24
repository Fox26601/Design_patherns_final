using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Keeps the player inside a square play area.
    /// </summary>
    public class WorldBoundsClamp : MonoBehaviour
    {
        [SerializeField] private float bound = 18f;

        public void Configure(float worldBound)
        {
            bound = worldBound;
        }

        private void LateUpdate()
        {
            var position = transform.position;
            position.x = Mathf.Clamp(position.x, -bound, bound);
            position.z = Mathf.Clamp(position.z, -bound, bound);
            transform.position = position;
        }
    }
}
