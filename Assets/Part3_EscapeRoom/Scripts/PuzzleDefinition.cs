using UnityEngine;

namespace Part3_EscapeRoom
{
    [CreateAssetMenu(fileName = "PuzzleDefinition", menuName = "DesignPatterns/EscapeRoom/Puzzle Definition")]
    public class PuzzleDefinition : ScriptableObject
    {
        public string PuzzleId;
        public StateConditionSO[] StatePrerequisites;
        public FlagConditionSO[] FlagPrerequisites;
        public string RequiredCode;
        public string RewardItemId = "goldKey";
        public string TargetObjectId = "safe";

        public bool CheckPrerequisites(InteractionContext context)
        {
            var composite = new CompositeCondition();

            if (StatePrerequisites != null)
            {
                foreach (var condition in StatePrerequisites)
                {
                    composite.Add(condition);
                }
            }

            if (FlagPrerequisites != null)
            {
                foreach (var condition in FlagPrerequisites)
                {
                    composite.Add(condition);
                }
            }

            return composite.Evaluate(context);
        }
    }
}
