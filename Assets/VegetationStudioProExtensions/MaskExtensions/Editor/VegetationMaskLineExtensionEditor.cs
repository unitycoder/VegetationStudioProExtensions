using AwesomeTechnologies;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;

namespace VegetationStudioProExtensions
{
    [CustomEditor(typeof(VegetationMaskLineExtension))]
    [CanEditMultipleObjects]
    public class VegetationMaskLineExtensionEditor : BaseEditor<VegetationMaskLineExtension>
    {
        private VegetationMaskLineExtension editorTarget;
        private VegetationMaskLine mask;

        private SerializedProperty dataSourceType;
        private SerializedProperty dataSource;
        private SerializedProperty closedPath;
        private SerializedProperty douglasPeuckerReductionTolerance;

        TrainController trainControllerIntegration;

        public void OnEnable()
        {
            editorTarget = (VegetationMaskLineExtension)target;

            mask = editorTarget.GetComponent<VegetationMaskLine>();

            dataSourceType = FindProperty(x => x.dataSourceType);
            dataSource = FindProperty(x => x.dataSource);
            closedPath = FindProperty(x => x.closedPath);
            douglasPeuckerReductionTolerance = FindProperty(x => x.douglasPeuckerReductionTolerance);

            trainControllerIntegration = new TrainController(editorTarget);
        }

        public override void OnInspectorGUI()
        {
            // we draw our own inspector
            // DrawDefaultInspector();

            serializedObject.Update();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.HelpBox("Create a line mask using various kinds of input", MessageType.Info);

                //
                // properties
                //

                EditorGUILayout.PropertyField(dataSourceType, new GUIContent("Data Source", "The data source for the line points"));

                GUILayout.BeginHorizontal();
                {
                    // show error color in case there's no container
                    if (editorTarget.dataSource == null)
                    {
                        SetErrorBackgroundColor();
                    }

                    EditorGUILayout.PropertyField(dataSource, new GUIContent("Container", "The transforms of the children of this container (parent) object will be used to create the line mask"));

                    // default color in case error color was set
                    SetDefaultBackgroundColor();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Children", GetChildCount().ToString());

                EditorGUILayout.PropertyField(closedPath, new GUIContent("Closed Path", "If Closed Path is selected, then the last point will be connected to the first point"));

                EditorGUILayout.PropertyField(douglasPeuckerReductionTolerance, new GUIContent("Node Reduction Tolerance", "Douglas Peucker node reduction tolerance. 0 = disabled."));

                EditorGUILayout.Space();

                //
                // buttons
                //

                GUILayout.BeginHorizontal();
                {
                    // enable create line mask button only if there are children
                    SetGuiEnabled(GetChildCount() > 0);

                    // create line mask
                    if (GUILayout.Button("Create Line Mask"))
                    {
                        CreateLineMask();
                    }

                    // enable gui again in case it got disabled
                    SetGuiEnabled(true);

                    // clear line mask
                    if (GUILayout.Button("Clear Line Mask"))
                    {
                        ClearLineMask();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private int GetChildCount()
        {
            if (editorTarget.dataSource == null)
                return 0;

            return editorTarget.dataSource.GetComponentInChildren<Transform>().childCount;
        }

        private void ClearLineMask()
        {
            mask.ClearNodes();

            VegetationStudioProUtils.RefreshVegetation();
        }

        private void CreateLineMask()
        {
            if (editorTarget.dataSource == null)
            {
                Debug.LogError("Container isn't set. Please specify a GameObject which contains transforms as children.");
                return;
            }

            List<Vector3> positions = GetPositions();

            // closed path: connect last with first position
            if( closedPath.boolValue && positions.Count > 1)
            {
                // add first position as last position
                positions.Add( positions[0]);
            }

            mask.ClearNodes();
            mask.AddNodesToEnd(positions.ToArray());

            VegetationStudioProUtils.RefreshVegetation();

        }

        /// <summary>
        /// Get the positions depending on the data source
        /// </summary>
        /// <returns></returns>        
        private List<Vector3> GetPositions()
        {
            List<Vector3> positions;

            switch ( editorTarget.dataSourceType)
            {
                case VegetationMaskLineExtension.DataSourceType.Container:
                    positions = GetContainerChildrenPositions();
                    break;
                case VegetationMaskLineExtension.DataSourceType.TrainController:
                    positions = trainControllerIntegration.GetTrainControllerPositions();
                    break;
                default:
                    throw new Exception("Invalid data source: " + editorTarget.dataSourceType);

            }

            positions = VegetationMaskUtils.ApplyDoublesPeucker(positions, editorTarget.douglasPeuckerReductionTolerance);

            return positions;
        }

        /// <summary>
        /// Positions are determined by the transforms of gameobjects.
        /// </summary>
        /// <returns></returns>
        private List<Vector3> GetContainerChildrenPositions()
        {
            List<Vector3> positions = editorTarget.dataSource.GetComponentsInChildren<Transform>().Select(x => x.position).ToList();

            return positions;
        }
    }
}
