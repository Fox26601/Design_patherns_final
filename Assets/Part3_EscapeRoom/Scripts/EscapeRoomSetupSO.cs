using System;
using UnityEngine;

namespace Part3_EscapeRoom
{
    [Serializable]
    public class InitialRoomState
    {
        public string ObjectId;
        public RoomObjectState State;
    }

    [CreateAssetMenu(fileName = "EscapeRoomSetup", menuName = "DesignPatterns/EscapeRoom/Room Setup")]
    public class EscapeRoomSetupSO : ScriptableObject
    {
        public string RoomId = "crimson_mini";
        public string SafeCode = "7391";
        public string HintText =
            "Click objects to examine or pick them up.\n" +
            "Select inventory with keys 1–6 (or click), then click a target.\n" +
            "Esc pauses.";

        public ItemDefinition[] Items;
        public InteractionRuleSO[] InteractionRules;
        public PuzzleDefinition SafePuzzle;
        public InitialRoomState[] InitialStates;

        [Tooltip("Objects spawned into the room at runtime.")]
        public RoomObjectSpawn[] Spawns;
    }
}
