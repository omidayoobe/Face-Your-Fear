/* InstantVR Input Handler
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.8.3
 * date: July 2, 2017
 * 
 * - Fixed rotation speed
 */

using UnityEngine;
using IVR;

public class InputHandler : MonoBehaviour {
    public enum WalkTypes {
        SmoothWalking,
        PointingTeleport,
        Teleport
    }
#if INSTANTVR_ADVANCED
    private InputDeviceIDs pointingDevice = InputDeviceIDs.None;
#endif

    public float playerHeight = 1.78F;

    public bool walking = true;
    public WalkTypes walkingType = WalkTypes.SmoothWalking;
    public bool sidestepping = true;
    public bool rotation = false;

    private InstantVR ivr;
    private ControllerInput controller0;

    private HeadMovements headMovements;
#if INSTANTVR_ADVANCED
    private IVR_HandMovements leftHandMovements;
    private IVR_HandMovements rightHandMovements;

    public enum FingerMovements {
        No,
        Yes,
        ClickGrabbing
    }
    public FingerMovements fingerMovements = FingerMovements.Yes;

    private bool leftGrabbing;
    private bool rightGrabbing;

    private bool leftLettingGo;
    private bool rightLettingGo;
#endif

    #region Start
    public void Start() {
        ivr = GetComponent<InstantVR>();
        ivr.SetPlayerHeight(playerHeight);

        Debug.Log("Please press <tab> to calibrate player height.");

        headMovements = ivr.headTarget.GetComponent<HeadMovements>();
#if INSTANTVR_ADVANCED
        leftHandMovements = ivr.leftHandTarget.GetComponent<IVR_HandMovements>();
        rightHandMovements = ivr.rightHandTarget.GetComponent<IVR_HandMovements>();
#endif

        // get the first player's controller
        controller0 = Controllers.GetController(0);

        // register for button down events
        controller0.left.OnButtonDownEvent += OnButtonDownLeft;
        controller0.right.OnButtonDownEvent += OnButtonDownRight;

        // register for button up events
        controller0.left.OnButtonUpEvent += OnButtonUpLeft;
        controller0.right.OnButtonUpEvent += OnButtonUpRight;
#if INSTANTVR_ADVANCED
        if (walkingType == WalkTypes.PointingTeleport) {
            pointingDevice = GetPointingDevice();
        }
#endif
    }

    // this function is called when a left button has been pressed
    private void OnButtonDownLeft(ControllerInput.Button buttonID) {
#if INSTANTVR_ADVANCED
        if (walking && walkingType == WalkTypes.PointingTeleport && buttonID == leftHandMovements.activationButton) {
            // When activation button is pressed we teleport to the focus position
            PointingTeleport(pointingDevice);
        }

        if (fingerMovements == FingerMovements.ClickGrabbing && buttonID == ControllerInput.Button.Trigger2) {
            if (leftHandMovements.grabbedObject == null && !leftLettingGo) {
                leftHandMovements.thumbCurl = 1;
                leftHandMovements.middleCurl = 1;
                leftHandMovements.ringCurl = 1;
                leftHandMovements.littleCurl = 1;
                leftGrabbing = true;
            } else if (leftHandMovements.grabbedObject != null && !leftGrabbing) {
                leftHandMovements.thumbCurl = 0;
                leftHandMovements.middleCurl = 0;
                leftHandMovements.ringCurl = 0;
                leftHandMovements.littleCurl = 0;
                leftLettingGo = true;
            }
        }
#endif
    }

