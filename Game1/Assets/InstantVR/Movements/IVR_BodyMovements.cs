/* Instant Body Movements
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.7.0
 * date: July 25, 2016
 *
 * - Implemented stretchlesstargets
 */

using UnityEngine;

namespace IVR {

    [ExecuteInEditMode]
    public class IVR_BodyMovements : MonoBehaviour {
        [HideInInspector]
        private InstantVR ivr;

        private Transform headTarget;
        [HideInInspector]
        public Transform rightHandTarget, leftHandTarget;
        private Transform hipTarget;
        private Transform rightFootTarget, leftFootTarget;

        [HideInInspector]
        private Transform rightHandOTarget, leftHandOTarget;

        [Tooltip("Are upper body movements allowed?")]
        public bool enableTorso = true;
        [Tooltip("Are leg movements allowed?")]
        public bool enableLegs = true;

        [HideInInspector]
        private Animator animator = null;
        [HideInInspector]
        public ArmMovements leftArm, rightArm;
        private Torso torso;
        private LegMovements leftLeg, rightLeg;

        [HideInInspector]
        private Rigidbody characterRigidbody;
        [HideInInspector]
        private Transform characterTransform;


        public static float maxHipAngle = 70;

        private bool fromNormCalculated = false;

        public void SetRightHandTarget(Transform newTarget) {
            rightHandOTarget = newTarget;
            rightHandTarget = newTarget;
        }

        public void SetLeftHandTarget(Transform newTarget) {
            leftHandOTarget = newTarget;
            leftHandTarget = newTarget;
        }

        public void StartMovements() {
            ivr = this.GetComponent<InstantVR>();

            headTarget = ivr.headTarget;
            leftHandTarget = ivr.leftHandTarget;
            rightHandTarget = ivr.rightHandTarget;
            hipTarget = ivr.hipTarget;
            leftFootTarget = ivr.leftFootTarget;
            rightFootTarget = ivr.rightFootTarget;

            animator = ivr.transform.GetComponentInChildren<Animator>();
            if (animator == null) {
                StopMovements();
            } else {

                characterRigidbody = animator.GetComponent<Rigidbody>();

                if (characterRigidbody != null)
                    characterTransform = characterRigidbody.GetComponent<Transform>();

                if (leftArm == null)
                    leftArm = new ArmMovements(ivr, ArmMovements.BodySide.Left, this);
                leftArm.Initialize(ivr, ArmMovements.BodySide.Left, this);

                if (rightArm == null)
                    rightArm = new ArmMovements(ivr, ArmMovements.BodySide.Right, this);
                rightArm.Initialize(ivr, ArmMovements.BodySide.Right, this);

                Transform neck, spine, hips;

                neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                if (neck == null) {
                    neck = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (neck == null) {
                        StopMovements();
                        return;
                    }
                }
                spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                torso = new Torso(neck, spine, hips, hipTarget, headTarget, rightArm);

                leftLeg = new LegMovements(ArmMovements.BodySide.Left, animator, hipTarget);
                rightLeg = new LegMovements(ArmMovements.BodySide.Right, animator, hipTarget);

                if (rightHandTarget != null) {
                    rightHandOTarget = rightHandTarget;
                }

                if (leftHandTarget != null) {
                    leftHandOTarget = leftHandTarget;
                }

                if (headTarget == null && enableTorso) {
                    GameObject neckTargetGO = new GameObject("Neck_Target");
                    headTarget = neckTargetGO.transform;
                    headTarget.parent = characterTransform;
                    headTarget.position = torso.neck.position;
                    headTarget.rotation = characterTransform.rotation;
                }

                if (enableLegs) {
                    if (rightFootTarget == null) {
                        GameObject rightFootTargetGO = new GameObject("Foot_R_Target");
                        rightFootTarget = rightFootTargetGO.transform;
                        rightFootTarget.parent = characterTransform;
                        rightFootTarget.position = rightLeg.foot.position;
                        rightFootTarget.rotation = characterTransform.rotation;
                    }

                    if (leftFootTarget == null) {
                        GameObject leftFootTargetGO = new GameObject("Foot_L_Target");
                        leftFootTarget = leftFootTargetGO.transform;
                        leftFootTarget.parent = characterTransform;
                        leftFootTarget.position = leftLeg.foot.position;
                        leftFootTarget.rotation = characterTransform.rotation;
                    }
                }


                if (IsInTPose(leftArm, rightArm)) {
                    CalculateFromNormTPose(leftArm, rightArm);
                } else {
                    CalculateFromNormTracking(leftArm, rightArm, leftHandTarget, rightHandTarget);
                }

                if (leftLeg.IsInTPose())
                    leftLeg.FromNormTPose();
                else
                    leftLeg.FromNormTracking(leftFootTarget);

                if (rightLeg.IsInTPose())
                    rightLeg.FromNormTPose();
                else
                    rightLeg.FromNormTracking(rightFootTarget);
            }
        }

