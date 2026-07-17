using Shared;
using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Observer channel for pickup collection notifications.
    /// Pattern: Observer (https://www.unitydesignpatterns.com/patterns/observer)
    /// </summary>
    [CreateAssetMenu(fileName = "PickupEventChannel", menuName = "DesignPatterns/Pickup Event Channel")]
    public class PickupEventChannel : EventChannel<PickupCollectedData> { }
}
