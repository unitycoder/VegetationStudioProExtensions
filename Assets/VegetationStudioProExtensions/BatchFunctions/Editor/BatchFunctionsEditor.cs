using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor.SceneManagement;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.TerrainSystem;

namespace VegetationStudioProExtensions
{
    /// <summary>
    /// Helper utilities to apply settings on multiple vegetation items.
    /// </summary>
    [CustomEditor(typeof(BatchFunctions))]
    [CanEditMultipleObjects]
    public class BatchFunctionsEditor : BaseEditor<BatchFunctions>
    {
        private BatchFunctions extension;

        private static VegetationStudioManager VegetationStudioInstance;

        private SerializedProperty biomeType;

        // local attributes which aren't persisted
        #region local attributes

        private VegetationRenderMode vegetationRenderMode = VegetationRenderMode.InstancedIndirect;

        #endregion local attributes

        public void OnEnable()
        {
            VegetationStudioInstance = VegetationStudioProUtils.FindVegetationStudioInstance();

            extension = (BatchFunctions)target;

            // biomes
            biomeType = FindProperty(x => x.biomeType);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.HelpBox("Apply settings on selected Biomes", MessageType.Info);

                EditorGUILayout.LabelField("Batch Filter", GUIStyles.GroupTitleStyle);
                {
                    EditorGUILayout.PropertyField(biomeType, new GUIContent("Biome Type", "The Biome type to be used."));
                }

            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Runtime Spawn", GUIStyles.GroupTitleStyle);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Enable ALL in Filter"))
                        {
                            SetRuntimeSpawn(true, extension.biomeType);
                        }

                        if (GUILayout.Button("Disable ALL in Filter"))
                        {
                            SetRuntimeSpawn(false, extension.biomeType);
                        }

                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Render Mode", GUIStyles.GroupTitleStyle);
                {
                    vegetationRenderMode = (VegetationRenderMode)EditorGUILayout.EnumPopup("Render Mode", vegetationRenderMode);

                    if (GUILayout.Button("Set Render Mode"))
                    {
                        SetRenderMode(vegetationRenderMode, extension.biomeType);
                    }

                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Refresh", GUIStyles.GroupTitleStyle);
                {
                    if (GUILayout.Button("Refresh Prefabs"))
                    {
                        RefreshPrefabs(extension.biomeType);
                    }

                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Quick Access", GUIStyles.GroupTitleStyle);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Generate Biome SplatMaps"))
                        {
                            GenerateBiomeSplatMaps();
                        }

                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Generate all biome splatmaps
        /// </summary>
        private void GenerateBiomeSplatMaps()
        {
            TerrainSystemPro[] terrainSystemProList = FindObjectsOfType<TerrainSystemPro>();
            foreach (TerrainSystemPro terrainSystemPro in terrainSystemProList)
            {
                terrainSystemPro.GenerateSplatMap(false);
                terrainSystemPro.ShowTerrainHeatmap(false);
            }
        }

        /// <summary>
        /// Set the runtime spawn flag on the selected biome type
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="selectedBiomeType"></param>
        private void SetRuntimeSpawn(bool enabled, BiomeType selectedBiomeType)
        {
            RuntimeSpawnProcessor processor = new RuntimeSpawnProcessor(enabled);
            int itemChangeCount = processor.Process(selectedBiomeType);

            // show confirmation messagebox
            EditorUtility.DisplayDialog("Set Runtime Spawn", "Runtime Spawn changed to " + enabled + " for " + itemChangeCount + " items in Biome " + selectedBiomeType, "Ok");

        }

        private void SetRenderMode(VegetationRenderMode vegetationRenderMode, BiomeType selectedBiomeType)
        {
            RenderModeProcessor processor = new RenderModeProcessor(vegetationRenderMode);
            int itemChangeCount = processor.Process(selectedBiomeType);

            // show confirmation messagebox
            EditorUtility.DisplayDialog("Set Render Mode", "Render mode changed to " + vegetationRenderMode + " for " + itemChangeCount + " items in Biome " + selectedBiomeType, "Ok");

        }