        public void StopMovements() {
            fromNormCalculated = false;
        }

        bool crouching = false;
        float bendAngle = 0;
        
        void Update() {
            if (!Application.isPlaying) {
                if (!fromNormCalculated || torso == null || leftArm == null || rightArm == null)
                    StartMovements();

                if (animator == null)
                    StopMovements();
                else
                    UpdateBodyMovements();
            }
        }
        
        public void UpdateBodyMovements() {
            if (torso.userNeckTarget) {
                if (enableTorso)
                    torso.CalculateHorizontal(headTarget);

                if (enableLegs)
                    torso.CalculateVertical(headTarget);

                leftArm.Calculate(leftHandTarget);
                rightArm.Calculate(rightHandTarget);

                CalculateHeadOrientation(torso.neck, headTarget);
            } else {
                if (bendAngle <= 0 && !crouching) {
                    leftArm.Calculate(leftHandTarget);
                    rightArm.Calculate(rightHandTarget);
                }

                if (enableTorso && !crouching)
                    bendAngle = torso.AutoHorizontal(rightHandOTarget, rightHandTarget, rightArm, leftHandOTarget, leftHandTarget, leftArm, headTarget);

                if (enableLegs && bendAngle >= maxHipAngle)
                    crouching = torso.AutoVertical(rightHandOTarget, rightHandTarget, rightArm, leftHandOTarget, leftHandTarget, leftArm, headTarget);
            }

            if (enableLegs) {
                rightLeg.Calculate(rightFootTarget.transform);
                leftLeg.Calculate(leftFootTarget.transform);
            }
        }

        private Vector3 minHeadAngles = new Vector3(-60, -90, -45);
        private Vector3 maxHeadAngles = new Vector3(70, 90, 45);

        void CalculateHeadOrientation(Transform neck, Transform neckTarget) {
            Vector3 localHeadAngles = (Quaternion.Inverse(hipTarget.rotation) * neckTarget.rotation).eulerAngles;
            localHeadAngles = Angles.ClampVector3(localHeadAngles, minHeadAngles, maxHeadAngles);
            neck.rotation = hipTarget.rotation * Quaternion.Euler(localHeadAngles) * torso.fromNormNeck;
        }

        public bool IsInTPose(ArmMovements leftArm, ArmMovements rightArm) {
            float d;
            Ray hand2hand = new Ray(leftArm.hand.position, rightArm.hand.position - leftArm.hand.position);

            // All lined up?
            d = DistanceToRay(hand2hand, leftArm.forearm.position);
            if (d > 0.1f) return false;

            d = DistanceToRay(hand2hand, leftArm.upperArm.position);
            if (d > 0.1f) return false;

            d = DistanceToRay(hand2hand, rightArm.upperArm.position);
            if (d > 0.1f) return false;

            d = DistanceToRay(hand2hand, rightArm.forearm.position);
            if (d > 0.1f) return false;

            // Arms stretched?
            d = Vector3.Distance(leftArm.upperArm.position, leftArm.hand.position);
            if (d < leftArm.length - 0.1f) return false;

            d = Vector3.Distance(rightArm.upperArm.position, rightArm.hand.position);
            if (d < rightArm.length - 0.1f) return false;

            Debug.Log("Arms are in T-pose");
            return true;
        }

        public static float DistanceToRay(Ray ray, Vector3 point) {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }

        public void CalculateFromNormTPose(ArmMovements leftArm, ArmMovements rightArm) {
            if (leftArm != null) {
                leftArm.CalculateFromNormTPose();
            }

            if (rightArm != null) {
                rightArm.CalculateFromNormTPose();
            }
            fromNormCalculated = true;
        }

