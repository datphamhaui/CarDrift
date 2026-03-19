using UnityEngine;
using UnityEngine.UI;

namespace CarUICompletePack
{
    [ExecuteInEditMode]
    public class SpeedometerController : MonoBehaviour
    {
        [Space]
        [Header("----- SPEEDOMETER -----")]
        public Image speedometerSliderImage;
        public Color speedometerSliderColor = Color.white;

        [Range(0f, 1f)]
        public float speedometerSliderValue = 0f; // original public field

        [Header("Speedometer Text")]
        public Text currentSpeedText;
        public Text currentGearText;

        [Space]
        [Header("----- GAS SLIDER -----")]
        public GameObject gasSlider;
        public bool isGasSliderVisible = true;

        [Header("Gas Slider Appearance")]
        public Image gasIcon;
        public Image gasSliderImage;
        public Color gasSliderColor = Color.white;

        [Range(0f, 1f)]
        public float gasSliderValue = 0f; // original public field

        // Property for gas slider with automatic UI update
        public float GasSliderValue
        {
            get => gasSliderValue;
            set
            {
                gasSliderValue = value;
                SetGasSliderProgress(); // automatically update fill
            }
        }

        // Property for speedometer slider with automatic UI update
        public float SpeedometerSliderValue
        {
            get => speedometerSliderValue;
            set
            {
                speedometerSliderValue = value;
                SetSpeedometerSliderProgress(); // automatically update fill
            }
        }

        void OnValidate()
        {
            SetGasSliderVisibility();
            SetGasSliderColor();
            SetGasSliderProgress();
            SetSpeedometerSliderColor();
            SetSpeedometerSliderProgress();
        }

        // Show or hide the gas slider
        void SetGasSliderVisibility() => gasSlider.SetActive(isGasSliderVisible);

        // Set the color for the gas slider and icon
        void SetGasSliderColor()
        {
            gasSliderImage.color = gasSliderColor;
            gasIcon.color = gasSliderColor;
        }

        // Update the gas slider fill based on current value
        void SetGasSliderProgress() => gasSliderImage.fillAmount = Mathf.Lerp(0f, 0.31f, gasSliderValue);

        // Set the color for the speedometer ring
        void SetSpeedometerSliderColor() => speedometerSliderImage.color = speedometerSliderColor;

        // Update the speedometer fill based on current value
        void SetSpeedometerSliderProgress() => speedometerSliderImage.fillAmount = Mathf.Lerp(0f, 0.7004f, speedometerSliderValue);
    }
}
