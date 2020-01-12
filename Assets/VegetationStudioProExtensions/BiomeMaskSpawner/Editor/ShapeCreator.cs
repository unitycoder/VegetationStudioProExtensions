using AwesomeTechnologies.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class ShapeCreator
    {

        /// <summary>
        /// Create list of nodes which form a rectangular shape
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static List<Vector3> CreateRectangularBoundsShape( Bounds bounds)
        {
            List<Vector3> nodes = new List<Vector3>();

            nodes.Add(new Vector3(bounds.center.x - bounds.extents.x, 0, bounds.center.z - bounds.extents.z));
            nodes.Add(new Vector3(bounds.center.x - bounds.extents.x, 0, bounds.center.z + bounds.extents.z));
            nodes.Add(new Vector3(bounds.center.x + bounds.extents.x, 0, bounds.center.z + bounds.extents.z));
            nodes.Add(new Vector3(bounds.center.x + bounds.extents.x, 0, bounds.center.z - bounds.extents.z));

            return nodes;
        }

        /// <summary>
        /// Distributes random points within bounds and creates a convex hull around it.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bounds"></param>
        /// <param name="count"></param>
        public static List<Vector3> CreateRandomShapeUsingConvexHull( Bounds bounds, int count)
        {
            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < count; i++)
            {
                points.Add(PolygonUtils.GetRandomPointXZ(bounds));
            }

            List<Vector2> convexHull = PolygonUtility.GetConvexHull(points);

            List<Vector3> nodes = convexHull.ConvertAll<Vector3>(item => new Vector3(item.x, 0, item.y));

            return nodes;

        }

        /// <summary>
        /// Modifies a polygon and creates a random shape.
        /// 
        /// Algorithm:
        /// 
        /// * create a new polygon, subdivided this way:
        ///   + get center of polygon
        ///   + create a list of angles for iteration
        ///   + iterate through the angles, create a line using the angle and find the intersection with a line of the polygon
        /// * iterate through the subdivided polygon
        ///   + move the vertex towards the center
        ///   + consider previous vertex in order to not move in too large steps
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bounds"></param>
        /// <param name="count"></param>
        public static List<Vector3> CreateRandomShape(List<Vector3> polygon, float ellipseRelaxationFactor, bool keepOriginalPoints, int angleStepCount, bool randomAngleMovement, float douglasPeuckerReductionTolerance)
        {

            List<Vector3> newPolygon = CreateRandomShape(polygon, ellipseRelaxationFactor, angleStepCount, randomAngleMovement, keepOriginalPoints, douglasPeuckerReductionTolerance);

            return newPolygon;
        }

        private static List<float> CreateAngleList( int angleStepCount, bool randomAngleMovement)
        {
            List<float> angleRadList = new List<float>();

            if (randomAngleMovement)
            {
                for (int i = 0; i < angleStepCount; i++)
                {

                    float angleRad = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);

                    // avoid duplicates
                    if (angleRadList.Contains(angleRad))
                    {
                        // the randomness should be enough to not have duplicates. just output some message in case we run into problems with not enough vertices later
                        Debug.Log("Duplicate angle: " + angleRad + ". Skipping");
                        continue;
                    }

                    angleRadList.Add(angleRad);
                }
            }
            else
            {
                // start with a random angle; otherwise with 0 we'd get similarities among different shapes
                float startAngle = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);

                for (int i = 0; i < angleStepCount; i++)
                {
                    float angleRad = startAngle + i * (float)Math.PI * 2 / angleStepCount;

                    angleRadList.Add(angleRad);
                }
               
            }

            // sort ascending
            angleRadList.Sort();

            return angleRadList;
        }

        /// <summary>
        /// Modifies a polygon and creates a random shape.
        /// 
        /// Algorithm:
        /// 
        /// * create a new polygon, subdivided this way:
        ///   + get center of polygon
        ///   + create a list of angles for iteration
        ///   + iterate through the angles, create a line using the angle and find the intersection with a line of the polygon
        /// * iterate through the subdivided polygon
        ///   + move the vertex towards the center
        ///   + consider previous vertex in order to not move in too large steps
        /// </summary>
        public static List<Vector3> CreateRandomShape(List<Vector3> polygon, float ellipseRelaxationFactor, int angleStepCount, bool randomAngleMovement, bool keepOriginalShape, float douglasPeuckerReductionTolerance)
        {
 
            Vector3 meanVector = PolygonUtils.GetMeanVector(polygon);
            float length = meanVector.magnitude * 2;


            #region create angles

            // get the list of angles to step through
            List<float> angleRadList = CreateAngleList(angleStepCount, randomAngleMovement);
            
            #endregion create angles

            #region create new polygon using angles

            List<Vector3> subdividedPolygon = new List<Vector3>();

            // add existing points in order to keep the general voronoi shape
            if (keepOriginalShape)
            {
                subdividedPolygon.AddRange(polygon);
            }

            for (int i = 0; i < angleRadList.Count; i++)
            {

                // convert angle from deg to rad
                float angle = angleRadList[i];

                float x = meanVector.x + length * Mathf.Cos(angle);
                float z = meanVector.z + length * Mathf.Sin(angle);

                // position on the ellipse
                Vector3 line1PositionA = meanVector;
                Vector3 line1PositionB = new Vector3(x, 0, z);
                
                for (var j = 0; j < polygon.Count; j++)
                {

                    int currIndex = j;
                    int nextIndex = j + 1;
                    if (nextIndex == polygon.Count)
                    {
                        nextIndex = 0;
                    }

                    Vector3 line2PositionA = new Vector3( polygon[currIndex].x, 0, polygon[currIndex].z);
                    Vector3 line2PositionB = new Vector3( polygon[nextIndex].x, 0, polygon[nextIndex].z);


                    Vector3 intersection = Vector3.zero;

                    // try and find an intersection. if one is found, add the point to the list
                    if(PolygonUtils.LineSegmentsIntersection(line1PositionA, line1PositionB, line2PositionA, line2PositionB, out intersection))
                    {
                        subdividedPolygon.Add(intersection);
                        break;
                    }

                }
                

            }

            // sort again
            PolygonUtils.SortClockWise(subdividedPolygon);

            #endregion create new polygon using angles


            #region create new polygon using the intersections
            List<Vector3> newPolygon = new List<Vector3>();

            float prevDistance = 0;

            for (int i = 0; i < subdividedPolygon.Count; i++)
            {
                Vector3 curr = subdividedPolygon[i];

                // position on the ellipse
                Vector3 position = curr;

                // from center to position
                float distance = (position - meanVector).magnitude;
                Vector3 direction = (position - meanVector).normalized;

                // move from center towards new position. but not too much, let it depend on the previous distance
               {
                    // move initially from 0 to max distance. otherwise use the previous value
                    float min = i == 0 ? distance * ellipseRelaxationFactor : prevDistance * ellipseRelaxationFactor;
                    float max = distance;

                    // the radius must be smaller during the next iteration, we are navigating around an ellipse => clamp the values
                    if (min > max)
                        min = max;

                    float moveDistance = UnityEngine.Random.Range(min, max);

                    distance = moveDistance;

                }

                position = meanVector + distance * direction;

                newPolygon.Add(position);

                prevDistance = distance;
            }
            #endregion create new polygon using the intersections

            if (douglasPeuckerReductionTolerance > 0)
            {
                // convert to vector2
                List<Vector2> vector2List = newPolygon.ConvertAll<Vector2>(item => new Vector2(item.x, item.z));

                // use vspro's DouglasPeuckerReduction algorithm
                vector2List = PolygonUtility.DouglasPeuckerReduction(vector2List, douglasPeuckerReductionTolerance);

                // convert back to vector3
                if (vector2List.Count >= 3)
                {
                    newPolygon = vector2List.ConvertAll<Vector3>(item => new Vector3(item.x, 0, item.y));
                }
            }

            return newPolygon;

        }
        public static Vector3[] CreateCircle(Vector3 position, float radius, int pointCount)
        {
            Vector3[] shape = new Vector3[pointCount];

            // create shape
            for (int i = 0; i < pointCount; i++)
            {

                float angle = i * Mathf.PI * 2f / pointCount;

                Vector3 nodePosition = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                shape[i] = nodePosition;
            }

            // shift to position
            for (int i = 0; i < shape.Length; i++)
            {
                Vector3 vertex = shape[i];

                vertex.x += position.x;
                vertex.z += position.z;

                shape[i] = vertex;
            }

            return shape;

        }

        /// <summary>
        /// Create a single hexagon.
        /// Credits to https://catlikecoding.com/unity/tutorials/hex-map
        /// </summary>
        /// <param name="position"></param>
        /// <param name="outerRadius"></param>
        /// <returns></returns>
        public static Vector3[] CreateHexagon(Vector3 position, float outerRadius)
        {
            // calculate inner radius
            float innerRadius = outerRadius * Mathf.Sqrt(3f) / 2;

            // create shape
            Vector3[] shape = {
                new Vector3(0f, 0f, outerRadius),
                new Vector3(innerRadius, 0f, 0.5f * outerRadius),
                new Vector3(innerRadius, 0f, -0.5f * outerRadius),
                new Vector3(0f, 0f, -outerRadius),
                new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
                new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
             };

            // shift to position
            for (int i = 0; i < shape.Length; i++)
            {
                Vector3 vertex = shape[i];

                vertex.x += position.x;
                vertex.z += position.z;

                shape[i] = vertex;
            }

            return shape;
        }


    }
}