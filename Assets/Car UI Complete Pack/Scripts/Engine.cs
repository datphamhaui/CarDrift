using UnityEngine;

namespace CarUICompletePack
{
    public class Engine : MonoBehaviour
    {
        [Header("Engine On/Off Element")]
        public GameObject engineOn; // The child element representing engine on/off state

        public void Toggle()
        {
            // Instantly toggle the active state of the engineOn element
            engineOn.SetActive(!engineOn.activeSelf);
        }
    }
}
