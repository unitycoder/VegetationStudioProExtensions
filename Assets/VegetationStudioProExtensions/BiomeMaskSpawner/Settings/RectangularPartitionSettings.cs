using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class RectangularPartitionSettings
    {

        [Serializable]
        public enum PartitionStrategy
        {
            Random,
            Ratio,
            Size,
        }

        /// <summary>
        /// The number of Biomes, the number of partitions is biome count - 1.
        /// </summary>
        public int biomeCount = 10;

        /// <summary>
        /// The strategy to select segments which should be partitioned
        /// </summary>
        public PartitionStrategy partitionStrategy = PartitionStrategy.Size;

        /// <summary>
        /// Shift and resize the bounds within its bounds. Makes the partitions look less rectangularly distributed.
        /// </summary>
        public float boundsShiftFactorMin = 1f;

        /// <summary>
        /// Shift and resize the bounds within its bounds. Makes the partitions look less rectangularly distributed.
        /// </summary>
        public float boundsShiftFactorMax = 1f;

    }
}
