using UnityEngine;

namespace CarUICompletePack
{
    public class HeadlightToggle : MonoBehaviour
    {
        [Header("Headlight States")]
        public GameObject offState;      // Off state child
        public GameObject lowBeamState;  // Low beam child
        public GameObject highBeamState; // High beam child

        private int currentState = 0; // 0 = Off, 1 = LowBeam, 2 = HighBeam
        private GameObject[] states; // Array to store state references

        private void Start()
        {
            // Store states in an array for easy switching
            states = new GameObject[] { offState, lowBeamState, highBeamState };

            // Ensure only the off state is active at the start
            SetState(0);
        }

        public void ToggleHeadlight()
        {
            currentState = (currentState + 1) % states.Length; // Cycle through states
            SetState(currentState);
        }

        private void SetState(int index)
        {
            // Disable all states first
            foreach (GameObject state in states)
            {
                state.SetActive(false);
            }

            // Activate the selected state
            states[index].SetActive(true);
        }
    }
}
