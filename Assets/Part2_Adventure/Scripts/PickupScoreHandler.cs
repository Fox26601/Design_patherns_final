using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Observer subscriber that updates score when a pickup is collected.
    /// Pattern: Observer (https://www.unitydesignpatterns.com/patterns/observer)
    /// </summary>
    public class PickupScoreHandler : MonoBehaviour
    {
        [SerializeField] private PickupEventChannel pickupEventChannel;
        [SerializeField] private ScoreService scoreService;

        private void OnEnable()
        {
            if (pickupEventChannel != null)
            {
                pickupEventChannel.Subscribe(OnPickupCollected);
            }
        }

        private void OnDisable()
        {
            if (pickupEventChannel != null)
            {
                pickupEventChannel.Unsubscribe(OnPickupCollected);
            }
        }

        private void OnPickupCollected(PickupCollectedData data)
        {
            if (scoreService == null)
            {
                return;
            }

            scoreService.AddPickupPoints();
        }
    }
}
