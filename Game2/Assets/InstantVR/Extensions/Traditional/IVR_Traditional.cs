/* InstantVR default target controller
 * Copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.7.3
 * date: February 14, 2017
 * 
 * - Added no game controller option
 */

using UnityEngine;

namespace IVR {

    [HelpURL("http://passervr.com/documentation/instantvr-extensions/game-controller-input/")]
    public class IVR_Traditional : IVR_Extension {
        public enum GameControllers {
            Xbox,
            PS4,
            Steelseries,
            GameSmart,
            SweexGA100,
            None
        }
        public GameControllers traditionalController = GameControllers.Xbox;
        
        public enum LeftRight {
            None,
            Left,
            Right
        }
        [Tooltip("This also maps GearVR touchpad")]
        public LeftRight mouseIsControllerStick = LeftRight.Right;
        public bool mouseAccumulation = true;

        void OnDestroy() {
            InstantVR ivr = GetComponent<InstantVR>();

            IVR_TraditionalHead traditionalHead = ivr.headTarget.GetComponent<IVR_TraditionalHead>();
            if (traditionalHead != null)
                DestroyImmediate(traditionalHead);

            IVR_TraditionalHand traditionalLeftHand = ivr.leftHandTarget.GetComponent<IVR_TraditionalHand>();
            if (traditionalLeftHand != null)
                DestroyImmediate(traditionalLeftHand);

            IVR_TraditionalHand traditionalRightHand = ivr.rightHandTarget.GetComponent<IVR_TraditionalHand>();
            if (traditionalRightHand != null)
                DestroyImmediate(traditionalRightHand);
        }

        private ControllerInput controller;

        public override void StartExtension(InstantVR ivr) {
            base.StartExtension(ivr);

            CheckAxis();
            controller = Controllers.GetController(0);
        }

        public override void UpdateExtension() {
            base.UpdateExtension();

            UpdateGameController();
            UpdateMouse();
        }

        private const float mouseSensitivity = 0.1F;
        private float mouseX;
        private float mouseY;
        private void UpdateMouse() {
            if (!mouseAccumulation) {
                mouseX = 0;
                mouseY = 0;
            }

            mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY += Input.GetAxis("Mouse Y") * mouseSensitivity;

            switch (mouseIsControllerStick) {
                case LeftRight.Left:
                    controller.left.stickHorizontal = controller.left.stickHorizontal + mouseX;
                    controller.left.stickVertical = controller.left.stickVertical + mouseY;

                    controller.left.left = mouseX < 0;
                    controller.left.right = mouseX > 0;
                    controller.left.up = mouseY > 0;
                    controller.left.down = mouseY < 0;
                    break;
                case LeftRight.Right:
                    controller.right.stickHorizontal = controller.right.stickHorizontal + mouseX;
                    controller.right.stickVertical = controller.right.stickVertical + mouseY;

                    controller.right.left = mouseX < 0;
                    controller.right.right = mouseX > 0;
                    controller.right.up = mouseY > 0;
                    controller.right.down = mouseY < 0;
                    break;
                default:
                    break;
            }
        
            controller.right.buttons[0] |= Input.GetMouseButton(0);
            controller.right.buttons[1] |= Input.GetMouseButton(1);
            controller.right.buttons[2] |= Input.GetMouseButton(2);
        }

        private void UpdateGameController() {
            if (controller != null) {
                switch (traditionalController) {
                    case GameControllers.Xbox:
                       UpdateXboxController();
                       break;
                    case GameControllers.PS4:
                        UpdatePS4Controller();
                        break;
                    case GameControllers.Steelseries:
                        UpdateSteelseriesController();
                        break;
                    case GameControllers.GameSmart:
                        UpdateGameSmartController();
                        break;
                    default:
                        break;
                }
            }
        }

        public void UpdateXboxController() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + Input.GetAxis("Horizontal"), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + Input.GetAxis("Vertical"), -1, 1);

