using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class VectorUtils
    {
        /// <summary>
        /// Rotate one point around another.
        /// 
        /// https://stackoverflow.com/questions/13695317/rotate-a-point-around-another-point
        /// Credits to Fraser
        /// 
        /// </summary>
        /// <param name="position">The point to rotate.</param>
        /// <param name="center">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        public static Vector3 Rotate(Vector3 position, Vector3 center, float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * (Mathf.PI / 180f);
            float cosTheta = Mathf.Cos(angleInRadians);
            float sinTheta = Mathf.Sin(angleInRadians);

            return new Vector3
            {
                x = cosTheta * (position.x - center.x) - sinTheta * (position.z - center.z) + center.x,
                y = 0,
                z = sinTheta * (position.x - center.x) + cosTheta * (position.z - center.z) + center.z
            };
        }
    }
}
