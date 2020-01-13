using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Biomes;
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
                // bounds
                float width = Random.Range(lineSettings.widthMin, lineSettings.widthMax);
                float height = Random.Range(lineSettings.heightMin, lineSettings.heightMax);

                // position
                // heuristic to make it less probable that the lines are outside the bounds
                // however they can still be outside, e. g. because of the rotation around a point
                // for our purpose it's acceptable
                float offsetX = width;
                float offsetZ = height;

                float x = Random.Range(bounds.min.x + offsetX, bounds.max.x - offsetX);
                float z = Random.Range(bounds.min.z + offsetZ, bounds.max.z - offsetZ);

                // angle
                float angle = Random.Range(lineSettings.angleMin, lineSettings.angleMax);

                // attached to biome: different position and angle
                if (lineSettings.attachedToBiome)
                {
                    Vector3 biomeEdgePosition = new Vector3();
                    float biomeAngle = 0f;

                    // get data from mask
                    GetRandomBiomeEdgePosition( out biomeEdgePosition, out biomeAngle);

                    // update position
                    x = biomeEdgePosition.x;
                    z = biomeEdgePosition.z;

                    // update angle
                    angle = biomeAngle;

                    // add a little bit of angle randomness if specified
                    angle += Random.Range(-lineSettings.attachedAngleDelta, lineSettings.attachedAngleDelta);
                    
                }

                // create mask nodes
                List<Vector3> nodes = new List<Vector3>();

                nodes.Add(new Vector3(x, 0, z));
                nodes.Add(new Vector3(x + width, 0, z));
                nodes.Add(new Vector3(x + width, 0, z + height));
                nodes.Add(new Vector3(x, 0, z + height));

                // rotate nodes around the center of the first edge
                Vector3 fromNode = nodes[0];
                Vector3 toNode = nodes[1];

                float distance = (toNode - fromNode).magnitude;
                Vector3 direction = (toNode - fromNode).normalized;

                Vector3 center = fromNode + direction * distance * 0.5f; // center of the edge

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
        /// Get a position and angle along the biome mask's edge.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="angle"></param>
        private void GetRandomBiomeEdgePosition( out Vector3 position, out float angle)
        {
            BiomeMaskArea mask = biomeMaskSpawner.extension.lineSettings.biomeMaskArea;

            // parameter consistency check
            if(mask == null)
            {
                Debug.LogError("No mask defined");

                position = Vector3.zero;
                angle = 0;
                return;
            }

            List<Vector3> positions = BiomeMaskUtils.GetPositions(mask);

            // sort clockwise, so that the pick algorithm works
            // if this were counterclockwise, then the angle would make the lines face inwards
            PolygonUtils.SortClockWise(positions);

            // get from node index
            int nodeIndexFrom = Random.Range(0, positions.Count); // note: int is exclusive last

            // get to node index, consider overlap
            int nodeIndexTo = nodeIndexFrom + 1;
            if (nodeIndexTo >= mask.Nodes.Count)
                nodeIndexTo = 0;

            // get nodes
            Vector3 positionFrom = mask.transform.position + positions[nodeIndexFrom];
            Vector3 positionTo = mask.transform.position + positions[nodeIndexTo];

            // having the lines flip inwards into the biome is just a matter of changing the access order of the nodes
            // leaving this here, maybe we find a use case later
            bool flipAngle = biomeMaskSpawner.extension.lineSettings.attachedAngleFlip;
            if ( flipAngle)
            {
                Vector3 tmp = positionFrom;
                positionFrom = positionTo;
                positionTo = tmp;
            }

            float distance = (positionTo - positionFrom).magnitude;
            Vector3 direction = (positionTo - positionFrom).normalized;

            // the position along the edge. 0=from, 0.5=center, 1=to
            float relativePosition = Random.Range(0f, 1f);

            // calculate the position
            position = positionFrom + direction * distance * relativePosition;

            // calculate the angle 90 degrees to the from-to points and convert to degrees
            angle = Mathf.Atan2(positionTo.z - positionFrom.z, positionTo.x - positionFrom.x) * Mathf.Rad2Deg;

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
