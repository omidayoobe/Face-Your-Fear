/* InstantVR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.8.6
 * date: August 14, 2017
 * 
 * - Fix: collisions
 */

using UnityEngine;

namespace IVR {

    [System.Serializable]
    [HelpURL("http://serrarens.nl/passervr/support/vr-component-functions/instantvr-function/")]
    public class InstantVR : MonoBehaviour {

        [Tooltip("Target Transform for the head")]
        public Transform headTarget;
        [Tooltip("Target Transform for the left hand")]
        public Transform leftHandTarget;
        [Tooltip("Target Transform for the right hand")]
        public Transform rightHandTarget;
        [Tooltip("Target Transform for the hip")]
        public Transform hipTarget;
        [Tooltip("Target Transform for the left foot")]
        public Transform leftFootTarget;
        [Tooltip("Target Transform for the right foot")]
        public Transform rightFootTarget;

        public enum BodySide {
            Unknown = 0,
            Left = 1,
            Right = 2,
        };

        [HideInInspector]
        private IVR_Extension[] extensions;

        [HideInInspector]
        private IVR_Controller[] headControllers;
        [HideInInspector]
        private IVR_Controller[] leftHandControllers, rightHandControllers;
        [HideInInspector]
        private IVR_Controller[] hipControllers;
        [HideInInspector]
        private IVR_Controller[] leftFootControllers, rightFootControllers;

        private IVR_Controller headController;
        public IVR_Controller HeadController { get { return headController; } set { headController = value; } }
        private IVR_Controller leftHandController, rightHandController;
        public IVR_Controller LeftHandController { get { return leftHandController; } set { leftHandController = value; } }
        public IVR_Controller RightHandController { get { return rightHandController; } set { rightHandController = value; } }
        private IVR_Controller hipController;
        public IVR_Controller HipController { get { return hipController; } set { hipController = value; } }
        private IVR_Controller leftFootController, rightFootController;
        public IVR_Controller LeftFootController { get { return leftFootController; } set { leftFootController = value; } }
        public IVR_Controller RightFootController { get { return rightFootController; } set { rightFootController = value; } }

        [HideInInspector]
        private IVR_Movements headMovements;
        [HideInInspector]
        public IVR_HandMovementsBase leftHandMovements, rightHandMovements;
        [HideInInspector]
        private IVR_BodyMovements bodyMovements;

        [HideInInspector]
        public Transform characterTransform;
        [HideInInspector]
        public float avatarNeckHeight;

        [HideInInspector]
        public int playerID = 0;

        [Tooltip("The character will step up a stair only if it is closer to the ground than the indicated value.")]
        public float stepOffset = 0.3F;

        public bool useGravity = true;

        public bool collisions = true;
        [HideInInspector]
        public bool triggerEntered = false, collided = false;
        [HideInInspector]
        public Vector3 hitNormal = Vector3.zero;
        [HideInInspector]
        public Vector3 inputDirection;

        private void OnDisable() {
            if (leftHandMovements.handRigidbody != null)
                leftHandMovements.handRigidbody.gameObject.SetActive(false);
            if (rightHandMovements.handRigidbody != null)
                rightHandMovements.handRigidbody.gameObject.SetActive(false);
        }

