using System;
using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Score storage without singleton pattern.
    /// Uses ScriptableObject as a shared service asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ScoreService", menuName = "DesignPatterns/Score Service")]
    public class ScoreService : ScriptableObject
    {
        [SerializeField] private int pointsPerPickup = 10;

        public int Score { get; private set; }
        public event Action<int> OnScoreChanged;

        public void ResetScore()
        {
            Score = 0;
            OnScoreChanged?.Invoke(Score);
        }

        public void AddPickupPoints()
        {
            Score += pointsPerPickup;
            OnScoreChanged?.Invoke(Score);
        }
    }
}
