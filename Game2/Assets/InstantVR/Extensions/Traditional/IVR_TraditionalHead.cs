/* InstantVR Traditional head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.4
 * date: April 15, 2016
 *
 * - new Controller Input
 */

using UnityEngine;

namespace IVR {

    public class IVR_TraditionalHead : IVR_Controller {

        [HideInInspector]
        private ControllerInput controller;

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_Traditional>();
            base.StartController(ivr);

            controller = Controllers.GetController(0);
        }

        public override void UpdateController() {
            tracking = true;
            if (selected && this.enabled) {
                if (controller != null) {
                    controllerPosition = Quaternion.Inverse(ivr.transform.rotation) * startPosition;
                    float xAngle = calculateStickXAngle(controller.right.stickVertical);
                    float yAngle = calculateStickYAngle(controller.right.stickHorizontal);
                    controllerRotation = Quaternion.Euler(xAngle, yAngle, 0);
                    base.UpdateController();
                }
            }
        }

        private static float maxXangle = 60;
        private static float maxYangle = 70;

        private float calculateStickXAngle(float stickVertical) {
            return -stickVertical * maxXangle;
        }

        private float calculateStickYAngle(float stickHorizontal) {
            return stickHorizontal * maxYangle;
        }

        public override void OnTargetReset() {
        }
    }
}