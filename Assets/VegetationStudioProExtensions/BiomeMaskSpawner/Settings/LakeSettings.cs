using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class LakeSettings
    {

#if RAM_2019

        /// <summary>
        /// Create a RAM 2019 lake
        /// </summary>
        public bool createLake = false;


        /// <summary>
        /// The lake profile to use
        /// </summary>
        public LakePolygonProfile lakeProfile = null;

        /// <summary>
        /// When active, the mask position will be used, but the RAM internal simulation will create the lake shape
        /// </summary>
        public bool ramInternalLakeCreation = false;

        #region RAM internal

        public float closeDistanceSimulation = 5f;
        [Range(1,180)]
        public int angleSimulation = 5;
        public float checkDistanceSimulation = 50;
        public bool removeFirstPointSimulation = true; // not ediable

        #endregion RAM internal

#endif

    }
}