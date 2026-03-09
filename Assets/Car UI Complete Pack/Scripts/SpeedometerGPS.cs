using UnityEngine;

namespace CarUICompletePack
{
    public class SpeedometerGPS : MonoBehaviour
    {
        private SpeedometerController speedometerController; 
        private GPSController gpsController; 

        void Awake()
        {
            speedometerController = FindObjectOfType<SpeedometerController>();
            gpsController = FindObjectOfType<GPSController>();
        }

        public void Start()
        {
            // Set values for the SPEEDOMETER components
            speedometerController.SpeedometerSliderValue = 0.2f; 
            speedometerController.currentGearText.text = "N";
            speedometerController.currentSpeedText.text = "0";

            // Set value for the GAS component
            speedometerController.GasSliderValue = 1f;

            // Set value for the DAMAGE component
            gpsController.DamageSliderValue = 1f;
        }
    }
}