        public void CalculateFromNormTracking(ArmMovements leftArm, ArmMovements rightArm, Transform leftHandTarget, Transform rightHandTarget) {
            if (leftArm != null) {
                leftArm.CalculateFromNormTracking(leftHandTarget);
            }

            if (rightArm != null) {
                rightArm.CalculateFromNormTracking(rightHandTarget);
            }
            fromNormCalculated = true;
        }
    }

    [System.Serializable]
    public class Torso {
        private Transform hipTarget;

        public Transform neck;
        public Transform spine;
        public Transform hips;

        public Quaternion fromNormNeck;
        public Quaternion fromNormTorso;
        public Quaternion fromNormHips;

        public float length;

        private Vector3 neckStartPosition;
        private Vector3 hipStartPosition;
        private Vector3 spineStartOrientation;
        public Quaternion spineStartRotation;

        public bool userNeckTarget;
        private Vector3 spineAxis;

        public Torso(Transform neck, Transform spine, Transform hips, Transform hipTarget, Transform neckTarget, ArmMovements arm) {
            this.hipTarget = hipTarget;

            this.neck = neck;
            neckStartPosition = neck.position;

            this.spine = spine;
            spineStartRotation = spine.rotation;
            spineStartOrientation = neck.position - spine.position;

            this.hips = hips;
            hipStartPosition = hips.position;

            Vector3 neckAtUpperArm = new Vector3(neck.position.x, arm.upperArm.position.y, neck.position.z);
            length = Vector3.Distance(spine.position, neckAtUpperArm);

            if (neckTarget != null) {
                fromNormNeck = Quaternion.Inverse(Quaternion.LookRotation(neckTarget.forward)) * neck.rotation;
                fromNormTorso = Quaternion.Inverse(Quaternion.LookRotation(neck.position - spine.position, hipTarget.forward)) * spine.rotation;
                fromNormHips = Quaternion.Inverse(Quaternion.LookRotation(hipTarget.forward)) * hips.rotation;
            }
            userNeckTarget = (neckTarget != null);
            if (hipTarget != null)
                spineAxis = spine.InverseTransformDirection(hipTarget.right);
        }

        public void CalculateHorizontal(Transform neckTarget) {
            if (hipTarget.gameObject.activeSelf)
                hips.position = new Vector3(hipTarget.position.x, hips.position.y, hipTarget.position.z);
            hips.rotation = Quaternion.LookRotation(hipTarget.forward, Vector3.up) * fromNormHips;

            spine.LookAt(neckTarget.transform, hipTarget.forward);
            spine.rotation *= fromNormTorso;
        }

        public float AutoHorizontal(Transform rightHandOTarget, Transform rightHandTarget, ArmMovements rightArm, Transform leftHandOTarget, Transform leftHandTarget, ArmMovements leftArm, Transform neckTarget) {
            float bendAngle = 0;
            Vector3 torsoTarget = Vector3.zero;
            Vector3 dShoulderNeck = (leftArm.upperArm.position - rightArm.upperArm.position) / 2;

            Vector3 leftToTarget = leftHandOTarget.position - leftArm.upperArmStartPosition;
            Vector3 rightToTarget = rightHandOTarget.position - rightArm.upperArmStartPosition;
            float leftOver = leftToTarget.magnitude - leftArm.length;
            float rightOver = rightToTarget.magnitude - rightArm.length;

            if (leftOver > 0) {
                if (rightOver > 0) {
                    Vector3 torsoTargetR = rightHandOTarget.position + dShoulderNeck;
                    Vector3 torsoTargetL = leftHandOTarget.position - dShoulderNeck;
                    float bendAngleR = BendAngle(torsoTargetR, rightArm);
                    float bendAngleL = BendAngle(torsoTargetL, leftArm);

                    if (bendAngleR > bendAngleL)
                        torsoTarget = torsoTargetR;
                    else
                        torsoTarget = torsoTargetL;
                } else
                    torsoTarget = leftHandOTarget.position - dShoulderNeck;
            } else if (rightOver > 0)
                torsoTarget = rightHandOTarget.position + dShoulderNeck;

            if (rightOver > 0 || leftOver > 0) {
                bendAngle = BendAngle(torsoTarget, rightArm); // arm should be left or right
                spine.rotation = spineStartRotation * Quaternion.AngleAxis(bendAngle, spineAxis);
            } else
                spine.rotation = spineStartRotation;

            rightArm.Calculate(rightHandTarget);
            leftArm.Calculate(leftHandTarget);

            return bendAngle;
        }

