using UnityEngine;

namespace CarUICompletePack
{
    public class RearMirrorController : MonoBehaviour
    {
        public Animator mirrorAnimator;
        private bool isDown = false;
        public GameObject mirrorOn;

        public void ToggleMirror()
        {
            isDown = !isDown;
            mirrorAnimator.SetBool("IsDown", isDown);

            mirrorOn.SetActive(isDown);
        }
    }
}