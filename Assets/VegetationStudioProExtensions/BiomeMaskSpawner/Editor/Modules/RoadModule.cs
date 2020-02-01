using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DelaunayVoronoi;
using InteractiveDelaunayVoronoi;
#if EASY_ROADS
using EasyRoads3Dv3;
#endif

namespace VegetationStudioProExtensions
{
    public class RoadModule
    {
        private SerializedProperty biomeMaskArea;
        private SerializedProperty road;
        private SerializedProperty smoothEnabled;
        private SerializedProperty closedTrack;
        private SerializedProperty minDistance;

        private BiomeMaskSpawnerExtensionEditor editor;
#if EASY_ROADS
        private ERRoadNetwork erRoadNetwork;
        private ERRoad erRoad;
#endif

        public RoadModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {

            biomeMaskArea = editor.FindProperty(x => x.roadSettings.biomeMaskArea);
            road = editor.FindProperty(x => x.roadSettings.road);
            smoothEnabled = editor.FindProperty(x => x.roadSettings.smoothEnabled);
            closedTrack = editor.FindProperty(x => x.roadSettings.closedTrack);
            minDistance = editor.FindProperty(x => x.roadSettings.minDistance);

#if EASY_ROADS
            // create a reference to the road network in the scene
            erRoadNetwork = new ERRoadNetwork();
            erRoad = null;

            if (road.objectReferenceValue != null)
            {
                erRoad = erRoadNetwork.GetRoadByGameObject(road.objectReferenceValue as GameObject);
            }
#endif

        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Road Settings", GUIStyles.GroupTitleStyle);

#if EASY_ROADS
            GUILayout.BeginHorizontal();
            {
                // show error color in case there's no container
                if (erRoad == null)
                {
                    editor.SetErrorBackgroundColor();
                }

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(road, new GUIContent("Road", "The Easy Roads road which will get the Biome Mask Area's nodes transferred to"));

                if (EditorGUI.EndChangeCheck())
                {
                    if (road.objectReferenceValue != null)
                    {
                        erRoad = erRoadNetwork.GetRoadByGameObject(road.objectReferenceValue as GameObject);
                    }
                }

                // default color in case error color was set
                editor.SetDefaultBackgroundColor();
            }
            GUILayout.EndHorizontal();

            // consistency check for EasyRoads road
            GUILayout.BeginHorizontal();
            if (erRoad == null)
            {
                EditorGUILayout.HelpBox("Easy Roads GameObject of type Road required", MessageType.Error);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(smoothEnabled, new GUIContent("Smooth", "Perform smoothing on the polygon"));
            EditorGUILayout.PropertyField(closedTrack, new GUIContent("Closed Track", "Set the Closed Track setting of the road"));
            EditorGUILayout.PropertyField(minDistance, new GUIContent("Min. Node Distance", "Minimum distance between nodes. Use this to remove close curve jitter"));
#else
            EditorGUILayout.HelpBox("Requires Easy Roads installed and 'EASY_ROADS' Scripting Define Symbol", MessageType.Error);

#endif
        }

        public void CreateMasks(List<Bounds> boundsList)
        {
#if EASY_ROADS
            CreateRoads( boundsList);
#endif
        }

#region Easy Roads Code
#if EASY_ROADS
        private void Clear()
        {
            erRoad = erRoadNetwork.GetRoadByGameObject(road.objectReferenceValue as GameObject);

            if (erRoad == null)
            {
                Debug.LogError("No road gameobject specified");
                return;
            }

            // the mask didn't get refreshed with the road, it stayed the old one => according to raoul this helps:
            // Object.DestroyImmediate(erRoad.gameObject.GetComponent<AwesomeTechnologies.VegetationMaskLine>());

            // delete all markers
            // TODO: find out how to do that properly
            while (erRoad.GetMarkerCount() > 0)
            {
                erRoad.DeleteMarker(0);
            }

            erRoad.Refresh();
        }


        /// <summary>
        /// Create rivers
        /// </summary>
        /// <param name="boundsList"></param>
        public void CreateRoads(List<Bounds> boundsList)
        {
            // remove all existing nodes from the road
            Clear();

            if (boundsList.Count == 0)
                return;

            // we only create a single random road
            /*
            foreach (Bounds bounds in boundsList)
            {
                // create masks
                CreateMasks(bounds);
            }
            */
            // get random bounds
            int index = Random.Range(0, boundsList.Count - 1);
            Bounds bounds = boundsList[index];

            // create shape and road
            CreateRoads(bounds);
        }

        private void CreateRoads(Bounds bounds)
        {
            // not too close to the bounds; the easy roads spline might move the road outside
            bounds.size *= 0.9f; 

            Vector2[] clipBounds = new Vector2[] {
                new Vector2( bounds.center.x - bounds.extents.x, bounds.center.z - bounds.extents.z),
                new Vector2( bounds.center.x + bounds.extents.x, bounds.center.z - bounds.extents.z),
                new Vector2( bounds.center.x + bounds.extents.x, bounds.center.z + bounds.extents.z),
                new Vector2( bounds.center.x - bounds.extents.x, bounds.center.z + bounds.extents.z),
            };

            int pointCount = editor.extension.voronoiSettings.pointCount;

            List<Vector3> maskNodes = CreateRoad(bounds, pointCount, clipBounds);

            List<Vector3> nodes = ShapeCreator.CreateRandomShape(maskNodes, //
                editor.extension.shapeSettings.RandomConvexity, //
                editor.extension.shapeSettings.keepOriginalPoints, //
                editor.extension.shapeSettings.RandomPointsCount, //
                editor.extension.shapeSettings.randomAngle, //
                editor.extension.shapeSettings.douglasPeuckerReductionTolerance);

            PolygonUtils.SortClockWise(nodes);

            if (smoothEnabled.boolValue)
            {
                List<Vector2> positionsXY = nodes.ConvertAll(item => new Vector2(item.x, item.z));
                positionsXY = getCurveSmoothingChaikin(positionsXY, 0.5f, 0);

                nodes = positionsXY.ConvertAll(item => new Vector3(item.x, 0, item.y));
            }

            nodes = AlignToTerrainHeight(nodes);

            // remove nodes that are too close to each other
            RemoveNodes(nodes, minDistance.floatValue);

            // set ER markers
            erRoad.AddMarkers(nodes.ToArray());

            // set closed track
            erRoad.ClosedTrack(closedTrack.boolValue);
        }

        private List<Vector3> CreateRoad(Bounds bounds, int pointCount, Vector2[] clipPolygon)
        {
            // get offset, eg when biome mask is used for clipping
            float xmin = bounds.center.x - bounds.extents.x;
            float zmin = bounds.center.z - bounds.extents.z;

            // get 0/0-based bounds for graph processing
            Bounds graphBounds = new Bounds(bounds.size / 2, bounds.size);

            DelaunayVoronoiGraph graph = new DelaunayVoronoiGraph();

            // algorithm is 0-based
            graph.GeneratePoints(0, bounds.size.x, bounds.size.z); // initialize with the dimensions which are used in the algorithm

            // add random points
            for (int i = 0; i < pointCount; i++)
            {
                // get random point starting at [0/0]
                Vector2 vector = PolygonUtils.GetRandomPointXZ(graphBounds);

                graph.AddPoint(vector.x, vector.y);
            }

            // create the graph using the points
            graph.CreateGraph();

            int cellIndex = Random.Range(0, pointCount - 1);

            // normalize clip polygon, shift to [0/0]
            Vector2[] offsetClipPolygon = clipPolygon.Select(item => new Vector2(item.x - xmin, item.y - zmin)).ToArray();

            // get cell, clip it at the clip polygon
            Cell cell = graph.GetVoronoiCell(cellIndex, offsetClipPolygon);

            if (cell == null)
                return null;

            // consider biome mask shift: shift points away from 0/0 if necessary
            Vector3 position = new Vector3(cell.Centroid.x + xmin, 0, cell.Centroid.y + zmin); // TODO: recalculate, might have changed because of clipping

            // consider biome mask shift: shift points away from 0/0 if necessary
            List<Vector3> nodes = cell.Vertices.Select(item => new Vector3(item.x + xmin, 0, item.y + zmin)).ToList();

            // apply random shape if requested
            if (editor.extension.shapeSettings.randomShape)
            {

                nodes = ShapeCreator.CreateRandomShape(nodes, //
                    editor.extension.shapeSettings.RandomConvexity, //
                    editor.extension.shapeSettings.keepOriginalPoints, //
                    editor.extension.shapeSettings.RandomPointsCount, //
                    editor.extension.shapeSettings.randomAngle, //
                    editor.extension.shapeSettings.douglasPeuckerReductionTolerance);
            }

            return nodes;


        }

        /// <summary>
        /// Remove nodes that are too close to each other
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="minDistance"></param>
        private void RemoveNodes(List<Vector3> positions, float minDistance)
        {
            List<int> removeList = new List<int>();

            // consider end node -> start node
            int prevIndex = positions.Count - 1;

            for (int i = 0; i < positions.Count; i++)
            {
                int currIndex = i;

                Vector3 prev = positions[prevIndex];
                Vector3 curr = positions[currIndex];

                float distance = (curr - prev).magnitude;

                if (distance < minDistance)
                {
                    removeList.Add(currIndex);
                }
                else
                {
                    prevIndex = currIndex;
                }

            }

            removeList.Reverse();

            foreach (int i in removeList)
            {
                positions.RemoveAt(i);
            }
        }

        private List<Vector3> AlignToTerrainHeight(List<Vector3> positions)
        {
            List<Vector3> alignedPositions = new List<Vector3>();

            foreach (Vector3 position in positions)
            {
                float x = position.x;
                float y = 0;
                float z = position.z;

                float? terrainHeight = TerrainUtils.GetTerrainHeight(x, z);

                if (terrainHeight != null)
                {
                    y = (float)terrainHeight;
                }
                else
                {
                    Debug.LogError("Terrain height was null at x=" + x + ", z=" + z);
                }

                Vector3 positionXYZ = new Vector3(x, y, z);

                alignedPositions.Add(positionXYZ);
            }

            return alignedPositions;
        }

        private List<Vector2> getCurveSmoothingChaikin(List<Vector2> points, float tension, int nrOfIterations)
        {
            // checks
            if (points == null || points.Count < 3)
                return null;

            if (nrOfIterations < 1)
                nrOfIterations = 1;
            else if (nrOfIterations > 10)
                nrOfIterations = 10;

            if (tension < 0)
                tension = 0;
            else if (tension > 1
             )
                tension = 1;

            // the tension factor defines a scale between corner cutting distance in segment half length, i.e. between 0.05 and 0.45
            // the opposite corner will be cut by the inverse (i.e. 1-cutting distance) to keep symmetry
            // with a tension value of 0.5 this amounts to 0.25 = 1/4 and 0.75 = 3/4 the original Chaikin values
            float cutdist = 0.05f + (tension * 0.4f);

            // make a copy of the pointlist and feed it to the iteration
            List<Vector2> nl = new List<Vector2>();
            var loopTo = points.Count - 1;
            for (int i = 0; i <= loopTo; i++)
                nl.Add(new Vector2(points[i].x, points[i].y));
            var loopTo1 = nrOfIterations;
            for (int i = 1; i <= loopTo1; i++)
                nl = getSmootherChaikin(nl, cutdist);

            return nl;
        }

        private List<Vector2> getSmootherChaikin(List<Vector2> points, float cuttingDist)
        {
            List<Vector2> nl = new List<Vector2>();

            // always add the first point
            nl.Add(new Vector2(points[0].x, points[0].y));

            Vector2 q, r;
            var loopTo = points.Count - 2;
            for (int i = 0; i <= loopTo; i++)
            {
                q = (1 - cuttingDist) * points[i] + cuttingDist * points[i + 1];
                r = cuttingDist * points[i] + (1 - cuttingDist) * points[i + 1];
                nl.Add(q);
                nl.Add(r);
            }

            // always add the last point
            nl.Add(new Vector2(points[points.Count - 1].x, points[points.Count - 1].y));

            return nl;
        }
#endif
#endregion Easy Roads Code
        }
}
