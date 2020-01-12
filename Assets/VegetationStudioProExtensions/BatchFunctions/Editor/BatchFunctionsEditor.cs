using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor.SceneManagement;
using AwesomeTechnologies.VegetationStudio;

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

        public void OnEnable()
        {
            FindVegetationStudioInstance();

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

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Set the runtime spawn flag on the selected biome type
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="selectedBiomeType"></param>
        private void SetRuntimeSpawn( bool enabled, BiomeType selectedBiomeType)
        {
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
                        // apply settings
                        vegetationItemInfoPro.EnableRuntimeSpawn = enabled;

                        // clear cache
                        vegetationSystemPro.ClearCache(vegetationItemInfoPro.VegetationItemID);

                    }

                    EditorUtility.SetDirty(vegetationPackagePro);
                }

                SetSceneDirty(vegetationSystemPro);
            }
        }

        
        void SetSceneDirty(VegetationSystemPro vegetationSystemPro)
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(vegetationSystemPro.gameObject.scene);
                EditorUtility.SetDirty(vegetationSystemPro);
            }
        }
        

        protected static void FindVegetationStudioInstance()
        {
            VegetationStudioInstance = (VegetationStudioManager)FindObjectOfType(typeof(VegetationStudioManager));
        }
    }
}