    // this function is called when a right button has been pressed
    private void OnButtonDownRight(ControllerInput.Button buttonID) {
        if (walking && walkingType == WalkTypes.Teleport && buttonID == ControllerInput.Button.ButtonA) {
            // When button One (A on Xbox controller) is pressed we teleport in the looking direction
            Teleport(ivr.transform.position + ivr.headTarget.transform.forward * 50 * Time.deltaTime);
        }
#if INSTANTVR_ADVANCED
        else if (walking && walkingType == WalkTypes.PointingTeleport && buttonID == rightHandMovements.activationButton) {
            // When activation button is pressed we teleport to the focus position
            PointingTeleport(pointingDevice);
        }

        if (fingerMovements == FingerMovements.ClickGrabbing && buttonID == ControllerInput.Button.Trigger2) {
            if (rightHandMovements.grabbedObject == null && !rightLettingGo) {
                rightHandMovements.thumbCurl = 1;
                rightHandMovements.middleCurl = 1;
                rightHandMovements.ringCurl = 1;
                rightHandMovements.littleCurl = 1;
                rightGrabbing = true;
            } else if (rightHandMovements.grabbedObject != null && !rightGrabbing) {
                rightHandMovements.thumbCurl = 0;
                rightHandMovements.middleCurl = 0;
                rightHandMovements.ringCurl = 0;
                rightHandMovements.littleCurl = 0;
                rightLettingGo = true;
            }
        }
#endif
    }

    private void OnButtonUpLeft(ControllerInput.Button buttonID) {
#if INSTANTVR_ADVANCED
        if (fingerMovements == FingerMovements.ClickGrabbing && buttonID == ControllerInput.Button.Trigger2) {
            if (leftLettingGo && leftHandMovements.grabbedObject == null) {
                leftLettingGo = false;
            } else if (leftGrabbing) {
                //Debug.Log("leftgrabbing FALSE");
                leftGrabbing = false;
                if (leftHandMovements.grabbedObject == null) {
                    leftHandMovements.thumbCurl = 0;
                    leftHandMovements.middleCurl = 0;
                    leftHandMovements.ringCurl = 0;
                    leftHandMovements.littleCurl = 0;
                }
            }
        }
#endif
    }

    private void OnButtonUpRight(ControllerInput.Button buttonID) {
#if INSTANTVR_ADVANCED
        if (fingerMovements == FingerMovements.ClickGrabbing && buttonID == ControllerInput.Button.Trigger2) {
            if (rightLettingGo && rightHandMovements.grabbedObject == null) {
                rightLettingGo = false;
            } else if (rightGrabbing) {
                rightGrabbing = false;
                if (rightHandMovements.grabbedObject == null) {
                    rightHandMovements.thumbCurl = 0;
                    rightHandMovements.middleCurl = 0;
                    rightHandMovements.ringCurl = 0;
                    rightHandMovements.littleCurl = 0;
                }
            }
        }
#endif
    }


    private InputDeviceIDs GetPointingDevice() {
        if (headMovements != null && headMovements.interaction == HeadMovements.InteractionType.Gazing)
            return InputDeviceIDs.Head;

#if INSTANTVR_ADVANCED
        if (leftHandMovements != null && leftHandMovements.interaction == IVR_HandMovements.InteractionType.Pointing)
            return InputDeviceIDs.LeftHand;

        if (rightHandMovements != null && rightHandMovements.interaction == IVR_HandMovements.InteractionType.Pointing)
            return InputDeviceIDs.RightHand;
#endif
        Debug.LogError("Gaze/Pointing interaction needs to be set on head or hand for PointingTeleport");
        return InputDeviceIDs.None;
    }
    #endregion

