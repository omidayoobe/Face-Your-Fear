/* InstantVR Input
 * Copyright (c) 2017 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.8.0
 * date: April 7, 2017
 * 
 * - Renamed button enum to A,B,X,Y
 * - Renamed bumper,trigger to trigger1,trigger2
 * - Fixed controllers not working in multiplayer
 */

using UnityEngine;

namespace IVR {

    public static class Controllers {
        public static ControllerInput[] controllers;

        public static void Update() {
            if (controllers != null) {
                for (int i = 0; i < controllers.Length; i++)
                    controllers[i].Update();
            }
        }

        public static ControllerInput GetController(int playerID) {
            if (controllers == null) {
                controllers = new ControllerInput[1];
                controllers[0] = new ControllerInput();
            }
            return controllers[0];
        }

        public static void Clear() {
            if (controllers != null) {
                for (int i = 0; i < controllers.Length; i++)
                    controllers[i].Clear();
            }
        }

        public static void EndFrame() {
            if (controllers != null) {
                for (int i = 0; i < controllers.Length; i++)
                    controllers[i].EndFrame();
            }
        }
    }

    public class ControllerInput {
        public enum Side {
            Left,
            Right
        }
        public enum Button {
            ButtonA = 0,
            ButtonB = 1,
            ButtonX = 2,
            ButtonY = 3,
            Trigger1 = 10,
            Trigger1Touch = 11,
            Trigger2 = 12,
            Trigger2Touch = 13,
            StickButton = 14,
            StickTouch = 15,
            Up = 20,
            Down = 21,
            Left = 22,
            Right = 23,
            Option = 30,
            None = 9999
        }

        public ControllerInputSide left;
        public ControllerInputSide right;

        public void Update() {
            left.Update();
            right.Update();
        }

        public ControllerInput() {
            left = new ControllerInputSide();
            right = new ControllerInputSide();
        }

        private bool cleared;
        public void Clear() {
            if (cleared)
                return;

            Update();

            cleared = true;
            left.Clear();
            right.Clear();
        }

        public void EndFrame() {
            cleared = false;
        }

        public bool GetButton(Side side, Button buttonID) {
            switch (side) {
                case Side.Left:
                    return left.GetButton(buttonID);
                case Side.Right:
                    return right.GetButton(buttonID);
                default:
                    return false;

            }
        }
    }

    public class ControllerInputSide {
        public float stickHorizontal;
        public float stickVertical;
        public bool stickButton;
        public bool stickTouch;
        public bool up;
        public bool down;
        public bool left;
        public bool right;

        public bool[] buttons = new bool[4];

        public float trigger1;
        public float trigger2;

        public bool option;

        public event OnButtonDown OnButtonDownEvent;
        public event OnButtonUp OnButtonUpEvent;

        public delegate void OnButtonDown(ControllerInput.Button buttonNr);
        public delegate void OnButtonUp(ControllerInput.Button buttonNr);

        private bool[] lastButtons = new bool[4];
        private bool lastTrigger1;
        private bool lastTrigger2;
        private bool lastStickButton;
        private bool lastOption;

        public void Update() {
            for (int i = 0; i < 4; i++) {
                if (buttons[i] && !lastButtons[i]) {
                    if (OnButtonDownEvent != null)
                        OnButtonDownEvent((ControllerInput.Button)i);

                } else if (!buttons[i] && lastButtons[i]) {
                    if (OnButtonUpEvent != null)
                        OnButtonUpEvent((ControllerInput.Button)i);
                }
                lastButtons[i] = buttons[i];
            }

            if (trigger1 > 0.2F && !lastTrigger1) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(ControllerInput.Button.Trigger1);
                lastTrigger1 = true;
            } else if (trigger1 < 0.1F && lastTrigger1) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(ControllerInput.Button.Trigger1);
                lastTrigger1 = false;
            }

