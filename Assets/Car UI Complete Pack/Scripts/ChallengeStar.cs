using UnityEngine;
using UnityEngine.UI;

namespace CarUICompletePack
{
    [ExecuteInEditMode]
    public class ChallengeStar : MonoBehaviour
    {
        [Space]
        [Header("----- CHALLENGE STAR -----")]
        public Image challengeStarProgressImage; // Image used to represent the challenge star progress
        public Text challengeStarText; // Text used to display challenge star-related info
        public Color challengeStarVisualColor = Color.white; // Unified color for challenge star progress and text
        public int challengeStarMaxValue = 10; // Maximum value for the star (used for text display)
        [Range(0f, 1f)]
        public float challengeStarProgressValue = 0f; // Normalized progress (0 to 1)

        void OnValidate()
        {
            SetChallengeStarVisualColor();
            SetChallengeStarProgress();
            UpdateChallengeStarText();
        }

        // Sets the color for both the progress bar and the text
        void SetChallengeStarVisualColor()
        {
            if (challengeStarProgressImage != null)
                challengeStarProgressImage.color = challengeStarVisualColor;

            if (challengeStarText != null)
                challengeStarText.color = challengeStarVisualColor;
        }

        // Updates the fill amount of the challenge star progress bar
        void SetChallengeStarProgress()
        {
            if (challengeStarProgressImage != null)
                challengeStarProgressImage.fillAmount = Mathf.Lerp(0f, 0.75f, challengeStarProgressValue);
        }

        // Updates the text to show only the current value (e.g., "7")
        void UpdateChallengeStarText()
        {
            if (challengeStarText != null)
            {
                int currentValue = Mathf.RoundToInt(challengeStarProgressValue * challengeStarMaxValue);
                challengeStarText.text = currentValue.ToString();
            }
        }
    }
}
