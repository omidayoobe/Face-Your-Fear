/* InstantVR Advanced Editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.8.0
 * date: January 29, 2017
 * 
 * - Fallback check added for INSTANTVR_ADVANCED / INSTANTVR_EDGE
 */

using UnityEditor;
using UnityEngine;
using System.IO;

namespace IVR {

    [CustomEditor(typeof(InstantVR))]
    public class InstantVR_Editor : Editor {
        public override void OnInspectorGUI() {
#if !(UNITY_5_5 || UNITY_5_6 || UNITY_2017_1 || UNITY_2017_2)
            EditorGUILayout.HelpBox("Only Unity 5.5, 5.6, 2017.1 and 2017.2 are supported", MessageType.Error);
#endif

            base.OnInspectorGUI();
            checkInstantVRpackage();

            InstantVR ivr = (InstantVR) target;

            CheckAvatar(ivr);
        }

        private void checkInstantVRpackage() {
            if (isAdvanced())
                GlobalDefine("INSTANTVR_ADVANCED");
            else
                GlobalUndefine("INSTANTVR_ADVANCED");

            if (isEdge())
                GlobalDefine("INSTANTVR_EDGE");
            else
                GlobalUndefine("INSTANTVR_EDGE");
        }

        private static bool isAdvanced() {
            string path = Application.dataPath + "/InstantVR/Editor/IVR_Advanced_Editor.cs";
            return File.Exists(path);
        }
        private static bool isEdge() {
            string path = Application.dataPath + "/InstantVR/Editor/IVR_Edge_Editor.cs";
            return File.Exists(path);
        }


        public static void GlobalDefine(string name) {
            string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!scriptDefines.Contains(name)) {
                string newScriptDefines = scriptDefines + " " + name;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
            }
        }

        public static void GlobalUndefine(string name) {
            string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (scriptDefines.Contains(name)) {
                int playMakerIndex = scriptDefines.IndexOf(name);
                string newScriptDefines = scriptDefines.Remove(playMakerIndex, name.Length);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
            }

        }

        private void CheckAvatar(InstantVR ivr) {
            Animator[] animator = ivr.GetComponentsInChildren<Animator>();
            if (animator.Length > 0) {
                if (animator.Length > 1) {
                    EditorGUILayout.HelpBox("More than one avatar has been found", MessageType.Warning, true);
                }
                if (ivr.characterTransform == null)
                    AlignTargetsWithAvatar(ivr, animator[0]);
                ivr.characterTransform = animator[0].transform;
            } else {
                ivr.characterTransform = null;
            }
        }

        private void AlignTargetsWithAvatar(InstantVR ivr, Animator animator) {
            if (ivr.headTarget != null) {
                Transform headBone = animator.GetBoneTransform(HumanBodyBones.Neck);
                if (headBone != null)
                    ivr.headTarget.transform.position = headBone.position;
                else {
                    headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (headBone != null)
                        ivr.headTarget.transform.position = headBone.position;
                }
            }

            if (ivr.leftHandTarget != null) {
                Transform leftHandBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                if (leftHandBone != null)
                    ivr.leftHandTarget.transform.position = leftHandBone.position;
            }

            if (ivr.rightHandTarget != null) {
                Transform rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                if (rightHandBone != null)
                    ivr.rightHandTarget.transform.position = rightHandBone.position;
            }

            if (ivr.hipTarget != null) {
                Transform hipBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (hipBone != null)
                    ivr.hipTarget.transform.position = hipBone.position;
            }

            if (ivr.leftFootTarget != null) {
                Transform leftFootBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                if (leftFootBone != null)
                    ivr.leftFootTarget.transform.position = leftFootBone.position;
            }

            if (ivr.rightFootTarget != null) {
                Transform rightFootBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                if (rightFootBone != null)
                    ivr.rightFootTarget.transform.position = rightFootBone.position;
            }
        }
    }


    [CustomEditor(typeof(IVR_BodyMovements))]
    public class IVR_Editor : Editor {
        private IVR_BodyMovements bodyMovements;
        private GameObject avatarGameObject;

        private bool prefabPose = true;

        void OnDestroy() {
            if (bodyMovements == null) {
                if (avatarGameObject != null && prefabPose == false) {
                    ResetAvatarToPrefabPose(avatarGameObject);
                }
            }
        }

        void Reset() {
            bodyMovements = (IVR_BodyMovements) target;
            InstantVR ivr = bodyMovements.GetComponent<InstantVR>();

            if (ivr == null) {
                Debug.LogWarning("Body Movements script requires Instant VR script on the game object");
                DestroyImmediate(bodyMovements);
                return;
            }

            IVR_BodyMovements[] bodyMovementsScripts = bodyMovements.GetComponents<IVR_BodyMovements>();
            if (bodyMovementsScripts.Length > 1) {
                Debug.LogError("You cant have more than one BodyMovements script");
                DestroyImmediate(bodyMovements); // why does it delete the first script, while target should be the new script...
                return;
            }

            Animator animator = bodyMovements.transform.GetComponentInChildren<Animator>();
            if (animator) {
                avatarGameObject = animator.gameObject;

                prefabPose = false;
                bodyMovements.StartMovements();
            }
        }

        private void ResetAvatarToPrefabPose(GameObject avatarGameObject) {
            Transform[] avatarBones = avatarGameObject.GetComponentsInChildren<Transform>();
            foreach (Transform bone in avatarBones)
                PrefabUtility.ResetToPrefabState(bone);
            prefabPose = true;
        }
    }
}