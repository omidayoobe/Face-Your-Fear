/* InstantVR Input Module
 * Copyright (c) 2016 by Passer VR
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.8.0
 * date: May 10, 2017
 * 
 * - Fixed pointing interaction when finger distal does not have a child
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace IVR {

    public enum InputDeviceIDs {
        Head,
        LeftHand,
        RightHand,
        None
    }

    public class InteractionEventData : PointerEventData {
        public InteractionEventData(EventSystem eventSystem) : base(eventSystem) { }

        public InputDeviceIDs inputDevice;
    }

    public class IVR_Interaction : BaseInputModule {

        private enum PointerType {
            Gaze,
            Touch
        }

        private class InteractionPointer {
            public Transform pointerTransform;
            public Vector3 localPointingDirection;
            public InteractionEventData data;
            public Vector2 previousPosition;

            public bool focusingEnabled = false;
            public bool focusing;
            public GameObject focusObject;
            public GameObject previousFocusObject;
            public Vector3 focusPosition;
            public float focusStart;
            public float focusTimeToTouch = 2;

            public bool touching = false;
            public GameObject touchedObject;
            public Vector3 touchPosition;

            public ControllerInputSide controllerInputSide;
            public ControllerInput.Button controllerButton;

            public bool hasClicked;

            public PointerType type;

            public InteractionPointer(PointerType type, Transform pointerTransform, EventSystem eventSystem) {
                this.type = type;
                this.pointerTransform = pointerTransform;
                this.data = new InteractionEventData(eventSystem);
            }

            public void ProcessFocus() {
                if (!focusingEnabled)
                    return;

                if (focusObject != previousFocusObject) {
                    if (previousFocusObject != null)
                        ExecuteEvents.ExecuteHierarchy(previousFocusObject, data, ExecuteEvents.pointerExitHandler);
                    if (focusObject != null) {
                        focusing = ExecuteEvents.ExecuteHierarchy(focusObject, data, ExecuteEvents.pointerEnterHandler);
                        focusStart = Time.time;
                        hasClicked = false;
                    }
                    previousFocusObject = focusObject;
                }
            }

            public void ProcessTouch() {
                if (!touching) { // first touch
                    touchedObject = data.pointerCurrentRaycast.gameObject;
                    if (touchedObject == null) // object is a 3D object, as we do not use Physicsraycaster, use the focusObject
                        touchedObject = focusObject;
                    touching = ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerDownHandler);

                } else { // we were already touching
                    if (data.delta.sqrMagnitude > 0) { // moved finger during touch
                        GameObject pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(data.pointerCurrentRaycast.gameObject);
                        if (!data.dragging) { // we were not dragging yet
                            if (pointerDrag != null) { // start dragging only where there is something to drag
                                data.pointerDrag = pointerDrag;
                                data.dragging = ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.beginDragHandler);
                            }
                        } else { // still dragging
                            ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.dragHandler);
                        }

                    } else { // finger did not move
                        if (data.dragging) { // we were dragging
                            ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.endDragHandler);
                            data.dragging = false;
                        }
                    }
                }
            }

            public void ProcessNoTouch() {
                if (touching) { // We were touching
                    if (data.dragging) {
                        ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.endDragHandler);
                        data.dragging = false;
                    }
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerClickHandler);
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerUpHandler);
                    touching = false;
                    touchedObject = null;
                }
            }

            public void ProcessClick() {
                if (hasClicked)
                    return;

                if (touchedObject != null) {
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerClickHandler);
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerUpHandler);
                } else {
                    ExecuteEvents.ExecuteHierarchy(focusObject, data, ExecuteEvents.pointerClickHandler);
                    ExecuteEvents.ExecuteHierarchy(focusObject, data, ExecuteEvents.pointerUpHandler);
                }
                hasClicked = true;
            }
        }
        private InteractionPointer[] pointers;
#if INSTANTVR_ADVANCED
#region FingerInput
        public void EnableFingerInputModule(IVR_HandMovements handMovements, bool isLeft, bool touch, float autoActivation) {
            if (pointers == null)
                pointers = new InteractionPointer[4]; // 0 = left index, 1 = right index, 2 = head, 3 = none;

            ControllerInput controllerInput = Controllers.GetController(0);

            Transform indexFingerDistal = null;
            InteractionPointer pointer = null;
            if (isLeft) {
                indexFingerDistal = handMovements.animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
                if (indexFingerDistal != null) {
                    if (indexFingerDistal.childCount > 0)
                        pointer = new InteractionPointer(touch ? PointerType.Touch : PointerType.Gaze, indexFingerDistal.GetChild(0), eventSystem);
                    else
                        pointer = new InteractionPointer(touch ? PointerType.Touch : PointerType.Gaze, indexFingerDistal, eventSystem);
                    pointers[(int) InputDeviceIDs.LeftHand] = pointer;
                }

                if (controllerInput != null)
                    pointer.controllerInputSide = controllerInput.left;
            } else {
                indexFingerDistal = handMovements.animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
                if (indexFingerDistal != null) {
                    if (indexFingerDistal.childCount > 0) 
                        pointer = new InteractionPointer(touch ? PointerType.Touch : PointerType.Gaze, indexFingerDistal.GetChild(0), eventSystem);
                    else
                        pointer = new InteractionPointer(touch ? PointerType.Touch : PointerType.Gaze, indexFingerDistal, eventSystem);
                    pointers[(int) InputDeviceIDs.RightHand] = pointer;
                }

                if (controllerInput != null)
                    pointer.controllerInputSide = controllerInput.right;
            }

            if (indexFingerDistal != null) {
                if (indexFingerDistal.childCount > 0) {
                    Transform indexFingerTip = indexFingerDistal.GetChild(0);
                    pointer.localPointingDirection = indexFingerTip.InverseTransformDirection(indexFingerTip.position - indexFingerDistal.position).normalized;
                } else {
                    pointer.localPointingDirection = indexFingerDistal.InverseTransformDirection(indexFingerDistal.position - indexFingerDistal.parent.position).normalized;
                }
            }
            pointer.focusTimeToTouch = autoActivation;
        }

        public void EnableFingerPointing(bool isPointing, bool isLeft) {
            InteractionPointer pointer = isLeft ? pointers[(int) InputDeviceIDs.LeftHand] : pointers[(int) InputDeviceIDs.RightHand];
            
            if (pointer != null) {
                pointer.focusingEnabled = isPointing; // only focusing when we are pointing
            }
        }

        public void OnFingerTouchStart(bool isLeft, GameObject obj) {
            InteractionPointer pointer = isLeft ? pointers[(int) InputDeviceIDs.LeftHand] : pointers[(int) InputDeviceIDs.RightHand];

            pointer.focusObject = obj;
            pointer.data.delta = Vector3.zero;
            pointer.ProcessTouch();
        }

        public void OnFingerTouchEnd(bool isLeft) {
            InteractionPointer pointer = isLeft ? pointers[(int) InputDeviceIDs.LeftHand] : pointers[(int) InputDeviceIDs.RightHand];

            pointer.ProcessNoTouch();
        }
#endregion
#endif
#region HeadInput
        public void EnableGazeInputModule(Transform cameraTransform, ControllerInput.Side inputSide, ControllerInput.Button activationButton, float autoActivation) {
            if (pointers == null)
                pointers = new InteractionPointer[3]; // 0 = left index, 1 = right index, 2 = head

            InteractionPointer pointer = new InteractionPointer(PointerType.Gaze, cameraTransform, eventSystem);
            pointer.focusingEnabled = true; // gaze is always focusing
            pointer.localPointingDirection = Vector3.forward;
            pointer.focusTimeToTouch = autoActivation;

            ControllerInput controllerInput = Controllers.GetController(0);
            if (controllerInput != null)
                pointer.controllerInputSide = (inputSide == ControllerInput.Side.Left) ? controllerInput.left : controllerInput.right;
            pointer.controllerButton = activationButton;

            pointers[(int) InputDeviceIDs.Head] = pointer;
        }
#endregion

        public Vector3 GetFocusPoint(InputDeviceIDs inputDeviceID) {
            return pointers[(int) inputDeviceID].focusPosition;
        }

        public GameObject GetFocusObject(InputDeviceIDs inputDeviceID) {
            if (pointers[(int) inputDeviceID].focusObject != null)
                return pointers[(int) inputDeviceID].focusObject;
            else
                return null;
        }

        public Vector3 GetTouchPoint(InputDeviceIDs inputDeviceID) {
            return pointers[(int) inputDeviceID].touchPosition;
        }

        public GameObject GetTouchObject(InputDeviceIDs inputDeviceID) {
            if (pointers[(int) inputDeviceID].touchedObject != null) {
                return pointers[(int) inputDeviceID].touchedObject;
            } else
                return null;
        }

        public float GetGazeDuration(InputDeviceIDs inputDeviceID) {
            return Time.time - pointers[(int) inputDeviceID].focusStart;
        }

        // This function is only being called when the game view has focus :(
        public override void Process() {
        }

        public void Update() {
            for (int i = 0; i < pointers.Length; i++) {
                if (pointers[i] != null)
                    ProcessPointer(pointers[i], (InputDeviceIDs) i);
            }
        }

        public void ProcessPointer(InputDeviceIDs inputDeviceID) {
            ProcessPointer(pointers[(int) inputDeviceID], inputDeviceID);
        }

        private void ProcessPointer(InteractionPointer pointer, InputDeviceIDs inputDeviceID) {
            CastRayFromPointer(pointer, inputDeviceID);

            pointer.ProcessFocus();
            if (pointer.focusObject != null && pointer.focusTimeToTouch != 0 && Time.time - pointer.focusStart > pointer.focusTimeToTouch) { // we are clicking
                pointer.ProcessClick();
            }

            if (pointer.type == PointerType.Gaze) {
                if (pointer.controllerInputSide.GetButton(pointer.controllerButton)) { // we are touching
                    pointer.ProcessTouch();
                } else {
                    pointer.ProcessNoTouch();
                }
            }

            if (pointer.data.pointerCurrentRaycast.gameObject == null) { // no focus
                return;
            }

            //Trigger Enter or Exit Events on the UI Element (like highlighting)
            base.HandlePointerExitAndEnter(pointer.data, pointer.data.pointerCurrentRaycast.gameObject);

            pointer.data.pressPosition = pointer.data.position;
            pointer.data.pointerPressRaycast = pointer.data.pointerCurrentRaycast;
            pointer.data.pointerPress = null; //Clear this for setting later
            pointer.data.useDragThreshold = true;

            if (pointer.type == PointerType.Touch) {
                float distance = DistanceTipToTransform(pointer.pointerTransform, pointer.data.pointerCurrentRaycast.gameObject.transform);
                if (distance < 0) { // we are touching
                    pointer.ProcessTouch();
                } else {
                    pointer.ProcessNoTouch();
                }
            }

            if (pointer.controllerInputSide.GetButton(pointer.controllerButton)) { // we are clicking
                pointer.ProcessTouch();
            } else {
                pointer.ProcessNoTouch();
            }
        }

        private void CastRayFromPointer(InteractionPointer pointer, InputDeviceIDs inputDeviceID) {
            pointer.data.Reset();
            pointer.data.inputDevice = inputDeviceID;

            Vector3 pointingDirection = pointer.pointerTransform.rotation * pointer.localPointingDirection;

            if (pointer.focusingEnabled) {
                RaycastHit hit;
                bool raycastHit = Physics.Raycast(pointer.pointerTransform.position, pointingDirection, out hit);
                if (raycastHit) {
                    pointer.focusPosition = hit.point;
                    pointer.focusObject = hit.transform.gameObject;
                } else {
                    pointer.focusPosition = pointer.pointerTransform.position + pointingDirection * 10;
                    pointer.focusObject = null;
                }

                pointer.data.position = Camera.main.WorldToScreenPoint(pointer.focusPosition);
            } else {
                pointer.focusPosition = pointer.pointerTransform.position;
                pointer.focusObject = null;

                pointer.data.position = Camera.main.WorldToScreenPoint(pointer.pointerTransform.position);
            }

            pointer.data.scrollDelta = Vector2.zero;
            pointer.data.delta = pointer.data.position - pointer.previousPosition;
            pointer.previousPosition = pointer.data.position;

            eventSystem.RaycastAll(pointer.data, m_RaycastResultCache);
            
            pointer.data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();

            if (pointer.data.pointerCurrentRaycast.gameObject != null) { // we are focusing on a UI element
                pointer.focusObject = pointer.data.pointerCurrentRaycast.gameObject;
                // EventSystem.RaycastAll always casts from main.camera. This is why we need a trick to look like it is casting from the pointerlocation (e.g. finger)
                // The result does not look right in scene view, but does look OK in game view
                Vector3 focusDirection = (pointer.focusPosition - Camera.main.transform.position).normalized;
                pointer.focusPosition = Camera.main.transform.position + focusDirection * pointer.data.pointerCurrentRaycast.distance; // pointer.data.pointerCurrentRaycast.worldPosition == Vector.zero unfortunately
                pointer.data.position = pointer.focusPosition;
            }

            pointer.touchPosition = pointer.data.pointerCurrentRaycast.worldPosition;
        }


        private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords) {
            cartCoords.Normalize();
            if (cartCoords.x == 0)
                cartCoords.x = Mathf.Epsilon;
            float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
            if (cartCoords.x < 0)
                outPolar += Mathf.PI;
            float outElevation = Mathf.Asin(cartCoords.y);
            return new Vector2(outPolar, outElevation);
        }

        private float DistanceTipToTransform(Transform fingerTip, Transform transform) {
            return (-transform.InverseTransformPoint(fingerTip.position).z * transform.lossyScale.z) - 0.005F;
        }
    }
}