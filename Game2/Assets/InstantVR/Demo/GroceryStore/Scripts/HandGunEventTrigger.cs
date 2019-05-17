using UnityEngine;
using UnityEngine.EventSystems;
using IVR;

public class HandGunEventTrigger : EventTrigger {
    public override void OnPointerClick(PointerEventData eventData) {
        base.OnPointerClick(eventData);

#if INSTANTVR_ADVANCED
        if (eventData.currentInputModule.GetType() == typeof(IVR_Interaction)) {
            InteractionEventData interactionData = (InteractionEventData) eventData;
            InstantVR ivr = eventData.currentInputModule.transform.GetComponent<InstantVR>();

            switch (interactionData.inputDevice) {
                case InputDeviceIDs.LeftHand:
                    if (ivr.leftHandMovements.GetType() == typeof(IVR_HandMovements)) {
                        IVR_HandMovements handMovements = (IVR_HandMovements) ivr.leftHandMovements;
                        handMovements.NetworkingGrab(this.gameObject);
                    }
                    break;
                case InputDeviceIDs.RightHand:
                    if (ivr.rightHandMovements.GetType() == typeof(IVR_HandMovements)) {
                        IVR_HandMovements handMovements = (IVR_HandMovements) ivr.rightHandMovements;
                        handMovements.NetworkingGrab(this.gameObject);
                    }
                    break;
            }
        }
#endif
    }
}
