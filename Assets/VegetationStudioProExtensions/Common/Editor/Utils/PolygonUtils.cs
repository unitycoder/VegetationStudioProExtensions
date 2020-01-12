using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class PolygonUtils
    {

        #region Bounds

        /// <summary>
        /// Get a random point within the bounds on the X/Z plane
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointXZ(Bounds bounds)
        {
            float xmin = bounds.center.x - bounds.extents.x;
            float xmax = bounds.center.x + bounds.extents.x;

            float zmin = bounds.center.z - bounds.extents.z;
            float zmax = bounds.center.z + bounds.extents.z;

            return new Vector2(UnityEngine.Random.Range(xmin, xmax), UnityEngine.Random.Range(zmin, zmax));
        }

        /// <summary>
        /// Shift and resize the bounds within its bounds. Makes the partitions look less rectangularly distributed.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Bounds ShiftResizeBounds(Bounds bounds, float boundsShiftFactorMin, float boundsShiftFactorMax)
        {
            float minFactor = boundsShiftFactorMin;
            float maxFactor = boundsShiftFactorMax;

            float factorX = UnityEngine.Random.Range(minFactor, maxFactor);
            float factorZ = UnityEngine.Random.Range(minFactor, maxFactor);

            float shiftDirectionX = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
            float shiftDirectionZ = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;

            float sizeX = bounds.size.x * factorX;
            float sizeZ = bounds.size.z * factorZ;

            float centerX = bounds.center.x + shiftDirectionX * (bounds.size.x - sizeX) / 2;
            float centerZ = bounds.center.z + shiftDirectionZ * (bounds.size.z - sizeZ) / 2;

            bounds = new Bounds(new Vector3(centerX, 0, centerZ), new Vector3(sizeX, 0, sizeZ));

            return bounds;
        }

        /// <summary>
        /// Create a polygon which consists of the bounding box XZ axis values
        /// </summary>
        /// <returns></returns>
        public static Vector2[] CreatePolygonXZ(Bounds bounds)
        {
            // clip polygon, bounding box
            float left = bounds.center.x - bounds.extents.x;
            float top = bounds.center.z - bounds.extents.z;
            float right = bounds.center.x + bounds.extents.x;
            float bottom = bounds.center.z + bounds.extents.z;

            Vector2[] clipPolygon = new Vector2[] { new Vector2(left, top), new Vector2(right, top), new Vector2(right, bottom), new Vector2(left, bottom) };

            return clipPolygon;
        }

        /// <summary>
        /// Get the bounds for the given positions
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Bounds GetBounds( Vector3[] positions)
        {
            Bounds bounds = new Bounds();

            foreach( Vector3 position in positions)
            {
                bounds.Encapsulate(position);
            }

            return bounds;
        }

        #endregion Bounds

        #region Vector

        /// <summary>
        /// Get the mean vector of the specified polygon
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector3 GetMeanVector(Vector3[] positions)
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
        /// Get the mean vector of the specified polygon
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector3 GetMeanVector(List<Vector3> positions)
        {
            return GetMeanVector(positions.ToArray());
        }

        /// <summary>
        /// Subdivide the given polygon
        /// </summary>
        /// <param name="sourcePolygon"></param>
        /// <returns></returns>
        public static List<Vector3> Subdivide(List<Vector3> sourcePolygon)
        {
            List<Vector3> subdividedPolygon = new List<Vector3>();

            for (var i = 0; i < sourcePolygon.Count; i++)
            {

                int currIndex = i;
                int nextIndex = i + 1;
                if (nextIndex == sourcePolygon.Count)
                {
                    nextIndex = 0;
                }

                Vector3 curr = sourcePolygon[currIndex];
                Vector3 next = sourcePolygon[nextIndex];

                Vector3[] segment = new Vector3[] { curr, next };

                Vector3 meanVector = GetMeanVector(segment);

                subdividedPolygon.Add(curr);
                subdividedPolygon.Add(meanVector);

            }

            return subdividedPolygon;

        }

        /// <summary>
        /// Sort the points of the polygon in clockwise order.
        /// </summary>
        /// <param name="polygon"></param>
        public static void SortClockWise(List<Vector3> polygon)
        {
            ClockwiseComparer comparer = new ClockwiseComparer(polygon);
            polygon.Sort(comparer);
        }

        /// <summary>
        /// Find the intersection of the line segments [p1,p2] to [p3,p4]
        /// 
        /// Credits to setchi:
        /// https://github.com/setchi/Unity-LineSegmentsIntersection/blob/master/Assets/LineSegmentIntersection/Scripts/Math2d.cs
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="intersection">The intersection point if there is any</param>
        /// <returns>true if the lines intersect, false otherwise</returns>
        public static bool LineSegmentsIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersection)
        {
            intersection = Vector3.zero;

            var d = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);

            if (d == 0.0f)
            {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.z - p3.z) - (p3.z - p1.z) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.z - p1.z) - (p3.z - p1.z) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.z = p1.z + u * (p2.z - p1.z);

            return true;
        }
        #endregion Vector

        #region Polygon

        /// <summary>
        /// Return True if the polygon is convex.
        /// Source: http://csharphelper.com/blog/2014/07/determine-whether-a-polygon-is-convex-in-c/
        /// Credits: Rod Stephens
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static bool PolygonIsConvex( Vector2[] points)
        {
            // For each set of three adjacent points A, B, C,
            // find the cross product AB · BC. If the sign of
            // all the cross products is the same, the angles
            // are all positive or negative (depending on the
            // order in which we visit them) so the polygon
            // is convex.
            bool got_negative = false;
            bool got_positive = false;
            int num_points = points.Length;
            int B, C;
            for (int A = 0; A < num_points; A++)
            {
                B = (A + 1) % num_points;
                C = (B + 1) % num_points;

                float cross_product =
                    CrossProductLength(
                        points[A].x, points[A].y,
                        points[B].x, points[B].y,
                        points[C].x, points[C].y);
                if (cross_product < 0)
                {
                    got_negative = true;
                }
                else if (cross_product > 0)
                {
                    got_positive = true;
                }
                if (got_negative && got_positive) return false;
            }

            // If we got this far, the polygon is convex.
            return true;
        }

        // Return the cross product AB x BC.
        // The cross product is a vector perpendicular to AB
        // and BC having length |AB| * |BC| * Sin(theta) and
        // with direction given by the right-hand rule.
        // For two vectors in the X-Y plane, the result is a
        // vector with X and Y components 0 so the Z component
        // gives the vector's length and direction.
        public static float CrossProductLength(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            // Get the vectors' coordinates.
            float BAx = Ax - Bx;
            float BAy = Ay - By;
            float BCx = Cx - Bx;
            float BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy - BAy * BCx);
        }
        #endregion Polygon
    }
}
