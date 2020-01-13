using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AwesomeTechnologies.VegetationSystem.Biomes;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class LineSettings
    {
        /// <summary>
        /// The number of lines to add
        /// </summary>
        public int count = 20;

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
        public float heightMax = 200;

        /// <summary>
        /// The minimum angle in degrees
        /// </summary>
        public float angleMin = 0;

        /// <summary>
        /// The maximum angle in degrees
        /// </summary>
        public float angleMax = 360;

        /// <summary>
        /// Attached to edge of Biome or loose on the terrain
        /// </summary>
        public bool attachedToBiome = false;

        /// <summary>
        /// The Biome mask area to which to attach the lines
        /// </summary>
        public BiomeMaskArea biomeMaskArea;

        /// <summary>
        /// The line is normally attached 90 degrees to the biome mask edge. This value allows for some randomness by modifying the angle +/- this value in degrees.
        /// </summary>
        public float attachedAngleDelta = 20f;
    }
}
