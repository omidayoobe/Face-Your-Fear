/* InstantVR Animator editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: February 5, 2016
 *
 * - added namespace
 */

using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(IVR_Animator))]
    public class IVR_Animator_Editor : IVR_Extension_Editor {

        private InstantVR ivr;
        private IVR_Animator ivranimator;

        private IVR_AnimatorHead animatorHead;
        private IVR_AnimatorHand animatorLeftHand, animatorRightHand;
        private IVR_AnimatorHip animatorHip;
        private IVR_AnimatorFoot animatorLeftFoot, animatorRightFoot;

        void OnDestroy() {
            if (ivranimator == null && ivr != null) {
                animatorHead = ivr.headTarget.GetComponent<IVR_AnimatorHead>();
                if (animatorHead != null)
                    DestroyImmediate(animatorHead, true);

                animatorLeftHand = ivr.leftHandTarget.GetComponent<IVR_AnimatorHand>();
                if (animatorLeftHand != null)
                    DestroyImmediate(animatorLeftHand, true);

                animatorRightHand = ivr.rightHandTarget.GetComponent<IVR_AnimatorHand>();
                if (animatorRightHand != null)
                    DestroyImmediate(animatorRightHand, true);

                animatorHip = ivr.hipTarget.GetComponent<IVR_AnimatorHip>();
                if (animatorHip != null)
                    DestroyImmediate(animatorHip, true);

                animatorLeftFoot = ivr.leftFootTarget.GetComponent<IVR_AnimatorFoot>();
                if (animatorLeftFoot != null)
                    DestroyImmediate(animatorLeftFoot, true);

                animatorRightFoot = ivr.rightFootTarget.GetComponent<IVR_AnimatorFoot>();
                if (animatorRightFoot != null)
                    DestroyImmediate(animatorRightFoot, true);

            }
        }

        void OnEnable() {
            ivranimator = (IVR_Animator)target;
            if (!ivranimator)
                return;

            ivr = ivranimator.GetComponent<InstantVR>();

            if (ivr != null) {
                animatorHead = ivr.headTarget.GetComponent<IVR_AnimatorHead>();
                if (animatorHead == null) {
                    animatorHead = ivr.headTarget.gameObject.AddComponent<IVR_AnimatorHead>();
                    animatorHead.extension = ivranimator;
                }

                animatorLeftHand = ivr.leftHandTarget.GetComponent<IVR_AnimatorHand>();
                if (animatorLeftHand == null) {
                    animatorLeftHand = ivr.leftHandTarget.gameObject.AddComponent<IVR_AnimatorHand>();
                    animatorLeftHand.extension = ivranimator;
                }

                animatorRightHand = ivr.rightHandTarget.GetComponent<IVR_AnimatorHand>();
                if (animatorRightHand == null) {
                    animatorRightHand = ivr.rightHandTarget.gameObject.AddComponent<IVR_AnimatorHand>();
                    animatorRightHand.extension = ivranimator;
                }

                animatorHip = ivr.hipTarget.GetComponent<IVR_AnimatorHip>();
                if (animatorHip == null) {
                    animatorHip = ivr.hipTarget.gameObject.AddComponent<IVR_AnimatorHip>();
                    animatorHip.extension = ivranimator;
                }

                animatorLeftFoot = ivr.leftFootTarget.GetComponent<IVR_AnimatorFoot>();
                if (animatorLeftFoot == null) {
                    animatorLeftFoot = ivr.leftFootTarget.gameObject.AddComponent<IVR_AnimatorFoot>();
                    animatorLeftFoot.extension = ivranimator;
                }

                animatorRightFoot = ivr.rightFootTarget.GetComponent<IVR_AnimatorFoot>();
                if (animatorRightFoot == null) {
                    animatorRightFoot = ivr.rightFootTarget.gameObject.AddComponent<IVR_AnimatorFoot>();
                    animatorRightFoot.extension = ivranimator;
                }

                IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
                if (ivranimator.priority == -1)
                    ivranimator.priority = extensions.Length - 1;
                for (int i = 0; i < extensions.Length; i++) {
                    if (ivranimator == extensions[i]) {
                        while (i < ivranimator.priority) {
                            MoveUp(animatorHead);
                            MoveUp(animatorLeftHand);
                            MoveUp(animatorRightHand);
                            MoveUp(animatorHip);
                            MoveUp(animatorLeftFoot);
                            MoveUp(animatorRightFoot);
                            ivranimator.priority--;
                            //Debug.Log ("Animator Move up to : " + i + " now: " + ivranimator.priority);
                        }
                        while (i > ivranimator.priority) {
                            MoveDown(animatorHead);
                            MoveDown(animatorLeftHand);
                            MoveDown(animatorRightHand);
                            MoveDown(animatorHip);
                            MoveDown(animatorLeftFoot);
                            MoveDown(animatorRightFoot);
                            ivranimator.priority++;
                            //Debug.Log ("Animator Move down to : " + i + " now: " + ivranimator.priority);
                        }
                    }
                }
            }
        }
    }
}