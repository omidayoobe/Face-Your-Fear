/* InstantVR Unity VR extension
 * Copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.8.0
 * date: April 18, 2017
 * 
 * - Moved cameraRoot creation for Unity 5.6 SteamVR fix
 */

using UnityEngine;

namespace IVR {

#if UNITY_ANDROID
    [HelpURL("http://passervr.com/documentation/instantvr-extensions/gear-vr/")]
#else
    [HelpURL("http://passervr.com/documentation/instantvr-extensions/oculus-rift/")]
#endif
    public class IVR_UnityVR : IVR_Extension {
        public override void StartExtension(InstantVR ivr) {
            base.StartExtension(ivr);

            IVR_UnityVRHead unityVrHead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();
            unityVrHead.cameraRoot = new GameObject("UnityVR Root");
            unityVrHead.cameraRoot.transform.parent = ivr.transform;
            unityVrHead.cameraRoot.transform.localScale = Vector3.one;
        }
    }
}