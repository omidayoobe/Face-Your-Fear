/* InstantVR Hand Movements Base
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.8.0
 * date: May 10, 2017
 * 
 * - Fix for collisions with own hands
 */

using UnityEngine;
using System.Collections;

namespace IVR {

    public class IVR_HandMovementsBase : IVR_Movements {
        [HideInInspector]
        public IVR_HandController selectedController;
        [HideInInspector]
        public Transform stretchlessTarget;
        [HideInInspector]
        public Rigidbody handRigidbody;

        public Vector3 storedCOM;
        public Vector3 grabLocation;
        public GameObject grabbedObject = null;

        public override void StartMovements(InstantVR _ivr) {
            base.StartMovements(_ivr);
            Animator animator = ivr.GetComponentInChildren<Animator>();

            if (animator != null) {
                GameObject hand;
                if (this.transform == ivr.leftHandTarget) {
                    hand = animator.GetBoneTransform(HumanBodyBones.LeftHand).gameObject;

                } else {
                    hand = animator.GetBoneTransform(HumanBodyBones.RightHand).gameObject;
                }
                handRigidbody = hand.GetComponentInChildren<Rigidbody>();
                if (handRigidbody == null)
                    handRigidbody = hand.AddComponent<Rigidbody>();

                handRigidbody.mass = 1;
                handRigidbody.drag = 0;
                handRigidbody.angularDrag = 10;
                handRigidbody.useGravity = false;
                handRigidbody.isKinematic = true;
                handRigidbody.interpolation = RigidbodyInterpolation.None;
                handRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }
        public virtual void UpdateAnimation() { }
        public virtual void MoveTo(IVR_HandController handController, Vector3 position, Quaternion rotation) { }
        public virtual IEnumerator LetGoAnimation(IVR_HandController handController) {
            yield return null;
        }
        public virtual void NetworkingGrab(GameObject obj) { }
        public virtual void NetworkingLetGo() { }

    }
}