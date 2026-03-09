using UnityEngine;

namespace CarUICompletePack
{
    public class ParkingSensorController : MonoBehaviour
    {
        [Header("Sensors")]
        public GameObject[] frontSensors; // 4 front sensors
        public GameObject[] rearSensors;  // 4 rear sensors

        [Header("UI Elements")]
        public GameObject ParkingSensorActive;   // GameObject that toggles sensors visibility (not a button component)
        public GameObject ParkingSensorUI;  // Parking sensors UI indicator

        [Header("Preview in Inspector")]
        [Range(0, 4)] public int frontSensorLevel = 0;  // Controls visibility of front sensors
        [Range(0, 4)] public int rearSensorLevel = 0;   // Controls visibility of rear sensors

        private bool sensorsActive = false;

        // This function is called automatically when you change a value in the Inspector
        void OnValidate()
        {
            // Update sensors visibility every time a value in Inspector is changed
            UpdateSensorVisibility();
        }

        // This function updates the visibility of the front and rear sensors based on their respective levels
        void UpdateSensorVisibility()
        {
            // Update front sensors based on frontSensorLevel
            for (int i = 0; i < frontSensors.Length; i++)
            {
                // If frontSensorLevel is greater than 0, activate the corresponding sensor
                frontSensors[i].SetActive(i < frontSensorLevel);
            }

            // Update rear sensors based on rearSensorLevel
            for (int i = 0; i < rearSensors.Length; i++)
            {
                // If rearSensorLevel is greater than 0, activate the corresponding sensor
                rearSensors[i].SetActive(i < rearSensorLevel);
            }

            // Update visibility of the whole parking sensor system
            if (ParkingSensorActive != null)
            {
                ParkingSensorActive.SetActive(sensorsActive);
            }
            if (ParkingSensorUI != null)
            {
                ParkingSensorUI.SetActive(sensorsActive);
            }
        }

        // Toggle function to activate/deactivate sensors manually
        public void ToggleSensorVisibility()
        {
            sensorsActive = !sensorsActive;  // Toggle the state
            UpdateSensorVisibility();  // Update visibility based on the new state
        }

        // This function can be called from other scripts or UI to update sensor visibility dynamically
        public void SetSensorLevels(int newFrontLevel, int newRearLevel)
        {
            frontSensorLevel = newFrontLevel;
            rearSensorLevel = newRearLevel;

            // Update the sensor visibility based on the new levels
            UpdateSensorVisibility();
        }
    }
}