        private void RefreshPrefabs(BiomeType selectedBiomeType)
        {
            // lengthy process, ask first
            bool isContinue = EditorUtility.DisplayDialog("Refresh Prefabs", "This might take a while depending on the number of vegetation items. Continue?", "Yes", "No");

            if (!isContinue)
                return;

            // apply action
            RefreshPrefabsProcessor processor = new RefreshPrefabsProcessor();
            int itemChangeCount = processor.Process(selectedBiomeType);

            // show confirmation messagebox
            EditorUtility.DisplayDialog("Refresh Prefabs", "Refresh Prefabs applied to " + itemChangeCount + " items in Biome " + selectedBiomeType, "Ok");

        }

        /// <summary>
        /// Set the runtime spawn for all items of a biome.
        /// </summary>
        private class RuntimeSpawnProcessor : VegetationItemsProcessor
        {
            private bool enabled;

            public RuntimeSpawnProcessor(bool enabled)
            {
                this.enabled = enabled;
            }

            public override void OnActionPerformed(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro)
            {
                Debug.Log("Setting Runtime Spawn of " + vegetationItemInfoPro.Name + " from " + vegetationItemInfoPro.EnableRuntimeSpawn + " to " + enabled);

                // apply settings
                vegetationItemInfoPro.EnableRuntimeSpawn = enabled;
            }
        }

        /// <summary>
        /// Set the render mode for all items of a biome.
        /// </summary>
        private class RenderModeProcessor : VegetationItemsProcessor
        {
            private VegetationRenderMode vegetationRenderMode;

            public RenderModeProcessor(VegetationRenderMode vegetationRenderMode)
            {
                this.vegetationRenderMode = vegetationRenderMode;
            }

            public override void OnActionPerformed(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro)
            {
                Debug.Log("Setting Render Mode of " + vegetationItemInfoPro.Name + " from " + vegetationItemInfoPro.VegetationRenderMode + " to " + vegetationRenderMode);

                // apply settings
                vegetationItemInfoPro.VegetationRenderMode = vegetationRenderMode;
            }
        }

        /// <summary>
        /// Apply "Refresh Prefab" to all items of a biome.
        /// </summary>
        private class RefreshPrefabsProcessor : VegetationItemsProcessor
        {
            public override void OnActionPerformed(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro)
            {
                Debug.Log("Refreshing Prefab " + vegetationItemInfoPro.Name);

                // apply settings
                vegetationPackagePro.RefreshVegetationItemPrefab(vegetationItemInfoPro);
            }
        }

        /// <summary>
        /// Process all vegetation items of the vegetation system for the selected biome and apply an action.
        /// </summary>
        private abstract class VegetationItemsProcessor
        {
            /// <summary>
            /// Apply this function to all items of the selected biome
            /// </summary>
            /// <param name="vegetationItemInfoPro"></param>
            public abstract void OnActionPerformed(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro);

            /// <summary>
            /// Iterate through all items of the selected biome
            /// </summary>
            /// <param name="selectedBiomeType"></param>
            /// <returns></returns>
            public int Process(BiomeType selectedBiomeType)
            {
                int itemChangeCount = 0;

                List<VegetationSystemPro> VegetationSystemList = VegetationStudioInstance.VegetationSystemList;
                for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
                {
                    VegetationSystemPro vegetationSystemPro = VegetationSystemList[i];

                    foreach (VegetationPackagePro vegetationPackagePro in vegetationSystemPro.VegetationPackageProList)
                    {

                        // filter biome type
                        if (vegetationPackagePro.BiomeType != selectedBiomeType)
                            continue;

                        foreach (VegetationItemInfoPro vegetationItemInfoPro in vegetationPackagePro.VegetationInfoList)
                        {
                            // perform the actual action
                            OnActionPerformed(vegetationPackagePro, vegetationItemInfoPro);

                            // clear cache
                            vegetationSystemPro.ClearCache(vegetationItemInfoPro.VegetationItemID);

                            itemChangeCount++;

                        }

                        EditorUtility.SetDirty(vegetationPackagePro);
                    }

                    vegetationSystemPro.RefreshVegetationSystem();

                    VegetationStudioProUtils.SetSceneDirty(vegetationSystemPro);
                }

                return itemChangeCount;
            }
        }
    }
}
