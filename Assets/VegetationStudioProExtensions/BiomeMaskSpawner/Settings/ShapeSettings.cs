using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class ShapeSettings
    {
        /// <summary>
        /// If true, create the mask as a random shape inside the bounding box. If false, use the bounding box as mask.
        /// </summary>
        public bool randomShape = false;

        /// <summary>
        /// The minimum number of random points that will be used to create the polygon
        /// </summary>
        public int randomPointsCountMin = 3;

        /// <summary>
        /// The maximum number of random points that will be used to create the polygon
        /// </summary>
        public int randomPointsCountMax = 10;

        /// <summary>
        /// Use uniform angle movement or random angle movement along the shape creation ellipse.
        /// </summary>
        public bool randomAngle = true;

        /// <summary>
        /// Keep the original points when a shape is e. g. subdivided
        /// </summary>
        public bool keepOriginalPoints = true;

        /// <summary>
        /// Reducing convexity randomly moves nodes of the polygon towards the center.
        /// A low value would result in a star-like shape. A higher value resembles more the original shape.
        /// The min/max values allow for some randomness.
        /// </summary>
        public float convexityMin = 0.8f;

        /// <summary>
        /// Reducing convexity randomly moves nodes of the polygon towards the center.
        /// A low value would result in a star-like shape. A higher value resembles more the original shape.
        /// The min/max values allow for some randomness.
        /// </summary>
        public float convexityMax = 1.0f;

        /// <summary>
        /// Dougles Peucker Reduction algorithm for node reduction.
        /// 0 = disabled.
        /// </summary>
        public float douglasPeuckerReductionTolerance = 0f;

        /// <summary>
        /// A resize factor which allows you to grow or shrink the mask.
        /// </summary>
        [Range(0.5f,1.5f)]
        public float resizeFactor = 1.0f;

        /// <summary>
        /// Get a random relaxation value which is between the min and max bounds.
        /// </summary>
        public int RandomPointsCount
        {
            get => UnityEngine.Random.Range(randomPointsCountMin, randomPointsCountMax);
        }

        /// <summary>
        /// Get a random convexity value which is between the min and max bounds.
        /// </summary>
        public float RandomConvexity {
            get => UnityEngine.Random.Range(convexityMin, convexityMax);
        }
    }
}
