using AwesomeTechnologies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem.Biomes;


// TODO: 
//    ask lennart to make GetMaskCenter public
namespace VegetationStudioProExtensions
{
    [CustomEditor(typeof(BiomeMaskAreaExtension))]
    [CanEditMultipleObjects]
    public class BiomeMaskAreaExtensionEditor : BaseEditor<BiomeMaskAreaExtension>
    {
        private BiomeMaskAreaExtension extension;
        private BiomeMaskArea mask;

        private float resizeFactor = 0.1f;

        public void Awake()
        {
            extension = (BiomeMaskAreaExtension) target;
            mask = extension.GetComponent<BiomeMaskArea>();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Center"))
            {
                CenterMainHandle();
            }
            else if (GUILayout.Button("Grow"))
            {
                ResizeMask(resizeFactor);
            }
            else if (GUILayout.Button("Shrink"))
            {
                ResizeMask(-resizeFactor);
            }

            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        public void CenterMainHandle()
        {
            Vector3 center = GetMaskCenter();
            Vector3 offset = mask.transform.position - center;

            mask.transform.position = center;

            foreach (AwesomeTechnologies.VegetationSystem.Biomes.Node node in mask.Nodes)
            {
                Vector3 worldPos = mask.transform.TransformPoint(node.Position);

                worldPos += offset;

                node.Position = mask.transform.InverseTransformPoint(worldPos);
            }

            UpdateMask();
        }

        /// <summary>
        /// Resize the mask
        /// </summary>
        /// <param name="factor">Positive values for growing, negative for shrinking. 0.1 means grow by 10%, -0.1 means shrink by 10%</param>
        public void ResizeMask( float factor)
        {

            Vector3 ori = mask.transform.position;

            Vector3 center = GetMaskCenter();
            Vector3 offset = mask.transform.position - center;

            foreach (AwesomeTechnologies.VegetationSystem.Biomes.Node node in mask.Nodes)
            {
                
                Vector3 worldPos = mask.transform.TransformPoint(node.Position);

                Vector3 distance = worldPos - center;

                worldPos += distance * factor;

                node.Position = mask.transform.InverseTransformPoint(worldPos);

            }

            UpdateMask();

        }

        private void UpdateMask()
        {
            // TODO: that's just a quick hack to update the mask. mask.PositionNodes(); doesn't seem to work and mask.Update() has an optimization in it
            mask.transform.position = mask.transform.position + new Vector3(0, 1, 0);
            mask.transform.position = mask.transform.position + new Vector3(0, -1, 0);

            // apply the mask to the vegetation
            mask.UpdateBiomeMask();
        }

        private Vector3 GetMaskCenter()
        {
            List<Vector3> worldPositions = mask.GetWorldSpaceNodePositions();
            return (GetMeanVector(worldPositions.ToArray()));
        }

        private Vector3 GetMeanVector(Vector3[] positions)
        {
            if (positions.Length == 0)
                return Vector3.zero;
            float x = 0f;
            float y = 0f;
            float z = 0f;
            foreach (Vector3 pos in positions)
            {
                x += pos.x;
                y += pos.y;
                z += pos.z;
            }
            return new Vector3(x / positions.Length, y / positions.Length, z / positions.Length);
        }
    }
}