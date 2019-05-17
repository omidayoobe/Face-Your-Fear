/* InstantVR Generic functions
 * Copyright (c) 2016 by Passer VR
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.3
 * date: May 22, 2016
 *
 * - Added Normalize Vector3
 */
using UnityEngine;

namespace IVR {

    public static class Angles {
        // Clamp all vector axis between the given min and max values
        // Angles are normalized
        public static Vector3 ClampVector3(Vector3 angles, Vector3 min, Vector3 max) {
            float x = Clamp(angles.x, min.x, max.x);
            float y = Clamp(angles.y, min.y, max.y);
            float z = Clamp(angles.z, min.z, max.z);
            return new Vector3(x, y, z);
        }

        // clamp the angle between the given min and max values
        // Angles are normalized
        public static float Clamp(float angle, float min, float max) {
            float normalizedAngle = Normalize(angle);
            return Mathf.Clamp(normalizedAngle, min, max);
        }

        // Determine the angle difference, result is a normalized angle
        public static float Difference(float a, float b) {
            float r = Normalize(b - a);
            return r;
        }

        // Normalize an angle to the range -180 < a <= 180
        public static float Normalize(float a) {
            while (a <= -180) a += 360;
            while (a > 180) a -= 360;
            return a;
        }

        // Normalize all vector axis to the range -180 < a <= 180
        public static Vector3 Normalize(Vector3 angles) {
            float x = Normalize(angles.x);
            float y = Normalize(angles.y);
            float z = Normalize(angles.z);
            return new Vector3(x, y, z);
        }
    }

    public static class Transforms {
        // transform local rotation to world rotation
        public static Quaternion TransformRotation(Transform transform, Quaternion localRotation) {
            if (transform.parent == null)
                return localRotation;
            else
                return transform.parent.rotation * localRotation;
        }

        //
        // Summary:
        //     ///
        //     Transforms rotation from local space to world space.
        //     ///
        //
        // Parameters:
        //   transform:
        public static Quaternion InverseTransformRotation(Transform transform, Quaternion rotation) {
            if (transform.parent == null)
                return rotation;
            else
                return Quaternion.Inverse(transform.parent.rotation) * rotation;
        }
    }
}