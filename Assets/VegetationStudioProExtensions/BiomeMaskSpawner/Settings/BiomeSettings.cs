using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AwesomeTechnologies.VegetationSystem;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class BiomeSettings
    {

        /// <summary>
        /// The Biome type to be used.
        /// </summary>
        public BiomeType biomeType = BiomeType.Default;

        /// <summary>
        /// The maximum relative Biome blend distance. 0 = no blending, 1 = full blending
        /// </summary>
        [Range(0f, 1f)]
        public float biomeBlendDistanceMin = 0f;

        /// <summary>
        /// The minimum relative Biome blend distance. 0 = no blending, 1 = full blending
        /// </summary>
        [Range(0f, 1f)]
        public float biomeBlendDistanceMax = 0f;

        /// <summary>
        /// Reducing density means that some of the partitions aren't used, i. e. they are removed randomly
        /// </summary>
        [Range(0, 1)]
        public float density = 1;

    }
}