            if (trigger2 > 0.2F && !lastTrigger2) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(ControllerInput.Button.Trigger2);
                lastTrigger2 = true;
            } else if (trigger2 < 0.1F && lastTrigger2) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(ControllerInput.Button.Trigger2);
                lastTrigger2 = false;
            }

            if (stickButton && !lastStickButton) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(ControllerInput.Button.StickButton);
            } else if (!stickButton && lastStickButton) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(ControllerInput.Button.StickButton);
            }
            lastStickButton = stickButton;

            if (option && !lastOption) {
                if (OnButtonDownEvent != null)
                    OnButtonDownEvent(ControllerInput.Button.Option);
            } else if (!option && lastOption) {
                if (OnButtonUpEvent != null)
                    OnButtonUpEvent(ControllerInput.Button.Option);
            }
            lastOption = option;
        }

        public void Clear() {
            stickHorizontal = 0;
            stickVertical = 0;
            stickButton = false;
            stickTouch = true;

            up = false;
            down = false;
            left = false;
            right = false;

            for (int i = 0; i < 4; i++)
                buttons[i] = false;

            trigger1 = 0;
            trigger2 = 0;

            option = false;
        }

        public bool GetButton(ControllerInput.Button buttonID) {
            switch (buttonID) {
                case ControllerInput.Button.ButtonA:
                    return buttons[0];
                case ControllerInput.Button.ButtonB:
                    return buttons[1];
                case ControllerInput.Button.ButtonX:
                    return buttons[2];
                case ControllerInput.Button.ButtonY:
                    return buttons[3];
                case ControllerInput.Button.Trigger1:
                    return trigger1 > 0.9F;
                case ControllerInput.Button.Trigger2:
                    return trigger2 > 0.9F;
                case ControllerInput.Button.StickButton:
                    return stickButton;
                case ControllerInput.Button.StickTouch:
                    return stickTouch;
                case ControllerInput.Button.Option:
                    return option;
                case ControllerInput.Button.Up:
                    return up;
                case ControllerInput.Button.Down:
                    return down;
                case ControllerInput.Button.Left:
                    return left;
                case ControllerInput.Button.Right:
                    return right;
                default:
                    return false;
            }
        }

        public void PressButton(ControllerInput.Button buttonID) {
            switch (buttonID) {
                case ControllerInput.Button.ButtonA:
                    buttons[0] = true;
                    break;
                case ControllerInput.Button.ButtonB:
                    buttons[1] = true;
                    break;
                case ControllerInput.Button.ButtonX:
                    buttons[2] = true;
                    break;
                case ControllerInput.Button.ButtonY:
                    buttons[3] = true;
                    break;
                case ControllerInput.Button.Option:
                    option = true;
                    break;
                default:
                    break;
            }
        }
    }
}

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions {

    [ActionCategory("InstantVR")]
    [Tooltip("Controller input axis")]
    public class GetControllerAxis : FsmStateAction {

        [RequiredField]
        [Tooltip("Left or right (side) controller")]
        public IVR.ControllerInput.Side controllerSide = IVR.ControllerInput.Side.Left;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Store the direction vector.")]
        public FsmVector3 storeVector;

        private IVR.ControllerInput controller0;

        public override void Awake() {
            controller0 = IVR.Controllers.GetController(0);
        }

        public override void OnUpdate() {
            IVR.ControllerInputSide controller = (controllerSide == IVR.ControllerInput.Side.Left) ? controller0.left : controller0.right;

            storeVector.Value = new Vector3(controller.stickHorizontal, 0, controller.stickVertical);
        }
    }

    [ActionCategory("InstantVR")]
    [Tooltip("Controller input button")]
    public class GetControllerButton : FsmStateAction {

        [RequiredField]
        [Tooltip("Left or right (side) controller")]
        public IVR.ControllerInput.Side controllerSide = IVR.ControllerInput.Side.Right;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Controller Button")]
        public IVR.ControllerInput.Button button;

        [UIHint(UIHint.Variable)]
        [Tooltip("Store Result Bool")]
        public FsmBool storeBool;

        [UIHint(UIHint.Variable)]
        [Tooltip("Store Result Float")]
        public FsmFloat storeFloat;

        [Tooltip("Event to send when the button is pressed.")]
        public FsmEvent buttonPressed;

        [Tooltip("Event to send when the button is released.")]
        public FsmEvent buttonReleased;

        private IVR.ControllerInput controller0;

        public override void Awake() {
            controller0 = IVR.Controllers.GetController(0);
        }

        public override void OnUpdate() {
            IVR.ControllerInputSide controller = (controllerSide == IVR.ControllerInput.Side.Left) ? controller0.left : controller0.right;

            bool oldBool = storeBool.Value;

            switch (button) {
                case IVR.ControllerInput.Button.StickButton:
                    storeBool.Value = controller.stickButton;
                    storeFloat.Value = controller.stickButton ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.Up:
                    storeBool.Value = controller.up;
                    storeFloat.Value = controller.up ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.Down:
                    storeBool.Value = controller.down;
                    storeFloat.Value = controller.down ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.Left:
                    storeBool.Value = controller.left;
                    storeFloat.Value = controller.left ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.Right:
                    storeBool.Value = controller.right;
                    storeFloat.Value = controller.right ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.ButtonA:
                    storeBool.Value = controller.buttons[0];
                    storeFloat.Value = controller.buttons[0] ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.ButtonB:
                    storeBool.Value = controller.buttons[1];
                    storeFloat.Value = controller.buttons[1] ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.ButtonX:
                    storeBool.Value = controller.buttons[2];
                    storeFloat.Value = controller.buttons[2] ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.ButtonY:
                    storeBool.Value = controller.buttons[3];
                    storeFloat.Value = controller.buttons[3] ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.Option:
                    storeBool.Value = controller.option;
                    storeFloat.Value = controller.option ? 1 : 0;
                    break;
                case IVR.ControllerInput.Button.Trigger1:
                    storeBool.Value = controller.trigger1 > 0.9F;
                    storeFloat.Value = controller.trigger1;
                    break;
                case IVR.ControllerInput.Button.Trigger2:
                    storeBool.Value = controller.trigger2 > 0.9F;
                    storeFloat.Value = controller.trigger2;
                    break;
            }

            if (storeBool.Value && !oldBool)
                Fsm.Event(buttonPressed);
            else if (!storeBool.Value && oldBool)
                Fsm.Event(buttonReleased);
        }
    }
}
#endif