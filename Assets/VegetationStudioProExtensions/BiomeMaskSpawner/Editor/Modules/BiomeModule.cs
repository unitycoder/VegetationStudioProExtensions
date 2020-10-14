using AwesomeTechnologies.VegetationSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Find the popup index, i. e. the filtered index; it isn't necessarily the one of the enum id
        /// </summary>
        /// <returns></returns>
        private int GetCurrentBiomePopupIndex()
        {
            // select first biome type as default
            BiomeType selectedBiomeType = addedBiomes.GetObjects()[0];

            // find the popup index, i. e. the filtered index; it isn't necessarily the one of the enum
            int currentPopupIndex = (int)selectedBiomeType;
            for (int i = 0; i < addedBiomes.GetObjects().Length; i++)
            {
                BiomeType currentBiomeType = addedBiomes.GetObjects()[i];

                if ((int)currentBiomeType == biomeType.intValue)
                {
                    currentPopupIndex = i;
                    break;
                }
            }

            return currentPopupIndex;
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Biome Settings", GUIStyles.GroupTitleStyle);
            {
                // find the popup index, i. e. the filtered index; it isn't necessarily the one of the enum
                int currentPopupIndex = GetCurrentBiomePopupIndex();

                int newIndex = EditorGUILayout.Popup("Biome Type", currentPopupIndex, addedBiomes.GetStrings());

                // map popup index to biome index
                BiomeType newSelectedBiomeType = addedBiomes.GetObjects()[newIndex];
                biomeType.intValue = (int) newSelectedBiomeType;

                EditorGUILayout.LabelField(new GUIContent("Biome Blend Distance", "The relative Biome blend distance. 0 = no blending, 1 = full blending."));
                EditorGuiUtilities.MinMaxEditor("Min", ref biomeBlendDistanceMin, "Max", ref biomeBlendDistanceMax, 0f, 1f, true);

                EditorGUILayout.PropertyField(biomeDensity, new GUIContent("Density", "Reducing density means that some of the partitions aren't used, i. e. they are removed randomly"));

            }


        }
    }
}
