using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    /// <summary>
    /// Comparer which allows you to sort a list of points clockwise around an origin.
    /// 
    /// Note: Found this online in several repositories, full credit to the person who created it, unfortunately couldn't find the original author.
    /// </summary>
    public class ClockwiseComparer : IComparer<Vector3>
    {
        /// <summary>
        /// 	ClockwiseComparer provides functionality for sorting a collection of Points such
        /// 	that they are ordered clockwise about a given origin.
        /// </summary>

        private Vector3 m_Origin;

        #region Properties

        /// <summary>
        /// 	Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        public Vector3 origin { get { return m_Origin; } set { m_Origin = value; } }

        #endregion

        /// <summary>
        /// 	Initializes a new instance of the ClockwiseComparer class.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparer(Vector3 origin)
        {
            m_Origin = origin;
        }

        /// <summary>
        /// 	Initializes a new instance of the ClockwiseComparer class and sets the origin to the mean vector, depending on the positions.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparer(List<Vector3> positions)
        {
            m_Origin = PolygonUtils.GetMeanVector(positions);
        }

        

        #region IComparer Methods

        /// <summary>
        /// 	Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        public int Compare(Vector3 first, Vector3 second)
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
        public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin)
        {

            //if (first == second)
            if (first.x == second.x && first.z == second.z)
                return 0;

            Vector3 firstOffset = first - origin;

            Vector3 secondOffset = second - origin;


            double angle1 = System.Math.Atan2(firstOffset.x, firstOffset.z);
            double angle2 = System.Math.Atan2(secondOffset.x, secondOffset.z);

            if (angle1 < angle2)
                return -1;

            if (angle1 > angle2)
                return 1;

            // Check to see which point is closest
            return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? -1 : 1;

        }
    }
}