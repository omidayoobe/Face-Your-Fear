/* InstantVR Extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.8
 * date: May 29, 2016
 *
 * - fixed extrapolation for positions
 */

using UnityEngine;

namespace IVR {

    public class IVR_Extension : MonoBehaviour {
        [HideInInspector]
        protected InstantVR ivr = null;
        [HideInInspector]
        public int priority = -1;
        [HideInInspector]
        public bool present = false;

        // It is not always a real tracker (e.g. Lighthouse, Hydra), I know...
        public Vector3 trackerPosition = Vector3.zero;
        public Vector3 trackerEulerAngles = Vector3.zero;
        //[HideInInspector]
        public Quaternion trackerRotation = Quaternion.identity;

        public bool showTracker = false;
        public GameObject trackerPrefab;
        [HideInInspector]
        protected GameObject trackerObj;

        public virtual void StartExtension(InstantVR ivr) {
            this.ivr = ivr;
            trackerRotation = Quaternion.Euler(trackerEulerAngles);
        }
        public virtual void UpdateExtension() {
            trackerRotation = Quaternion.Euler(trackerEulerAngles);
            TrackerObject();
        }
        public virtual void LateUpdateExtension() { }

        #region TrackerObject
        private void TrackerObject() {
            if (showTracker)
                ShowTracker();
            else
                HideTracker();
        }

        private void ShowTracker() {
            if (trackerObj == null)
                CreateTrackerObj();

            Vector3 localTrackerPosition = trackerPosition;
            Quaternion localTrackerRotation = trackerRotation * Quaternion.AngleAxis(180, Vector3.up);

            trackerObj.transform.position = ivr.transform.position + (ivr.transform.rotation * localTrackerPosition);
            trackerObj.transform.rotation = ivr.transform.rotation * localTrackerRotation;

            Debug.DrawRay(trackerObj.transform.position, trackerObj.transform.forward);
        }

        private void HideTracker() {
            if (trackerObj != null) {
                MonoBehaviour.Destroy(trackerObj);
            }
        }

        private void CreateTrackerObj() {
            if (trackerPrefab == null) {
                trackerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trackerObj.transform.localScale = new Vector3(0.2f, 0.1f, 0.05f);
                Collider c = trackerObj.GetComponent<Collider>();
                Destroy(c);
            } else {
                trackerObj = MonoBehaviour.Instantiate(trackerPrefab);
            }
        }
        #endregion
    }

    public class IVR_HandController : IVR_Controller {
        /*
        public float thumbInput;
        public float indexInput;
        public float middleInput;
        public float ringInput;
        public float littleInput;
        */
    }

    public class IVR_Controller : MonoBehaviour {

        [HideInInspector]
        protected InstantVR ivr;
        [HideInInspector]
        public IVR_Extension extension;

        protected Vector3 startPosition;
        public Vector3 GetStartPosition() { return startPosition; }

        protected Quaternion startRotation;
        public Quaternion GetStartRotation() { return startRotation; }

        protected Vector3 controllerPosition = Vector3.zero;
        protected Quaternion controllerRotation = Quaternion.identity;

        [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Quaternion rotation;

        protected bool extrapolation = false;

        /*
        protected bool present = false;
        public bool isPresent() {
            return present;
        }
        */
        protected bool tracking = false;
        public bool isTracking() {
            return tracking;
        }
        public void setTracking(bool b) {
            tracking = b;
        }

        protected bool selected = false;
        public bool isSelected() {
            return selected;
        }
        public void SetSelection(bool selection) {
            selected = selection;
        }

        [HideInInspector]
        private float updateTime;

        [HideInInspector]
        private Vector3 lastPosition = Vector3.zero;
        [HideInInspector]
        private Quaternion lastRotation = Quaternion.identity;
        private Vector3 positionalVelocity = Vector3.zero;
        private float angularVelocity = 0;
        private Vector3 velocityAxis = Vector3.one;

        void Start() {
            updateTime = Time.time;
        }

        public virtual void StartController(InstantVR ivr) {
            this.ivr = ivr;

            startPosition = transform.position - ivr.transform.position;
            startRotation = Quaternion.Inverse(ivr.transform.rotation) * transform.rotation;

            lastPosition = startPosition;
            lastRotation = startRotation;
        }

        public virtual void OnTargetReset() {
            if (selected)
                Calibrate(true);
        }

        public void Calibrate(bool calibrateOrientation) {
            //extension.trackerPosition = ...

            //if (calibrateOrientation) {
            //    extension.trackerRotation = Quaternion.Inverse(controllerRotation);
            //}
        }

        public void TransferCalibration(IVR_Controller lastController) {
        }

        [HideInInspector]
        private bool indirectUpdate = false;

        public virtual void UpdateController() {
            Vector3 localPosition = extension.trackerPosition + extension.trackerRotation * controllerPosition;
            Quaternion localRotation = extension.trackerRotation * controllerRotation;

            position = ivr.transform.position + ivr.transform.rotation * localPosition;
            rotation = ivr.transform.rotation * localRotation;

            if (selected) { // this should be moved out of here in the future, because it removes the possibility to combine controllers
                if (extrapolation == false) {
                    transform.position = position;
                    transform.rotation = rotation;
                } else {
                    float deltaTime = Time.time - updateTime;
                    CalculateVelocity(deltaTime);

                    if (deltaTime > 0) {
                        updateTime = Time.time;
                        indirectUpdate = true;
                    }
                }
            }
        }

        private void CalculateVelocity(float deltaTime) {
            if (deltaTime > 0) {
                float angle = 0;
                Quaternion rotationalChange = Quaternion.Inverse(lastRotation) * rotation;

                Vector3 newVelocityAxis;
                rotationalChange.ToAngleAxis(out angle, out newVelocityAxis);
                if (angle == 0)
                    newVelocityAxis = Vector3.one;

                positionalVelocity = (positionalVelocity + 2 * (position - lastPosition) / deltaTime) / 3;
                angularVelocity = (angularVelocity + 2 * angle / deltaTime) / 3;
                velocityAxis = (velocityAxis + 2 * newVelocityAxis) / 3;

                lastPosition = position;
                lastRotation = rotation;
            }
        }

        void Update() {
            if (indirectUpdate) {
                float dTime = Time.time - updateTime;
                if (dTime < 0.5F) { //extrapolate for 0.5s maximum
                    transform.position = lastPosition + positionalVelocity * dTime;
                    transform.rotation = lastRotation * Quaternion.AngleAxis(angularVelocity * dTime, velocityAxis);
                }
            }
        }
    }
}