        public float BendAngle(Vector3 torsoTarget, ArmMovements arm) {
            float baseAngle = Vector3.Angle(spineStartOrientation, torsoTarget - spine.position);

            float dSpine2Target = Vector3.Distance(spine.position, torsoTarget);
            float spineAngle = Mathf.Acos((dSpine2Target * dSpine2Target + length * length - arm.length * arm.length) / (2 * dSpine2Target * length)) * Mathf.Rad2Deg;
            if (float.IsNaN(spineAngle)) spineAngle = 0;

            return Mathf.Min(baseAngle - spineAngle, IVR_BodyMovements.maxHipAngle);
        }

        public void CalculateVertical(Transform neckTarget) {
            float dY = hipTarget.position.y - hipStartPosition.y;
            hips.position = new Vector3(hips.position.x, hipStartPosition.y + dY, hips.position.z);
        }

        public bool AutoVertical(Transform rightHandOTarget, Transform rightHandTarget, ArmMovements rightArm, Transform leftHandOTarget, Transform leftHandTarget, ArmMovements leftArm, Transform neckTarget) {
            Vector3 neckDelta = Vector3.zero;

            Vector3 leftToTarget = leftHandOTarget.position - leftArm.upperArmStartPosition;
            Vector3 rightToTarget = rightHandOTarget.position - rightArm.upperArmStartPosition;
            float leftOver = leftToTarget.magnitude - leftArm.length;
            float rightOver = rightToTarget.magnitude - rightArm.length;
            if (leftOver > 0) {
                if (rightOver > leftOver)
                    neckDelta = rightToTarget.normalized * rightOver;
                else
                    neckDelta = leftToTarget.normalized * leftOver;
            } else if (rightOver > 0)
                neckDelta = rightToTarget.normalized * rightOver;

            neckTarget.position = neckStartPosition + new Vector3(0, neckDelta.y, 0);

            float dY = neckTarget.transform.position.y - neck.position.y;
            if (hips.position.y + dY < hipStartPosition.y) {
                hips.Translate(0, dY, 0, Space.World);
            } else if (hips.position.y + dY > hipStartPosition.y) {
                hips.position = new Vector3(hips.position.x, hipStartPosition.y, hips.position.z);
            }

            rightArm.Calculate(rightHandTarget);
            leftArm.Calculate(leftHandTarget);

            return (hips.position.y < hipStartPosition.y);
        }
    }
    public abstract class Digit {
        public abstract void Init(Transform hand, Vector3 handRightAxis, Vector3 axis2, bool bodySideLeft, int fingerIndex);

        public abstract void Update(float inputValue);
    }

    [System.Serializable]
    public class Finger : Digit {
        public Transform transform;
        private int fingerIndex;
        public float input;
        private float val;
        private Vector3 curlAxis, swingAxis;
        private float grabAmount = 0;
        public int nPhalanges;
        private Phalanx[] phalanges;

        public override void Init(Transform hand, Vector3 fingerAxis, Vector3 armAxis, bool bodySideLeft, int fingerIndex) {
            if (transform != null) {
                this.fingerIndex = fingerIndex;
                curlAxis = fingerAxis;
                swingAxis = Vector3.Cross(fingerAxis, armAxis);
                if (bodySideLeft)
                    swingAxis = -swingAxis;

                if (phalanges == null || phalanges.Length == 0)
                    phalanges = new Phalanx[3];

                if (phalanges[0] == null || phalanges[0].transform == null) {
                    phalanges[0] = new Phalanx(transform);
                    nPhalanges = 1;
                }

                if ((phalanges[1] == null || phalanges[1].transform == null) && phalanges[0].transform.childCount == 1) {
                    phalanges[1] = new Phalanx(phalanges[0].transform.GetChild(0).transform);
                    nPhalanges = 2;
                }

                if ((phalanges[2] == null || phalanges[2].transform == null) && phalanges[1].transform.childCount == 1) {
                    phalanges[2] = new Phalanx(phalanges[1].transform.GetChild(0).transform);
                    nPhalanges = 3;
                }
            }
        }

