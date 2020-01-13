using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    /// <summary>
    /// Create rectangular shapes and rotate them randomly.
    /// No clipping is performed, the shapes are intended to be just some thin forest lines for details.
    /// No biome intersection check is performed, might be more natural this way.
    /// </summary>
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
            
            for( int i=0; i < lineSettings.count; i++)
            {

                float width = Random.Range(lineSettings.widthMin, lineSettings.widthMax);
                float height = Random.Range(lineSettings.heightMin, lineSettings.heightMax);

                // heuristic to make it less probable that the lines are outside the bounds
                // however they can still be outside, e. g. because of the rotation around a point
                // for our purpose it's acceptable
                float offsetX = width;
                float offsetZ = height;

                float x = Random.Range(bounds.min.x + offsetX, bounds.max.x - offsetX);
                float z = Random.Range(bounds.min.z + offsetZ, bounds.max.z - offsetZ);

                float angle = Random.Range(0, 360);

                List<Vector3> nodes = new List<Vector3>();

                nodes.Add(new Vector3(x, 0, z));
                nodes.Add(new Vector3(x + width, 0, z));
                nodes.Add(new Vector3(x + width, 0, z + height));
                nodes.Add(new Vector3(x, 0, z + height));

                Vector3 center = nodes[0];

                for (int j = 0; j < nodes.Count; j++)
                {
                    Vector3 node = nodes[j];
                    node = VectorUtils.Rotate(node, center, angle);
                    nodes[j] = node;
                }

                Vector3 position = new Vector3(x, 0, z);

                int maskId = biomeMaskSpawner.GetNextMaskId();
                CreateBiomeMaskArea("Biome Mask " + maskId, "Mask " + maskId, position, nodes);

            }
        }

        /// <summary>
        /// Create a biome mask.
        /// The blend distance is simply calculated using the mean vector.
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <param name="maskName"></param>
        /// <param name="position"></param>
        /// <param name="nodes"></param>
        private void CreateBiomeMaskArea(string gameObjectName, string maskName, Vector3 position, List<Vector3> nodes)
        {
            float blendDistanceMin = biomeMaskSpawner.extension.biomeSettings.biomeBlendDistanceMin;
            float blendDistanceMax = biomeMaskSpawner.extension.biomeSettings.biomeBlendDistanceMax;

            float biomeBlendDistance = UnityEngine.Random.Range(blendDistanceMin, blendDistanceMax);

            // mean vector is just a simple measure. please adapt if you need more accuracy
            Vector3 meanVector = PolygonUtils.GetMeanVector(nodes);

            float blendDistance = meanVector.magnitude / 2f * biomeBlendDistance;

            // create the mask using the provided parameters
            biomeMaskSpawner.CreateBiomeMaskArea(gameObjectName, maskName, position, nodes, blendDistance);

        }
    }
}
