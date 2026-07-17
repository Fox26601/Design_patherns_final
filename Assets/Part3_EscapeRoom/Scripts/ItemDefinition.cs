using UnityEngine;

namespace Part3_EscapeRoom
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "DesignPatterns/EscapeRoom/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        public string ItemId;
        public string DisplayName;
        public ItemType Type;
        public Sprite Icon;
    }
}
