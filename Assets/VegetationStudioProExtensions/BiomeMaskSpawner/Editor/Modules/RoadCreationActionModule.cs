using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static VegetationStudioProExtensions.ProcessingSettings;

namespace VegetationStudioProExtensions
{
    public class RoadCreationActionModule : IActionModule
    {
        private BiomeMaskSpawnerExtensionEditor editor;

        public RoadCreationActionModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnInspectorGUI()
        {
            //
            // creation buttons
            //
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Creation", GUIStyles.GroupTitleStyle);

            GUILayout.BeginHorizontal();
            {

                // create biome mask
                if (GUILayout.Button("Create Road Markers"))
                {
                    ApplyCreateAction();
                }
                else if (GUILayout.Button("Clear"))
                {
                    ApplyClearAction();
                }

            }
            GUILayout.EndHorizontal();
        }


        private void ApplyCreateAction()
        {
            // get bounds for terrain partitioning
            List<Bounds> boundsList = editor.GetBoundsToProcess();

            // perform partitioning and create masks
            // this can have loose case statements, we only list the ones we support in this action module
            switch (editor.extension.boundsSettings.partitionAlgorithm)
            {
                case PartitionAlgorithm.Road:
                    editor.roadModule.CreateMasks(boundsList);
                    break;

                default:
                    throw new System.ArgumentException("Unsupported Partition Algorithm " + editor.extension.boundsSettings.partitionAlgorithm);
            }

        }

        private void ApplyClearAction()
        {
            // perform partitioning and create masks
            // this can have loose case statements, we only list the ones we support in this action module
            switch (editor.extension.boundsSettings.partitionAlgorithm)
            {
                case PartitionAlgorithm.Road:
                    editor.roadModule.Clear();
                    break;

                default:
                    throw new System.ArgumentException("Unsupported Partition Algorithm " + editor.extension.boundsSettings.partitionAlgorithm);
            }
        }
    }
}