            if (axis6available && axis7available) {
                controller.left.left |= Input.GetAxis("Axis 6") < 0;
                controller.left.right |= Input.GetAxis("Axis 6") > 0;
                controller.left.up |= Input.GetAxis("Axis 7") > 0;
                controller.left.down |= Input.GetAxis("Axis 7") < 0;
            }

            if (axis4available && axis5available) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + Input.GetAxis("Axis 4"), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical + Input.GetAxis("Axis 5"), -1, 1);
            }

            if (axis9available && axis10available) {
                controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 +Input.GetAxis("Axis 9"), -1, 1);
                controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + Input.GetAxis("Axis 10"), -1, 1);
            }

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            controller.left.option |= Input.GetKey(KeyCode.JoystickButton6);
            controller.right.option |= Input.GetKey(KeyCode.JoystickButton7);

            controller.right.buttons[0] |= Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[1] |= Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[2] |= Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[3] |= Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= Input.GetKey(KeyCode.JoystickButton8);
            controller.right.stickButton |= Input.GetKey(KeyCode.JoystickButton9);
        }

        public void UpdatePS4Controller() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + Input.GetAxis("Horizontal"), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + Input.GetAxis("Vertical"), -1, 1);

            if (axis7available && axis8available) {
                controller.left.left |= Input.GetAxis("Axis 7") < 0;
                controller.left.right |= Input.GetAxis("Axis 7") > 0;
                controller.left.up |= Input.GetAxis("Axis 8") > 0;
                controller.left.down |= Input.GetAxis("Axis 8") < 0;
            }

            if (axis3available && axis6available) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + Input.GetAxis("Axis 3"), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical + Input.GetAxis("Axis 6"), -1, 1);
            }

            if (axis4available && axis5available) {
                controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + Input.GetAxis("Axis 4"), -1, 1);
                controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + Input.GetAxis("Axis 5"), -1, 1);
            }

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            controller.left.option |= Input.GetKey(KeyCode.JoystickButton8);
            controller.right.option |= Input.GetKey(KeyCode.JoystickButton9);

            controller.right.buttons[0] |= Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[1] |= Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[2] |= Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[3] |= Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= Input.GetKey(KeyCode.JoystickButton10);
            controller.right.stickButton |= Input.GetKey(KeyCode.JoystickButton11);
        }

        public void UpdateSteelseriesController() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + Input.GetAxis("Horizontal"), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + Input.GetAxis("Vertical"), -1, 1);

            if (axis5available && axis6available) {
                controller.left.left |= Input.GetAxis("Axis 5") < 0;
                controller.left.right |= Input.GetAxis("Axis 5") > 0;
                controller.left.up |= Input.GetAxis("Axis 6") > 0;
                controller.left.down |= Input.GetAxis("Axis 6") < 0;
            }

            if (axis3available && axis4available) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + Input.GetAxis("Axis 3"), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical + Input.GetAxis("Axis 4"), -1, 1);
            }

            if (axis13available && axis12available) {
                controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + Input.GetAxis("Axis 13"), -1, 1);
                controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + Input.GetAxis("Axis 12"), -1, 1);
            }

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            controller.left.option |= Input.GetKey(KeyCode.JoystickButton10);

            controller.right.buttons[0] |= Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[1] |= Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[2] |= Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[3] |= Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= Input.GetKey(KeyCode.JoystickButton8);
            controller.right.stickButton |= Input.GetKey(KeyCode.JoystickButton9);
        }

        public void UpdateGameSmartController() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + Input.GetAxis("Horizontal"), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + Input.GetAxis("Vertical"), -1, 1);

            if (axis5available && axis6available) {
                controller.left.left |= Input.GetAxis("Axis 5") < 0;
                controller.left.right |= Input.GetAxis("Axis 5") > 0;
                controller.left.up |= Input.GetAxis("Axis 6") > 0;
                controller.left.down |= Input.GetAxis("Axis 6") < 0;
            }

            if (axis3available && axis6available) {
                controller.right.stickHorizontal = Mathf.Clamp(controller.right.stickHorizontal + Input.GetAxis("Axis 3"), -1, 1);
                controller.right.stickVertical = Mathf.Clamp(controller.right.stickVertical - Input.GetAxis("Axis 4"), -1, 1);
            }

            controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + (Input.GetKey(KeyCode.JoystickButton6) ? 1 : 0), -1, 1);
            controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + (Input.GetKey(KeyCode.JoystickButton7) ? 1 : 0), -1, 1);

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (Input.GetKey(KeyCode.JoystickButton4) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (Input.GetKey(KeyCode.JoystickButton5) ? 1 : 0), -1, 1);

            controller.left.option |= Input.GetKey(KeyCode.JoystickButton8);
            controller.right.option |= Input.GetKey(KeyCode.JoystickButton9);

            controller.right.buttons[0] |= Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[1] |= Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[2] |= Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[3] |= Input.GetKey(KeyCode.JoystickButton3);

            controller.left.stickButton |= Input.GetKey(KeyCode.JoystickButton10);
            controller.right.stickButton |= Input.GetKey(KeyCode.JoystickButton11);
        }

        public void UpdateGA100Controller() {
            controller.left.stickHorizontal = Mathf.Clamp(controller.left.stickHorizontal + Input.GetAxis("Horizontal"), -1, 1);
            controller.left.stickVertical = Mathf.Clamp(controller.left.stickVertical + Input.GetAxis("Vertical"), -1, 1);

            controller.left.trigger2 = Mathf.Clamp(controller.left.trigger2 + (Input.GetKey(KeyCode.JoystickButton7) ? 1 : 0), -1, 1);
            controller.right.trigger2 = Mathf.Clamp(controller.right.trigger2 + (Input.GetKey(KeyCode.JoystickButton9) ? 1 : 0), -1, 1);

            controller.left.trigger1 = Mathf.Clamp(controller.left.trigger1 + (Input.GetKey(KeyCode.JoystickButton6) ? 1 : 0), -1, 1);
            controller.right.trigger1 = Mathf.Clamp(controller.right.trigger1 + (Input.GetKey(KeyCode.JoystickButton8) ? 1 : 0), -1, 1);

            controller.left.option |= Input.GetKey(KeyCode.JoystickButton10);
            controller.right.option |= Input.GetKey(KeyCode.JoystickButton11);

            controller.right.buttons[0] |= Input.GetKey(KeyCode.JoystickButton0);
            controller.right.buttons[1] |= Input.GetKey(KeyCode.JoystickButton1);
            controller.right.buttons[2] |= Input.GetKey(KeyCode.JoystickButton2);
            controller.right.buttons[3] |= Input.GetKey(KeyCode.JoystickButton3);
        }

        [HideInInspector]
        private bool axis3available;
        [HideInInspector]
        private bool axis4available;
        [HideInInspector]
        private bool axis5available;
        [HideInInspector]
        private bool axis6available;
        [HideInInspector]
        private bool axis7available;
        [HideInInspector]
        private bool axis8available;
        [HideInInspector]
        private bool axis9available;
        [HideInInspector]
        private bool axis10available;
        //[HideInInspector]
        //private bool axis11available;
        [HideInInspector]
        private bool axis12available;
        [HideInInspector]
        private bool axis13available;

        private void CheckAxis() {
            axis3available = IsAxisAvailable("Axis 3");
            axis4available = IsAxisAvailable("Axis 4");
            axis5available = IsAxisAvailable("Axis 5");
            axis6available = IsAxisAvailable("Axis 6");
            axis7available = IsAxisAvailable("Axis 7");
            axis8available = IsAxisAvailable("Axis 8");
            axis9available = IsAxisAvailable("Axis 9");
            axis10available = IsAxisAvailable("Axis 10");
            //axis11available = IsAxisAvailable("Axis 11");
            axis12available = IsAxisAvailable("Axis 12");
            axis13available = IsAxisAvailable("Axis 13");
        }

        private bool IsAxisAvailable(string axisName) {
            try {
                Input.GetAxis(axisName);
                return true;
            }
            catch (System.Exception) {
                return false;
            }
        }
    }
}