using UnityEngine;
using UnityEngine.UI;

namespace CarUICompletePack
{
    [ExecuteInEditMode]
    public class ChallengeWheel : MonoBehaviour
    {
        [Space]
        [Header("----- CHALLENGE WHEEL -----")]
        public Image challengeWheelProgressImage; // Image used to represent the challenge wheel progress
        public Text challengeWheelText; // Text used to display challenge wheel-related info
        public Color challengeWheelVisualColor = Color.white; // Unified color for challenge wheel progress and text
        public int challengeWheelMaxValue = 10; // Maximum value for the wheel (used for text display)
        [Range(0f, 1f)]
        public float challengeWheelProgressValue = 0f; // Normalized progress (0 to 1)

        void OnValidate()
        {
            SetChallengeWheelVisualColor();
            SetChallengeWheelProgress();
            UpdateChallengeWheelText();
        }

        void SetChallengeWheelVisualColor()
        {
            if (challengeWheelProgressImage != null)
                challengeWheelProgressImage.color = challengeWheelVisualColor;

            if (challengeWheelText != null)
                challengeWheelText.color = challengeWheelVisualColor;
        }

        void SetChallengeWheelProgress()
        {
            if (challengeWheelProgressImage != null)
                challengeWheelProgressImage.fillAmount = Mathf.Lerp(0f, 0.75f, challengeWheelProgressValue);
        }

        void UpdateChallengeWheelText()
        {
            if (challengeWheelText != null)
            {
                int currentValue = Mathf.RoundToInt(challengeWheelProgressValue * challengeWheelMaxValue);
                challengeWheelText.text = currentValue.ToString();
            }
        }
    }
}
