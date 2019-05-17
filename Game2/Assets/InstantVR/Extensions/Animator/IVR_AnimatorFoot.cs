/* InstantVR Animator foot
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.6
 * date: May 5, 2016
 *
 * - Fixed foot animation when teleporting
 */

using UnityEngine;

namespace IVR {

    public class IVR_AnimatorFoot : IVR_Controller {
        public bool followHip = true;
        public bool legMovements = true;

        [HideInInspector]
        private bool isLeftFoot;

        [HideInInspector]
        IVR_AnimatorFoot leftFoot, rightFoot;

        [HideInInspector]
        protected IVR_Animate leftFootAnimation, rightFootAnimation;
        [HideInInspector]
        protected Vector3 startHipFootL;
        [HideInInspector]
        protected Vector3 startHipFootR;
        [HideInInspector]
        protected Vector3 startHipFoot;
        [HideInInspector]
        protected Vector3 startFootLposition, startFootRposition;
        [HideInInspector]
        private Vector3 localStartPosition;

        [HideInInspector]
        Vector3 stepStart = Vector3.zero;

        void Start() { }

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_Animator>();
            base.StartController(ivr);

            startHipFootL = ivr.hipTarget.InverseTransformPoint(ivr.leftFootTarget.position);
            startHipFootR = ivr.hipTarget.InverseTransformPoint(ivr.rightFootTarget.position);

            if (transform == ivr.leftFootTarget) {
                isLeftFoot = true;
                leftFoot = this;
                rightFoot = ivr.rightFootTarget.GetComponent<IVR_AnimatorFoot>();
                leftFootAnimation = gameObject.AddComponent<IVR_Animate>();
                lastPos = transform.position;
                startHipFoot = startHipFootL;
                localStartPosition = ivr.transform.InverseTransformPoint(ivr.leftFootTarget.position);
            } else {
                rightFoot = this;
                leftFoot = ivr.leftFootTarget.GetComponent<IVR_AnimatorFoot>();
                rightFootAnimation = gameObject.AddComponent<IVR_Animate>();
                lastPos = transform.position;
                startHipFoot = startHipFootR;
                localStartPosition = ivr.transform.InverseTransformPoint(ivr.rightFootTarget.position);
            }

            startFootLposition = ivr.leftFootTarget.position - transform.position;
            startFootRposition = ivr.rightFootTarget.position - transform.position;

            lastHipPosition = ivr.hipTarget.position;
        }

        public override void UpdateController() {
            if (enabled) {
                tracking = true;

                if (leftFootAnimation == null)
                    leftFootAnimation = ivr.leftFootTarget.GetComponent<IVR_Animate>();
                if (rightFootAnimation == null)
                    rightFootAnimation = ivr.rightFootTarget.GetComponent<IVR_Animate>();

                if (selected) {
                    if (legMovements)
                        FeetAnimation();
                    else if (followHip)
                        FollowHip();
                }
            } else
                tracking = false;
        }

        private void FollowHip() {
            Vector3 hipLocalPosition = ivr.hipTarget.position - ivr.transform.position;
            Quaternion hipLocalRotation = Quaternion.Inverse(ivr.transform.rotation) * ivr.hipTarget.rotation;
            controllerPosition = hipLocalPosition + (hipLocalRotation * startHipFoot);
            controllerRotation = hipLocalRotation * startRotation;
            base.UpdateController();

            Vector3 localPosition = ivr.transform.InverseTransformPoint(transform.position);
            if (localPosition.y < localStartPosition.y)
                transform.position = new Vector3(transform.position.x, ivr.transform.position.y + localStartPosition.y, transform.position.z);
        }


        [HideInInspector]
        private Vector3 curSpeed;
        [HideInInspector]
        private Vector3 lastHipPosition = Vector3.zero;
        [HideInInspector]
        private Vector3 basePointStart, basePointDelta;
        [HideInInspector]
        private bool lastStepLeft = false, lastStepRight = false;

