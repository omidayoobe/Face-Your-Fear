/* InstantVR Oculus Rift / Samsung Gear VR extension editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: February 5, 2016
 *
 * - added namespace
 */

using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(IVR_UnityVR))]
    public class IVR_UnityVR_Editor : IVR_Extension_Editor {

        private InstantVR ivr;
        private IVR_UnityVR ivrUnity;

        private IVR_UnityVRHead unityHead;

        public override void OnInspectorGUI() {
#if UNITY_STANDALONE_WIN
            if (PlayerSettings.virtualRealitySupported == false)
                EditorGUILayout.HelpBox("VirtualRealitySupported needs to be enabled in Player Settings for SteamVR/Oculus support", MessageType.Warning, true);

            ivrUnity = (IVR_UnityVR)target;

#elif UNITY_ANDROID
        if (PlayerSettings.virtualRealitySupported == false)
            EditorGUILayout.HelpBox("VirtualRealitySupported needs to be enabled in Player Settings for Gear VR/Cardboard support", MessageType.Warning, true);
#endif
            base.OnInspectorGUI();
        }

        void OnDestroy() {
            if (ivrUnity == null && ivr != null) {
                unityHead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();
                if (unityHead != null)
                    DestroyImmediate(unityHead, true);
            }
        }

        void OnEnable() {
            ivrUnity = (IVR_UnityVR)target;
            if (!ivrUnity)
                return;

            ivr = ivrUnity.GetComponent<InstantVR>();

            if (ivr != null) {
                unityHead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();
                if (unityHead == null) {
                    unityHead = ivr.headTarget.gameObject.AddComponent<IVR_UnityVRHead>();
                    unityHead.extension = ivrUnity;
                }

                IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
                if (ivrUnity.priority == -1)
                    ivrUnity.priority = extensions.Length - 1;
                for (int i = 0; i < extensions.Length; i++) {
                    if (ivrUnity == extensions[i]) {
                        while (i < ivrUnity.priority) {
                            MoveUp(unityHead);
                            ivrUnity.priority--;
                            //Debug.Log ("Rift Move up to : " + i + " now: " + ivrRift.priority);
                        }
                        while (i > ivrUnity.priority) {
                            MoveDown(unityHead);
                            ivrUnity.priority++;
                            //Debug.Log ("Rift Move down to : " + i + " now: " + ivrRift.priority);
                        }
                    }
                }
            }
        }
    }
}