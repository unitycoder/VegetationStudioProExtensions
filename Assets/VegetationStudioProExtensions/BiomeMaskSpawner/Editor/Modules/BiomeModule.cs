using AwesomeTechnologies.VegetationSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class BiomeModule : ISettingsModule
    {
        private SerializedProperty biomeType;
        private SerializedProperty biomeBlendDistanceMin;
        private SerializedProperty biomeBlendDistanceMax;
        private SerializedProperty biomeDensity;

        private BiomeMaskSpawnerExtensionEditor editor;

        /// <summary>
        /// The biome types as objects and strings. Used in the popup component.
        /// </summary>
        private PopupData<BiomeType> addedBiomes;

        public BiomeModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
            biomeType = editor.FindProperty(x => x.biomeSettings.biomeType);
            biomeBlendDistanceMin = editor.FindProperty(x => x.biomeSettings.biomeBlendDistanceMin);
            biomeBlendDistanceMax = editor.FindProperty(x => x.biomeSettings.biomeBlendDistanceMax);
            biomeDensity = editor.FindProperty(x => x.biomeSettings.density);

            // get only the added biome types, we don't want all of the enum in the popup
            addedBiomes = new PopupData<BiomeType>(VegetationStudioProUtils.GetAddedBiomeTypes().ToArray());

        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Biome Settings", GUIStyles.GroupTitleStyle);
            {
                biomeType.intValue = EditorGUILayout.Popup("Biome Type", biomeType.intValue, addedBiomes.GetStrings());

                EditorGUILayout.LabelField(new GUIContent("Biome Blend Distance", "The relative Biome blend distance. 0 = no blending, 1 = full blending."));
                EditorGuiUtilities.MinMaxEditor("Min", ref biomeBlendDistanceMin, "Max", ref biomeBlendDistanceMax, 0f, 1f, true);

                EditorGUILayout.PropertyField(biomeDensity, new GUIContent("Density", "Reducing density means that some of the partitions aren't used, i. e. they are removed randomly"));

            }


        }
    }
}
