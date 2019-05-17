/* InstantVR Movements
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.3.1
 * date: February 5, 2016
 *
 * - added ivr variable
 */

using UnityEngine;

namespace IVR {
    public class IVR_Movements : MonoBehaviour {
        [HideInInspector]
        public InstantVR ivr;

        public virtual void StartMovements(InstantVR _ivr) {
            ivr = _ivr;
        }

        public virtual void UpdateMovements() { }
    }
}