using System;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay.VectorUtils
{
  /// <summary>
  /// Comparer which allows you to sort a list of points clockwise around an origin.
  /// 
  /// Note: Found this online in several repositories, full credit to the person who created it, unfortunately couldn't find the original author.
  /// </summary>
    public class ClockwiseComparerVector : IComparer<Vector2>
    {
        /// <summary>
        /// 	ClockwiseComparer provides functionality for sorting a collection of Vectors such
        /// 	that they are ordered clockwise about a given origin.
        /// </summary>

        private Vector2 m_Origin;

        #region Properties

        /// <summary>
        /// 	Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        public Vector2 origin { get { return m_Origin; } set { m_Origin = value; } }

        #endregion

        /// <summary>
        /// 	Initializes a new instance of the ClockwiseComparer class.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparerVector(Vector2 origin)
        {
            m_Origin = origin;
        }

        /// <summary>
        /// 	Initializes a new instance of the ClockwiseComparer class and sets the origin to the mean vector, depending on the positions.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparerVector(List<Vector2> positions)
        {
            m_Origin = GetMeanVector(positions);
        }

        private Vector2 GetMeanVector(List<Vector2> positions)
        {
            if (positions.Count == 0)
                return new Vector2(0, 0);

            float x = 0f;
            float y = 0f;

            foreach (Vector2 pos in positions)
            {
                x += pos.x;
                y += pos.y;
            }
            return new Vector2(x / (float)positions.Count, y / (float)positions.Count);
        }

        #region IComparer Methods

        /// <summary>
        /// 	Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        public int Compare(Vector2 first, Vector2 second)
        {
            return IsClockwise(first, second, m_Origin);
        }

        #endregion

        /// <summary>
        /// 	Returns 1 if first comes before second in clockwise order.
        /// 	Returns -1 if second comes before first.
        /// 	Returns 0 if the points are identical.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        /// <param name="origin">Origin.</param>
        public static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin)
        {

            //if (first == second)
            if (first.x == second.x && first.y == second.y)
                return 0;

            //Vector firstOffset = first - origin;
            Vector2 firstOffset = new Vector2(first.x - origin.x, first.y - origin.y);

            //Vector secondOffset = second - origin;
            Vector2 secondOffset = new Vector2(second.x - origin.x, second.y - origin.y);


            double angle1 = System.Math.Atan2(firstOffset.x, firstOffset.y);
            double angle2 = System.Math.Atan2(secondOffset.x, secondOffset.y);

            if (angle1 < angle2)
                return -1;

            if (angle1 > angle2)
                return 1;

            // Check to see which point is closest
            double firstMagnitude = Math.Sqrt(firstOffset.x * firstOffset.x + firstOffset.y * firstOffset.y);
            double secondMagnitude = Math.Sqrt(secondOffset.x * secondOffset.x + secondOffset.y * secondOffset.y);

            //return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? -1 : 1;
            return (firstMagnitude < secondMagnitude) ? -1 : 1;

        }
    }
}
