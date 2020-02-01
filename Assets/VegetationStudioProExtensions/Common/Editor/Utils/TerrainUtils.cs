using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class TerrainUtils
    {
        /// <summary>
        /// Gets the terrain height by raycasting down at the position x/z.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float? GetTerrainHeight( float x, float z)
        {
            float raycastStart = 5000f;
            float raycastLength = 10000f;

            // get y position on terrain by raycasting
            Vector3 position = new Vector3(x, raycastStart, z);

            var hits = Physics.RaycastAll(position, Vector3.down, raycastLength);
            for (int j = 0; j < hits.Length; j++)
            {
                if (hits[j].collider is TerrainCollider)
                {
                    return hits[j].point.y;
                }
            }

            return null;
        }
    }
}