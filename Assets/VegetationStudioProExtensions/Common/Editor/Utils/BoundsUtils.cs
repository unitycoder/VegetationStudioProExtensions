using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class BoundsUtils
    {
        /// <summary>
        /// Get the enclosing bounds of the prefab, including children (e. g. in case of a house including doors, windows, etc)
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static Bounds GetPrefabBounds(GameObject prefab)
        {

            Renderer renderer = prefab.GetComponent<Renderer>();
            if (renderer == null)
            {
                // LOD case: renderer is in the children
                renderer = prefab.GetComponentInChildren<Renderer>();
            }

            // calculate bounds including children (eg houses including windows, doors, etc)
            Bounds bounds = renderer.bounds;
            foreach (var r in prefab.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(r.bounds);
            }

            return bounds;
        }
    }
}
