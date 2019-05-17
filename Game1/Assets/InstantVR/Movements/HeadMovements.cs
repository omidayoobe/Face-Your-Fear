/* InstantVR Head Movements
 * Copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.7.0
 * date: December 2, 2016
 *
 * - New gaze selection using InputModule
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace IVR {

    public class HeadMovements : IVR_Movements {
        public enum InteractionType {
            None,
            Gazing,
        }

        public InteractionType interaction;
        public float autoActivation = 0;
        public ControllerInput.Button activationButton = ControllerInput.Button.ButtonA;
        public ControllerInput.Side controllerSide = ControllerInput.Side.Right;
        public GameObject focusPointObj;

        public GameObject lookingAtObject;
        [HideInInspector]
        public Vector3 focusPoint;

        protected Transform headcam;

        private IVR_Interaction inputModule;

        public override void StartMovements(InstantVR ivr) {
            base.StartMovements(ivr);

            Camera headCamera = ivr.GetComponentInChildren<Camera>();
            headcam = headCamera.transform;

            if (interaction != InteractionType.None) {
                inputModule = ivr.GetComponent<IVR_Interaction>();
                if (inputModule == null) {
                    EventSystem eventSystem = FindObjectOfType<EventSystem>();
                    if (eventSystem != null)
                        DestroyImmediate(eventSystem.gameObject);
                    inputModule = ivr.gameObject.AddComponent<IVR_Interaction>();
                }

                inputModule.EnableGazeInputModule(headcam, controllerSide, activationButton, autoActivation);
            }

            if (interaction != InteractionType.None && focusPointObj == null) {
                focusPointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                focusPointObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                Destroy(focusPointObj.GetComponent<Collider>());
            }
        }

        protected float lastFocus;

        public override void UpdateMovements() {
            if (interaction != InteractionType.None) {
                inputModule.ProcessPointer(InputDeviceIDs.Head);
                lookingAtObject = inputModule.GetFocusObject(InputDeviceIDs.Head);
                focusPoint = inputModule.GetFocusPoint(InputDeviceIDs.Head);
                if (focusPointObj != null) {
                    focusPointObj.transform.position = focusPoint;
                }

                IVR_Reticle reticle = focusPointObj.GetComponent<IVR_Reticle>();
                if (reticle != null) {
                    reticle.gazePhase = Mathf.Clamp01(inputModule.GetGazeDuration(InputDeviceIDs.Head) / autoActivation);
                }

            }

            base.UpdateMovements();
        }
    }

}