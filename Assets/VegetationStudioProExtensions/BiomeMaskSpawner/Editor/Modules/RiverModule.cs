using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VegetationStudioProExtensions
{
    public class RiverModule : ISettingsModule
    {
#if RAM_2019
        private SerializedProperty riverCount;
        private SerializedProperty riverAutomation;
        private SerializedProperty riverSplineProfile;
        private SerializedProperty riverWidthMin;
        private SerializedProperty riverWidthMax;
        private SerializedProperty riverSimulatedRiverLengthMin;
        private SerializedProperty riverSimulatedRiverLengthMax;
        private SerializedProperty riverSimulatedRiverPointsMin;
        private SerializedProperty riverSimulatedRiverPointsMax;
        private SerializedProperty riverSimulatedMinStepSizeMin;
        private SerializedProperty riverSimulatedMinStepSizeMax;
        private SerializedProperty riverSimulatedNoUp;
        private SerializedProperty riverSimulatedBreakOnUp;
        private SerializedProperty riverNoiseWidth;
        private SerializedProperty riverNoiseMultiplierWidthMin;
        private SerializedProperty riverNoiseMultiplierWidthMax;
        private SerializedProperty riverNoiseSizeWidthMin;
        private SerializedProperty riverNoiseSizeWidthMax;
#endif

        private BiomeMaskSpawnerExtensionEditor editor;

        public RiverModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
#if RAM_2019
            riverCount = editor.FindProperty(x => x.riverSettings.count);
            riverAutomation = editor.FindProperty(x => x.riverSettings.automation);
            riverSplineProfile = editor.FindProperty(x => x.riverSettings.splineProfile);
            riverWidthMin = editor.FindProperty(x => x.riverSettings.widthMin);
            riverWidthMax = editor.FindProperty(x => x.riverSettings.widthMax);
            riverSimulatedRiverLengthMin = editor.FindProperty(x => x.riverSettings.simulatedRiverLengthMin);
            riverSimulatedRiverLengthMax = editor.FindProperty(x => x.riverSettings.simulatedRiverLengthMax);
            riverSimulatedRiverPointsMin = editor.FindProperty(x => x.riverSettings.simulatedRiverPointsMin);
            riverSimulatedRiverPointsMax = editor.FindProperty(x => x.riverSettings.simulatedRiverPointsMax);
            riverSimulatedMinStepSizeMin = editor.FindProperty(x => x.riverSettings.simulatedMinStepSizeMin);
            riverSimulatedMinStepSizeMax = editor.FindProperty(x => x.riverSettings.simulatedMinStepSizeMax);
            riverSimulatedNoUp = editor.FindProperty(x => x.riverSettings.simulatedNoUp);
            riverSimulatedBreakOnUp = editor.FindProperty(x => x.riverSettings.simulatedBreakOnUp);
            riverNoiseWidth = editor.FindProperty(x => x.riverSettings.noiseWidth);
            riverNoiseMultiplierWidthMin = editor.FindProperty(x => x.riverSettings.noiseMultiplierWidthMin);
            riverNoiseMultiplierWidthMax = editor.FindProperty(x => x.riverSettings.noiseMultiplierWidthMax);
            riverNoiseSizeWidthMin = editor.FindProperty(x => x.riverSettings.noiseSizeWidthMin);
            riverNoiseSizeWidthMax = editor.FindProperty(x => x.riverSettings.noiseSizeWidthMax);
#endif
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("River", GUIStyles.GroupTitleStyle);
#if RAM_2019

            EditorGUILayout.PropertyField(riverCount, new GUIContent("Count", "The number of rivers to add."));

            // keep the count value >= 0
            if (riverCount.intValue < 1)
            {
                riverCount.intValue = 1;
            }

            EditorGUILayout.PropertyField(riverAutomation, new GUIContent("Automation", "Whether to simulate or to generate the rivers"));

            EditorGUILayout.PropertyField(riverSplineProfile, new GUIContent("River Profile", ""));

            EditorGUILayout.LabelField(new GUIContent("Width", ""));
            EditorGuiUtilities.MinMaxEditor("Min", ref riverWidthMin, "Max", ref riverWidthMax, 1, 1000f, true);

            EditorGUILayout.LabelField(new GUIContent("Length", ""));
            EditorGuiUtilities.MinMaxEditor("Min", ref riverSimulatedRiverLengthMin, "Max", ref riverSimulatedRiverLengthMax, 1f, 5000f, true);

            EditorGUILayout.LabelField(new GUIContent("Points Interval", ""));
            EditorGuiUtilities.MinMaxEditorInt("Min", ref riverSimulatedRiverPointsMin, "Max", ref riverSimulatedRiverPointsMax, 1, 100, true);

            EditorGUILayout.LabelField(new GUIContent("Sampling Interval", ""));
            EditorGuiUtilities.MinMaxEditor("Min", ref riverSimulatedMinStepSizeMin, "Max", ref riverSimulatedMinStepSizeMax, 0.5f, 5f, true);

            EditorGUILayout.PropertyField(riverSimulatedNoUp, new GUIContent("Block Uphill", ""));
            EditorGUILayout.PropertyField(riverSimulatedBreakOnUp, new GUIContent("Break Uphill", ""));

            EditorGUILayout.PropertyField(riverNoiseWidth, new GUIContent("Add Width Noise", ""));

            if (riverNoiseWidth.boolValue)
            {

                EditorGUILayout.LabelField(new GUIContent("Noise Multiplier Width", ""));
                EditorGuiUtilities.MinMaxEditor("Min", ref riverNoiseMultiplierWidthMin, "Max", ref riverNoiseMultiplierWidthMax, 1f, 10f, true);

                EditorGUILayout.LabelField(new GUIContent("Noise Scale Width", ""));
                EditorGuiUtilities.MinMaxEditor("Min", ref riverNoiseSizeWidthMin, "Max", ref riverNoiseSizeWidthMax, 0.1f, 50f, true);

            }
#else

            EditorGUILayout.HelpBox("Requires RAM 2019 installed and 'RAM_2019' Scripting Define Symbol", MessageType.Error);

#endif
        }

        /// <summary>
        /// Create rivers
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

        private void CreateMasks(Bounds bounds)
        {
#if RAM_2019
            RiverSettings riverSettings = editor.extension.riverSettings;

            for (int i = 0; i < riverSettings.count; i++)
            {
                // just some offset so that we don't start at an edge
                float offsetX = bounds.size.x / 10f;
                float offsetZ = bounds.size.z / 10f;

                float x = Random.Range(bounds.min.x + offsetX, bounds.max.x - offsetX);
                float y = 0;
                float z = Random.Range(bounds.min.z + offsetZ, bounds.max.z - offsetZ);

                // get y position on terrain by raycasting
                float? terrainHeight = TerrainUtils.GetTerrainHeight(x, z);
                if (terrainHeight != null)
                {
                    y = (float)terrainHeight;
                }

                Vector3 position = new Vector3(x, y, z);

                int maskId = editor.GetNextMaskId();
                CreateRiver("River " + maskId, position);

            }
#endif
        }

        public void CreateRiver(string gameObjectName, Vector3 position)
        {
#if RAM_2019
            RiverSettings riverSettings = editor.extension.riverSettings;

            GameObject parentGameObject = editor.extension.transform.gameObject;

            // create new gameobject
            RamSpline spline = RamSpline.CreateSpline(AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"));

            // ensure gameobject gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(spline.gameObject, parentGameObject);

            // set position
            spline.transform.position = Vector3.zero; // keeping it unique for the gameobject; using the point position we'd have to adjust the river points

            // apply river settings
            spline.width = 100;
            spline.currentProfile = riverSettings.splineProfile;
            spline.width = Random.Range(riverSettings.widthMin, riverSettings.widthMax);

            // apply simulation settings
            spline.AddPoint(position);

            spline.simulatedRiverLength = Random.Range(riverSettings.simulatedRiverLengthMin, riverSettings.simulatedRiverLengthMax);
            spline.simulatedRiverPoints = Random.Range(riverSettings.simulatedRiverPointsMin, riverSettings.simulatedRiverPointsMax);
            spline.simulatedMinStepSize = Random.Range(riverSettings.simulatedMinStepSizeMin, riverSettings.simulatedMinStepSizeMax);

            spline.simulatedNoUp = riverSettings.simulatedNoUp;
            spline.simulatedBreakOnUp = riverSettings.simulatedBreakOnUp;
            spline.noiseWidth = riverSettings.noiseWidth;

            spline.simulatedMinStepSize = Random.Range(riverSettings.simulatedMinStepSizeMin, riverSettings.simulatedMinStepSizeMax);
            spline.simulatedMinStepSize = Random.Range(riverSettings.simulatedMinStepSizeMin, riverSettings.simulatedMinStepSizeMax);

            spline.noiseMultiplierWidth = Random.Range(riverSettings.noiseMultiplierWidthMin, riverSettings.noiseMultiplierWidthMax);
            spline.noiseSizeWidth = Random.Range(riverSettings.noiseSizeWidthMin, riverSettings.noiseSizeWidthMax);

            // simulate
            bool generate = riverSettings.automation == RiverSettings.Automation.Generate;
            spline.SimulateRiver(generate);

            // register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(spline.gameObject, "Create " + spline.gameObject.name);

#endif
        }

        /// <summary>
        /// Workaround for a RAM bug: Simulated meshes don't get removed after the spline got deleted.
        /// TODO: remove after RAM gameobject deletion got fixed
        /// </summary>
        public static void RemoveRiverSimulationMeshes(GameObject container)
        {
#if RAM_2019
            RamSpline[] gameObjects = container.transform.gameObject.GetComponentsInChildren<RamSpline>();

            foreach (RamSpline go in gameObjects)
            {
                if (go.meshGO)
                {
                    if (Application.isEditor)
                        Object.DestroyImmediate(go.meshGO);
                    else
                        Object.Destroy(go.meshGO);

                }
            }
#endif
        }
    }
}
