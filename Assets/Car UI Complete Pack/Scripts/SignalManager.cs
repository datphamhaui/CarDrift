using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CarUICompletePack
{
    public class SignalManager : MonoBehaviour
    {
        [Header("Blinking Settings")]
        public float timeBlinking = 0.5f; // Time interval for blinking effect

        private Coroutine blinkCoroutine; // Active blinking coroutine
        private Image activeSignal; // Currently active signal

        public void ToggleSignal(Image newSignal)
        {
            // Stop blinking if the same signal is already active
            if (activeSignal == newSignal)
            {
                StopBlinking();
            }
            else
            {
                // Stop any previously active signal
                if (activeSignal != null)
                    StopBlinking();

                StartBlinking(newSignal);
            }
        }

        private void StartBlinking(Image signal)
        {
            activeSignal = signal;
            blinkCoroutine = StartCoroutine(BlinkEffect(signal));
        }

        private void StopBlinking()
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            if (activeSignal != null)
                activeSignal.gameObject.SetActive(false);

            activeSignal = null;
        }

        private IEnumerator BlinkEffect(Image signal)
        {
            signal.gameObject.SetActive(true);
            while (true)
            {
                signal.enabled = !signal.enabled;
                yield return new WaitForSeconds(timeBlinking);
            }
        }
    }
}
