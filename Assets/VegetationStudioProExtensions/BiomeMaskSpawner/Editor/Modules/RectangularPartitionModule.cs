using SutherlandHodgmanAlgorithm;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static VegetationStudioProExtensions.RectangularPartitionSettings;
using static VegetationStudioProExtensions.ShapeSettings;

namespace VegetationStudioProExtensions
{
    public class RectangularPartitionModule : ISettingsModule
    {
        /// <summary>
        /// A subdivision is usually performed at position 50%. This value allows you to modify the split position. 
        /// A value of 10 means the random partition will happen at one of the 10% steps, e. g. at 10%, at 20%, etc.
        /// This way you can get non-uniform segments.
        /// </summary>
        private int SEGMENT_SUBDIVISION_PARTS_MAX = 10;

        /// <summary>
        /// Whether to split the rectangle horizontally or vertically.
        /// </summary>
        private enum SplitType
        {
            Horizontal,
            Vertical
        }

        private SerializedProperty partitionStrategy;
        private SerializedProperty biomeCount;
        private SerializedProperty boundsShiftFactorMin;
        private SerializedProperty boundsShiftFactorMax;

        private BiomeMaskSpawnerExtensionEditor editor;

        public RectangularPartitionModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
            partitionStrategy = editor.FindProperty(x => x.rectangularPartitionSettings.partitionStrategy);
            biomeCount = editor.FindProperty(x => x.rectangularPartitionSettings.biomeCount);
            boundsShiftFactorMin = editor.FindProperty(x => x.rectangularPartitionSettings.boundsShiftFactorMin);
            boundsShiftFactorMax = editor.FindProperty(x => x.rectangularPartitionSettings.boundsShiftFactorMax);
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Partitions", GUIStyles.GroupTitleStyle);

            EditorGUILayout.PropertyField(biomeCount, new GUIContent("Biome Count", "The number of Biomes the terrain should have when full density is selected"));

            // consistency check: never allow the number of partitions to be smaller than 1
            biomeCount.intValue = Utils.ClipMin(biomeCount.intValue, 2);

            EditorGUILayout.PropertyField(partitionStrategy, new GUIContent("Split Strategy", "The strategy to select the terraiin segments which should be partitioned."));

            EditorGUILayout.LabelField(new GUIContent("Partition Shift", "Relative factor to shift and resize the partition bounds within its bounds. Makes the partitions look less rectangularly distributed. Reduces the size."));
            EditorGuiUtilities.MinMaxEditor("Min", ref boundsShiftFactorMin, "Max", ref boundsShiftFactorMax, 0.1f, 1f, true);
        }

        public void CreateMasks(List<Bounds> boundsList)
        {
            foreach (Bounds bounds in boundsList)
            {
                // create masks
                CreateMasks(bounds);
            }

        }

        private List<Bounds> Subdivide(Bounds sourceBounds, int steps)
        {
            List<Bounds> boundsSegments = new List<Bounds>();
            boundsSegments.Add(sourceBounds);

            // subdivide it
            for (int i = 0; i < steps; i++)
            {
                // pick next bounds object from the list
                int index = GetNextSegmentIndex(boundsSegments, editor.extension.rectangularPartitionSettings.partitionStrategy);

                Bounds bounds = boundsSegments[index];

                // remove it from the list
                boundsSegments.Remove(bounds);

                // random decision about splitting horizontal or vertical
                // SplitType splitType = UnityEngine.Random.Range(0, 2) == 0 ? SplitType.Horizontal : SplitType.Vertical;

                // split at the larger edge
                SplitType splitType = bounds.size.x < bounds.size.z ? SplitType.Horizontal : SplitType.Vertical;

                // randomness with the subdivision, otherwise with 2 it would always be in the center
                int segmentSubdivisionMax = SEGMENT_SUBDIVISION_PARTS_MAX;

                // randomness with the subdivision, otherwise with 2 it would always be in the center
                int segmentSubdivision = UnityEngine.Random.Range(2, segmentSubdivisionMax);

                // horizontal split
                switch (splitType)
                {
                    case SplitType.Horizontal:
                        SubdivideX(boundsSegments, bounds, segmentSubdivision);
                        break;
                    case SplitType.Vertical:
                        SubdivideZ(boundsSegments, bounds, segmentSubdivision);
                        break;
                    default:
                        throw new System.ArgumentException("Unsupported SplitType " + splitType);

                }
            }

            return boundsSegments;
        }

