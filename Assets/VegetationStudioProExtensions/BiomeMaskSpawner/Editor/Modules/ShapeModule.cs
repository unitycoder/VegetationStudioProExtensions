using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class ShapeModule
    {
        private SerializedProperty randomShape;
        private SerializedProperty randomPointsCount;
        private SerializedProperty randomPointsCountMin;
        private SerializedProperty randomPointsCountMax;

        private SerializedProperty keepOriginalPoints;
        private SerializedProperty convexityMin;
        private SerializedProperty convexityMax;
        private SerializedProperty douglasPeuckerReductionTolerance;

        private SerializedProperty resizeFactor;

        private BiomeMaskSpawnerExtensionEditor editor;

        public ShapeModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
            randomShape = editor.FindProperty(x => x.shapeSettings.randomShape);
            randomPointsCountMin = editor.FindProperty(x => x.shapeSettings.randomPointsCountMin);
            randomPointsCountMax = editor.FindProperty(x => x.shapeSettings.randomPointsCountMax);
            keepOriginalPoints = editor.FindProperty(x => x.shapeSettings.keepOriginalPoints);
            convexityMin = editor.FindProperty(x => x.shapeSettings.convexityMin);
            convexityMax = editor.FindProperty(x => x.shapeSettings.convexityMax);
            douglasPeuckerReductionTolerance = editor.FindProperty(x => x.shapeSettings.douglasPeuckerReductionTolerance);
            resizeFactor = editor.FindProperty(x => x.shapeSettings.resizeFactor);
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Shape", GUIStyles.GroupTitleStyle);
            {

                EditorGUILayout.PropertyField(resizeFactor, new GUIContent("Resize Factor", "Grow or shrink the mask."));

                EditorGUILayout.PropertyField(randomShape, new GUIContent("Random Shape", "If selected, create the mask as a random shape inside the bounding box. If unselected, use the bounding box as mask."));

                if (randomShape.boolValue)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(keepOriginalPoints, new GUIContent("Keep Original Points", "Keep the original points in case of a subdivision algorithm."));

                    EditorGUILayout.LabelField(new GUIContent("Convexity", "Relative value to randomly move the shape bounds towards the center, within the original bounds."));
                    EditorGuiUtilities.MinMaxEditor("Min", ref convexityMin, "Max", ref convexityMax, 0f, 1f, true);

                    EditorGUILayout.PropertyField(douglasPeuckerReductionTolerance, new GUIContent("Node Reduction Tolerance", "Douglas Peucker node reduction tolerance. 0 = disabled."));

                    // only values >= 0 allowed
                    douglasPeuckerReductionTolerance.floatValue = Utils.ClipMin(douglasPeuckerReductionTolerance.floatValue, 0f);

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(new GUIContent("Polygon Points Count", "The number of points on the polygon."));
                    EditorGuiUtilities.MinMaxEditorInt("Min", ref randomPointsCountMin, "Max", ref randomPointsCountMax, 3, 60, true);

                }
            }
        }
    }
}
