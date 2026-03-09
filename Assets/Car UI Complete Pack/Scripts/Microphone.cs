using UnityEngine;

namespace CarUICompletePack
{
    public class Microphone : MonoBehaviour
    {
        [Header("Microphone On/Off Element")]
        public GameObject MicrophoneOn; // The child element representing Microphone on/off state

        public void Toggle()
        {
            // Instantly toggle the active state of the MicrophoneOn element
            MicrophoneOn.SetActive(!MicrophoneOn.activeSelf);
        }
    }
}
