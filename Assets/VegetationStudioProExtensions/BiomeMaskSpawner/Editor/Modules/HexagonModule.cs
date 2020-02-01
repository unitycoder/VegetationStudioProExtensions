using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SutherlandHodgmanAlgorithm;
using UnityEditor;

namespace VegetationStudioProExtensions
{
    // just a test to see how it looks like
    // TODO:
    //    + individual distribution needs clipping
    //    + proper calculation of the hexagon size in x and z direction (needs proper positioning instead of starting at the center)
    public class HexagonModule
    {
        private SerializedProperty hexagonRadius;

        private BiomeMaskSpawnerExtensionEditor editor;

        public HexagonModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
            hexagonRadius = editor.FindProperty(x => x.hexagonSettings.radius);
        }

        public void OnInspectorGUI()
        {

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Hexagon", GUIStyles.GroupTitleStyle);

            EditorGUILayout.PropertyField(hexagonRadius, new GUIContent("Radius", "The outer radius of a hexagon"));

            hexagonRadius.floatValue = Utils.ClipMin(hexagonRadius.floatValue, 10f);
        }

        /// <summary>
        /// Create Biome masks for the specified bounds list
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


        private float GetOuterRadius(Bounds bounds)
        {
            return editor.extension.hexagonSettings.radius;
        }

        private float GetInnerRadius(float outerRadius)
        {
            return outerRadius * Mathf.Sqrt(3f) / 2;
        }

        /// <summary>
        /// Create Biome masks for the specified bounds
        /// </summary>
        /// <param name="bounds"></param>
        private void CreateMasks(Bounds bounds)
        {
            float outerRadius = GetOuterRadius(bounds);

            // bounds for clipping
            Vector2[] clipPolygon = editor.GetBiomeClipPolygon(bounds);

            float density = editor.extension.biomeSettings.density;

            List<Vector3> positions = GetPositions(bounds);

            foreach (Vector3 position in positions)
            {
                // skip randomly
                if (density != 1 && UnityEngine.Random.Range(0f, 1f) >= density)
                {
                    continue;
                }

                Vector3[] hexagon = ShapeCreator.CreateHexagon(position, outerRadius);

                // clip, convert to vector2
                Vector2[] polygonXY = hexagon.Select(item => new Vector2(item.x, item.z)).ToArray();
                Vector2[] clippedPoints = SutherlandHodgman.GetIntersectedPolygon(polygonXY, clipPolygon);

                if (clippedPoints == null ||clippedPoints.Length < 3)
                    continue;

                // convert back to vector3
                hexagon = clippedPoints.Select(item => new Vector3(item.x, 0, item.y)).ToArray();

                int maskId = editor.GetNextMaskId();

                List<Vector3> nodes = hexagon.OfType<Vector3>().ToList();

                // apply random shape if requested
                if (editor.extension.shapeSettings.randomShape)
                {

                    nodes = ShapeCreator.CreateRandomShape(nodes, //
                        editor.extension.shapeSettings.RandomConvexity, //
                        editor.extension.shapeSettings.keepOriginalPoints, //
                        editor.extension.shapeSettings.RandomPointsCount, //
                        editor.extension.shapeSettings.randomAngle, //
                        editor.extension.shapeSettings.douglasPeuckerReductionTolerance);
                }

                CreateBiomeMaskArea("Biome Mask " + maskId, "Mask " + maskId, position, nodes);


            }
        }

        private List<Vector3> GetPositions(Bounds bounds)
        {

            float outerRadius = GetOuterRadius( bounds);
            float innerRadius = GetInnerRadius( outerRadius);

            float width = bounds.size.x;
            float height = bounds.size.z;

            float stepsX = width / innerRadius / 2f + 2; // +2: ensure there are enough exagons, we may not start exactly at a whole hexagon; TODO: proper calculation & alignment if someone wants proper hexagon fields
            float stepsZ = height / innerRadius / 2f + 2; // +2 ensure there are enough exagons, we may not start exactly at a whole hexagon; TODO: proper calculation & alignment if someone wants proper hexagon fields
            
            List<Vector3> positions = new List<Vector3>();

            for (float z = 0; z < stepsZ; z++)
            {

                for (float x = 0; x < stepsX; x++)
                {
                    Vector3 position = new Vector3();

                    position.x = x * (innerRadius * 2f) + (z % 2f) * innerRadius;
                    position.y = 0f;
                    position.z = z * (outerRadius * 1.5f);

                    position.x += bounds.center.x - bounds.extents.x;
                    position.z += bounds.center.z - bounds.extents.z;

                    positions.Add(position);
                }

                
                
            }

            return positions;
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
            float blendDistanceMin = editor.extension.biomeSettings.biomeBlendDistanceMin;
            float blendDistanceMax = editor.extension.biomeSettings.biomeBlendDistanceMax;

            float biomeBlendDistance = UnityEngine.Random.Range(blendDistanceMin, blendDistanceMax);

            // mean vector is just a simple measure. please adapt if you need more accuracy
            Vector3 meanVector = PolygonUtils.GetMeanVector(nodes);

            float blendDistance = meanVector.magnitude / 2f * biomeBlendDistance;

            // create the mask using the provided parameters
            editor.CreateBiomeMaskArea(gameObjectName, maskName, position, nodes, blendDistance);

        }
    }
}
