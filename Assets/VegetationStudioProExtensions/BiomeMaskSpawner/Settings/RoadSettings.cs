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

        /// <summary>
        /// Optionally place a GameObject on the road.
        /// </summary>
        public bool autoPlaceGameObject = false;

        /// <summary>
        /// The GameObject to be placed on the road.
        /// </summary>
        public GameObject placementGameObject = null;
    }
}
