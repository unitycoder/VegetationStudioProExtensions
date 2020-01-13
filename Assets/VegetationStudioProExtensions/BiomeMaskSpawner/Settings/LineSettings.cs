using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class LineSettings
    {
        /// <summary>
        /// The number of lines to add
        /// </summary>
        public float count = 20;

        /// <summary>
        /// The minimum line width
        /// </summary>
        public float widthMin = 5;

        /// <summary>
        /// The maximum line width
        /// </summary>
        public float widthMax = 10;
        
        /// <summary>
        /// The minimum line height
        /// </summary>
        public float heightMin = 10;

        /// <summary>
        /// The maximum line height
        /// </summary>
        public float heightMax = 100;

        /// <summary>
        /// Attached to edge of Biome or loose on the terrain
        /// </summary>
        public bool attachedToBiome = false;

    }
}
