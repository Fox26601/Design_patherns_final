using System;
using System.Collections.Generic;
using UnityEngine;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Coordinates inventory, interactions, and room state for the escape room.
    /// </summary>
    public class EscapeRoomController
    {
        private readonly Dictionary<string, bool> _flags = new();
        private readonly Dictionary<string, ItemDefinition> _itemsById = new();
        private PuzzleDefinition _safePuzzle;
        private string _safeCode = "7391";

        public Inventory Inventory { get; } = new();
        public InteractionResolver Resolver { get; } = new();
        public RoomStateController RoomState { get; } = new();

        public bool HasEscaped { get; private set; }
        public bool HasReadNote => GetFlag("hasReadNote");

        public event Action OnWinRequested;
        public event Action OnFlagsChanged;

        public void Configure(EscapeRoomSetupSO setup)
        {
            _itemsById.Clear();
            _flags.Clear();
            HasEscaped = false;

            if (setup == null)
            {
                Resolver.RegisterHandler(new KeyOnDrawerHandler());
                Resolver.RegisterHandler(new KeyOnDoorHandler());
                RoomState.SetState("drawer", RoomObjectState.Closed);
                RoomState.SetState("note", RoomObjectState.Locked);
                RoomState.SetState("safe", RoomObjectState.Locked);
                RoomState.SetState("door", RoomObjectState.Locked);
                return;
            }

            _safeCode = string.IsNullOrEmpty(setup.SafeCode) ? "7391" : setup.SafeCode;
            _safePuzzle = setup.SafePuzzle;

            if (setup.Items != null)
            {
                foreach (var item in setup.Items)
                {
                    if (item != null && !string.IsNullOrEmpty(item.ItemId))
                    {
                        _itemsById[item.ItemId] = item;
                    }
                }
            }

            if (setup.InitialStates != null)
            {
                foreach (var initial in setup.InitialStates)
                {
                    if (initial != null && !string.IsNullOrEmpty(initial.ObjectId))
                    {
                        RoomState.SetState(initial.ObjectId, initial.State);
                    }
                }
            }
            else
            {
                RoomState.SetState("drawer", RoomObjectState.Closed);
                RoomState.SetState("note", RoomObjectState.Locked);
                RoomState.SetState("safe", RoomObjectState.Locked);
                RoomState.SetState("door", RoomObjectState.Locked);
            }

            if (setup.InteractionRules != null)
            {
                foreach (var rule in setup.InteractionRules)
                {
                    Resolver.RegisterHandler(rule);
                }
            }

            // Fallback hardcoded handlers if no SO rules registered.
            if (setup.InteractionRules == null || setup.InteractionRules.Length == 0)
            {
                Resolver.RegisterHandler(new KeyOnDrawerHandler());
                Resolver.RegisterHandler(new KeyOnDoorHandler());
            }
        }

        public ItemDefinition GetItemDefinition(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            return _itemsById.TryGetValue(itemId, out var def) ? def : null;
        }

        public string GetDisplayName(string itemId)
        {
            var def = GetItemDefinition(itemId);
            if (def != null && !string.IsNullOrEmpty(def.DisplayName))
            {
                return def.DisplayName;
            }

            return itemId switch
            {
                "rustyKey" => "Rusty Key",
                "goldKey" => "Gold Key",
                "note" => "Note",
                _ => itemId
            };
        }

        public InteractionContext CreateContext() =>
            new(Inventory, RoomState, this);

        public InteractionResult TryInteract(IInteractable target)
        {
            var context = CreateContext();
            if (target == null || !target.CanInteract(context))
            {
                return new InteractionResult(false, "Cannot interact right now.");
            }

            return target.Interact(context);
        }

        public UsageResult TryUseItemOn(string itemId, IInteractable target)
        {
            if (!Inventory.HasItem(itemId))
            {
                return new UsageResult(false, "Item not in inventory.");
            }

            return Resolver.TryUseItemOnTarget(itemId, target, CreateContext());
        }

        public InteractionResult TryOpenSafe(string enteredCode)
        {
            var context = CreateContext();
            if (RoomState.GetState("safe") == RoomObjectState.Open)
            {
                return new InteractionResult(false, "The safe is already open.");
            }

            if (_safePuzzle != null)
            {
                if (!_safePuzzle.CheckPrerequisites(context))
                {
                    return new InteractionResult(false, "You need more clues first.");
                }
            }
            else
            {
                if (RoomState.GetState("drawer") != RoomObjectState.Open)
                {
                    return new InteractionResult(false, "You need more clues first.");
                }

                if (!HasReadNote)
                {
                    return new InteractionResult(false, "You should read the note first.");
                }
            }

            var expectedCode = _safePuzzle != null && !string.IsNullOrEmpty(_safePuzzle.RequiredCode)
                ? _safePuzzle.RequiredCode
                : _safeCode;

            if (!string.Equals(enteredCode?.Trim(), expectedCode, StringComparison.Ordinal))
            {
                return new InteractionResult(false, "Wrong code.");
            }

            RoomState.SetState("safe", RoomObjectState.Open);
            var rewardId = _safePuzzle != null && !string.IsNullOrEmpty(_safePuzzle.RewardItemId)
                ? _safePuzzle.RewardItemId
                : "goldKey";
            Inventory.AddItem(rewardId);
            return new InteractionResult(true, "Safe opened. You found a Gold Key.");
        }

        public InteractionResult ReadNote()
        {
            if (RoomState.GetState("note") != RoomObjectState.Revealed &&
                RoomState.GetState("drawer") != RoomObjectState.Open)
            {
                return new InteractionResult(false, "There is nothing here.");
            }

            SetFlag("hasReadNote", true);
            return new InteractionResult(true, $"The note reads: Code {_safeCode}");
        }

        public bool GetFlag(string flagName) =>
            !string.IsNullOrEmpty(flagName) && _flags.TryGetValue(flagName, out var value) && value;

        public void SetFlag(string flagName, bool value)
        {
            if (string.IsNullOrEmpty(flagName))
            {
                return;
            }

            _flags[flagName] = value;
            OnFlagsChanged?.Invoke();
        }

        public void RequestWin()
        {
            if (HasEscaped)
            {
                return;
            }

            HasEscaped = true;
            OnWinRequested?.Invoke();
        }

        public bool CheckWin() => HasEscaped || RoomState.GetState("door") == RoomObjectState.Unlocked;
    }
}
