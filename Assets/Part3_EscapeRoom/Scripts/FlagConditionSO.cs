using UnityEngine;

namespace Part3_EscapeRoom
{
    [CreateAssetMenu(fileName = "FlagCondition", menuName = "DesignPatterns/EscapeRoom/Flag Condition")]
    public class FlagConditionSO : ScriptableObject, ICondition
    {
        public string FlagName = "hasReadNote";
        public bool RequiredValue = true;

        public bool Evaluate(InteractionContext context)
        {
            if (context.Controller == null || string.IsNullOrEmpty(FlagName))
            {
                return false;
            }

            return context.Controller.GetFlag(FlagName) == RequiredValue;
        }
    }
}