        protected virtual void Awake() {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            extensions = this.GetComponents<IVR_Extension>();
            foreach (IVR_Extension extension in extensions)
                extension.StartExtension(this);

            headControllers = headTarget.GetComponents<IVR_Controller>();
            leftHandControllers = leftHandTarget.GetComponents<IVR_Controller>();
            rightHandControllers = rightHandTarget.GetComponents<IVR_Controller>();
            hipControllers = hipTarget.GetComponents<IVR_Controller>();
            leftFootControllers = leftFootTarget.GetComponents<IVR_Controller>();
            rightFootControllers = rightFootTarget.GetComponents<IVR_Controller>();

            headController = FindActiveController(headControllers);
            leftHandController = FindActiveController(leftHandControllers);
            rightHandController = FindActiveController(rightHandControllers);
            hipController = FindActiveController(hipControllers);
            leftFootController = FindActiveController(leftFootControllers);
            rightFootController = FindActiveController(rightFootControllers);

            headMovements = headTarget.GetComponent<IVR_Movements>();

            leftHandMovements = leftHandTarget.GetComponent<IVR_HandMovementsBase>();
            if (leftHandMovements != null)
                leftHandMovements.selectedController = (IVR_HandController)FindActiveController(leftHandControllers);

            rightHandMovements = rightHandTarget.GetComponent<IVR_HandMovementsBase>();
            if (rightHandMovements != null)
                rightHandMovements.selectedController = (IVR_HandController)FindActiveController(rightHandControllers);

            DetermineAvatar();

            foreach (IVR_Controller c in headControllers)
                c.StartController(this);
            foreach (IVR_Controller c in leftHandControllers)
                c.StartController(this);
            foreach (IVR_Controller c in rightHandControllers)
                c.StartController(this);
            foreach (IVR_Controller c in hipControllers)
                c.StartController(this);
            foreach (IVR_Controller c in leftFootControllers)
                c.StartController(this);
            foreach (IVR_Controller c in rightFootControllers)
                c.StartController(this);

            bodyMovements = GetComponent<IVR_BodyMovements>();
            if (bodyMovements != null)
                bodyMovements.StartMovements();

            if (headMovements && headMovements.enabled)
                headMovements.StartMovements(this);
            if (leftHandMovements != null && leftHandMovements.enabled)
                leftHandMovements.StartMovements(this);
            if (rightHandMovements != null && rightHandMovements.enabled)
                rightHandMovements.StartMovements(this);

            InitGroundcheck();
        }

        private IVR_Controller FindActiveController(IVR_Controller[] controllers) {
            for (int i = 0; i < controllers.Length; i++) {
                if (controllers[i].isTracking())
                    return (controllers[i]);
            }
            return null;
        }

        #region Avatar
        private void DetermineAvatar() {
            Animator[] animators = GetComponentsInChildren<Animator>();
            for (int i = 0; i < animators.Length; i++) {
                Avatar avatar = animators[i].avatar;
                if (avatar.isValid && avatar.isHuman) {
                    characterTransform = animators[i].transform;

                    if (collisions) {
                        AddRigidbody(this.gameObject);
                        AddRigidbody(characterTransform.gameObject);
                    }
                }

            }

            avatarNeckHeight = GetAvatarNeckHeight();

            if (collisions)
                AddCharacterColliders(this, proximitySpeed);
        }

        private float GetAvatarNeckHeight() {
            if (characterTransform == null)
                return headTarget.transform.localPosition.y;
       
            Animator avatarRig = characterTransform.GetComponent<Animator>();
            Transform avatarNeck = avatarRig.GetBoneTransform(HumanBodyBones.Neck);
            if (avatarNeck != null)
                return (avatarNeck.position.y - avatarRig.transform.position.y);
            else
                return headTarget.transform.localPosition.y;
        }

        #endregion

        public void Update() {
            Controllers.Clear();
            UpdateExtensions();
            UpdateControllers();

            UpdateMovements();
        }

        public void LateUpdate() {
            LateUpdateExtensions();
            Controllers.EndFrame();
            inputDirection = Vector3.zero;
        }

        private void UpdateExtensions() {
            foreach (IVR_Extension extension in extensions)
                extension.UpdateExtension();
        }

        private void LateUpdateExtensions() {
            foreach (IVR_Extension extension in extensions)
                extension.LateUpdateExtension();
        }

