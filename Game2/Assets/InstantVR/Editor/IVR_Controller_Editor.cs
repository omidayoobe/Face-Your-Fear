using UnityEditor;

namespace IVR {
    [CustomEditor(typeof(IVR_Controller))]
    public class IVR_Controller_Editor : Editor {

        public void OnEnable() {
            IVR_Controller ivrController = (IVR_Controller)target;
            IVR_Extension ivrExtension = ivrController.extension;

            if (ivrExtension == null) {
                //Debug.Log ("Destroyed Extension, removing Controller: " + ivrController);
                DestroyImmediate(ivrController, true);
            }
        }
    }
}