        private float grabSpeed = 0.4f;
        public override void Update(float inputValue) {
            if (transform != null) {
                input = inputValue;
                if (grabAmount > 0)
                    val = grabAmount;
                else {
                    float d = input - val;
                    if (d > grabSpeed)
                        val += grabSpeed;
                    else if (d < -grabSpeed)
                        val -= grabSpeed;
                    else
                        val += d;
                }
                float val2 = Mathf.Max(0, 0.1f - val) * 20;
                val2 *= (fingerIndex - 2);
                Bend(val, val2);

                if (grabAmount > 0) {
                    if (input < grabAmount) {
                        // drop the object
                        grabAmount = 0f;
                    }
                }
            }
        }

        private void Bend(float val1, float val2) {
            if (phalanges != null) {
                switch (nPhalanges) {
                    case 3:
                        phalanges[0].Bend(val1 * 45, curlAxis, val2, -swingAxis);
                        phalanges[1].Bend(val1 * 90, curlAxis, val2, -swingAxis);
                        phalanges[2].Bend(val1 * 90, curlAxis, val2, -swingAxis);
                        break;
                    case 2:
                        phalanges[0].Bend(val1 * 90, curlAxis, val2, -swingAxis);
                        phalanges[1].Bend(val1 * 90, curlAxis, val2, -swingAxis);
                        break;
                    case 1:
                        phalanges[0].Bend(val1 * 135, curlAxis, val2, -swingAxis);
                        break;
                }
            }
        }

        public bool hasGrabbed() {
            return (grabAmount > 0);
        }

        public void Grab() {
            grabAmount = val;
        }

        public void GrabAt(float amount) {
            grabAmount = amount;
        }

        public void UnGrab() {
            grabAmount = 0;
        }
    }

    [System.Serializable]
    public class Thumb : Digit {
        public Transform transform;
        public float input;
        public Transform characterTransform;
        private float curlValue, grabAmount = 0;
        private Vector3 swingAxis, curlAxis;

        private Phalanx phalanx0;
        private Phalanx phalanx1;
        private Phalanx phalanx2;

        public override void Init(Transform hand, Vector3 fingerBendAxis, Vector3 forearmAxis, bool bodySideLeft, int fingerIndex) {
            if (transform != null) {
                curlAxis = fingerBendAxis;
                if (bodySideLeft) {
                    swingAxis = -Vector3.Cross(fingerBendAxis, forearmAxis);
                } else {
                    swingAxis = Vector3.Cross(fingerBendAxis, forearmAxis);
                }

                if (phalanx0 == null || phalanx0.transform == null) {
                    phalanx0 = new Phalanx(transform);
                }
                if ((phalanx1 == null || phalanx1.transform == null) && phalanx0.transform.childCount == 1) {
                    phalanx1 = new Phalanx(phalanx0.transform.GetChild(0).transform);
                }
                if ((phalanx2 == null || phalanx2.transform == null) && phalanx1.transform.childCount == 1) {
                    phalanx2 = new Phalanx(phalanx1.transform.GetChild(0).transform);
                }
            }
        }

        private float grabSpeed = 0.4f;
        public override void Update(float inputValue) {
            if (phalanx1 != null) {
                if (transform != null) {
                    input = inputValue;
                    if (grabAmount > 0)
                        curlValue = grabAmount;
                    else {
                        float d = input - curlValue;
                        if (d > grabSpeed)
                            curlValue += grabSpeed;
                        else if (d < -grabSpeed)
                            curlValue -= grabSpeed;
                        else
                            curlValue += d;
                    }

                    if (phalanx0 != null)
                        phalanx0.Bend(curlValue * 20, curlAxis, 0, swingAxis);
                    if (phalanx1 != null)
                        phalanx1.Bend(curlValue * 40, curlAxis, 0, swingAxis);
                    if (phalanx2 != null)
                        phalanx2.Bend(curlValue * 80, curlAxis, 0, swingAxis);
                }
            }
        }

