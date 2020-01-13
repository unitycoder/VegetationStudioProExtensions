using AwesomeTechnologies;
using AwesomeTechnologies.Utility;
using System.Collections.Generic;
using UnityEngine;
using Node = AwesomeTechnologies.Node;

namespace VegetationStudioProExtensions
{
    public class VegetationMaskUtils
    {
        /// <summary>
        /// Force update on the mask in scene view
        /// </summary>
        /// <param name="mask"></param>
        public static void UpdateMask(VegetationMaskArea mask)
        {
            // TODO: that's just a quick hack to update the mask. mask.PositionNodes(); doesn't seem to work and mask.Update() has an optimization in it
            mask.transform.position = mask.transform.position + new Vector3(0, 1, 0);
            mask.transform.position = mask.transform.position + new Vector3(0, -1, 0);

            // apply the mask to the vegetation
            mask.UpdateVegetationMask();
        }

        /// <summary>
        /// Move the main handle into the center of the maks polygon
        /// </summary>
        /// <param name="mask"></param>
        public static void CenterMainHandle(VegetationMaskArea mask)
        {
            Vector3 center = GetMaskCenter( mask);
            Vector3 offset = mask.transform.position - center;

            mask.transform.position = center;

            foreach ( Node node in mask.Nodes)
            {
                Vector3 worldPos = mask.transform.TransformPoint(node.Position);

                worldPos += offset;

                node.Position = mask.transform.InverseTransformPoint(worldPos);
            }

            UpdateMask(mask);
        }

        /// <summary>
        /// Get the center of the mask polygon
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static Vector3 GetMaskCenter(VegetationMaskArea mask)
        {
            List<Vector3> worldPositions = mask.GetWorldSpaceNodePositions();
            return PolygonUtils.GetMeanVector(worldPositions.ToArray());
        }

        /// <summary>
        /// Grow the mask by the given factor
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="factor"></param>
        public static void Grow(VegetationMaskArea mask, float factor)
        {
            ResizeMask(mask, Mathf.Abs(factor));
        }

        /// <summary>
        /// Shrink the mask by the given factor
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="factor"></param>
        public static void Shrink(VegetationMaskArea mask, float factor)
        {
            ResizeMask(mask, -Mathf.Abs(factor));
        }

        /// <summary>
        /// Resize the mask
        /// </summary>
        /// <param name="factor">Positive values for growing, negative for shrinking. 0.1 means grow by 10%, -0.1 means shrink by 10%</param>
        public static void ResizeMask(VegetationMaskArea mask, float factor)
        {
            Vector3 center = GetMaskCenter(mask);

            foreach ( Node node in mask.Nodes)
            {

                Vector3 worldPos = mask.transform.TransformPoint(node.Position);

                Vector3 distance = worldPos - center;

                worldPos += distance * factor;

                node.Position = mask.transform.InverseTransformPoint(worldPos);

            }

            UpdateMask(mask);

        }

        /// <summary>
        /// Insert a new node between every node segment of the area
        /// </summary>
        public static void Subdivide(VegetationMaskArea mask)
        {
            List<Node> originalNodes = new List<Node>();
            originalNodes.AddRange(mask.Nodes);

            for (var i = 0; i < originalNodes.Count; i++)
            {
                Node curr = originalNodes[i];
                Node next = mask.GetNextNode(curr);

                Vector3[] segment = new Vector3[] { curr.Position, next.Position };

                Vector3 meanVector = PolygonUtils.GetMeanVector(segment);

                int index = mask.GetNodeIndex(curr);

                Node newNode = new Node()
                {
                    Position = meanVector
                };

                mask.Nodes.Insert(index + 1, newNode);
            }

            UpdateMask(mask);
        }

        /// <summary>
        /// Remove every 2nd node
        /// </summary>
        public static void Unsubdivide(VegetationMaskArea mask)
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

            UpdateMask(mask);
        }

        /// <summary>
        /// Distribute the nodes along a circle
        /// </summary>
        public static void CreateCircle(VegetationMaskArea mask)
        {
            float radius = GetRadius(mask);

            Vector3[] hexagon = ShapeCreator.CreateCircle(mask.transform.position, radius, mask.Nodes.Count);

            mask.Nodes.Clear();
            mask.AddNodesToEnd(hexagon);

            // center main handle, implicitly updates the mask
            CenterMainHandle(mask);

        }

        public static float GetRadius(VegetationMaskArea mask)
        {
            Vector3 maskCenter = GetMaskCenter(mask);
            List<Vector3> worldPositions = mask.GetWorldSpaceNodePositions();

            // calculate the radius by using the average distance from the mask center
            float magnitudeSum = 0f;
            foreach (Vector3 worldPosition in worldPositions)
            {
                magnitudeSum += (worldPosition - maskCenter).magnitude;
            }

            float radius = magnitudeSum / mask.Nodes.Count;

            return radius;
        }

        public static void CreateHexagon(VegetationMaskArea mask)
        {

            float radius = GetRadius(mask);

            Vector3[] hexagon = ShapeCreator.CreateHexagon(mask.transform.position, radius);

            mask.Nodes.Clear();
            mask.AddNodesToEnd(hexagon);

            // center main handle, implicitly updates the mask
            CenterMainHandle(mask);
        }

        /// <summary>
        /// Transform the mask and convert it into its convex hull.
        /// </summary>
        /// <param name="mask"></param>
        public static void ConvexHull(VegetationMaskArea mask)
        {
            Vector3 maskPosition = mask.transform.position;

            List<Vector2> positionsXY = mask.Nodes.ConvertAll<Vector2>(item => new Vector2(item.Position.x, item.Position.z));

            List<Vector2> convexHull = PolygonUtility.GetConvexHull(positionsXY);

            mask.Nodes.Clear();
            foreach (Vector2 nodePosition in convexHull)
            {
                mask.AddNodeToEnd(new Vector3(maskPosition.x + nodePosition.x, 0, maskPosition.z + nodePosition.y));
            }

            // center main handle, implicitly updates the mask
            CenterMainHandle(mask);
        }

        /// <summary>
        /// Create a list of node positions.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static List<Vector3> GetPositions(VegetationMaskArea mask)
        {
            return mask.Nodes.ConvertAll<Vector3>(item => new Vector3(item.Position.x, item.Position.y, item.Position.z));
        }
    }
}
