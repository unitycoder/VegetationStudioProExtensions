using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VegetationStudioProExtensions
{
    public class EditorGuiUtilities
    {
        /// <summary>
        /// Min/Max range slider with float fields
        /// </summary>
        /// <param name="label"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        public static void MinMaxEditor(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label);

                minValue = EditorGUILayout.FloatField("", minValue, GUILayout.Width(50));
                EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
                maxValue = EditorGUILayout.FloatField("", maxValue, GUILayout.Width(50));

                if (minValue < minLimit) minValue = minLimit;
                if (maxValue > maxLimit) maxValue = maxLimit;

            }
            GUILayout.EndHorizontal();

        }

        /// <summary>
        /// Min/Max range slider with float fields. Values must be >= 0.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        public static void MinMaxEditor(string minLabel, ref SerializedProperty minValueProperty, string maxLabel, ref SerializedProperty maxValueProperty, bool indent)
        {
            MinMaxEditor(minLabel, ref minValueProperty, maxLabel, ref maxValueProperty, null, null, indent);
        }


        /// <summary>
        /// Min/Max range slider with float fields. Values must be >= 0.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        public static void MinMaxEditor(string minLabel, ref SerializedProperty minValueProperty, string maxLabel, ref SerializedProperty maxValueProperty, float? minLimit, float? maxLimit, bool indent)
        {
            if( indent)
            {
                EditorGUI.indentLevel++;

            }

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(minValueProperty, new GUIContent(minLabel));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(maxValueProperty, new GUIContent(maxLabel));
            }
            GUILayout.EndHorizontal();

            if (indent)
            {
                EditorGUI.indentLevel--;

            }

            // min must never be < minLimit if it is specified
            if (minLimit != null)
            {
                if (minValueProperty.floatValue < minLimit) minValueProperty.floatValue = (float)minLimit;
                if (minValueProperty.floatValue >= maxLimit) minValueProperty.floatValue = (float)maxLimit;
            }

            // max must never be < min
            if (maxValueProperty.floatValue < minValueProperty.floatValue) maxValueProperty.floatValue = minValueProperty.floatValue;

            // max must never be > maxLimit if it is specified
            if (maxLimit != null)
            {
                if (maxValueProperty.floatValue > maxLimit) maxValueProperty.floatValue = (float)maxLimit;
            }

        }

        public static void MinMaxEditorInt(string minLabel, ref SerializedProperty minValueProperty, string maxLabel, ref SerializedProperty maxValueProperty, int? minLimit, int? maxLimit, bool indent)
        {
            if (indent)
            {
                EditorGUI.indentLevel++;

            }

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(minValueProperty, new GUIContent(minLabel));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(maxValueProperty, new GUIContent(maxLabel));
            }
            GUILayout.EndHorizontal();

            if (indent)
            {
                EditorGUI.indentLevel--;

            }

            // min must never be < minLimit if it is specified
            if (minLimit != null)
            {
                if (minValueProperty.intValue < minLimit) minValueProperty.intValue = (int)minLimit;
            }

            // max must never be < min
            if (maxValueProperty.intValue < minValueProperty.intValue) maxValueProperty.intValue = minValueProperty.intValue;

            // max must never be > maxLimit if it is specified
            if (maxLimit != null)
            {
                if (maxValueProperty.intValue > maxLimit) maxValueProperty.intValue = (int)maxLimit;
            }

        }

    }
}