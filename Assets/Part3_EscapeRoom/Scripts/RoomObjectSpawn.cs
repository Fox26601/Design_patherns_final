using System;
using UnityEngine;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Object kinds used when spawning room content from EscapeRoomSetup.
    /// </summary>
    public enum RoomObjectKind
    {
        Pickup = 0,
        Drawer = 1,
        Note = 2,
        Safe = 3,
        Door = 4,
        Decoy = 5
    }

    /// <summary>
    /// Spawn entry for one interactable in the room (position, kind, optional item id).
    /// </summary>
    [Serializable]
    public class RoomObjectSpawn
    {
        public string ObjectId;
        public string DisplayLabel;
        public RoomObjectKind Kind;
        public Vector3 Position;
        public Vector3 Scale = Vector3.one;
        public Color Color = Color.white;
        public string ItemId;
        public string ExamineText;
        public bool HideUntilRevealed;
        public PrimitiveType Primitive = PrimitiveType.Cube;
    }
}
