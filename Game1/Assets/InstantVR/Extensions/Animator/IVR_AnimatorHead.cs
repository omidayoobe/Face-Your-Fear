/* InstantVR Animator
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.4
 * date: April 15, 2016
 * 
 * - added namespace
 */
using UnityEngine;

namespace IVR {

    public class IVR_AnimatorHead : IVR_Controller {

        public float headWeight = 0.9f;

        //    private IVR.HeadMovementsBase headMovements;
        //    /private Transform cameraTransform;

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_Animator>();
            base.StartController(ivr);
            tracking = true;

            //        headMovements = GetComponent<IVR.HeadMovementsBase>();
            //        cameraTransform = ivr.GetComponentInChildren<Camera>().transform;
        }

        public override void UpdateController() {
            controllerPosition = startPosition;
            controllerRotation = startRotation;

            base.UpdateController();
            UpdateEyes();

        }

        public virtual void UpdateEyes() {
            /*
            Quaternion headRotation = transform.rotation * Quaternion.Inverse(ivr.characterTransform.rotation);
            Quaternion eyeRotation = Quaternion.LerpUnclamped(Quaternion.identity, headRotation, 1 / headWeight); // should be non-linear

            Vector3 lookDirection = eyeRotation * ivr.characterTransform.forward;

            headMovements.focusPoint = LookTarget(cameraTransform.position, lookDirection);
            */
            /*
            if (cameraTransform)
                lookTarget.position = LookTarget(cameraTransform.position, lookDirection);
            else
                lookTarget.position = LookTarget(transform.position, lookDirection);
            */
        }

        private Vector3 LookTarget(Vector3 centerEyePosition, Vector3 lookDirection) {

            RaycastHit hit;
            if (Physics.Raycast(centerEyePosition, lookDirection, out hit, 10)) {
                return hit.point;
            } else {
                // look to 'infinity'
                return centerEyePosition + lookDirection * 10;
            }
        }
    }
}