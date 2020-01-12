using AwesomeTechnologies.VegetationSystem.Biomes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class BoundsProcessingSettings
    {
        [Serializable]
        public enum PartitionAlgorithm
        {
            Voronoi,
            Rectangular,  
            Hexagon
        }

        [Serializable]
        public enum BoundsProcessing
        {
            CombinedTerrains,
            IndividualTerrains,
            Biome
        }

        /// <summary>
        /// Process all terrains as a single combined terrain or all terrains individually.
        /// </summary>
        public BoundsProcessing boundsProcessing = BoundsProcessing.CombinedTerrains;

        /// <summary>
        /// The algorithm to use for terrain paritioning
        /// </summary>
        public PartitionAlgorithm partitionAlgorithm = PartitionAlgorithm.Voronoi;

        /// <summary>
        /// When BoundsProcessing.Biome is set, then this mask will be used for the bounds intersection calculation
        /// </summary>
        public BiomeMaskArea biomeMaskArea;

        /// <summary>
        /// Editor helper attribute to indicated whether the biome mask area is valid or not
        /// </summary>
        public bool biomeMaskAreaValid = false;

    }
}
