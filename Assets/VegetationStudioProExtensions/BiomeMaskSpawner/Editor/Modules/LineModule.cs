using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VegetationStudioProExtensions
{
    public class LineModule
    {
        private BiomeMaskSpawnerExtensionEditor biomeMaskSpawner;

        public LineModule(BiomeMaskSpawnerExtensionEditor biomeMaskSpawner)
        {
            this.biomeMaskSpawner = biomeMaskSpawner;
        }

        /// <summary>
        /// Create Biome masks for the specified bounds
        /// </summary>
        /// <param name="boundsList"></param>
        public void CreateMasks(List<Bounds> boundsList)
        {
            foreach (Bounds bounds in boundsList)
            {
                // create masks
                CreateMasks(bounds);
            }

        }

        /// <summary>
        /// Create Biome masks for the specified bounds
        /// </summary>
        /// <param name="bounds"></param>
        private void CreateMasks(Bounds bounds)
        {
            LineSettings lineSettings = biomeMaskSpawner.extension.lineSettings;
            Debug.Log("TODO: create lines");
            
            for( int i=0; i < lineSettings.count; i++)
            {
                float width = Random.Range(lineSettings.widthMin, lineSettings.widthMax);
                float height = Random.Range(lineSettings.heightMin, lineSettings.heightMax);
            }

        }
    }
}
