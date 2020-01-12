using AwesomeTechnologies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// TODO: 
//    ask lennart to make GetMaskCenter public
namespace VegetationStudioProExtensions
{
    [CustomEditor(typeof(VegetationMaskAreaExtension))]
    [CanEditMultipleObjects]
    public class VegetationMaskAreaExtensionEditor : BaseEditor<VegetationMaskAreaExtension>
    {
        private VegetationMaskAreaExtension extension;
        private VegetationMaskArea mask;

        private float resizeFactor = 0.1f;

        private float buttonMaxWidth = 100f;

        public void Awake()
        {
            extension = (VegetationMaskAreaExtension) target;
            mask = extension.GetComponent<VegetationMaskArea>();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Center", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.CenterMainHandle( mask);
            }
            else if (GUILayout.Button("Grow", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.Grow(mask, resizeFactor);
            }
            else if (GUILayout.Button("Shrink", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.Shrink(mask, resizeFactor);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Circle", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.CreateCircle(mask);
            }
            else if (GUILayout.Button("Hexagon", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.CreateHexagon(mask);
            }
            else if (GUILayout.Button("Convex Hull", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.ConvexHull(mask);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Subdivide", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.Subdivide( mask);
            }
            else if (GUILayout.Button("Unsubdivide", GUILayout.MaxWidth(buttonMaxWidth)))
            {
                VegetationMaskUtils.Unsubdivide( mask);
            }
           
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

    }
}
