using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class RiverSettings
    {
        public enum Automation
        {
            Simulate,
            Generate
        }

#if RAM_2019

        public int count = 5;

        public Automation automation = Automation.Simulate;

        public SplineProfile splineProfile = null;

        public float widthMin = 40;
        public float widthMax = 100;

        public float simulatedRiverLengthMin = 100;
        public float simulatedRiverLengthMax = 500;

        public int simulatedRiverPointsMin = 10;
        public int simulatedRiverPointsMax = 30;

        public float simulatedMinStepSizeMin = 1f;
        public float simulatedMinStepSizeMax = 5f;

        public bool simulatedNoUp = false;
        public bool simulatedBreakOnUp = false;

        public bool noiseWidth = false;

        public float noiseMultiplierWidthMin = 4f;
        public float noiseMultiplierWidthMax = 10f;

        public float noiseSizeWidthMin = 0.5f;
        public float noiseSizeWidthMax = 1.5f;

#endif

    }
}