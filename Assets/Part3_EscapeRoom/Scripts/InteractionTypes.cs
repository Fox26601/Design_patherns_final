using System;
using System.Collections.Generic;

namespace Part3_EscapeRoom
{
    public readonly struct InteractionResult
    {
        public bool Success { get; }
        public string Message { get; }

        public InteractionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

    public readonly struct UsageResult
    {
        public bool Success { get; }
        public string Message { get; }

        public UsageResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

    public readonly struct InteractionContext
    {
        public Inventory Inventory { get; }
        public RoomStateController RoomState { get; }
        public EscapeRoomController Controller { get; }

        public InteractionContext(
            Inventory inventory,
            RoomStateController roomState,
            EscapeRoomController controller)
        {
            Inventory = inventory;
            RoomState = roomState;
            Controller = controller;
        }
    }
}
