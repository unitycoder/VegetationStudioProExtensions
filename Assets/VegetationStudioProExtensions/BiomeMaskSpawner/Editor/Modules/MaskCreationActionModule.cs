using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static VegetationStudioProExtensions.ProcessingSettings;

namespace VegetationStudioProExtensions
{
    public class MaskCreationActionModule : IActionModule
    {

        /// <summary>
        /// The factor for growing or shrinking.
        /// </summary>
        private float resizeFactor = 0.05f;

        private BiomeMaskSpawnerExtensionEditor editor;

        public MaskCreationActionModule(BiomeMaskSpawnerExtensionEditor editor)
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
                if (GUILayout.Button("Clear & Add"))
                {
                    editor.ResetMaskId();
                    RemoveContainerChildren();
                    ApplyCreateAction();
                }
                else if (GUILayout.Button("Clear"))
                {
                    editor.ResetMaskId();
                    RemoveContainerChildren();
                }
                else if (GUILayout.Button("Add"))
                {
                    ApplyCreateAction();
                }

            }
            GUILayout.EndHorizontal();

            //
            // modification
            //
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Modification", GUIStyles.GroupTitleStyle);

            GUILayout.BeginHorizontal();
            {

                if (GUILayout.Button("Grow All"))
                {
                    GrowAll();
                }
                else if (GUILayout.Button("Shrink All"))
                {
                    ShrinkAll();
                }

            }
            GUILayout.EndHorizontal();

        }

        private void GrowAll()
        {
            // get all biome mask area gameobjects of this gameobject
            GameObject parentGo = editor.extension.transform.gameObject;
            BiomeMaskArea[] masks = parentGo.GetComponentsInChildren<BiomeMaskArea>();

            foreach (BiomeMaskArea mask in masks)
            {
                BiomeMaskUtils.Grow(mask, resizeFactor);
            }

        }

        private void ShrinkAll()
        {
            // get all biome mask area gameobjects of this gameobject
            GameObject parentGo = editor.extension.transform.gameObject;
            BiomeMaskArea[] masks = parentGo.GetComponentsInChildren<BiomeMaskArea>();

            foreach (BiomeMaskArea mask in masks)
            {
                BiomeMaskUtils.Shrink(mask, resizeFactor);
            }
        }

        private void ApplyCreateAction()
        {
            // get bounds for terrain partitioning
            List<Bounds> boundsList = editor.GetBoundsToProcess();

            // perform partitioning and create masks
            // this can have loose case statements, we only list the ones we support in this action module
            switch (editor.extension.boundsSettings.partitionAlgorithm)
            {
                case PartitionAlgorithm.Voronoi:
                    editor.voronoiModule.CreateMasks(boundsList);
                    break;

                case PartitionAlgorithm.Rectangular:
                    editor.rectangularPartitionModule.CreateMasks(boundsList);
                    break;

                case PartitionAlgorithm.Hexagon:
                    editor.hexagonModule.CreateMasks(boundsList);
                    break;

                case PartitionAlgorithm.Line:
                    editor.lineModule.CreateMasks(boundsList);
                    break;

                case PartitionAlgorithm.River:
                    editor.riverModule.CreateMasks(boundsList);
                    break;

                default:
                    throw new System.ArgumentException("Unsupported Partition Algorithm " + editor.extension.boundsSettings.partitionAlgorithm);
            }

            RefreshTerrainHeightmap();
        }


        /// <summary>
        /// Remove all children of the Biome Creator
        /// </summary>
        public void RemoveContainerChildren()
        {
            GameObject container = editor.extension.transform.gameObject as GameObject;

            // Workaround for a RAM bug: Simulated meshes don't get removed after the spline got deleted => we delete them manually
            // TODO: remove after RAM gameobject deletion got fixed
            if (editor.processingModule.GetSelectedPartitionAlgorithm() == PartitionAlgorithm.River)
            {
                RiverModule.RemoveRiverSimulationMeshes(container);
            }

            // register undo
            Undo.RegisterFullObjectHierarchyUndo(container, "Remove " + container);

            List<Transform> list = new List<Transform>();
            foreach (Transform child in container.transform)
            {
                list.Add(child);
            }

            foreach (Transform child in list)
            {
                GameObject go = child.gameObject;

                BiomeMaskSpawnerExtensionEditor.DestroyImmediate(go);

            }


        }

        public void RefreshTerrainHeightmap()
        {
            List<VegetationSystemPro> VegetationSystemList = BiomeMaskSpawnerExtensionEditor.VegetationStudioInstance.VegetationSystemList;

            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].RefreshTerrainHeightmap();
            }
        }

    }
}