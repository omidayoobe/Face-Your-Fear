/* InstantVR Head Movements editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.1
 * date: Februari 13, 2016
 * 
 * - updates for Edge version
 */
/*
using UnityEngine;
 using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(HeadMovements))]
    public class HeadMovements_Editor : Editor {

        //static private HeadMovements headMovements;
        static private bool showLeftEye = true;
        static private bool showRightEye = true;

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            //HeadMovements headMovements = (HeadMovements)target;
        }
        
        private SkinnedMeshRenderer[] FindAvatarMeshes(HeadMovements headMovements) {
            InstantVR[] ivrs = FindObjectsOfType<InstantVR>();
            InstantVR ivr = null;

            for (int i = 0; i < ivrs.Length; i++)
                if (ivrs[i].headTarget == headMovements.transform)
                    ivr = ivrs[i];

            Transform avatar = ivr.transform.GetComponentInChildren<Animator>().transform;
            SkinnedMeshRenderer[] renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            Mesh[] meshes = new Mesh[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
                meshes[i] = renderers[i].sharedMesh;
            return renderers;
        }

        private string[] DistillAvatarMeshNames(SkinnedMeshRenderer[] meshes) {
            string[] names = new string[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
                names[i] = meshes[i].name;

            return names;
        }

        private int FindMeshWithBlendshapes(SkinnedMeshRenderer[] renderers) {
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i].sharedMesh.blendShapeCount > 0)
                    return i;

            return 0;
        }

        private string[] GetBlendshapes(SkinnedMeshRenderer renderer) {
            string[] blendShapes = new string[renderer.sharedMesh.blendShapeCount];
            for (int i = 0; i < blendShapes.Length; i++) {
                blendShapes[i] = renderer.sharedMesh.GetBlendShapeName(i);
            }
            return blendShapes;
        }

        private void FindBlendshapeWith(string[] blendshapes, string namepart1, string namepart2, ref int blendshape ) {
            for (int i = 0; i < blendshapes.Length; i++) {
                if (blendshapes[i].Contains(namepart1) && blendshapes[i].Contains(namepart2))
                    blendshape = i;
            }
        }
        
    }
}
*/