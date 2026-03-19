using UnityEngine;
using UnityEngine.UI;

namespace CarUICompletePack
{
    [ExecuteInEditMode]
    public class ChallengeSteeringWheel : MonoBehaviour
    {
        [Space]
        [Header("----- CHALLENGE STEERING WHEEL -----")]
        public Image challengeSteeringWheelProgressImage; // Image used to represent the challenge steering wheel progress
        public Text challengeSteeringWheelText; // Text used to display challenge steering wheel-related info
        public Color challengeSteeringWheelVisualColor = Color.white; // Unified color for challenge steering wheel progress and text
        public int challengeSteeringWheelMaxValue = 10; // Maximum value for the steering wheel (used for text display)
        [Range(0f, 1f)]
        public float challengeSteeringWheelProgressValue = 0f; // Normalized progress (0 to 1)

        void OnValidate()
        {
            SetChallengeSteeringWheelVisualColor();
            SetChallengeSteeringWheelProgress();
            UpdateChallengeSteeringWheelText();
        }

        // Sets the color for both the progress bar and the text
        void SetChallengeSteeringWheelVisualColor()
        {
            if (challengeSteeringWheelProgressImage != null)
                challengeSteeringWheelProgressImage.color = challengeSteeringWheelVisualColor;

            if (challengeSteeringWheelText != null)
                challengeSteeringWheelText.color = challengeSteeringWheelVisualColor;
        }

        // Updates the fill amount of the challenge steering wheel progress bar
        void SetChallengeSteeringWheelProgress()
        {
            if (challengeSteeringWheelProgressImage != null)
                challengeSteeringWheelProgressImage.fillAmount = Mathf.Lerp(0f, 0.75f, challengeSteeringWheelProgressValue);
        }

        // Updates the text to show only the current value (e.g., "7")
        void UpdateChallengeSteeringWheelText()
        {
            if (challengeSteeringWheelText != null)
            {
                int currentValue = Mathf.RoundToInt(challengeSteeringWheelProgressValue * challengeSteeringWheelMaxValue);
                challengeSteeringWheelText.text = currentValue.ToString();
            }
        }
    }
}