        private void UpdateControllers() {
            if (leftHandMovements != null)
                leftHandMovements.selectedController = (IVR_HandController)UpdateController(leftHandControllers, leftHandMovements.selectedController);
            if (rightHandMovements != null)
                rightHandMovements.selectedController = (IVR_HandController)UpdateController(rightHandControllers, rightHandMovements.selectedController);

            hipController = UpdateController(hipControllers, hipController);
            leftFootController = UpdateController(leftFootControllers, leftFootController);
            rightFootController = UpdateController(rightFootControllers, rightFootController);
            // Head needs to be after hands because of traditional controller.
            headController = UpdateController(headControllers, headController);
        }

        private IVR_Controller UpdateController(IVR_Controller[] controllers, IVR_Controller lastActiveController) {
            if (controllers != null) {
                int lastIndex = 0, newIndex = 0;

                IVR_Controller activeController = null;
                for (int i = 0; i < controllers.Length; i++) {
                    if (controllers[i] != null) {
                        controllers[i].UpdateController();
                        if (activeController == null && controllers[i].isTracking()) {
                            activeController = controllers[i];
                            controllers[i].SetSelection(true);
                            newIndex = i;
                        } else
                            controllers[i].SetSelection(false);

                        if (controllers[i] == lastActiveController)
                            lastIndex = i;
                    }
                }

                if (lastIndex < newIndex && lastActiveController != null) { // we are degrading
                    activeController.TransferCalibration(lastActiveController);
                }

                return activeController;
            } else
                return null;
        }

        private void UpdateMovements() {
            if (characterTransform != null) {
                if (headMovements && headMovements.enabled)
                    headMovements.UpdateMovements();
                if (leftHandMovements && leftHandMovements.enabled)
                    leftHandMovements.UpdateMovements();
                if (rightHandMovements && rightHandMovements.enabled)
                    rightHandMovements.UpdateMovements();
                if (bodyMovements != null)
                    bodyMovements.UpdateBodyMovements();
            }

            CalculateSpeed();
            PlaceBodyCapsule();
            DetermineCollision();
            if (useGravity)
                groundCheck();
        }

        #region groundCheck
        //private float soleThickness;

        private void InitGroundcheck() {
            //float leftSoleThickness = leftFootTarget.position.y - transform.position.y;
            //float rightSoleThickness = rightFootTarget.position.y - transform.position.y;
            //soleThickness = Mathf.Min(leftSoleThickness, rightSoleThickness);
        }

        private void groundCheck() {
            if (!GrabbedStaticObject()) {
                Vector3 middleFootPosition = (leftFootTarget.transform.position + rightFootTarget.transform.position) / 2;
                Vector3 rayStart = new Vector3(middleFootPosition.x, transform.position.y + stepOffset, middleFootPosition.z);

                RaycastHit[] hits;
                hits = Physics.RaycastAll(rayStart, Vector3.down, stepOffset + 0.1F);
                for (int i = 0; i < hits.Length; i++) {
                    RaycastHit hit = hits[i];
                    if (hit.distance < stepOffset + 0.1F && !hit.collider.isTrigger && 
                        (hit.rigidbody == null ||
                        (hit.rigidbody != null && hit.rigidbody != leftHandMovements.handRigidbody && hit.rigidbody != rightHandMovements.handRigidbody))
                        ) {
                        gravitationalVelocity = Vector3.zero;
                        transform.position += Vector3.up * (stepOffset - hit.distance);
                        return;
                    }
                }
                Fall();
            }
        }

        Vector3 gravitationalVelocity;

        private void Fall() {
            gravitationalVelocity += Physics.gravity * Time.deltaTime;
            transform.Translate(gravitationalVelocity * Time.deltaTime);
        }

        #endregion

        private bool GrabbedStaticObject() {
            if (leftHandMovements != null &&
                leftHandMovements.grabbedObject != null &&
                leftHandMovements.grabbedObject.isStatic) {
                return true;
            } else
            if (rightHandMovements != null &&
                rightHandMovements.grabbedObject != null &&
                rightHandMovements.grabbedObject.isStatic) {
                return true;
            }            
            return false;
        }

