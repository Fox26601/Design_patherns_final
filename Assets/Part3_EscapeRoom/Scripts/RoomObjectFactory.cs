using UnityEngine;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Creates interactables from RoomObjectSpawn data.
    /// </summary>
    public static class RoomObjectFactory
    {
        public static InteractableView Spawn(Transform parent, RoomObjectSpawn spawn)
        {
            if (spawn == null || string.IsNullOrEmpty(spawn.ObjectId))
            {
                return null;
            }

            var primitive = spawn.Primitive;
            if (spawn.Kind == RoomObjectKind.Pickup && primitive == PrimitiveType.Cube)
            {
                primitive = PrimitiveType.Cylinder;
            }

            var go = GameObject.CreatePrimitive(primitive);
            go.name = string.IsNullOrEmpty(spawn.DisplayLabel) ? spawn.ObjectId : spawn.DisplayLabel.Replace(" ", string.Empty);
            go.transform.SetParent(parent, false);
            go.transform.position = spawn.Position;
            go.transform.localScale = spawn.Scale == Vector3.zero ? Vector3.one : spawn.Scale;
            go.GetComponent<Renderer>().material.color = spawn.Color;
            go.AddComponent<EscapeRoomInteractableHighlight>();

            var examine = string.IsNullOrEmpty(spawn.ExamineText) ? spawn.DisplayLabel : spawn.ExamineText;
            var label = string.IsNullOrEmpty(spawn.DisplayLabel) ? spawn.ObjectId : spawn.DisplayLabel;

            InteractableView view = spawn.Kind switch
            {
                RoomObjectKind.Pickup => CreatePickup(go, spawn, examine, label),
                RoomObjectKind.Drawer => CreateTyped<DrawerInteractable>(go, spawn.ObjectId, examine, label, false),
                RoomObjectKind.Note => CreateTyped<NoteInteractable>(go, spawn.ObjectId, examine, label, spawn.HideUntilRevealed),
                RoomObjectKind.Safe => CreateTyped<SafeInteractable>(go, spawn.ObjectId, examine, label, false),
                RoomObjectKind.Door => CreateTyped<DoorInteractable>(go, spawn.ObjectId, examine, label, false),
                RoomObjectKind.Decoy => CreateTyped<DecoyInteractable>(go, spawn.ObjectId, examine, label, false),
                _ => null
            };

            return view;
        }

        private static InteractableView CreatePickup(GameObject go, RoomObjectSpawn spawn, string examine, string label)
        {
            var itemId = string.IsNullOrEmpty(spawn.ItemId) ? spawn.ObjectId : spawn.ItemId;
            var pickup = go.AddComponent<PickupInteractable>();
            pickup.ConfigurePickup(itemId, label, null);
            pickup.SetDisplayLabel(label);
            return pickup;
        }

        private static InteractableView CreateTyped<T>(
            GameObject go,
            string objectId,
            string examine,
            string label,
            bool hideUntilRevealed)
            where T : InteractableView
        {
            var view = go.AddComponent<T>();
            view.Configure(objectId, examine, null, hideUntilRevealed);
            view.SetDisplayLabel(label);
            return view;
        }

        public static RoomObjectSpawn[] CreateDefaultSpawns()
        {
            return new[]
            {
                new RoomObjectSpawn
                {
                    ObjectId = "rustyKey",
                    DisplayLabel = "Rusty Key",
                    Kind = RoomObjectKind.Pickup,
                    Position = new Vector3(-1.8f, 0.15f, -1.2f),
                    Scale = new Vector3(0.3f, 0.1f, 0.3f),
                    Color = new Color(0.55f, 0.4f, 0.2f, 1f),
                    ItemId = "rustyKey",
                    ExamineText = "A rusty key.",
                    Primitive = PrimitiveType.Cylinder
                },
                new RoomObjectSpawn
                {
                    ObjectId = "drawer",
                    DisplayLabel = "Drawer",
                    Kind = RoomObjectKind.Drawer,
                    Position = new Vector3(-3.5f, 1.05f, 3.2f),
                    Scale = new Vector3(1.2f, 0.35f, 0.85f),
                    Color = new Color(0.4f, 0.28f, 0.18f, 1f),
                    ExamineText = "A wooden drawer."
                },
                new RoomObjectSpawn
                {
                    ObjectId = "note",
                    DisplayLabel = "Note",
                    Kind = RoomObjectKind.Note,
                    Position = new Vector3(-3.5f, 1.45f, 2.95f),
                    Scale = new Vector3(0.55f, 0.25f, 0.55f),
                    Color = new Color(0.92f, 0.9f, 0.75f, 1f),
                    ExamineText = "A crumpled note.",
                    HideUntilRevealed = true
                },
                new RoomObjectSpawn
                {
                    ObjectId = "safe",
                    DisplayLabel = "Safe",
                    Kind = RoomObjectKind.Safe,
                    Position = new Vector3(3.2f, 0.75f, 3.4f),
                    Scale = new Vector3(1.1f, 1.2f, 0.9f),
                    Color = new Color(0.35f, 0.38f, 0.42f, 1f),
                    ExamineText = "A metal safe with a keypad."
                },
                new RoomObjectSpawn
                {
                    ObjectId = "door",
                    DisplayLabel = "Door",
                    Kind = RoomObjectKind.Door,
                    Position = new Vector3(0f, 1.4f, 5.65f),
                    Scale = new Vector3(1.6f, 2.6f, 0.25f),
                    Color = new Color(0.45f, 0.2f, 0.18f, 1f),
                    ExamineText = "The exit door."
                },
                new RoomObjectSpawn
                {
                    ObjectId = "herring",
                    DisplayLabel = "Red Box (decoy)",
                    Kind = RoomObjectKind.Decoy,
                    Position = new Vector3(2.5f, 0.35f, -2.5f),
                    Scale = new Vector3(0.85f, 0.85f, 0.85f),
                    Color = new Color(0.85f, 0.08f, 0.08f, 1f),
                    ExamineText = "This red box does nothing. Keep searching."
                }
            };
        }
    }
}
