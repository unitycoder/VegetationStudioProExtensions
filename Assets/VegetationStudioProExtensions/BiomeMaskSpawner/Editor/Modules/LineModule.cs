using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    /// <summary>
    /// Create rectangular shapes and rotate them randomly.
    /// No clipping is performed, the shapes are intended to be just some thin forest lines for details.
    /// No biome intersection check is performed, might be more natural this way.
    /// </summary>
    public class LineModule : ISettingsModule
    {
        private SerializedProperty lineCount;
        private SerializedProperty lineWidthMin;
        private SerializedProperty lineWidthMax;
        private SerializedProperty lineHeightMin;
        private SerializedProperty lineHeightMax;
        private SerializedProperty lineAngleMin;
        private SerializedProperty lineAngleMax;
        private SerializedProperty lineAttachedToBiome;
        private SerializedProperty lineBiomeMaskArea;
        private SerializedProperty lineAttachedAngleDelta;
        private SerializedProperty lineAttachedAngleFlip;

        private BiomeMaskSpawnerExtensionEditor editor;

        public LineModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
            lineCount = editor.FindProperty(x => x.lineSettings.count);
            lineHeightMin = editor.FindProperty(x => x.lineSettings.heightMin);
            lineHeightMax = editor.FindProperty(x => x.lineSettings.heightMax);
            lineWidthMin = editor.FindProperty(x => x.lineSettings.widthMin);
            lineWidthMax = editor.FindProperty(x => x.lineSettings.widthMax);
            lineAngleMin = editor.FindProperty(x => x.lineSettings.angleMin);
            lineAngleMax = editor.FindProperty(x => x.lineSettings.angleMax);
            lineAttachedToBiome = editor.FindProperty(x => x.lineSettings.attachedToBiome);
            lineBiomeMaskArea = editor.FindProperty(x => x.lineSettings.biomeMaskArea);
            lineAttachedAngleDelta = editor.FindProperty(x => x.lineSettings.attachedAngleDelta);
            lineAttachedAngleFlip = editor.FindProperty(x => x.lineSettings.attachedAngleFlip);
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Lines", GUIStyles.GroupTitleStyle);

            EditorGUILayout.PropertyField(lineCount, new GUIContent("Count", "The number of lines to add."));

            // keep the count value >= 0
            if (lineCount.intValue < 0)
            {
                lineCount.intValue = 0;
            }

            EditorGUILayout.LabelField(new GUIContent("Line Width", "The minimum and maximum line widths"));
            EditorGuiUtilities.MinMaxEditor("Min", ref lineWidthMin, "Max", ref lineWidthMax, 1, 100, true);

            EditorGUILayout.LabelField(new GUIContent("Line Length", "The minimum and maximum line lengths"));
            EditorGuiUtilities.MinMaxEditor("Min", ref lineHeightMin, "Max", ref lineHeightMax, 1, 1000, true);

            EditorGUILayout.LabelField(new GUIContent("Angle", "The rotation angle limits in degrees"));
            EditorGuiUtilities.MinMaxEditor("Min", ref lineAngleMin, "Max", ref lineAngleMax, 0, 360, true);

            EditorGUILayout.PropertyField(lineAttachedToBiome, new GUIContent("Attached to Biome", "The line is attached to the edge of an existing biome or loose on the terrain"));

            if (lineAttachedToBiome.boolValue)
            {
                EditorGUILayout.PropertyField(lineBiomeMaskArea, new GUIContent("Biome Mask", "The Biome used for line attachment."));

                // show error in case the mask doesn't exist
                if (lineBiomeMaskArea.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("The Biome Mask must be defined!", MessageType.Error);
                }

                EditorGUILayout.PropertyField(lineAttachedAngleDelta, new GUIContent("Angle Offset", "The line is normally attached 90 degrees to the biome mask edge. This value allows for some randomness by modifying the angle +/- this value in degrees."));

                EditorGUILayout.PropertyField(lineAttachedAngleFlip, new GUIContent("Inwards", "If active, then the lines will be drawn inwards, into the biome."));

            }
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
            LineSettings lineSettings = editor.extension.lineSettings;
            
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

                int maskId = editor.GetNextMaskId();
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
            BiomeMaskArea mask = editor.extension.lineSettings.biomeMaskArea;

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
            bool flipAngle = editor.extension.lineSettings.attachedAngleFlip;
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
