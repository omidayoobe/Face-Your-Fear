using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(IVR_Traditional))]
    public class IVR_Traditional_Editor : IVR_Extension_Editor {

        private InstantVR ivr;
        private IVR_Traditional ivrTraditional;

        private IVR_TraditionalHead traditionalHead;
        private IVR_TraditionalHand traditionalLeftHand, traditionalRightHand;

        void OnDestroy() {
            if (ivrTraditional == null && ivr != null) {
                traditionalHead = ivr.headTarget.GetComponent<IVR_TraditionalHead>();
                if (traditionalHead != null)
                    DestroyImmediate(traditionalHead, true);

                traditionalLeftHand = ivr.leftHandTarget.GetComponent<IVR_TraditionalHand>();
                if (traditionalLeftHand != null)
                    DestroyImmediate(traditionalLeftHand, true);

                traditionalRightHand = ivr.rightHandTarget.GetComponent<IVR_TraditionalHand>();
                if (traditionalRightHand != null)
                    DestroyImmediate(traditionalRightHand, true);
            }
        }

        void OnEnable() {
            ivrTraditional = (IVR_Traditional)target;
            if (!ivrTraditional)
                return;

            ivr = ivrTraditional.GetComponent<InstantVR>();

            if (ivr != null) {
                traditionalHead = ivr.headTarget.GetComponent<IVR_TraditionalHead>();
                if (traditionalHead == null) {
                    traditionalHead = ivr.headTarget.gameObject.AddComponent<IVR_TraditionalHead>();
                    traditionalHead.extension = ivrTraditional;
                }

                traditionalLeftHand = ivr.leftHandTarget.GetComponent<IVR_TraditionalHand>();
                if (traditionalLeftHand == null) {
                    traditionalLeftHand = ivr.leftHandTarget.gameObject.AddComponent<IVR_TraditionalHand>();
                    traditionalLeftHand.extension = ivrTraditional;
                }

                traditionalRightHand = ivr.rightHandTarget.GetComponent<IVR_TraditionalHand>();
                if (traditionalRightHand == null) {
                    traditionalRightHand = ivr.rightHandTarget.gameObject.AddComponent<IVR_TraditionalHand>();
                    traditionalRightHand.extension = ivrTraditional;
                }

                IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
                if (ivrTraditional.priority == -1)
                    ivrTraditional.priority = extensions.Length - 1;
                for (int i = 0; i < extensions.Length; i++) {
                    if (ivrTraditional == extensions[i]) {
                        while (i < ivrTraditional.priority) {
                            MoveUp(traditionalHead);
                            MoveUp(traditionalLeftHand);
                            MoveUp(traditionalRightHand);
                            ivrTraditional.priority--;
                            //Debug.Log ("Traditional Move up to : " + i + " now: " + ivrTraditional.priority);
                        }
                        while (i > ivrTraditional.priority) {
                            MoveDown(traditionalHead);
                            MoveDown(traditionalLeftHand);
                            MoveDown(traditionalRightHand);
                            ivrTraditional.priority++;
                            //Debug.Log ("Traditional Move down to : " + i + " now: " + ivrTraditional.priority);
                        }
                    }
                }
            }
        }
    }
}