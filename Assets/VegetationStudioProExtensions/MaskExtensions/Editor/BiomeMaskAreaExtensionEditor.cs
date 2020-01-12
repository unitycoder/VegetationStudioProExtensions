using AwesomeTechnologies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Node = AwesomeTechnologies.VegetationSystem.Biomes.Node;

// TODO: 
//    ask lennart to make GetMaskCenter public
namespace VegetationStudioProExtensions
{
    [CustomEditor(typeof(BiomeMaskAreaExtension))]
    [CanEditMultipleObjects]
    public class BiomeMaskAreaExtensionEditor : BaseEditor<BiomeMaskAreaExtension>
    {
        private BiomeMaskAreaExtension extension;
        private BiomeMaskArea mask;

        private float resizeFactor = 0.1f;

        private float buttonMaxWidth = 100f;

        public void Awake()
        {
            extension = (BiomeMaskAreaExtension) target;
            mask = extension.GetComponent<BiomeMaskArea>();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Center", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.CenterMainHandle(mask);
            }
            else if (GUILayout.Button("Grow", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.Grow( mask, resizeFactor);
            }
            else if (GUILayout.Button("Shrink", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.Shrink(mask, -resizeFactor);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Circle", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.CreateCircle(mask);
            }
            else if (GUILayout.Button("Hexagon", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.CreateHexagon(mask);
            }
            else if (GUILayout.Button("Convex Hull", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.ConvexHull(mask);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Subdivide", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.Subdivide( mask);
            }
            else if (GUILayout.Button("Unsubdivide", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                BiomeMaskUtils.Unsubdivide( mask);
            }

            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

 
    }
}