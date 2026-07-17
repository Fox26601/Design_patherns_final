#if UNITY_EDITOR
using System;
using System.Linq;
using Part3_EscapeRoom;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// Editor checks for escape-room setup: spawns, kinds, inventory, and escape sequence.
    /// </summary>
    public static class EscapeRoomRequirementsVerify
    {
        [MenuItem("DesignPatterns/Verify Escape Room Requirements")]
        public static void VerifyFromMenu()
        {
            if (!Run(out var message))
            {
                throw new Exception(message);
            }

            Debug.Log(message);
        }

        public static void VerifyBatch()
        {
            if (!Run(out var message))
            {
                Debug.LogError(message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log(message);
            EditorApplication.Exit(0);
        }

        private static bool Run(out string message)
        {
            var setup = Resources.Load<EscapeRoomSetupSO>("EscapeRoomSetup");
            if (setup == null)
            {
                message = "FAIL: EscapeRoomSetup missing from Resources.";
                return false;
            }

            if (setup.Spawns == null || setup.Spawns.Length < 6)
            {
                message = $"FAIL: Spawns must have ≥6 entries (got {setup.Spawns?.Length ?? 0}).";
                return false;
            }

            var kinds = setup.Spawns.Select(s => s.Kind).Distinct().ToArray();
            if (kinds.Length < 5)
            {
                message = $"FAIL: Need ≥5 RoomObjectKind values in Spawns (got {kinds.Length}).";
                return false;
            }

            var decoy = setup.Spawns.FirstOrDefault(s => s.Kind == RoomObjectKind.Decoy);
            if (decoy == null ||
                decoy.ExamineText == null ||
                decoy.ExamineText.IndexOf("does nothing", StringComparison.OrdinalIgnoreCase) < 0)
            {
                message = "FAIL: Decoy spawn missing clear 'does nothing' examine text.";
                return false;
            }

            var root = new GameObject("_EscapeRoomVerifyRoot");
            try
            {
                foreach (var spawn in setup.Spawns)
                {
                    RoomObjectFactory.Spawn(root.transform, spawn);
                }

                var views = root.GetComponentsInChildren<InteractableView>(true);
                if (views.Length < 6)
                {
                    message = $"FAIL: Factory spawned {views.Length} interactables, expected ≥6.";
                    return false;
                }

                var controller = new EscapeRoomController();
                controller.Configure(setup);
                foreach (var view in views)
                {
                    view.Bind(controller);
                }

                var rusty = views.OfType<PickupInteractable>().First(v => v.InteractableId == "rustyKey" || v.name.Contains("Rusty"));
                var drawer = views.OfType<DrawerInteractable>().First();
                var note = views.OfType<NoteInteractable>().First();
                var safe = views.OfType<SafeInteractable>().First();
                var door = views.OfType<DoorInteractable>().First();
                var decoyView = views.OfType<DecoyInteractable>().First();

                var decoyResult = controller.TryInteract(decoyView);
                if (!decoyResult.Success ||
                    decoyResult.Message.IndexOf("does nothing", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    message = $"FAIL: Decoy feedback unclear: '{decoyResult.Message}'";
                    return false;
                }

                var pickup = controller.TryInteract(rusty);
                if (!pickup.Success || !controller.Inventory.HasItem("rustyKey"))
                {
                    message = $"FAIL: Could not pick up rusty key: '{pickup.Message}'";
                    return false;
                }

                var useKey = controller.TryUseItemOn("rustyKey", drawer);
                if (!useKey.Success || !controller.Inventory.WasConsumed("rustyKey"))
                {
                    message = $"FAIL: Key on drawer failed: '{useKey.Message}'";
                    return false;
                }

                if (controller.Inventory.HasItem("rustyKey") || controller.Inventory.ConsumedItems.Count == 0)
                {
                    message = "FAIL: Used key still in collected slots / ConsumedItems empty.";
                    return false;
                }

                var readNote = controller.TryInteract(note);
                if (!readNote.Success || !controller.HasReadNote)
                {
                    message = $"FAIL: Note read failed: '{readNote.Message}'";
                    return false;
                }

                var openSafe = controller.TryOpenSafe(setup.SafeCode);
                if (!openSafe.Success || !controller.Inventory.HasItem("goldKey"))
                {
                    message = $"FAIL: Safe open failed: '{openSafe.Message}'";
                    return false;
                }

                var useGold = controller.TryUseItemOn("goldKey", door);
                if (!useGold.Success || !controller.CheckWin())
                {
                    message = $"FAIL: Door unlock / win failed: '{useGold.Message}'";
                    return false;
                }

                if (!controller.Inventory.WasConsumed("goldKey"))
                {
                    message = "FAIL: Gold key was not marked consumed.";
                    return false;
                }

                message =
                    "PASS: Escape Room requirements — " +
                    $"{setup.Spawns.Length} spawns, {kinds.Length} kinds, " +
                    "found/used inventory, decoy copy, full escape sequence.";
                return true;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
