using AwesomeTechnologies.VegetationSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class BatchFunctions : MonoBehaviour
    {
        /// <summary>
        /// Whether all biomes are modified or only a filtered subset
        /// </summary>
        public bool modifyAll = true;

        /// <summary>
        /// The selected biome type
        /// </summary>
        public BiomeType biomeType = BiomeType.Default;
    }
}