        public void SetPlayerHeight(float height) {
            if (height <= 0)
                return;

            float neckHeight = 0.875F * height;
            ScaleTracking(avatarNeckHeight - neckHeight);
        }

        public void Calibrate() {
            foreach (Transform t in headTarget.parent) {
                t.gameObject.SendMessage("OnTargetReset", SendMessageOptions.DontRequireReceiver);
            }

            float neckHeight = headTarget.transform.position.y - transform.position.y;
            ScaleTracking(avatarNeckHeight - neckHeight);
        }

        private void ScaleTracking(float deltaY) {
            IVR_UnityVRHead unityVRhead = headTarget.GetComponent<IVR_UnityVRHead>();
            if (unityVRhead.cameraRoot != null)
                unityVRhead.extension.trackerPosition += new Vector3(0, deltaY, 0);
        }

        protected void AddRigidbody(GameObject gameObject) {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            if (rb != null) {
                rb.mass = 75;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        public float walkingSpeed = 1;
        public float rotationSpeed = 60;

        public bool proximitySpeed = true;
        [Range(0.1F, 1)]
        public float proximitySpeedRate = 0.8f;
        private const float proximitySpeedStep = 0.05f;

        [HideInInspector]
        private CapsuleCollider bodyCapsule;
        [HideInInspector]
        private CapsuleCollider bodyCollider;

        public void Move(float x, float y, float z) {
            Move(new Vector3(x, y, z));
        }

        public void Move(Vector3 translationVector) {
            Move(translationVector, false);
        }

        public void Move(Vector3 translationVector, bool allowUp) {
            translationVector = CheckMovement(translationVector);// does not support body pull
            if (translationVector.magnitude > 0) {
                //Vector3 translation = translationVector * Time.deltaTime;
                // Does not belong here
                if (allowUp) {
                    transform.position += translationVector;
                } else {
                    transform.position += new Vector3(translationVector.x, 0, translationVector.z);
                }
            }
        }

        public void Rotate(float angle) {
            transform.RotateAround(headTarget.position, Vector3.up, angle * rotationSpeed); // * Time.deltaTime); //Does not belong here
        }

        private float curProximitySpeed = 1;
        private Vector3 directionVector = Vector3.zero;

        public Vector3 CheckMovement(Vector3 inputMovement) {
            float maxAcceleration = 0;
            float sidewardSpeed = 0;
            float forwardSpeed = inputMovement.z;

            if (proximitySpeed)
                curProximitySpeed = CalculateProximitySpeed(bodyCapsule, curProximitySpeed);

            if (forwardSpeed != 0 || directionVector.z != 0) {
                if (inputMovement.z < 0)
                    forwardSpeed *= 0.6f;

                forwardSpeed *= walkingSpeed;

                if (proximitySpeed)
                    forwardSpeed *= curProximitySpeed;


                float acceleration = forwardSpeed - directionVector.z;
                maxAcceleration = 1f * Time.deltaTime;
                acceleration = Mathf.Clamp(acceleration, -maxAcceleration * 1, maxAcceleration);
                forwardSpeed = directionVector.z + acceleration;
            }

            sidewardSpeed = inputMovement.x;

            if (sidewardSpeed != 0 || directionVector.x != 0) {

                sidewardSpeed *= walkingSpeed;

                if (proximitySpeed)
                    sidewardSpeed *= curProximitySpeed;

                float acceleration = sidewardSpeed - directionVector.x;
                maxAcceleration = 1f * Time.deltaTime;
                acceleration = Mathf.Clamp(acceleration, -maxAcceleration, maxAcceleration);
                sidewardSpeed = directionVector.x + acceleration;
            }

            directionVector = new Vector3(sidewardSpeed, 0, forwardSpeed);
            Vector3 worldDirectionVector = hipTarget.TransformDirection(directionVector);
            inputDirection = worldDirectionVector;

            if (collisions && (collided || (!proximitySpeed && triggerEntered))) {
                float angle = Vector3.Angle(worldDirectionVector, hitNormal);
                if (angle > 90) {
                    directionVector = Vector3.zero;
                    worldDirectionVector = Vector3.zero;
                }
            }
            return worldDirectionVector;
        }

        [HideInInspector]
        public Vector3 velocity;
        private Vector3 lastRootPosition;

        private void CalculateSpeed() {
            if (lastRootPosition.magnitude > 0) {
                Vector3 deltaHip = hipTarget.position - lastRootPosition;
                velocity = new Vector3(deltaHip.x, 0, deltaHip.z) / Time.deltaTime;
            }
            lastRootPosition = hipTarget.position;
        }

        private float CalculateProximitySpeed(CapsuleCollider cc, float curProximitySpeed) {
            float proximitySpeedStep = 0.05F / proximitySpeedRate;

            if (triggerEntered) {
                if (cc.radius > 0.25f) {
                    // Do we need to decrease the proximity speed?
                    cc.radius -= proximitySpeedStep;
                    cc.height += proximitySpeedStep;
                    curProximitySpeed = GetProximitySpeed(cc);
                }
            } else if (curProximitySpeed < 1) {
                // Can we increase the proximity speed?
#if UNITY_5_3
                Vector3 extents = new Vector3(cc.radius + proximitySpeedStep, (cc.height - proximitySpeedStep) / 2, cc.radius + proximitySpeedStep);
                Collider[] results = Physics.OverlapBox(hipTarget.position + cc.center, extents);
#else
                Vector3 capsuleCenter = transform.position + cc.center;
                Vector3 offset = ((cc.height - cc.radius) / 2) * Vector3.up;
                Vector3 point1 = capsuleCenter + offset;
                Vector3 point2 = capsuleCenter - offset;
                Collider[] results = Physics.OverlapCapsule(point1, point2, cc.radius + proximitySpeedStep);
#endif
                bool staticCollision = false;
                Rigidbody ivrRigidbody = transform.GetComponent<Rigidbody>();
                Rigidbody characterRigidbody = characterTransform.GetComponent<Rigidbody>();
                for (int i = 0; i < results.Length; i++) {
                    if (!results[i].isTrigger && results[i].attachedRigidbody != characterRigidbody &&
                        results[i].attachedRigidbody != leftHandMovements.handRigidbody && results[i].attachedRigidbody != rightHandMovements.handRigidbody &&
                        results[i].attachedRigidbody != ivrRigidbody) {

                        staticCollision = true;
                    }
                }

                if (!staticCollision) {
                    cc.radius += proximitySpeedStep;
                    cc.height -= proximitySpeedStep;
                    curProximitySpeed = GetProximitySpeed(cc);
                }
            }

            return curProximitySpeed;
        }

        private static float GetProximitySpeed(CapsuleCollider cc) {
            return EaseIn(1, (-0.80f), 1 - cc.radius, 0.75f);
        }

        private static float EaseIn(float start, float distance, float elapsedTime, float duration) {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            //return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
            return distance * elapsedTime * elapsedTime + start;
        }

        private float colliderRadius = 0.4F;

        public void AddCharacterColliders(InstantVR ivr, bool proximitySpeed) {
            // HMM. this should be done by AddRigidbody on the characterTransform....
            //Rigidbody rb = ivr.gameObject.AddComponent<Rigidbody>();
            //if (rb != null) {
            //    rb.mass = 1;
            //    rb.useGravity = false;
            //    rb.isKinematic = true;
            //}

            float avatarHeight = (ivr.headTarget.position.y - ivr.transform.position.y) + 0.3F;
            Vector3 colliderCenter = Vector3.up * (avatarHeight + ivr.stepOffset) / 2;
            
            ivr.bodyCollider = ivr.gameObject.AddComponent<CapsuleCollider>();
            if (ivr.bodyCollider != null) {
                ivr.bodyCollider.isTrigger = false;
                ivr.bodyCollider.height = avatarHeight - ivr.stepOffset;
                ivr.bodyCollider.radius = colliderRadius - 0.05F;
                ivr.bodyCollider.center = colliderCenter;
            }

            ivr.bodyCapsule = ivr.gameObject.AddComponent<CapsuleCollider>();
            if (ivr.bodyCapsule != null) {
                ivr.bodyCapsule.isTrigger = true;
                if (proximitySpeed) {
                    ivr.bodyCapsule.height = 0.80F;
                    ivr.bodyCapsule.radius = 1F;
                } else {
                    ivr.bodyCapsule.height = avatarHeight - ivr.stepOffset;
                    ivr.bodyCapsule.radius = colliderRadius;
                }
                ivr.bodyCapsule.center = colliderCenter;
            }
        }

        private void PlaceBodyCapsule() {
            if (collisions) {
                Vector3 colliderPosition = Quaternion.Inverse(transform.rotation) * (hipTarget.position - transform.position);
                if (bodyCapsule != null)
                    bodyCapsule.center = new Vector3(colliderPosition.x, bodyCapsule.center.y, colliderPosition.z);
                bodyCollider.center = new Vector3(colliderPosition.x, bodyCollider.center.y, colliderPosition.z);
            }
        }

        private void DetermineCollision() {
            if (proximitySpeed) {
                if (triggerEntered && bodyCapsule.radius <= 0.25F) {
                    collided = true;
                    if (velocity.sqrMagnitude > 0.01F) {
                        hitNormal = DetermineHitNormal(velocity);
                        Debug.DrawRay(transform.position, hitNormal);
                    }
                } else if (collided && bodyCapsule.radius > 0.25F) {
                    collided = false;
                }
            } else {
                if (triggerEntered) {
                    collided = true;
                    if (velocity.sqrMagnitude > 0.01F)
                        hitNormal = DetermineHitNormal(velocity);
                } else if (collided) {
                    collided = false;
                }
            }
        }

        private Vector3 DetermineHitNormal(Vector3 inputDirection) {
            CapsuleCollider cc = bodyCapsule;
            Vector3 capsuleCenter = transform.position + cc.center;
            Vector3 capsuleOffset = ((cc.height - cc.radius) / 2) * Vector3.up;

            Vector3 backSweep = inputDirection.normalized * (cc.radius + 0.1F);
            Vector3 top = capsuleCenter + capsuleOffset - backSweep;
            Vector3 bottom = capsuleCenter - capsuleOffset - backSweep;

            Vector3 hitNormal;
            if (CapsulecastAllNormal(top, bottom, cc.radius, inputDirection.normalized, inputDirection.magnitude * Time.deltaTime + cc.radius + 0.1F, out hitNormal))
                return hitNormal;

            return -inputDirection.normalized;
        }

        private bool CapsulecastAllNormal(Vector3 top, Vector3 bottom, float radius, Vector3 direction, float maxDistance, out Vector3 hitNormal) {
            RaycastHit[] hits = Physics.CapsuleCastAll(top, bottom, radius, direction, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            hitNormal = Vector3.zero;
            for (int i = 0; i < hits.Length; i++) {
                if (!hits[i].collider.isTrigger && hits[i].point.sqrMagnitude > 0) {
                    hitNormal = hits[i].normal;
                    return true;
                }
            }
            return false;
        }

        public void OnTriggerStay(Collider otherCollider) {
            Rigidbody rigidbody = otherCollider.attachedRigidbody;
            if (!otherCollider.isTrigger &&
                rigidbody != gameObject.GetComponent<Rigidbody>()
                && rigidbody != leftHandMovements.handRigidbody && rigidbody != rightHandMovements.handRigidbody
                ) {
                triggerEntered = true;
            }
        }

        public void OnTriggerExit() {
            triggerEntered = false;
        }
    }
}