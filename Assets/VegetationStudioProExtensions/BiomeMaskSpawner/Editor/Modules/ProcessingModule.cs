using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static VegetationStudioProExtensions.ProcessingSettings;

namespace VegetationStudioProExtensions
{
    public class ProcessingModule
    {
        private SerializedProperty terrainProcessing;
        private SerializedProperty boundsBiomeMaskArea;
        private SerializedProperty boundsBiomeMaskAreaValid;
        private SerializedProperty partitionAlgorithm;

        private BiomeMaskSpawnerExtensionEditor editor;

        public ProcessingModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
            terrainProcessing = editor.FindProperty(x => x.boundsSettings.boundsProcessing);
            partitionAlgorithm = editor.FindProperty(x => x.boundsSettings.partitionAlgorithm);
            boundsBiomeMaskArea = editor.FindProperty(x => x.boundsSettings.biomeMaskArea);
            boundsBiomeMaskAreaValid = editor.FindProperty(x => x.boundsSettings.biomeMaskAreaValid);
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Terrain Processing", GUIStyles.GroupTitleStyle);

            EditorGUILayout.PropertyField(partitionAlgorithm, new GUIContent("Algorithm", "The algorithm to use for terrain partitioning."));

            EditorGUILayout.PropertyField(terrainProcessing, new GUIContent("Bounds", "Process all terrains as a single combined terrain or all terrains individually."));

            BoundsProcessing selectedTerrainProcessing = (BoundsProcessing)System.Enum.GetValues(typeof(BoundsProcessing)).GetValue(terrainProcessing.enumValueIndex);

            if (selectedTerrainProcessing == BoundsProcessing.Biome)
            {

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(boundsBiomeMaskArea, new GUIContent("Biome Mask", "The Biome used for clipping."));

                // check if the changed biome mask is convex
                if (EditorGUI.EndChangeCheck() || editor.performInitialConsistencyCheck)
                {

                    if (boundsBiomeMaskArea.objectReferenceValue != null)
                    {
                        boundsBiomeMaskAreaValid.boolValue = false;

                        BiomeMaskArea biomeMaskArea = (BiomeMaskArea)boundsBiomeMaskArea.objectReferenceValue;

                        Vector2[] clipPolygon = editor.GetBiomeClipPolygon(biomeMaskArea);

                        if (clipPolygon != null)
                        {
                            // consistency check: clip polygon must be convex for sutherland hodgman
                            bool isConvex = PolygonUtils.PolygonIsConvex(clipPolygon);
                            if (isConvex)
                            {
                                boundsBiomeMaskAreaValid.boolValue = true;
                            }
                            else
                            {
                                Debug.LogError("Invalid clipping mask: " + biomeMaskArea.name + " (" + biomeMaskArea.MaskName + ")");
                            }
                        }
                    }
                }

                // show error in case the mask doesn't exist
                if (boundsBiomeMaskArea.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("The Biome Mask must be defined!", MessageType.Error);
                }
                // show error in case the mask isn't convex
                else if (!boundsBiomeMaskAreaValid.boolValue)
                {
                    EditorGUILayout.HelpBox("The Biome Mask must be convex!", MessageType.Error);
                }
            }
        }

        public PartitionAlgorithm GetSelectedPartitionAlgorithm()
        {
            PartitionAlgorithm selectedPartitionAlgorithm = (PartitionAlgorithm)System.Enum.GetValues(typeof(PartitionAlgorithm)).GetValue(partitionAlgorithm.enumValueIndex);

            return selectedPartitionAlgorithm;
        }
    }
}