        private void DrawAxis(Vector3 position, Quaternion rotation) {
            Debug.DrawRay(position, rotation * Vector3.right * 0.1F, Color.red);
            Debug.DrawRay(position, rotation * Vector3.up * 0.1F, Color.green);
            Debug.DrawRay(position, rotation * Vector3.forward * 0.1F, Color.blue);
        }
    }

    [System.Serializable]
    public class Phalanx {
        public Transform transform;
        public Quaternion z;

        public Phalanx(Transform newTransform) {
            transform = newTransform;
            z = transform.localRotation;
        }

        public void Bend(float bendFactor1, Vector3 axis1, float bendFactor2, Vector3 axis2) {
            transform.localRotation = z * Quaternion.AngleAxis(bendFactor1, axis1) * Quaternion.AngleAxis(bendFactor2, axis2);

        }
    }

    [System.Serializable]
    public class ArmMovements {
        private InstantVR ivr;

        private Animator animator;
        public Transform upperArm;
        public Transform forearm;
        public Transform hand;
#if INSTANTVR_ADVANCED
        private IVR_HandMovementsBase handMovements;
#endif

        public float length;
        private float upperArmLength, forearmLength;
        [HideInInspector]
        private float upperArmLength2, forearmLength2;

        [HideInInspector]
        public Vector3 upperArmStartPosition;

        public enum BodySide {
            Left,
            Right
        };
        private BodySide bodySide;

        [HideInInspector]
        public Quaternion fromNormUpperArm = Quaternion.identity;
        [HideInInspector]
        public Quaternion fromNormForearm = Quaternion.identity;
        [HideInInspector]
        public Quaternion fromNormHand = Quaternion.identity;

        public ArmMovements(InstantVR ivr, BodySide bodySide, IVR_BodyMovements bodyMovements) {
            Initialize(ivr, bodySide, bodyMovements);
        }

        public void Initialize(InstantVR ivr, BodySide bodySide, IVR_BodyMovements bodyMovements) {
            this.ivr = ivr;

            this.bodySide = bodySide;
            animator = ivr.GetComponentInChildren<Animator>();

            if (bodySide == BodySide.Left) {
                upperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                forearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                hand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

#if INSTANTVR_ADVANCED
                handMovements = ivr.leftHandMovements;
#endif
            } else {
                upperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                forearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                hand = animator.GetBoneTransform(HumanBodyBones.RightHand);

#if INSTANTVR_ADVANCED
                handMovements = ivr.rightHandMovements;
#endif
            }

            if (upperArm != null) {
                upperArmLength = Vector3.Distance(upperArm.position, forearm.position);
                forearmLength = Vector3.Distance(forearm.position, hand.position);
                length = upperArmLength + forearmLength;
                if (length == 0)
                    Debug.LogError("Avatar arm positions are incorrect. Please restore avatar in T-pose.");

                upperArmLength2 = upperArmLength * upperArmLength;
                forearmLength2 = forearmLength * forearmLength;

                upperArmStartPosition = upperArm.position;
            }
        }

        public void CalculateFromNormTPose() {
            fromNormUpperArm = Quaternion.Inverse(Quaternion.LookRotation(forearm.position - upperArm.position)) * upperArm.rotation;
            fromNormForearm = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * forearm.rotation;
            fromNormHand = Quaternion.Inverse(Quaternion.LookRotation(hand.position - forearm.position)) * hand.rotation;
        }

        public void CalculateFromNormTracking(Transform handTarget) {
            fromNormUpperArm = Quaternion.Inverse(CalculateUpperArmNorm(handTarget)) * upperArm.rotation;
            fromNormForearm = Quaternion.Inverse(CalculateForearmNorm(handTarget)) * forearm.rotation;
            fromNormHand = Quaternion.Inverse(handTarget.rotation) * hand.rotation;
        }

        Quaternion upperArmNormRotation;

