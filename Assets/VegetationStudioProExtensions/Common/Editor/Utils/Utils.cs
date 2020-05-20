using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class Utils
    {
        /// <summary>
        /// Value can't be lower than minValue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <returns></returns>
        public static float ClipMin(float value, float minValue)
        {
            if (value < minValue)
            {
                value = minValue;
            }

            return value;
        }

        /// <summary>
        /// Value can't be lower than minValue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <returns></returns>
        public static int ClipMin(int value, int minValue)
        {
            if (value < minValue)
            {
                value = minValue;
            }

            return value;
        }

        /// <summary>
        /// Value can't be higher than maxValue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static float ClipMax(float value, float maxValue)
        {
            if (value > maxValue)
            {
                value = maxValue;
            }

            return value;
        }

        /// <summary>
        /// Value can't be higher than maxValue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int ClipMax(int value, int maxValue)
        {
            if (value > maxValue)
            {
                value = maxValue;
            }

            return value;
        }
    }
}