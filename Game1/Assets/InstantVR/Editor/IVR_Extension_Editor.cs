using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(IVR_Extension))]
    public class IVR_Extension_Editor : Editor {


        protected void MoveUp(IVR_Controller controller) {
            IVR_Controller[] controllers = controller.gameObject.GetComponents<IVR_Controller>();
            int controllerIndex = FindControllerIndex(controllers, controller);
            if (controllerIndex > 0) {
                if (controllers[controllerIndex].extension != null && controllers[controllerIndex - 1].extension != null) {
                    if (controllers[controllerIndex].extension.priority <= controllers[controllerIndex - 1].extension.priority + 1) {
                        UnityEditorInternal.ComponentUtility.MoveComponentUp(controllers[controllerIndex]);
                    }
                }
            }
        }

        protected void MoveDown(IVR_Controller controller) {
            IVR_Controller[] controllers = controller.gameObject.GetComponents<IVR_Controller>();
            int controllerIndex = FindControllerIndex(controllers, controller);
            if (controllerIndex > -1 && controllerIndex < controllers.Length - 1) {
                if (controllers[controllerIndex].extension != null && controllers[controllerIndex + 1].extension != null) {
                    if (controllers[controllerIndex].extension.priority >= controllers[controllerIndex + 1].extension.priority - 1) {
                        UnityEditorInternal.ComponentUtility.MoveComponentDown(controllers[controllerIndex]);
                    }
                }
            }
        }

        private int FindControllerIndex(IVR_Controller[] controllers, IVR_Controller controller) {
            for (int i = 0; i < controllers.Length; i++) {
                if (controllers[i] == controller)
                    return i;
            }
            return -1;
        }

    }
}