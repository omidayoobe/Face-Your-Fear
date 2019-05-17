using UnityEngine;
using IVR;

public class IVR_SphereReticle : IVR_Reticle {
    void Update() {
        if (gazePhase < 1)
            transform.localScale = Vector3.one * (1 - gazePhase) * 0.1F;
        else
            transform.localScale = Vector3.one * 0.1F;
    }
}