        protected void FeetAnimation() {
            curSpeed = ivr.hipTarget.position - lastHipPosition;
            curSpeed = ivr.hipTarget.InverseTransformDirection(curSpeed);
            curSpeed = new Vector3(curSpeed.x, 0, curSpeed.z);

            lastHipPosition = ivr.hipTarget.position;

            basePointDelta = ivr.transform.position - basePointStart; // how much did the basepoint move?
            basePointDelta = new Vector3(basePointDelta.x, 0, basePointDelta.z);

            // turning
            if (leftFootAnimation.f == 0 && rightFootAnimation.f == 0) { // We are not stepping yet
                if (isLeftFoot) {
                    float delta = ivr.hipTarget.eulerAngles.y - ivr.leftFootTarget.eulerAngles.y;
                    delta = AngleDifference(ivr.leftFootTarget.eulerAngles.y, ivr.hipTarget.eulerAngles.y);
                    if (delta < -45) {
                        FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, false);
                    }
                } else {
                    float delta = ivr.hipTarget.eulerAngles.y - ivr.rightFootTarget.eulerAngles.y;
                    delta = AngleDifference(ivr.rightFootTarget.eulerAngles.y, ivr.hipTarget.eulerAngles.y);
                    if (delta > 45) {
                        FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, false);
                    }
                }
            }

            // translating
            if (leftFoot.lastStepLeft == true && leftFootAnimation.startStep == false) {
                if (!isLeftFoot)
                    FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, true);
                else
                    FootStaying();
            } else if (leftFootAnimation.startStep == true) {
                if (isLeftFoot)
                    FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, true);
                else
                    FootStaying();
            } else if (rightFoot.lastStepRight == true && rightFootAnimation.startStep == false) {
                if (isLeftFoot)
                    FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, true);
                else
                    FootStaying();
            } else if (rightFootAnimation.startStep == true) {
                if (!isLeftFoot)
                    FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, true);
                else
                    FootStaying();

            } else if (curSpeed.magnitude < 0.05F && (curSpeed.x > 0.001F || curSpeed.z > 0.001F || curSpeed.z < -0.001F)) {
                if (!isLeftFoot)
                    FootStepRight(rightFootAnimation, ivr.hipTarget.position, ivr.rightFootTarget.position, false);

            } else if (curSpeed.magnitude < 0.05F && curSpeed.x < -0.001F) {
                if (isLeftFoot)
                    FootStepLeft(leftFootAnimation, ivr.hipTarget.position, ivr.leftFootTarget.position, false);

            } else {
                leftFoot.lastStepLeft = false;
                rightFoot.lastStepRight = false;
                lastPos = transform.position;
            }


            if (isLeftFoot) {
                if (leftFootAnimation.f >= 1) {
                    leftFootAnimation.startStep = false;
                    leftFootAnimation.f = 0;
                } else {
                    FootStepping(leftFootAnimation, startHipFootL);
                }
            } else {
                if (rightFootAnimation.f >= 1) {
                    rightFootAnimation.startStep = false;
                    rightFootAnimation.f = 0;
                } else {
                    FootStepping(rightFootAnimation, startHipFootR);
                }
            }
        }

        private float AngleDifference(float a, float b) {
            float r = b - a;
            return NormalizeAngle(r);
        }

        private float NormalizeAngle(float a) {
            while (a < -180) a += 360;
            while (a > 180) a -= 360;
            return a;
        }

        protected void FootStepLeft(IVR_Animate footAnimation, Vector3 hipTargetPosition, Vector3 footTargetPosition, bool follow) {
            if (footAnimation.startStep == false) {
                basePointStart = ivr.transform.position;
                stepStart = new Vector3(footTargetPosition.x, ivr.transform.position.y, footTargetPosition.z);
                footAnimation.startStep = true;

                if (!follow)
                    leftFoot.lastStepLeft = true;
                rightFoot.lastStepRight = false;
            }
        }

        protected void FootStepRight(IVR_Animate footAnimation, Vector3 hipTargetPosition, Vector3 footTargetPosition, bool follow) {
            if (footAnimation.startStep == false) {
                basePointStart = ivr.transform.position;
                stepStart = new Vector3(footTargetPosition.x, ivr.transform.position.y, footTargetPosition.z);
                footAnimation.startStep = true;

                if (!follow)
                    rightFoot.lastStepRight = true;
                leftFoot.lastStepLeft = false;
            }
        }

        private void FootStepping(IVR_Animate footAnimation, Vector3 startHipFoot) {
            if (footAnimation.f >= 1) {
                footAnimation.startStep = false;
                footAnimation.f = 0;
            } else if (footAnimation.f > 0) {
                stepStart = new Vector3(stepStart.x, ivr.transform.position.y, stepStart.z);
                Vector3 stepTarget = ivr.hipTarget.TransformPoint(startHipFoot) - stepStart;
                stepTarget = new Vector3(stepTarget.x, startPosition.y, stepTarget.z);

                if (curSpeed.x != 0 || curSpeed.z != 0) {
                    Vector3 avgSpeed2 = basePointDelta / footAnimation.f;
                    stepTarget += avgSpeed2 / 2;
                }

                Vector3 newPosition = footAnimation.AnimationLerp(Vector3.zero, stepTarget, ref footAnimation.f);

                transform.position = stepStart + newPosition;
                transform.rotation = ivr.hipTarget.rotation; // should be eased towards this rotation in the end
                lastPos = transform.position;
            }
        }

        Vector3 lastPos = Vector3.zero;
        private void FootStaying() {
            transform.position = new Vector3(lastPos.x, transform.position.y, lastPos.z);
        }
    }
}