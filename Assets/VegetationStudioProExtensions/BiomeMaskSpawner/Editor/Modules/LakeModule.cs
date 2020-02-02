using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class LakeModule : ISettingsModule
    {
        private SerializedProperty lakeCreateLake;

#if RAM_2019
        private SerializedProperty lakeLakeProfile;

        private SerializedProperty lakeRamInternalLakeCreation;
        private SerializedProperty lakeCloseDistanceSimulation;
        private SerializedProperty lakeAngleSimulation;
        private SerializedProperty lakeCheckDistanceSimulation;
#endif


        private BiomeMaskSpawnerExtensionEditor editor;

        public LakeModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
            lakeCreateLake = editor.FindProperty(x => x.lakeSettings.createLake);

#if RAM_2019
            lakeLakeProfile = editor.FindProperty(x => x.lakeSettings.lakeProfile);

            lakeRamInternalLakeCreation = editor.FindProperty(x => x.lakeSettings.ramInternalLakeCreation);
            lakeCloseDistanceSimulation = editor.FindProperty(x => x.lakeSettings.closeDistanceSimulation);
            lakeAngleSimulation = editor.FindProperty(x => x.lakeSettings.angleSimulation);
            lakeCheckDistanceSimulation = editor.FindProperty(x => x.lakeSettings.checkDistanceSimulation);
#endif
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

                EditorGUILayout.LabelField("Lake Settings", GUIStyles.GroupTitleStyle);
                {

                    EditorGUILayout.PropertyField(lakeCreateLake, new GUIContent("Create Lake", "Create a RAM 2019 Lake"));

#if RAM_2019

                    if(lakeCreateLake.boolValue)
                    {
                        EditorGUILayout.PropertyField(lakeLakeProfile, new GUIContent("Lake Profile", "The lake profile to use"));
                
                        EditorGUILayout.PropertyField(lakeRamInternalLakeCreation, new GUIContent("RAM Simulation", "Set vertex position at mask position, but use RAM internal simulation instead of biome shape"));

                       if(lakeRamInternalLakeCreation.boolValue)
                       {
                            EditorGUILayout.PropertyField(lakeAngleSimulation, new GUIContent("Angle", ""));

                            EditorGUILayout.PropertyField(lakeCloseDistanceSimulation, new GUIContent("Point Distance", ""));
                            EditorGUILayout.PropertyField(lakeCheckDistanceSimulation, new GUIContent("Check Distance", ""));
                       }
                    }
#else
                if (lakeCreateLake.boolValue)
                {
                    EditorGUILayout.HelpBox("Requires RAM 2019 installed and 'RAM_2019' Scripting Define Symbol", MessageType.Error);
                }
#endif
            }


        }

        public void CreateLake(BiomeMaskArea mask, string gameObjectName, List<Vector3> nodes)
        {
#if RAM_2019
            LakePolygon lakePolygon = RamLakeCreator.CreateLakePolygon(editor.extension.lakeSettings, mask, mask.transform.gameObject, gameObjectName, nodes);
#endif
        }
    }
}
