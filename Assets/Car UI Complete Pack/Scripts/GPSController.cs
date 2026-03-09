using UnityEngine;
using UnityEngine.UI;

namespace CarUICompletePack
{
    [ExecuteInEditMode]
    public class GPSController : MonoBehaviour
    {
        [Space]
        [Header("----- DAMAGE SLIDER -----")]
        public GameObject damageSlider;
        public bool isDamageSliderVisible = true;

        [Space]
        [Header("Damage Slider Appearance")]
        public Image damageSliderImage;
        public Image damageIcon;
        public Color damageSliderColor = Color.white;

        [Range(0f, 1f)]
        public float damageSliderValue = 0f; // original public field

        // Property that updates UI automatically when changed
        public float DamageSliderValue
        {
            get => damageSliderValue;
            set
            {
                damageSliderValue = value;
                SetDamageSliderProgress(); // update the visual fill
            }
        }

        void OnValidate()
        {
            SetDamageSliderVisibility();
            SetDamageSliderColor();
            SetDamageSliderProgress();
        }

        // Show or hide the damage slider
        void SetDamageSliderVisibility() => damageSlider.SetActive(isDamageSliderVisible);

        // Update the slider and icon color
        void SetDamageSliderColor()
        {
            damageSliderImage.color = damageSliderColor;
            damageIcon.color = damageSliderColor;
        }

        // Update the fill amount based on damage value
        void SetDamageSliderProgress() => damageSliderImage.fillAmount = Mathf.Lerp(0f, 0.31f, damageSliderValue);
    }
}
