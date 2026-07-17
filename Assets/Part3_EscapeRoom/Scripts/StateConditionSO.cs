using UnityEngine;

namespace Part3_EscapeRoom
{
    [CreateAssetMenu(fileName = "StateCondition", menuName = "DesignPatterns/EscapeRoom/State Condition")]
    public class StateConditionSO : ScriptableObject, ICondition
    {
        public string ObjectId;
        public RoomObjectState RequiredState;

        public bool Evaluate(InteractionContext context)
        {
            if (context.RoomState == null || string.IsNullOrEmpty(ObjectId))
            {
                return false;
            }

            return context.RoomState.GetState(ObjectId) == RequiredState;
        }
    }
}
