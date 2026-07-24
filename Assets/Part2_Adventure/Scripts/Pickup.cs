using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Collectible pickup that notifies observers via event channel.
    /// Does not know about score UI or ScoreService.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        public static readonly Color BrightYellow = new(1f, 0.9f, 0.1f, 1f);

        [SerializeField] private int pickupId;
        [SerializeField] private PickupEventChannel eventChannel;
        [SerializeField] private int pointsAwarded = 10;

        public int PickupId => pickupId;

        private void Awake()
        {
            ApplyYellowVisual();
        }

        public void Configure(int id, PickupEventChannel channel, int points = 10)
        {
            pickupId = id;
            eventChannel = channel;
            pointsAwarded = points;
            ApplyYellowVisual();
        }

        private void ApplyYellowVisual()
        {
            var meshRenderer = GetComponent<Renderer>();
            if (meshRenderer == null)
            {
                return;
            }

            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return;
            }

            var material = new Material(shader) { color = BrightYellow };
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", BrightYellow);
            }

            meshRenderer.material = material;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() == null)
            {
                return;
            }

            eventChannel?.Raise(new PickupCollectedData
            {
                PickupId = pickupId,
                PointsAwarded = pointsAwarded,
                WorldPosition = transform.position
            });

            Destroy(gameObject);
        }
    }
}