        protected Quaternion CalculateUpperArmNorm(Transform handTarget) {
            float dShoulderTarget = Vector3.Distance(upperArm.position, handTarget.position);
            float shoulderAngle = Mathf.Acos((dShoulderTarget * dShoulderTarget + upperArmLength2 - forearmLength2) / (2 * dShoulderTarget * upperArmLength)) * Mathf.Rad2Deg;
            if (float.IsNaN(shoulderAngle)) shoulderAngle = 0;

            Vector3 upperArmUp = ivr.hipTarget.InverseTransformDirection(handTarget.up);
            if (bodySide == BodySide.Left)
                upperArmUp = (upperArmUp + (Vector3.left + Vector3.up).normalized) / 2;
            else
                upperArmUp = (upperArmUp + (Vector3.right + Vector3.up).normalized) / 2;

            float upperArmUpX = upperArmUp.x;
            float upperArmUpY = upperArmUp.y;
            float upperArmUpZ = upperArmUp.z;

            Vector3 dHandUpper = ivr.hipTarget.InverseTransformDirection(handTarget.position - upperArm.position);

            if (bodySide == BodySide.Right) {
                if (dHandUpper.x < 0)
                    upperArmUpZ -= dHandUpper.x * 10;
                upperArmUpX = Mathf.Clamp(upperArmUpX, 0.3F, 1);
            } else {
                if (dHandUpper.x > 0)
                    upperArmUpZ += dHandUpper.x * 10;
                upperArmUpX = Mathf.Clamp(upperArmUpX, -0.3F, 1);

                shoulderAngle = -shoulderAngle;
            }
            if (dHandUpper.y > 0)
                upperArmUpY += dHandUpper.y * 10;

            upperArmUpY = Mathf.Clamp(upperArmUpY, 0.01F, 1);
            upperArmUpZ = Mathf.Clamp(upperArmUpZ, -0.1F, 0.1F);
            upperArmUp = ivr.hipTarget.TransformDirection(new Vector3(upperArmUp.x, upperArmUpY, upperArmUpZ));

            upperArmNormRotation = Quaternion.LookRotation(handTarget.position - upperArm.position, upperArmUp);
            upperArmNormRotation = Quaternion.AngleAxis(shoulderAngle, upperArmNormRotation * Vector3.up) * upperArmNormRotation;
#if IVR_DEBUG
            Debug.DrawRay(upperArm.position, upperArmNormRotation * Vector3.forward * upperArmLength, Color.blue);
            Passer.Monitor.DrawAxisYZ(upperArm.position, upperArmNormRotation);
#endif
            return upperArmNormRotation;
        }

        protected Quaternion CalculateForearmNorm(Transform handTarget) {
            Vector3 forearmUp = (handTarget.up + upperArmNormRotation * Vector3.up) / 2;
            Quaternion forearmNormRotation = Quaternion.LookRotation(handTarget.position - forearm.position, forearmUp);
#if IVR_DEBUG
            Debug.DrawRay(forearm.position, forearmNormRotation * Vector3.forward * forearmLength, Color.blue);
            Passer.Monitor.DrawAxisYZ(forearm.position, forearmNormRotation);
#endif
            return forearmNormRotation;
        }

        protected Quaternion CalculateHandNorm(Transform handTarget) {
            Quaternion handNormRotation = Quaternion.LookRotation(handTarget.position - forearm.position, handTarget.up);

            return handNormRotation;
        }

        public void Calculate(Transform handTarget) {
            if (handTarget != null) {
                upperArm.rotation = CalculateUpperArmNorm(handTarget) * fromNormUpperArm;
                forearm.rotation = CalculateForearmNorm(handTarget) * fromNormForearm;
#if INSTANTVR_ADVANCED

                if (handMovements != null && handMovements.stretchlessTarget != null) {
                    Vector3 armDirection = handMovements.transform.position - upperArm.position; // handTarget.position creates instability
                    float distance = armDirection.magnitude;
                    // 0.01F is to get the arm IK stable at full stretch
                    if (distance > length - 0.01F) {
                        handMovements.stretchlessTarget.position = upperArm.position + armDirection.normalized * (length - 0.01F);
                    } else {
                        handMovements.stretchlessTarget.localPosition = Vector3.zero;
                    }
                }
#endif
                hand.rotation = handTarget.rotation * fromNormHand;
            }
        }
    }

    [System.Serializable]
    public class LegMovements {
        private Transform characterTransform;
        public Transform upperLeg;
        public Transform lowerLeg;
        public Transform foot;

        private Quaternion fromNormUpperLeg;
        private Quaternion fromNormLowerLeg;
        private Quaternion fromNormFoot;

        private float upperLegLength, lowerLegLength;
        private float upperLegLength2, lowerLegLength2;

