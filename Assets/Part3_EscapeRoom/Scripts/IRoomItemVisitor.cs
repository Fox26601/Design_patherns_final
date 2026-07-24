namespace Part3_EscapeRoom
{
    /// <summary>
    /// Visitor operations over room interactables.
    /// Pattern: Visitor (https://www.unitydesignpatterns.com/patterns/visitor)
    /// </summary>
    public interface IRoomItemVisitor
    {
        void VisitPickup(PickupInteractable pickup);
        void VisitNote(NoteInteractable note);
        void VisitDrawer(DrawerInteractable drawer);
        void VisitDoor(DoorInteractable door);
        void VisitSafe(SafeInteractable safe);
        void VisitDecoy(DecoyInteractable decoy);
    }
}
