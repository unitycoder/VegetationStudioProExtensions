using UnityEngine;
using UnityEditor;
using System.Linq;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections.Generic;
using static VegetationStudioProExtensions.ShapeSettings;
using static VegetationStudioProExtensions.ProcessingSettings;

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

        #region settings modules
        public RectangularPartitionModule rectangularPartitionModule = null;
        public VoronoiModule voronoiModule = null;
        public HexagonModule hexagonModule = null;
        public LineModule lineModule = null;
        public LakeModule lakeModule = null;
        public ShapeModule shapeModule = null;
        public BiomeModule biomeModule = null;
        public ProcessingModule processingModule = null;
        public RiverModule riverModule = null;
        public RoadModule roadModule = null;
        #endregion settings modules

        #region action modules
        public MaskCreationActionModule maskCreationActionModule = null;
        public RoadCreationActionModule roadCreationActionModule = null;
        public VegetationStudioProActionModule vegetationStudioProActionModule = null;
        #endregion action modules

        public static VegetationStudioManager VegetationStudioInstance;

        private BiomeMaskSpawnerExtensionEditor editor;

        public bool performInitialConsistencyCheck = false;

        public void OnEnable()
        {
            this.editor = this;

            VegetationStudioInstance = VegetationStudioProUtils.FindVegetationStudioInstance();

            extension = (BiomeMaskSpawnerExtension)target;

            #region settings modules
            rectangularPartitionModule = new RectangularPartitionModule(this);
            voronoiModule = new VoronoiModule(this);
            hexagonModule = new HexagonModule(this);
            lineModule = new LineModule(this);
            lakeModule = new LakeModule(this);
            shapeModule = new ShapeModule(this);
            biomeModule = new BiomeModule(this);
            processingModule = new ProcessingModule(this);
            riverModule = new RiverModule(this);
            roadModule = new RoadModule(this);
            #endregion settings modules

            #region action modules
            maskCreationActionModule = new MaskCreationActionModule(this);
            roadCreationActionModule = new RoadCreationActionModule(this);
            vegetationStudioProActionModule = new VegetationStudioProActionModule(this);
            #endregion action modules

            #region module OnEnable

            biomeModule.OnEnable();
            lakeModule.OnEnable();
            shapeModule.OnEnable();
            processingModule.OnEnable();
            rectangularPartitionModule.OnEnable();
            voronoiModule.OnEnable();
            hexagonModule.OnEnable();
            lineModule.OnEnable();
            riverModule.OnEnable();
            roadModule.OnEnable();

            #endregion module OnEnable

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
                biomeModule.OnInspectorGUI();

                //
                // Lake
                //
                lakeModule.OnInspectorGUI();

                //
                // Processing
                //
                processingModule.OnInspectorGUI();

                // modules depending on partition algorithm
                PartitionAlgorithm selectedPartitionAlgorithm = processingModule.GetSelectedPartitionAlgorithm();

                // partitioning algorithm
                switch (selectedPartitionAlgorithm)
                {
                    case PartitionAlgorithm.Voronoi:
                        voronoiModule.OnInspectorGUI();
                        shapeModule.OnInspectorGUI();
                        break;

                    case PartitionAlgorithm.Rectangular:
                        rectangularPartitionModule.OnInspectorGUI();
                        shapeModule.OnInspectorGUI();
                        break;

                    case PartitionAlgorithm.Hexagon:
                        hexagonModule.OnInspectorGUI();
                        shapeModule.OnInspectorGUI();
                        break;

                    case PartitionAlgorithm.Line:
                        lineModule.OnInspectorGUI();
                        break;

                    case PartitionAlgorithm.River:
                        riverModule.OnInspectorGUI();
                        break;

                    case PartitionAlgorithm.Road:
                        roadModule.OnInspectorGUI();
                        voronoiModule.OnInspectorGUI();
                        shapeModule.OnInspectorGUI();
                        break;

                    default:
                        throw new System.ArgumentException("Unsupported Partition Algorithm " + extension.boundsSettings.partitionAlgorithm);
                }


                // action buttons
                switch (selectedPartitionAlgorithm)
                {
                    case PartitionAlgorithm.Voronoi:
                    case PartitionAlgorithm.Rectangular:
                    case PartitionAlgorithm.Hexagon:
                    case PartitionAlgorithm.Line:
                    case PartitionAlgorithm.River:
                        maskCreationActionModule.OnInspectorGUI();
                        vegetationStudioProActionModule.OnInspectorGUI();
                        break;

                    case PartitionAlgorithm.Road:
                        roadCreationActionModule.OnInspectorGUI();
                        vegetationStudioProActionModule.OnInspectorGUI();
                        break;

                    default:
                        throw new System.ArgumentException("Unsupported Partition Algorithm " + extension.boundsSettings.partitionAlgorithm);
                }

            }
            GUILayout.EndVertical();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            editor.serializedObject.ApplyModifiedProperties();

            // initial check has been performed
            performInitialConsistencyCheck = false;
        }



        /// <summary>
        /// Create a list of bounds to process. Either all terrains combined or individually
        /// </summary>
        /// <returns></returns>
        public List<Bounds> GetBoundsToProcess()
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

        public void ResetMaskId()
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

            // grow/shrink mask
            nodes = PolygonUtils.Resize(nodes, extension.shapeSettings.resizeFactor);

            // random shape inside bounds
            SetMaskNodes(mask, nodes);

            // put move handle into the center of the polygon
            BiomeMaskUtils.CenterMainHandle(mask);

            #region Lake Polygon
            // create a lake and re-parent the biome to it
            if (extension.lakeSettings.createLake)
            {
                lakeModule.CreateLake( mask, gameObjectName, nodes);
            }
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
            if ( extension.boundsSettings.boundsProcessing == ProcessingSettings.BoundsProcessing.Biome)
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