        private void SubdivideX(List<Bounds> boundsSegments, Bounds bounds, int segmentSubdivision)
        {
            int segmentSubdivisionPosition = UnityEngine.Random.Range(1, segmentSubdivision);

            float size = bounds.size.z; // use z length
            float topSegmentSize = size * segmentSubdivisionPosition / segmentSubdivision;
            float bottomSegmentSize = size - topSegmentSize;

            Vector3 topCenter = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z - bounds.extents.z + topSegmentSize / 2);
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z + bounds.extents.z - bottomSegmentSize / 2);

            Vector3 topSize = new Vector3(bounds.size.x, bounds.size.y, topSegmentSize);
            Vector3 bottomSize = new Vector3(bounds.size.x, bounds.size.y, bottomSegmentSize);

            Bounds topBounds = new Bounds(topCenter, topSize);
            Bounds bottomBounds = new Bounds(bottomCenter, bottomSize);

            boundsSegments.Add(topBounds);
            boundsSegments.Add(bottomBounds);
        }

        private void SubdivideZ(List<Bounds> boundsSegments, Bounds bounds, int segmentSubdivision)
        {
            int segmentSubdivisionPosition = UnityEngine.Random.Range(1, segmentSubdivision);

            float size = bounds.size.x; // use x length
            float leftSegmentSize = size * segmentSubdivisionPosition / segmentSubdivision;
            float rightSegmentSize = size - leftSegmentSize;

            Vector3 leftCenter = new Vector3(bounds.center.x - bounds.extents.x + leftSegmentSize / 2, bounds.center.y, bounds.center.z);
            Vector3 rightCenter = new Vector3(bounds.center.x + bounds.extents.x - rightSegmentSize / 2, bounds.center.y, bounds.center.z);

            Vector3 leftSize = new Vector3(leftSegmentSize, bounds.size.y, bounds.size.z);
            Vector3 rightSize = new Vector3(rightSegmentSize, bounds.size.y, bounds.size.z);

            Bounds leftBounds = new Bounds(leftCenter, leftSize);
            Bounds rightBounds = new Bounds(rightCenter, rightSize);

            boundsSegments.Add(leftBounds);
            boundsSegments.Add(rightBounds);
        }



        private int GetNextSegmentIndex(List<Bounds> boundsSegments, PartitionStrategy strategy)
        {
            switch (strategy)
            {
                case PartitionStrategy.Random:
                    {
                        int index = UnityEngine.Random.Range(0, boundsSegments.Count - 1);
                        return index;
                    }
                case PartitionStrategy.Ratio:
                    {
                        int selectedIndex = 0;
                        float smallestRatio = boundsSegments[selectedIndex].size.x > boundsSegments[selectedIndex].size.z ? boundsSegments[selectedIndex].size.x / boundsSegments[selectedIndex].size.z : boundsSegments[selectedIndex].size.z / boundsSegments[selectedIndex].size.x;
                        smallestRatio = boundsSegments[selectedIndex].size.magnitude;

                        for (int i = 1; i < boundsSegments.Count; i++)
                        {
                            float currRatio = boundsSegments[i].size.x > boundsSegments[i].size.z ? boundsSegments[i].size.x / boundsSegments[i].size.z : boundsSegments[i].size.z / boundsSegments[i].size.x;
                            currRatio = boundsSegments[selectedIndex].size.magnitude;

                            if (currRatio < smallestRatio)
                            {
                                smallestRatio = currRatio;
                                selectedIndex = i;
                            }
                        }

                        return selectedIndex;
                    }
                case PartitionStrategy.Size:
                    {
                        int index = 0;
                        float largestSize = boundsSegments[0].size.x * boundsSegments[0].size.z;

                        for (int i = 1; i < boundsSegments.Count; i++)
                        {
                            float currSize = boundsSegments[i].size.x * boundsSegments[i].size.z;

                            if (currSize > largestSize)
                            {
                                largestSize = currSize;
                                index = i;
                            }
                        }

                        return index;
                    }
                default:
                    throw new System.ArgumentException("Unsupported PartitionStrategy " + strategy);
            }

        }

        /// <summary>
        /// Create Biome masks for the specified bounds list
        /// </summary>
        /// <param name="boundsList"></param>
        private void CreateMasks(Bounds bounds)
        {
            // for 2 biomes we need to split the terrain once. for n biomes we need to split it n-1 times
            int partitionCount = editor.extension.rectangularPartitionSettings.biomeCount - 1;

            float density = editor.extension.biomeSettings.density;

            List<Bounds> boundsList = Subdivide(bounds, partitionCount);
            for (int i = 0; i < boundsList.Count; i++)
            {
                // skip randomly
                if (density != 1 && UnityEngine.Random.Range(0f, 1f) >= density)
                {
                    continue;
                }

                int maskId = editor.GetNextMaskId();

                Bounds boundsSegment = boundsList[i];

                CreateBiomeMaskArea("Biome Mask Mask " + maskId, "Mask " + maskId, boundsSegment);

            }
        }

        private void CreateBiomeMaskArea(string gameObjectName, string maskName, Bounds bounds)
        {
            // modify bounds, reduce its rectangular distribution
            float minFactor = editor.extension.rectangularPartitionSettings.boundsShiftFactorMin;
            float maxFactor = editor.extension.rectangularPartitionSettings.boundsShiftFactorMax;

            bounds = PolygonUtils.ShiftResizeBounds(bounds, minFactor, maxFactor);

            List<Vector3> nodes;

            if (editor.extension.shapeSettings.randomShape)
            {
                // old mechanism: convex hull
                // nodes = ShapeCreator.CreateRandomShapeUsingConvexHull(bounds, randomPointCount);

                // new mechanism: random shape with parameters like convexity
                nodes = ShapeCreator.CreateRandomShape(
                            ShapeCreator.CreateRectangularBoundsShape(bounds),
                            editor.extension.shapeSettings.RandomConvexity, //
                            editor.extension.shapeSettings.keepOriginalPoints, //
                            editor.extension.shapeSettings.RandomPointsCount, //
                            editor.extension.shapeSettings.randomAngle, //
                            editor.extension.shapeSettings.douglasPeuckerReductionTolerance);
            }
            // exact bounds
            else
            {

                nodes = ShapeCreator.CreateRectangularBoundsShape(bounds);

            }


            // bounds for clipping
            Vector2[] clipPolygon = editor.GetBiomeClipPolygon(bounds);

            // clip, convert to vector2
            Vector2[] polygonXY = nodes.Select(item => new Vector2(item.x, item.z)).ToArray();
            Vector2[] clippedPoints = SutherlandHodgman.GetIntersectedPolygon(polygonXY, clipPolygon);

            if (clippedPoints == null || clippedPoints.Length < 3)
                return;

            // convert back to vector3
            nodes = clippedPoints.Select(item => new Vector3(item.x, 0, item.y)).ToList();

            // create the biome mask using the nodes
            CreateBiomeMaskArea(gameObjectName, maskName, bounds, nodes);

        }

        private void CreateBiomeMaskArea(string gameObjectName, string maskName, Bounds bounds, List<Vector3> nodes)
        {


            float blendDistanceMin = editor.extension.biomeSettings.biomeBlendDistanceMin;
            float blendDistanceMax = editor.extension.biomeSettings.biomeBlendDistanceMax;

            float biomeBlendDistance = UnityEngine.Random.Range(blendDistanceMin, blendDistanceMax);
            float blendDistance = bounds.extents.magnitude / 2f * biomeBlendDistance;

            // create the mask using the provided parameters
            editor.CreateBiomeMaskArea(gameObjectName, maskName, bounds.center, nodes, blendDistance);

        }
    }
}
