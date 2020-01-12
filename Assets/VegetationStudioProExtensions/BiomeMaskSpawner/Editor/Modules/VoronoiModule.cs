using DelaunayVoronoi;
using InteractiveDelaunayVoronoi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VegetationStudioProExtensions
{
    public class VoronoiModule
    {
        private BiomeMaskSpawnerExtensionEditor biomeMaskSpwaner;

        public VoronoiModule(BiomeMaskSpawnerExtensionEditor biomeMaskSpawner)
        {
            this.biomeMaskSpwaner = biomeMaskSpawner;
        }

        public void CreateMasks(List<Bounds> boundsList)
        {
            foreach (Bounds bounds in boundsList)
            {
                int pointCount = biomeMaskSpwaner.extension.voronoiSettings.pointCount;

                // create masks
                CreateMasks(bounds, pointCount);
            }

        }

        private void CreateMasks(Bounds bounds, int pointCount)
        {
            // get offset, eg when biome mask is used for clipping
            float xmin = bounds.center.x - bounds.extents.x;
            float zmin = bounds.center.z - bounds.extents.z;

            // get 0/0-based bounds for graph processing
            Bounds graphBounds = new Bounds(bounds.size / 2, bounds.size);

            DelaunayVoronoiGraph graph = new DelaunayVoronoiGraph();

            // algorithm is 0-based
            graph.GeneratePoints(0, bounds.size.x, bounds.size.z); // initialize with the dimensions which are used in the algorithm

            // add random points
            for (int i = 0; i < pointCount; i++)
            {
                // get random point starting at [0/0]
                Vector2 vector = PolygonUtils.GetRandomPointXZ(graphBounds);

                graph.AddPoint(vector.x, vector.y);
            }

            // create the graph using the points
            graph.CreateGraph();

            // bounds for clipping
            Vector2[] clipPolygon = biomeMaskSpwaner.GetBiomeClipPolygon(bounds);

            // normalize clip polygon, shift to [0/0]
            Vector2[] offsetClipPolygon = clipPolygon.Select(item => new Vector2(item.x - xmin, item.y - zmin)).ToArray();

            float density = biomeMaskSpwaner.extension.biomeSettings.density;

            for (int i = 0; i < pointCount; i++)
            {
                // skip randomly
                if (density  != 1 && UnityEngine.Random.Range(0f, 1f) >= density)
                {
                    continue;
                }

                int maskId = biomeMaskSpwaner.GetNextMaskId();

                // get cell, clip it at the clip polygon
                Cell cell = graph.GetVoronoiCell( i, offsetClipPolygon);

                if (cell == null)
                    continue;

                // consider biome mask shift: shift points away from 0/0 if necessary
                Vector3 position = new Vector3( cell.Centroid.x + xmin, 0, cell.Centroid.y + zmin); // TODO: recalculate, might have changed because of clipping

                // consider biome mask shift: shift points away from 0/0 if necessary
                List<Vector3> nodes = cell.Vertices.Select(item => new Vector3(item.x + xmin, 0, item.y + zmin)).ToList();

                // apply random shape if requested
                if (biomeMaskSpwaner.extension.shapeSettings.randomShape)
                {

                    nodes = ShapeCreator.CreateRandomShape(nodes, //
                        biomeMaskSpwaner.extension.shapeSettings.RandomConvexity, //
                        biomeMaskSpwaner.extension.shapeSettings.keepOriginalPoints, //
                        biomeMaskSpwaner.extension.shapeSettings.RandomPointsCount, //
                        biomeMaskSpwaner.extension.shapeSettings.randomAngle, //
                        biomeMaskSpwaner.extension.shapeSettings.douglasPeuckerReductionTolerance);
                }

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
            float blendDistanceMin = biomeMaskSpwaner.extension.biomeSettings.biomeBlendDistanceMin;
            float blendDistanceMax = biomeMaskSpwaner.extension.biomeSettings.biomeBlendDistanceMax;

            float biomeBlendDistance = UnityEngine.Random.Range(blendDistanceMin, blendDistanceMax);

            // mean vector is just a simple measure. please adapt if you need more accuracy
            Vector3 meanVector = PolygonUtils.GetMeanVector(nodes);

            float blendDistance = meanVector.magnitude / 2f * biomeBlendDistance;

            // create the mask using the provided parameters
            biomeMaskSpwaner.CreateBiomeMaskArea(gameObjectName, maskName, position, nodes, blendDistance);

        }


    }
}
