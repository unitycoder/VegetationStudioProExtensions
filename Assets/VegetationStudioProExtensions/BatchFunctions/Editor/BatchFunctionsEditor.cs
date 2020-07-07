using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem;
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

        private SerializedProperty modifyAll;
        private SerializedProperty biomeType;

        // local attributes which aren't persisted
        #region local attributes

        private VegetationRenderMode vegetationRenderMode = VegetationRenderMode.InstancedIndirect;

        /// <summary>
        /// The biome types as objects and strings. Used in the popup component.
        /// </summary>
        private PopupData<BiomeType> addedBiomes;

        #endregion local attributes


        public void OnEnable()
        {
            VegetationStudioInstance = VegetationStudioProUtils.FindVegetationStudioInstance();

            extension = (BatchFunctions)target;

            // biomes
            modifyAll = FindProperty(x => x.modifyAll);
            biomeType = FindProperty(x => x.biomeType);

            // get only the added biome types, we don't want all of the enum in the popup
            addedBiomes = new PopupData<BiomeType>(VegetationStudioProUtils.GetAddedBiomeTypes().ToArray());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.HelpBox("Apply settings on selected Biomes", MessageType.Info);

                EditorGUILayout.LabelField("Batch Filter", GUIStyles.GroupTitleStyle);
                {
                    EditorGUILayout.PropertyField(modifyAll, new GUIContent("Modify All", "Whether a filter should be used or the changes should be applied to all biomes"));

                    if (!modifyAll.boolValue)
                    {
                        biomeType.intValue = EditorGUILayout.Popup("Biome Type Filter", biomeType.intValue, addedBiomes.GetStrings());
                    }
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
                            SetRuntimeSpawn(true, GetFilteredBiomeTypes());
                        }

                        if (GUILayout.Button("Disable ALL in Filter"))
                        {
                            SetRuntimeSpawn(false, GetFilteredBiomeTypes());
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
                        SetRenderMode(vegetationRenderMode, GetFilteredBiomeTypes());
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
                        RefreshPrefabs(GetFilteredBiomeTypes());
                    }

                }
            }
            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("CleanUp", GUIStyles.GroupTitleStyle);
                {
                    if (GUILayout.Button("Delete Unused Vegetation Items"))
                    {
                        DeleteUnusedVegetationItems(GetFilteredBiomeTypes());
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


            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Touch React Active (NatureManufacture Materials)", GUIStyles.GroupTitleStyle);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Enable ALL in Filter"))
                        {
                            SetTouchReactActive(true, GetFilteredBiomeTypes());
                        }

                        if (GUILayout.Button("Disable ALL in Filter"))
                        {
                            SetTouchReactActive(false, GetFilteredBiomeTypes());
                        }

                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Get the selected biome type filter. Either all in an array or just a single element array.
        /// </summary>
        /// <returns></returns>
        private BiomeType[] GetFilteredBiomeTypes()
        {
            if( extension.modifyAll)
            {
                return addedBiomes.GetObjects();
            }
            else
            {
                return new BiomeType[] { extension.biomeType };
            }
            
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
        /// <param name="selectedBiomeTypes"></param>
        private void SetRuntimeSpawn(bool enabled, BiomeType[] selectedBiomeTypes)
        {
            RuntimeSpawnProcessor processor = new RuntimeSpawnProcessor(enabled);
            int itemChangeCount = processor.Process(selectedBiomeTypes);

            // show confirmation messagebox
            EditorUtility.DisplayDialog("Set Runtime Spawn", "Runtime Spawn changed to " + enabled + " for " + itemChangeCount + " items", "Ok");

        }

        private void SetRenderMode(VegetationRenderMode vegetationRenderMode, BiomeType[] selectedBiomeTypes)
        {
            RenderModeProcessor processor = new RenderModeProcessor(vegetationRenderMode);
            int itemChangeCount = processor.Process(selectedBiomeTypes);

            // show confirmation messagebox
            EditorUtility.DisplayDialog("Set Render Mode", "Render mode changed to " + vegetationRenderMode + " for " + itemChangeCount + " items", "Ok");

        }

        private void RefreshPrefabs(BiomeType[] selectedBiomeTypes)
        {
            // lengthy process, ask first
            bool isContinue = EditorUtility.DisplayDialog("Refresh Prefabs", "This might take a while depending on the number of vegetation items. Continue?", "Yes", "No");

            if (!isContinue)
                return;

            // apply action
            RefreshPrefabsProcessor processor = new RefreshPrefabsProcessor();
            int itemChangeCount = processor.Process(selectedBiomeTypes);

            // show confirmation messagebox
            EditorUtility.DisplayDialog("Refresh Prefabs", "Refresh Prefabs applied to " + itemChangeCount + " items", "Ok");

        }

        private void DeleteUnusedVegetationItems(BiomeType[] selectedBiomeTypes)
        {

            bool isContinue = EditorUtility.DisplayDialog("Delete Unused Vegetation Items", "Delete all Vegetation Items which have runtime-spawn unselected. Continue?", "Yes", "No");

            if (!isContinue)
                return;

            DeleteUnusedVegetationItemsProcessor processor = new DeleteUnusedVegetationItemsProcessor();
            int itemChangeCount = processor.Process(selectedBiomeTypes);

            EditorUtility.DisplayDialog("Delete Unused Vegetation Items", "Delete Unused Vegetation Items examined and processed " + itemChangeCount + " items", "Ok");
        }

        /// <summary>
        /// Set the TouchReactActive material value for all vegetation items of the selected biome type
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="selectedBiomeTypes"></param>
        private void SetTouchReactActive(bool active, BiomeType[] selectedBiomeTypes)
        {
            TouchReactActiveProcessor processor = new TouchReactActiveProcessor(active);
            int itemChangeCount = processor.Process(selectedBiomeTypes);

            // show confirmation messagebox
            EditorUtility.DisplayDialog("Set Touch React Active", "TouchReactActive changed to " + active + " for Grass and Plants", "Ok");

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
        /// Apply "Delete Unused Vegetation Items" to all items of a biome.
        /// </summary>
        private class DeleteUnusedVegetationItemsProcessor : VegetationItemsProcessor
        {
            public override void OnActionPerformed(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro)
            {
                // skip all which have runtime spawn enabled
                if (vegetationItemInfoPro.EnableRuntimeSpawn)
                    return;

                Debug.Log("Deleting Vegetation Item " + vegetationItemInfoPro.Name);
                
                // apply settings
                vegetationPackagePro.VegetationInfoList.Remove(vegetationItemInfoPro);

            }
        }

        /// <summary>
        /// Set the TouchReactActive material value for all items of a biome which support that.
        /// </summary>
        private class TouchReactActiveProcessor : VegetationItemsProcessor
        {
            private string propertyId = "_TouchReactActive";
            private float propertyValue;

            public TouchReactActiveProcessor(bool active)
            {
                this.propertyValue = active ? 1 : 0;
            }

            public override void OnActionPerformed(VegetationPackagePro vegetationPackagePro, VegetationItemInfoPro vegetationItemInfoPro)
            {

                // apply settings
                GameObject vegetationPrefab = vegetationItemInfoPro.VegetationPrefab;

                VegetationType vegetationType = vegetationItemInfoPro.VegetationType;

                // support only grass & plants. doesn't make much sense to have trees and objects use touch react
                if(vegetationType != VegetationType.Grass && vegetationType != VegetationType.Plant)
                {
                    Debug.Log("Only Grass & Plant supported. Skipping " + vegetationPrefab.name);
                    return;
                }

                // get materials
                Material[] vegetationItemMaterials = AwesomeTechnologies.Shaders.ShaderSelector.GetVegetationItemMaterials(vegetationPrefab);

                // process items
                foreach (Material material in vegetationItemMaterials)
                {
                    if (material.HasProperty(propertyId))
                    {
                        Debug.Log("Setting _TouchReactActive of " + vegetationItemInfoPro.Name + ", material: " + material.name + " from " + material.GetFloat(propertyId) + " to " + propertyValue);

                        material.SetFloat(propertyId, propertyValue);
                    }
                }
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
            /// <param name="selectedBiomeTypes"></param>
            /// <returns></returns>
            public int Process(BiomeType[] selectedBiomeTypes)
            {
                int itemChangeCount = 0;

                List<VegetationSystemPro> VegetationSystemList = VegetationStudioInstance.VegetationSystemList;
                for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
                {
                    VegetationSystemPro vegetationSystemPro = VegetationSystemList[i];

                    foreach (VegetationPackagePro vegetationPackagePro in vegetationSystemPro.VegetationPackageProList)
                    {
                        
                        // filter biome type
                        if (!ArrayUtility.Contains(selectedBiomeTypes, vegetationPackagePro.BiomeType))
                            continue;

                        // note: the delete mechanism operates on the list itself => we have to use a clone => using ToArray() on the list
                        foreach (VegetationItemInfoPro vegetationItemInfoPro in vegetationPackagePro.VegetationInfoList.ToArray())
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
