using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class RoadSettings
    {
        public BiomeMaskArea biomeMaskArea;

        public GameObject road;

        public bool smoothEnabled;

        public bool closedTrack;

        [Range(0, 300)]
        public float minDistance = 30f;

    }
}
