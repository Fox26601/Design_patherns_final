using TMPro;
using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// HUD score display subscribed to ScoreService.
    /// </summary>
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private ScoreService scoreService;
        [SerializeField] private TMP_Text scoreText;

        private void OnEnable()
        {
            if (scoreService != null)
            {
                scoreService.OnScoreChanged += UpdateScore;
                UpdateScore(scoreService.Score);
            }
        }

        private void OnDisable()
        {
            if (scoreService != null)
            {
                scoreService.OnScoreChanged -= UpdateScore;
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
    }
}
