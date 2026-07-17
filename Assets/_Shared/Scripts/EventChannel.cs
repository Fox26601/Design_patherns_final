using System;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Generic ScriptableObject event channel for Observer pattern decoupling.
    /// Pattern: Observer (https://www.unitydesignpatterns.com/patterns/observer)
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        private event Action<T> OnEventRaised;

        public void Raise(T payload)
        {
            OnEventRaised?.Invoke(payload);
        }

        public void Subscribe(Action<T> listener)
        {
            OnEventRaised += listener;
        }

        public void Unsubscribe(Action<T> listener)
        {
            OnEventRaised -= listener;
        }
    }
}
