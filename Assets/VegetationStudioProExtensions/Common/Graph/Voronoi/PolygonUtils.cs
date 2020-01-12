using Delaunay.VectorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;

namespace InteractiveDelaunayVoronoi
{
    public class PolygonUtils
    {

        /// <summary>
        /// Get the mean vector of the specified polygon
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector2 GetMeanVector(List<Vector2> positions)
        {
            return GetMeanVector(positions.ToArray());
        }

        /// <summary>
        /// Get the mean vector of the specified polygon
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector2 GetMeanVector(Vector2[] positions)
        {
            if (positions.Length == 0)
                return new Vector2(0, 0);

            float x = 0;
            float y = 0;

            foreach (Vector2 pos in positions)
            {
                x += pos.x;
                y += pos.y;
            }
            return new Vector2(x / positions.Length, y / positions.Length);
        }

        /// <summary>
        /// Sort the points of the polygon in clockwise order.
        /// </summary>
        /// <param name="polygon"></param>
        public static void SortClockWise(List<Vector2> polygon)
        {
            ClockwiseComparerVector comparer = new ClockwiseComparerVector(polygon);
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
        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
        {
            intersection = new Vector2(0,0);

            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f)
            {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.y = p1.y + u * (p2.y - p1.y);

            return true;
        }

        /// <summary>
        /// Check if a point is inside, outside or on an ellipse
        /// inside = < 1, on = 1, outside = > 1
        /// For ON comparison better cast to int.
        /// https://www.geeksforgeeks.org/check-if-a-point-is-inside-outside-or-on-the-ellipse/
        /// </summary>
        /// <param name="ellipseX"></param>
        /// <param name="ellipseY"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="ellipseWidth"></param>
        /// <param name="ellipseHeight"></param>
        /// <returns></returns>        
        public static double IsInEllipse(double ellipseX, double ellipseY, double ellipseWidth, double ellipseHeight, double x, double y)
        {

            double p = (Math.Pow((x - ellipseX), 2) /
                     Math.Pow(ellipseWidth, 2)) +
                    (Math.Pow((y - ellipseY), 2) /
                     Math.Pow(ellipseHeight, 2));

            return p;
        }
    }

}
