namespace Part3_EscapeRoom
{
    /// <summary>
    /// Room element that accepts Visitor operations.
    /// Pattern: Visitor (https://www.unitydesignpatterns.com/patterns/visitor)
    /// </summary>
    public interface IRoomItem
    {
        void Accept(IRoomItemVisitor visitor);
    }
}
