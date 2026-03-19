using UnityEngine;

namespace CarUICompletePack
{
    public class SeatBelt : MonoBehaviour
    {
        [Header("Seatbelt Off and On")]
        public GameObject seatBeltOff; // The 'off' state child
        public GameObject seatBeltOn;  // The 'on' state child

        private void Start()
        {
            // Initially set the seatbelt to be off
            seatBeltOff.SetActive(true);
            seatBeltOn.SetActive(false);
        }

        public void ToggleSeatBelt()
        {
            // Toggle between off and on states
            seatBeltOff.SetActive(!seatBeltOff.activeSelf);
            seatBeltOn.SetActive(!seatBeltOn.activeSelf);
        }
    }
}
