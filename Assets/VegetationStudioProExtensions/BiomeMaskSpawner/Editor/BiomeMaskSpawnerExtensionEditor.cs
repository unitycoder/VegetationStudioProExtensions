using UnityEngine;
using UnityEditor;
using System.Linq;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections.Generic;
using static VegetationStudioProExtensions.ShapeSettings;
using static VegetationStudioProExtensions.BoundsProcessingSettings;

namespace VegetationStudioProExtensions
{
    [ExecuteInEditMode()]
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BiomeMaskSpawnerExtension))]
    public class BiomeMaskSpawnerExtensionEditor : BaseEditor<BiomeMaskSpawnerExtension>
    {
        /// <summary>
        /// Use ResetMaskId and GetMaskId to get a sequence of ascending ids.
        /// </summary>
        private int maskId = -1;

        public BiomeMaskSpawnerExtension extension;

        private RectangularPartitionModule rectangularPartitionModule = null;
        private VoronoiModule voronoiModule = null;
        private HexagonModule hexagonModule = null;
        private LineModule lineModule = null;

        private SerializedProperty biomeType;
        private SerializedProperty biomeBlendDistanceMin;
        private SerializedProperty biomeBlendDistanceMax;
        private SerializedProperty biomeCount;
        private SerializedProperty biomeDensity;

#if RAM_2019
        private SerializedProperty lakeCreateLake;
        private SerializedProperty lakeLakeProfile;

        private SerializedProperty lakeRamInternalLakeCreation;
        private SerializedProperty lakeCloseDistanceSimulation;
        private SerializedProperty lakeAngleSimulation;
        private SerializedProperty lakeCheckDistanceSimulation;
#endif

        private SerializedProperty partitionStrategy;
        private SerializedProperty terrainProcessing;
        private SerializedProperty boundsBiomeMaskArea;
        private SerializedProperty boundsBiomeMaskAreaValid;

        private SerializedProperty partitionAlgorithm;
        private SerializedProperty randomShape;
        private SerializedProperty randomPointsCount;
        private SerializedProperty randomPointsCountMin;
        private SerializedProperty randomPointsCountMax;
        private SerializedProperty boundsShiftFactorMin;
        private SerializedProperty boundsShiftFactorMax;
        private SerializedProperty keepOriginalPoints;
        private SerializedProperty convexityMin;
        private SerializedProperty convexityMax;
        private SerializedProperty douglasPeuckerReductionTolerance;
        private SerializedProperty voronoiPointCount;
        private SerializedProperty hexagonRadius;

        private SerializedProperty lineCount;
        private SerializedProperty lineWidthMin;
        private SerializedProperty lineWidthMax;
        private SerializedProperty lineHeightMin;
        private SerializedProperty lineHeightMax;
        private SerializedProperty lineAttachedToBiome;

        private static VegetationStudioManager VegetationStudioInstance;

        /// <summary>
        /// The factor for growing or shrinking.
        /// </summary>
        private float resizeFactor = 0.05f;

        BiomeMaskSpawnerExtensionEditor editor;

        private bool performInitialConsistencyCheck = false;

        public void OnEnable()
        {
            this.editor = this;

            FindVegetationStudioInstance();

            extension = (BiomeMaskSpawnerExtension)target;

            rectangularPartitionModule = new RectangularPartitionModule(this);
            voronoiModule = new VoronoiModule(this);
            hexagonModule = new HexagonModule(this);
            lineModule = new LineModule(this);

            // biomes
            biomeType = FindProperty(x => x.biomeSettings.biomeType);
            biomeBlendDistanceMin = FindProperty(x => x.biomeSettings.biomeBlendDistanceMin);
            biomeBlendDistanceMax = FindProperty(x => x.biomeSettings.biomeBlendDistanceMax);
            biomeDensity = FindProperty(x => x.biomeSettings.density);

            // lake
#if RAM_2019
            lakeCreateLake = FindProperty(x => x.lakeSettings.createLake);
            lakeLakeProfile = FindProperty(x => x.lakeSettings.lakeProfile);

            lakeRamInternalLakeCreation = FindProperty(x => x.lakeSettings.ramInternalLakeCreation);
            lakeCloseDistanceSimulation = FindProperty(x => x.lakeSettings.closeDistanceSimulation);
            lakeAngleSimulation = FindProperty(x => x.lakeSettings.angleSimulation);
            lakeCheckDistanceSimulation = FindProperty(x => x.lakeSettings.checkDistanceSimulation);
#endif
            // shape
            randomShape = FindProperty(x => x.shapeSettings.randomShape);
            randomPointsCountMin = FindProperty(x => x.shapeSettings.randomPointsCountMin);
            randomPointsCountMax = FindProperty(x => x.shapeSettings.randomPointsCountMax);
            keepOriginalPoints = FindProperty(x => x.shapeSettings.keepOriginalPoints);
            convexityMin = FindProperty(x => x.shapeSettings.convexityMin);
            convexityMax = FindProperty(x => x.shapeSettings.convexityMax);
            douglasPeuckerReductionTolerance = FindProperty(x => x.shapeSettings.douglasPeuckerReductionTolerance);

            // bounds
            terrainProcessing = FindProperty(x => x.boundsSettings.boundsProcessing);
            partitionAlgorithm = FindProperty(x => x.boundsSettings.partitionAlgorithm);
            boundsBiomeMaskArea = FindProperty(x => x.boundsSettings.biomeMaskArea);
            boundsBiomeMaskAreaValid = FindProperty(x => x.boundsSettings.biomeMaskAreaValid);

            // rectangular terrain partitioning
            partitionStrategy = FindProperty(x => x.rectangularPartitionSettings.partitionStrategy);
            biomeCount = FindProperty(x => x.rectangularPartitionSettings.biomeCount);
            boundsShiftFactorMin = FindProperty(x => x.rectangularPartitionSettings.boundsShiftFactorMin);
            boundsShiftFactorMax = FindProperty(x => x.rectangularPartitionSettings.boundsShiftFactorMax);

            // voronoi
            voronoiPointCount = FindProperty(x => x.voronoiSettings.pointCount);

            // hexagon
            hexagonRadius = FindProperty(x => x.hexagonSettings.radius);

            // line
            lineCount = FindProperty(x => x.lineSettings.count);
            lineHeightMin = FindProperty(x => x.lineSettings.heightMin);
            lineHeightMax = FindProperty(x => x.lineSettings.heightMax);
            lineWidthMin = FindProperty(x => x.lineSettings.widthMin);
            lineWidthMax = FindProperty(x => x.lineSettings.widthMax);
            lineAttachedToBiome = FindProperty(x => x.lineSettings.attachedToBiome);


        #region Consistency Check
        performInitialConsistencyCheck = true;
            #endregion Consistency Check
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            editor.serializedObject.Update();

            // we draw our own inspector
            // DrawDefaultInspector();

            serializedObject.Update();


            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.HelpBox("Spawn Biome Masks on the Terrain", MessageType.Info);

                //
                // Biomes
                //

                EditorGUILayout.LabelField("Biome Settings", GUIStyles.GroupTitleStyle);
                {
                    EditorGUILayout.PropertyField(biomeType, new GUIContent("Biome Type", "The Biome type to be used."));

                    EditorGUILayout.LabelField(new GUIContent("Biome Blend Distance", "The relative Biome blend distance. 0 = no blending, 1 = full blending."));
                    EditorGuiUtilities.MinMaxEditor("Min", ref biomeBlendDistanceMin, "Max", ref biomeBlendDistanceMax, 0f, 1f, true);

                    EditorGUILayout.PropertyField(biomeDensity, new GUIContent("Density", "Reducing density means that some of the partitions aren't used, i. e. they are removed randomly"));

                }

#if RAM_2019
                //
                // Lake
                //
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Lake Settings", GUIStyles.GroupTitleStyle);
                {
                 
                    EditorGUILayout.PropertyField(lakeCreateLake, new GUIContent("Create Lake", "Create a RAM 2019 Lake"));

                    if(lakeCreateLake.boolValue)
                    {
                        EditorGUILayout.PropertyField(lakeLakeProfile, new GUIContent("Lake Profile", "The lake profile to use"));
                
                        EditorGUILayout.PropertyField(lakeRamInternalLakeCreation, new GUIContent("RAM Simulation", "Set vertex position at mask position, but use RAM internal simulation instead of biome shape"));

                       if(lakeRamInternalLakeCreation.boolValue)
                       {
                            EditorGUILayout.PropertyField(lakeAngleSimulation, new GUIContent("Angle", ""));

                            EditorGUILayout.PropertyField(lakeCloseDistanceSimulation, new GUIContent("Point Distance", ""));
                            EditorGUILayout.PropertyField(lakeCheckDistanceSimulation, new GUIContent("Check Distance", ""));
                       }
                    }
                }
#endif

                //
                // Processing
                //
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Terrain Processing", GUIStyles.GroupTitleStyle);

                EditorGUILayout.PropertyField(partitionAlgorithm, new GUIContent("Algorithm", "The algorithm to use for terrain partitioning."));

                EditorGUILayout.PropertyField(terrainProcessing, new GUIContent("Bounds", "Process all terrains as a single combined terrain or all terrains individually."));

                BoundsProcessing selectedTerrainProcessing = (BoundsProcessing)System.Enum.GetValues(typeof(BoundsProcessing)).GetValue(terrainProcessing.enumValueIndex);

                if(selectedTerrainProcessing == BoundsProcessing.Biome) {

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.PropertyField(boundsBiomeMaskArea, new GUIContent("Biome Mask", "The Biome used for clipping."));

                    // check if the changed biome mask is convex
                    if(EditorGUI.EndChangeCheck() || performInitialConsistencyCheck)
                    {

                        if (boundsBiomeMaskArea.objectReferenceValue != null)
                        {
                            boundsBiomeMaskAreaValid.boolValue = false;

                            BiomeMaskArea biomeMaskArea = (BiomeMaskArea)boundsBiomeMaskArea.objectReferenceValue;

                            Vector2[] clipPolygon = GetBiomeClipPolygon(biomeMaskArea);

                            if (clipPolygon != null)
                            {
                                // consistency check: clip polygon must be convex for sutherland hodgman
                                bool isConvex = PolygonUtils.PolygonIsConvex(clipPolygon);
                                if (isConvex)
                                {
                                    boundsBiomeMaskAreaValid.boolValue = true;
                                }
                                else
                                {
                                    Debug.LogError("Invalid clipping mask: " + biomeMaskArea.name + " (" + biomeMaskArea.MaskName + ")");
                                }
                            }
                        }
                    }

                    // show error in case the mask doesn't exist
                    if (boundsBiomeMaskArea.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("The Biome Mask must be defined!", MessageType.Error);
                    }
                    // show error in case the mask isn't convex
                    else if (!boundsBiomeMaskAreaValid.boolValue)
                    {
                        EditorGUILayout.HelpBox("The Biome Mask must be convex!", MessageType.Error);
                    }
                }

                PartitionAlgorithm selectedPartitionAlgorithm = (PartitionAlgorithm)System.Enum.GetValues(typeof(PartitionAlgorithm)).GetValue(partitionAlgorithm.enumValueIndex);

                // partitioning algorithm
                switch (selectedPartitionAlgorithm)
                {
                    case PartitionAlgorithm.Voronoi:

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Voronoi", GUIStyles.GroupTitleStyle);

                        EditorGUILayout.PropertyField(voronoiPointCount, new GUIContent("Points", "The number of Points which will be used to create the Voronoi diagram"));

                        voronoiPointCount.intValue = Utils.ClipMin(voronoiPointCount.intValue, 2);


                        break;

                    case PartitionAlgorithm.Rectangular:

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Partitions", GUIStyles.GroupTitleStyle);

                        EditorGUILayout.PropertyField(biomeCount, new GUIContent("Biome Count", "The number of Biomes the terrain should have when full density is selected"));

                        // consistency check: never allow the number of partitions to be smaller than 1
                        biomeCount.intValue = Utils.ClipMin(biomeCount.intValue, 2);

                        EditorGUILayout.PropertyField(partitionStrategy, new GUIContent("Split Strategy", "The strategy to select the terraiin segments which should be partitioned."));

                        EditorGUILayout.LabelField(new GUIContent("Partition Shift", "Relative factor to shift and resize the partition bounds within its bounds. Makes the partitions look less rectangularly distributed. Reduces the size."));
                        EditorGuiUtilities.MinMaxEditor("Min", ref boundsShiftFactorMin, "Max", ref boundsShiftFactorMax, 0.1f, 1f, true);
                        break;

                    case PartitionAlgorithm.Hexagon:

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Hexagon", GUIStyles.GroupTitleStyle);

                        EditorGUILayout.PropertyField(hexagonRadius, new GUIContent("Radius", "The outer radius of a hexagon"));

                        hexagonRadius.floatValue = Utils.ClipMin(hexagonRadius.floatValue, 10f);

                        break;

                    case PartitionAlgorithm.Line:

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Lines", GUIStyles.GroupTitleStyle);

                        EditorGUILayout.PropertyField(lineCount, new GUIContent("Count", "The number of lines to add."));

                        EditorGUILayout.LabelField(new GUIContent("Line Width", "The minimum and maximum line widths"));
                        EditorGuiUtilities.MinMaxEditor("Min", ref lineWidthMin, "Max", ref lineWidthMax, 1, 100, true);

                        EditorGUILayout.LabelField(new GUIContent("Line Height", "The minimum and maximum line heights"));
                        EditorGuiUtilities.MinMaxEditor("Min", ref lineHeightMin, "Max", ref lineHeightMax, 1, 1000, true);

                        EditorGUILayout.PropertyField(lineAttachedToBiome, new GUIContent("Attached to Biome", "The line is attached to the edge of an existing biome or loose on the terrain"));

                        break;

                    default:
                        throw new System.ArgumentException("Unsupported Partition Algorithm " + extension.boundsSettings.partitionAlgorithm);
                }

                //
                // Shape
                //
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Shape", GUIStyles.GroupTitleStyle);
                {

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

                //
                // creation buttons
                //
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Creation", GUIStyles.GroupTitleStyle);

                GUILayout.BeginHorizontal();
                {

                    // create biome mask
                    if (GUILayout.Button("Clear & Add"))
                    {
                        ResetMaskId();
                        RemoveContainerChildren();
                        PartitionTerrain();
                    }
                    else if (GUILayout.Button("Clear"))
                    {
                        ResetMaskId();
                        RemoveContainerChildren();
                    }
                    else if (GUILayout.Button("Add"))
                    {
                        PartitionTerrain();
                    }

                }
                GUILayout.EndHorizontal();

                //
                // modification
                //
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Modification", GUIStyles.GroupTitleStyle);

                GUILayout.BeginHorizontal();
                {

                    if (GUILayout.Button("Grow All"))
                    {
                        GrowAll();
                    }
                    else if (GUILayout.Button("Shrink All"))
                    {
                        ShrinkAll();
                    }

                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            editor.serializedObject.ApplyModifiedProperties();

            // initial check has been performed
            performInitialConsistencyCheck = false;
        }

        private void PartitionTerrain()
        {
            // get bounds for terrain partitioning
            List<Bounds> boundsList = GetBoundsToProcess();

            // perform partitioning and create masks
            switch (extension.boundsSettings.partitionAlgorithm)
            {
                case PartitionAlgorithm.Voronoi:
                    voronoiModule.CreateMasks(boundsList);
                    break;

                case PartitionAlgorithm.Rectangular:
                    rectangularPartitionModule.CreateMasks(boundsList);
                    break;

                case PartitionAlgorithm.Hexagon:
                    hexagonModule.CreateMasks(boundsList);
                    break;

                case PartitionAlgorithm.Line:
                    lineModule.CreateMasks(boundsList);
                    break;

                default:
                    throw new System.ArgumentException("Unsupported Partition Algorithm " + extension.boundsSettings.partitionAlgorithm);
            }

            RefreshTerrainHeightmap();
        }

        /// <summary>
        /// Create a list of bounds to process. Either all terrains combined or individually
        /// </summary>
        /// <returns></returns>
        private List<Bounds> GetBoundsToProcess()
        {
            List<Bounds> boundsList = new List<Bounds>();

            List<VegetationSystemPro> VegetationSystemList = VegetationStudioInstance.VegetationSystemList;
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemPro vegetationSystemPro = VegetationSystemList[i];

                switch (extension.boundsSettings.boundsProcessing)
                {
                    case BoundsProcessing.CombinedTerrains:

                        // get combined bounds
                        Bounds bounds = vegetationSystemPro.VegetationSystemBounds;

                        // use these bounds
                        boundsList.Add(bounds);

                        break;

                    case BoundsProcessing.IndividualTerrains:
                        for (int t = 0; t < vegetationSystemPro.VegetationStudioTerrainObjectList.Count; t++)
                        {
                            // get individual terrain bounds
                            GameObject terrain = vegetationSystemPro.VegetationStudioTerrainObjectList[t];

                            IVegetationStudioTerrain vegetationStudioTerrain = VegetationStudioTerrain.GetIVegetationStudioTerrain(terrain);
                            bounds = vegetationStudioTerrain.TerrainBounds;

                            // use these bounds
                            boundsList.Add(bounds);

                        }
                        break;

                    case BoundsProcessing.Biome:

                        Bounds biomeBounds;

                        // get biome mask area
                        Vector2[] biomeClipPolygonXY = GetBiomeClipPolygon();

                        // use mask to get bounds
                        if(biomeClipPolygonXY != null)
                        {
                            Vector3[] biomeClipPolygonXZ = biomeClipPolygonXY.Select(item => new Vector3(item.x, 0, item.y)).ToArray();

                            biomeBounds = PolygonUtils.GetBounds(biomeClipPolygonXZ);
                        }
                        // fall back to the vegetation system mask
                        else
                        {
                            biomeBounds = vegetationSystemPro.VegetationSystemBounds;
                            Debug.LogError("Invalid biome clip polygon. Using vegetation system bounds");
                        }

                        // use these bounds
                        boundsList.Add(biomeBounds);

                        break;
                    default:
                        throw new System.ArgumentException("Unsupported TerrainProcessing " + extension.boundsSettings.boundsProcessing);
                }

            }

            return boundsList;
        }

        protected static void FindVegetationStudioInstance()
        {
            VegetationStudioInstance = (VegetationStudioManager)FindObjectOfType(typeof(VegetationStudioManager));
        }

        public void RefreshTerrainHeightmap()
        {
            List<VegetationSystemPro> VegetationSystemList = VegetationStudioInstance.VegetationSystemList;

            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].RefreshTerrainHeightmap();
            }
        }

        private void ResetMaskId()
        {
            maskId = -1;
        }

        public int GetNextMaskId()
        {
            maskId++;
            return maskId;
        }



        public void CreateBiomeMaskArea(string gameObjectName, string maskName, Vector3 position, List<Vector3> nodes, float blendDistance)
        {
            GameObject parentGameObject = extension.transform.gameObject;

            // create new gameobject
            GameObject biomeGameObject = new GameObject(gameObjectName);

            // add this component
            biomeGameObject.AddComponent<BiomeMaskAreaExtension>();

            // ensure gameobject gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(biomeGameObject, parentGameObject);

            // set position
            biomeGameObject.transform.position = position; // that's actually not necessary, we call CenterMainHandle after the mask nodes were created

            // modify created biome
            BiomeMaskArea mask = biomeGameObject.GetComponent<BiomeMaskArea>();
            mask.BiomeType = extension.biomeSettings.biomeType;

            // blend distance
            mask.BlendDistance = blendDistance;

            // create nodes
            mask.MaskName = maskName;

            // random shape inside bounds
            SetMaskNodes(mask, nodes);

            // put move handle into the center of the polygon
            BiomeMaskUtils.CenterMainHandle(mask);

            #region Lake Polygon
#if RAM_2019
            // create a lake and re-parent the biome to it
            if (extension.lakeSettings.createLake)
            {
                LakePolygon lakePolygon = RamLakeCreator.CreateLakePolygon(extension.lakeSettings, mask, mask.transform.gameObject, gameObjectName, nodes);

            }
#endif
            #endregion Lake Polygon

            // tegister the creation in the undo system
            Undo.RegisterCreatedObjectUndo(biomeGameObject, "Create " + biomeGameObject.name);

        }

        /// <summary>
        /// Clear the nodes of the mask and set the provided ones.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="nodes"></param>
        private void SetMaskNodes(BiomeMaskArea mask, List<Vector3> nodes)
        {
            mask.ClearNodes();

            foreach (Vector3 node in nodes)
            {
                mask.AddNodeToEnd(node);
            }

            mask.PositionNodes();
        }

        private void CreateBiomeMaskArea()
        {
            // create new gameobject
            GameObject go = new GameObject("Biome Mask Area");

            // add this component
            go.AddComponent<BiomeMaskAreaExtension>();

            // modify created biome
            BiomeMaskArea mask = go.GetComponent<BiomeMaskArea>();
            mask.BiomeType = extension.biomeSettings.biomeType;

            // TODO: raycast terrain
            // position it to the center of the viewport
            SceneView.lastActiveSceneView.MoveToView(go.transform); // TODO: center of screen, recreate nodes

            // ensure gameobject gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, extension.transform.gameObject);
            // reparent gameobject
            //go.transform.SetParent(extension.transform.gameObject.transform);

            // register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        }

        /// <summary>
        /// Remove all children of the Biome Creator
        /// </summary>
        public void RemoveContainerChildren()
        {
            GameObject container = extension.transform.gameObject as GameObject;

            // register undo
            Undo.RegisterFullObjectHierarchyUndo(container, "Remove " + container);

            List<Transform> list = new List<Transform>();
            foreach (Transform child in container.transform)
            {
                list.Add(child);
            }

            foreach (Transform child in list)
            {
                GameObject go = child.gameObject;

                BiomeMaskSpawnerExtensionEditor.DestroyImmediate(go);

            }


        }

        private void GrowAll()
        {
            // get all biome mask area gameobjects of this gameobject
            GameObject parentGo = extension.transform.gameObject;
            BiomeMaskArea[] masks = parentGo.GetComponentsInChildren<BiomeMaskArea>();

            foreach (BiomeMaskArea mask in masks)
            {
                BiomeMaskUtils.Grow(mask, resizeFactor);
            }

        }

        private void ShrinkAll()
        {
            // get all biome mask area gameobjects of this gameobject
            GameObject parentGo = extension.transform.gameObject;
            BiomeMaskArea[] masks = parentGo.GetComponentsInChildren<BiomeMaskArea>();

            foreach (BiomeMaskArea mask in masks)
            {
                BiomeMaskUtils.Shrink(mask, resizeFactor);
            }
        }

        /// <summary>
        /// If bounds clip setting is set to biome, get the clip polygon from it
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetBiomeClipPolygon()
        {
            return GetBiomeClipPolygon(extension.boundsSettings.biomeMaskArea);
        }

        /// <summary>
        /// If bounds clip setting is set to biome, get the clip polygon from it
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetBiomeClipPolygon( BiomeMaskArea biomeMaskArea)
        {
            if (biomeMaskArea == null)
                return null;

            float biomePositionX = biomeMaskArea.transform.position.x;
            float biomePositionZ = biomeMaskArea.transform.position.z;

            Vector2[] clipPolygon = biomeMaskArea.Nodes.ConvertAll<Vector2>(
                        item => new Vector2(
                              biomePositionX + item.Position.x,
                              biomePositionZ + item.Position.z
                            )).ToArray();

            return clipPolygon;

        }

        /// <summary>
        /// Get the biome clip polygon for the given bounds considering the biome mask settings.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public Vector2[] GetBiomeClipPolygon( Bounds bounds)
        {
            Vector2[] clipPolygon = PolygonUtils.CreatePolygonXZ(bounds);

            // optionally use a biome as clip polygon. default is bounds
            if ( extension.boundsSettings.boundsProcessing == BoundsProcessingSettings.BoundsProcessing.Biome)
            {

                Vector2[] biomeClipPolygon = GetBiomeClipPolygon();
                if (biomeClipPolygon != null)
                {
                    clipPolygon = GetBiomeClipPolygon();

                    // consistency check: clip polygon must be convex for sutherland hodgman
                    bool isConvex = PolygonUtils.PolygonIsConvex(clipPolygon);
                    if (!isConvex)
                    {
                        Debug.LogError("Biome mask clip polygon isn't convex");
                    }
                }
            }

            return clipPolygon;
        }

    }
}
