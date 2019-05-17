/* InstantVR Animator
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: February 5, 2016
 *
 * - added namespace
 */

using UnityEngine;

namespace IVR {

    public class IVR_Animator : IVR_Extension {
        void OnDestroy() {
            InstantVR ivr = this.GetComponent<InstantVR>();

            if (ivr != null) {
                IVR_AnimatorHead defaultHead = ivr.headTarget.GetComponent<IVR_AnimatorHead>();
                if (defaultHead != null)
                    DestroyImmediate(defaultHead);

                IVR_AnimatorHand defaultLeftHand = ivr.leftHandTarget.GetComponent<IVR_AnimatorHand>();
                if (defaultLeftHand != null)
                    DestroyImmediate(defaultLeftHand);

                IVR_AnimatorHand defaultRightHand = ivr.rightHandTarget.GetComponent<IVR_AnimatorHand>();
                if (defaultRightHand != null)
                    DestroyImmediate(defaultRightHand);

                IVR_AnimatorHip defaultHip = ivr.hipTarget.GetComponent<IVR_AnimatorHip>();
                if (defaultHip != null)
                    DestroyImmediate(defaultHip);

                IVR_AnimatorFoot defaultLeftFoot = ivr.leftFootTarget.GetComponent<IVR_AnimatorFoot>();
                if (defaultLeftFoot != null)
                    DestroyImmediate(defaultLeftFoot);

                IVR_AnimatorFoot defaultRightFoot = ivr.rightFootTarget.GetComponent<IVR_AnimatorFoot>();
                if (defaultRightFoot != null)
                    DestroyImmediate(defaultRightFoot);
            }
        }
    }

    public class IVR_Animate : MonoBehaviour {

        public bool startStep = false;

        public KeyFrame[] keyFrames = {
        new IVR_Animate.KeyFrame(0, 12, new Vector3(0,0,0), new Vector3(80,0,0), 60),
        new IVR_Animate.KeyFrame(12, 24, new Vector3(0,0.3f,0.5f), new Vector3(20,0,0), 60),
        new IVR_Animate.KeyFrame(24, 60, new Vector3(0,0,1f), new Vector3(-30,0,0), 60)
    };

        private int nrFrames = 24;
        private float frameSpeed = 60;

        public float f = 0;
        public int keyFrameNr1, keyFrameNr2;

        public void Initialize() {
        }

        void Update() {
            if (startStep)
                f = CalculateFraction(f);
        }

        public Vector3 AnimationLerp(Vector3 from, Vector3 to, ref float t) {
            Mathf.Clamp01(t);

            UpdateKeyFrames(from, to);

            CalculateKeyFrameNr(keyFrames, t, out keyFrameNr1, out keyFrameNr2);
            return InterpolatedPosition(keyFrames[keyFrameNr1], keyFrames[keyFrameNr2], t);
        }

        private float CalculateFraction(float f) {
            f += Time.deltaTime * (frameSpeed / nrFrames);
            if (f >= 1) {
                startStep = false;
                //f = 0;
            }
            return f;
        }

        private void UpdateKeyFrames(Vector3 origin, Vector3 target) {
            keyFrames = new KeyFrame[3];
            keyFrames[0] = new KeyFrame(0, 12, origin, Vector3.zero, 24);
            keyFrames[1] = new KeyFrame(12, 24, new Vector3(origin.x / 2 + target.x / 2, target.y / 2 + 0.3f, origin.z / 2 + target.z / 2), Vector3.zero, 24);
            keyFrames[2] = new KeyFrame(24, 60, new Vector3(target.x, target.y, target.z), Vector3.zero, 24);
        }

        void CalculateKeyFrameNr(KeyFrame[] keyFrames, float f, out int keyFrameNr1, out int keyFrameNr2) {
            keyFrameNr1 = 0;
            keyFrameNr2 = 1;

            for (int i = 0; i < keyFrames.Length - 1; i++) {
                if (f >= keyFrames[i].fromF && f < keyFrames[i + 1].fromF) {
                    keyFrameNr1 = i;
                    keyFrameNr2 = i + 1;
                }
            }
        }

        private Vector3 InterpolatedPosition(KeyFrame keyFrame1, KeyFrame keyFrame2, float t) {
            float frameLen = keyFrames[keyFrameNr2].fromF - keyFrames[keyFrameNr1].fromF;
            float localT = (t - keyFrame1.fromF) / frameLen;

            return Vector3.Lerp(keyFrame1.position, keyFrame2.position, localT);
        }


        [System.Serializable]
        public class KeyFrame {
            public Vector3 position;
            public Vector3 euler;
            public float fromF;
            public float toF;

            public KeyFrame(int fromFrameNr_in, int toFrameNr_in, Vector3 position_in, Vector3 euler_in, float totalFrames) {
                position = position_in;
                euler = euler_in;
                fromF = fromFrameNr_in / totalFrames;
                toF = toFrameNr_in / totalFrames;
            }
        }
    }
}