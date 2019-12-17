using AwesomeTechnologies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// TODO: 
//    ask lennart to make GetMaskCenter public
namespace VegetationStudioProExtensions
{
    [CustomEditor(typeof(VegetationMaskAreaExtension))]
    [CanEditMultipleObjects]
    public class VegetationMaskAreaExtensionEditor : BaseEditor<VegetationMaskAreaExtension>
    {
        private VegetationMaskAreaExtension extension;
        private VegetationMaskArea mask;

        private float resizeFactor = 0.1f;

        public void Awake()
        {
            extension = (VegetationMaskAreaExtension) target;
            mask = extension.GetComponent<VegetationMaskArea>();
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

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Subdivide"))
            {
                Subdivide();
            }
            else if (GUILayout.Button("Unsubdivide"))
            {
                Unsubdivide();
            }
            else if (GUILayout.Button("Circle"))
            {
                CreateCircle();
            }

            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        public void CenterMainHandle()
        {
            Vector3 center = GetMaskCenter();
            Vector3 offset = mask.transform.position - center;

            mask.transform.position = center;

            foreach (Node node in mask.Nodes)
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

            foreach (Node node in mask.Nodes)
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
            mask.UpdateVegetationMask();
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

        /// <summary>
        /// Insert a new node between every node segment of the area
        /// </summary>
        private void Subdivide()
        {
            List<Node> originalNodes = new List<Node>();
            originalNodes.AddRange(mask.Nodes);

            for (var i = 0; i < originalNodes.Count; i++)
            {
                Node curr = originalNodes[i];
                Node next = mask.GetNextNode(curr);

                Vector3[] segment = new Vector3[] { curr.Position, next.Position };

                Vector3 meanVector = GetMeanVector(segment);

                int index = mask.GetNodeIndex(curr);

                Node newNode = new Node()
                {
                    Position = meanVector
                };

                mask.Nodes.Insert(index + 1, newNode);
            }

            UpdateMask();
        }

        /// <summary>
        /// Remove every 2nd node
        /// </summary>
        private void Unsubdivide()
        {
            // ensure there is at least the specified number of nodes left
            int minimumNodeCount = 3;

            if (mask.Nodes.Count <= minimumNodeCount)
                return;

            int count = mask.Nodes.Count;
            for (var i = mask.Nodes.Count - 1; i >= 0; i -= 2)
            {
                mask.Nodes.RemoveAt(i);

                if (mask.Nodes.Count < minimumNodeCount)
                    break;

            }

            UpdateMask();
        }

        /// <summary>
        /// Distribute the nodes along a circle
        /// </summary>
        private void CreateCircle()
        {

            Vector3 maskCenter = GetMaskCenter();
            List<Vector3> worldPositions = mask.GetWorldSpaceNodePositions();

            // calculate the radius by using the average distance from the mask center
            float magnitudeSum = 0f;
            foreach (Vector3 worldPosition in worldPositions)
            {
                magnitudeSum += (worldPosition - maskCenter).magnitude;
            }

            float radius = magnitudeSum / mask.Nodes.Count;


            // distribute along the circle
            int count = mask.Nodes.Count;
            for (int i = 0; i < count; i++)
            {
                Node node = mask.Nodes[i];

                float angle = i * Mathf.PI * 2f / count;

                Vector3 newPosition = new Vector3(Mathf.Cos(angle) * radius, node.Position.y, Mathf.Sin(angle) * radius);

                node.Position = newPosition;
            }

            UpdateMask();

        }
    }
}
