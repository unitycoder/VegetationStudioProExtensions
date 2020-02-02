using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class VegetationStudioProActionModule : IActionModule
    {
        private BiomeMaskSpawnerExtensionEditor editor;

        public VegetationStudioProActionModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Vegetation Studio Pro", GUIStyles.GroupTitleStyle);

            GUILayout.BeginHorizontal();
            {

                // create biome mask
                if (GUILayout.Button("Refresh Vegetation"))
                {
                    VegetationStudioProUtils.RefreshVegetation();
                }

            }
            GUILayout.EndHorizontal();
        }

    }
}
