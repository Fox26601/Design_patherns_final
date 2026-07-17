using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Payload for pickup collection events.
    /// </summary>
    public struct PickupCollectedData
    {
        public int PickupId;
        public int PointsAwarded;
        public Vector3 WorldPosition;
    }
}