        public LegMovements(ArmMovements.BodySide bodySide_in, Animator animator, Transform characterTransform_in) {
            characterTransform = characterTransform_in;

            if (bodySide_in == ArmMovements.BodySide.Left) {
                upperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                lowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            } else {
                upperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                lowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                foot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            }

            upperLegLength = Vector3.Distance(upperLeg.position, lowerLeg.position);
            lowerLegLength = Vector3.Distance(lowerLeg.position, foot.position);

            upperLegLength2 = upperLegLength * upperLegLength;
            lowerLegLength2 = lowerLegLength * lowerLegLength;

            FromNormTPose();
        }

        public bool IsInTPose() {
            float d;
            Ray hip2foot = new Ray(upperLeg.position, foot.position - upperLeg.position);

            // All lined up?
            d = IVR_BodyMovements.DistanceToRay(hip2foot, lowerLeg.position);
            if (d > 0.1f) return false;

            // All vertical?
            if (Mathf.Abs(hip2foot.direction.x) > 0.2f)
                return false;
            if (Mathf.Abs(hip2foot.direction.z) > 0.2f)
                return false;

            return true;
        }

        public void FromNormTPose() {
            fromNormUpperLeg = Quaternion.Inverse(UpperLegNorm()) * upperLeg.rotation;
            fromNormLowerLeg = Quaternion.Inverse(LowerLegNorm(foot)) * lowerLeg.rotation;
            fromNormFoot = Quaternion.Inverse(FootNorm(foot)) * foot.rotation;
        }

        public void FromNormTracking(Transform footTarget) {
            fromNormUpperLeg = Quaternion.Inverse(UpperLegNorm()) * upperLeg.rotation;
            fromNormLowerLeg = Quaternion.Inverse(LowerLegNorm(footTarget)) * lowerLeg.rotation;
            fromNormFoot = Quaternion.Inverse(footTarget.rotation) * foot.rotation;
        }

        private Quaternion UpperLegNorm() {
            return Quaternion.LookRotation(lowerLeg.position - upperLeg.position, characterTransform.forward);
        }

        private Quaternion LowerLegNorm(Transform footTarget) {
            return Quaternion.LookRotation(footTarget.position - lowerLeg.position, characterTransform.forward);
        }

        private Quaternion FootNorm(Transform footTarget) {
            return Quaternion.LookRotation(characterTransform.forward);
        }

        private Quaternion CalculateUpperLegNorm(Transform footTarget) {
            float dHipTarget = Vector3.Distance(upperLeg.position, footTarget.position);
            float hipAngle = Mathf.Acos((dHipTarget * dHipTarget + upperLegLength2 - lowerLegLength2) / (2 * upperLegLength * dHipTarget)) * Mathf.Rad2Deg;
            if (float.IsNaN(hipAngle)) hipAngle = 0;

            Vector3 upper2foot = footTarget.position - upperLeg.position;
            Vector3 axis = -Vector3.Cross(upper2foot, footTarget.forward);
            /*
            Debug.DrawRay(upperLeg.position, axis, Color.red);
            Debug.DrawRay(upperLeg.position, upper2foot, Color.blue);
            */
            Quaternion upperLegNormRotation = Quaternion.LookRotation(upper2foot, footTarget.forward);
            upperLegNormRotation = Quaternion.AngleAxis(-hipAngle, axis) * upperLegNormRotation;

            return upperLegNormRotation;
        }

        private Quaternion CalculateLowerLegNorm(Transform footTarget) {
            //Debug.DrawLine(lowerLeg.position, footTarget.position, Color.blue);
            Quaternion lowerLegNormRotation = Quaternion.LookRotation(footTarget.position - lowerLeg.position, footTarget.forward);

            return lowerLegNormRotation;
        }

        private Quaternion CalculateFootNorm(Transform footTarget) {
            Quaternion footNormRotation = footTarget.rotation;

            return footNormRotation;
        }

        public void Calculate(Transform footTarget) {
            if (upperLeg != null) {
                upperLeg.rotation = CalculateUpperLegNorm(footTarget) * fromNormUpperLeg;
                lowerLeg.rotation = CalculateLowerLegNorm(footTarget) * fromNormLowerLeg;
                foot.rotation = CalculateFootNorm(footTarget) * fromNormFoot;
            }
        }
    }
}