using System;
using System.Collections.Generic;

namespace Part3_EscapeRoom
{
    public class RoomStateController
    {
        private readonly Dictionary<string, RoomObjectState> _states = new();

        public event Action<string, RoomObjectState> OnStateChanged;

        public void SetState(string objectId, RoomObjectState newState)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                return;
            }

            _states[objectId] = newState;
            OnStateChanged?.Invoke(objectId, newState);
        }

        public RoomObjectState GetState(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                return RoomObjectState.Closed;
            }

            return _states.TryGetValue(objectId, out var state) ? state : RoomObjectState.Closed;
        }
    }
}