    #region Update
    public void Update() {
        CheckQuit();

        if (walkingType == WalkTypes.SmoothWalking) {
            // move the character using the left analog stick
            float horizontal = 0;
            float vertical = 0;

            // forward/backward walking using the left analog stick up/down
            if (walking)
                vertical = controller0.left.stickVertical;

            // left/right sidestepping using the left analog stick left/right
            if (sidestepping)
                horizontal = controller0.left.stickHorizontal;

            // now move the character
            if (walking || sidestepping)
                ivr.Move(horizontal * Time.deltaTime, 0, vertical * Time.deltaTime);

            if (rotation) {
                // rotate the character using the right analog stick left/right
                horizontal = controller0.left.stickHorizontal * 50;
                ivr.Rotate(horizontal * Time.deltaTime);
            }
        }
        // calibrate tracking when both left & right option buttons are pressed
        if ((controller0.left.option && controller0.right.option) || Input.GetKeyDown(KeyCode.Tab))
            ivr.Calibrate();

#if INSTANTVR_ADVANCED
        switch (fingerMovements) {
            case FingerMovements.Yes:
                UpdateFingerMovements();
                break;
            case FingerMovements.ClickGrabbing:
                UpdateFingerClickGrabbing();
                break;
            default:
                break;
        }
#endif
    }

#if INSTANTVR_ADVANCED
    private void UpdateFingerMovements() {
        if (leftHandMovements) {
            leftHandMovements.thumbCurl = Mathf.Max(controller0.left.trigger2, controller0.left.trigger1);
            leftHandMovements.thumbCurl = controller0.left.stickTouch ? leftHandMovements.thumbCurl : -0.5F;
            leftHandMovements.indexCurl = controller0.left.trigger1;
            leftHandMovements.middleCurl = Mathf.Max(controller0.left.trigger2, controller0.left.trigger1);
            leftHandMovements.ringCurl = Mathf.Max(controller0.left.trigger2, controller0.left.trigger1);
            leftHandMovements.littleCurl = Mathf.Max(controller0.left.trigger2, controller0.left.trigger1);
        }

        if (rightHandMovements) {
            rightHandMovements.thumbCurl = Mathf.Max(controller0.right.trigger2, controller0.right.trigger1);
            rightHandMovements.thumbCurl = controller0.right.stickTouch ? rightHandMovements.thumbCurl : -0.5F;
            rightHandMovements.indexCurl = controller0.right.trigger1;
            rightHandMovements.middleCurl = Mathf.Max(controller0.right.trigger2, controller0.right.trigger1);
            rightHandMovements.ringCurl = Mathf.Max(controller0.right.trigger2, controller0.right.trigger1);
            rightHandMovements.littleCurl = Mathf.Max(controller0.right.trigger2, controller0.right.trigger1);
        }
    }

    private void ClickGrabDirect() {
        if (leftHandMovements && leftHandMovements.touchingObject != null) {
            leftHandMovements.NetworkingGrab(leftHandMovements.touchingObject);
        } else
            if (rightHandMovements && rightHandMovements.touchingObject != null) {
            rightHandMovements.NetworkingGrab(rightHandMovements.touchingObject);
        }
    }

    private void UpdateFingerClickGrabbing() {
        if (leftHandMovements) {
            leftHandMovements.indexCurl = controller0.left.trigger1;
            if (leftGrabbing) {
                leftHandMovements.thumbCurl = 1;
                leftHandMovements.middleCurl = 1;
                leftHandMovements.ringCurl = 1;
                leftHandMovements.littleCurl = 1;
            }
        }

        if (rightHandMovements) {
            rightHandMovements.indexCurl = controller0.right.trigger1;
        }
    }
#endif

    private void PointingTeleport(InputDeviceIDs pointingDevice) {
        Vector3 targetPosition;
        switch (pointingDevice) {
            case InputDeviceIDs.Head:
                targetPosition = headMovements.focusPoint;
                break;
#if INSTANTVR_ADVANCED
            case InputDeviceIDs.LeftHand:
                targetPosition = leftHandMovements.focusPoint;
                break;
            case InputDeviceIDs.RightHand:
                targetPosition = rightHandMovements.focusPoint;
                break;
#endif
            default:
                targetPosition = ivr.transform.position;
                break;
        }

        Teleport(targetPosition);
    }

    public LayerMask teleportLayerMask = Physics.DefaultRaycastLayers;

    private void Teleport(Vector3 position) {
        RaycastHit hit;

        // raycast to ensure that we do not teleport into objects
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 1.5F, teleportLayerMask)) {
            position += Vector3.up * ((hit.point.y - ivr.transform.position.y) + 0.05f);
        }

        ivr.transform.position = position;
    }
    #endregion

    public void ControllerPressButtonA() {
        controller0.right.PressButton(ControllerInput.Button.ButtonA);
    }
    public void ControllerPressButtonB() {
        controller0.right.PressButton(ControllerInput.Button.ButtonB);
    }
    public void ControllerPressButtonX() {
        controller0.right.PressButton(ControllerInput.Button.ButtonX);
    }
    public void ControllerPressButtonY() {
        controller0.right.PressButton(ControllerInput.Button.ButtonY);
    }
    public void ControllerPressRightOption() {
        controller0.right.PressButton(ControllerInput.Button.Option);
    }
    public void ControllerPressLeftOption() {
        controller0.left.PressButton(ControllerInput.Button.Option);
    }

    private void CheckQuit() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}