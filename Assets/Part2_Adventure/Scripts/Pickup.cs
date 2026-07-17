using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Collectible pickup that notifies observers via event channel.
    /// Does not know about score UI or ScoreService.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private int pickupId;
        [SerializeField] private PickupEventChannel eventChannel;
        [SerializeField] private int pointsAwarded = 10;

        public int PickupId => pickupId;

        public void Configure(int id, PickupEventChannel channel, int points = 10)
        {
            pickupId = id;
            eventChannel = channel;
            pointsAwarded = points;
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
