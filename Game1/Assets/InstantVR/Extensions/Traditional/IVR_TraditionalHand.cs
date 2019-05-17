/* InstantVR Traditional hand
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.8.9
 * date: June 19, 2016
 * 
 * - Fixed fingers not being reset when not tracking
 */

using UnityEngine;

namespace IVR {

    public class IVR_TraditionalHand : IVR_HandController {

#if INSTANTVR_ADVANCED
        [HideInInspector]
        private IVR_HandMovementsBase handMovements;
#endif
        [HideInInspector]
        private IVR_AnimatorHand handAnimator;

        [HideInInspector]
        private bool joystick2available;
        [HideInInspector]
        private bool startBackAvailable;
        [HideInInspector]
        private bool fingerAxisAvailable, indexFingerAxisAvailable;

        void Start() {
        }

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_Traditional>();
            base.StartController(ivr);
            tracking = true;

            handAnimator = GetComponent<IVR_AnimatorHand>();
#if INSTANTVR_ADVANCED
            handMovements = GetComponent<IVR_HandMovementsBase>();
#endif
        }

        public override void UpdateController() {
            if (enabled) {
                if (handAnimator != null) {
                    handAnimator.UpdateController();
#if INSTANTVR_ADVANCED
                    if (handMovements!= null && selected) {
                        handMovements.thumbCurl = 0;
                        handMovements.indexCurl = 0;
                        handMovements.middleCurl = 0;
                        handMovements.ringCurl = 0;
                        handMovements.littleCurl = 0;
                    }
#endif
                    position = handAnimator.position;
                    rotation = handAnimator.rotation;
                    if (selected) {
                        transform.position = position;
                        transform.rotation = rotation;
                    }
                }
            }
        }

        /*
                // when we are holding an object alredy, we must explicitly drop it.
                if (handMovements.grabbedObject != null && lastFingerInput <= 0) {
                    lastFingerInput = fingerInput;
                    fingerInput = 1;
                } else {
                    if (selected)
                        lastFingerInput = fingerInput;
                    else
                        lastFingerInput = 0;
                }
         */


        public override void OnTargetReset() {
            if (selected) {
                Calibrate(true);
            }
        }
    }
}