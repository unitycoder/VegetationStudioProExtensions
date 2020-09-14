using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace DelaunayVoronoi
{
    // Some names like "Cell" are too common. So in case someone creates a Cell class in global namespace we might run into conflicts.
    // Putting the internal classes inside the namespace makes the compiler search for the internal ones before the global ones.
    using Delaunay.VectorUtils;
    using InteractiveDelaunayVoronoi;
    using SutherlandHodgmanAlgorithm;

    /// <summary>
    /// DelaunayVoronoi wrapper class to keep the code independent from the original DelaunayVoronoi implementation
    /// </summary>
    public class DelaunayVoronoiGraph
    {

        private DelaunayTriangulator delaunay = new DelaunayTriangulator();
        private Voronoi voronoi = new Voronoi();

        private IEnumerable<Triangle> triangulation;
        private IEnumerable<Edge> vornoiEdges;

        /// <summary>
        /// The list of points on the screen. 
        /// Initialized with empty list to avoid nullpointer exceptions.
        /// </summary>
        private IEnumerable<Point> points = new List<Point>();

        public void GeneratePoints(int amount, double maxX, double maxY)
        {
            this.points = delaunay.GeneratePoints(amount, maxX, maxY);
        }

        public void AddPoint(double x, double y)
        {
            AddPoint(x, y, true);
        }
        
        public void AddPoint(double x, double y, bool ignoreDuplicates)
        {
            Point point = new Point(x, y);

            // don't allow duplicates, they'll cause problems
            if (ignoreDuplicates)
            {
                if (((List<Point>)points).Contains(point))
                    return;
            }

            // add new point to points list
            ((List<Point>)points).Add(point);
        }

        /// <summary>
        /// Clear any calculated data out of the points list so that we can recreate the graph using the same points
        /// </summary>
        private void ResetData()
        {

            foreach (Point point in points)
            {
                point.AdjacentTriangles.Clear();

            }

        }

        public void SetLastPoint(double x, double y)
        {
            Point point = new Point(x, y);

            // add new point to points list
            int count = ((List<Point>)points).Count;
            if (count > 2)
            {
                int lastPointIndex = count - 1;
                ((List<Point>)points)[lastPointIndex] = point;
            }
        }

        public int GetLastPointIndex()
        {
            return ((List<Point>)points).Count - 1;
        }

        public void CreateGraph()
        {
            // reset data
            ResetData();

            // delaunay
            var delaunayTimer = Stopwatch.StartNew();
            this.triangulation = delaunay.BowyerWatson(points);
            delaunayTimer.Stop();

            // voronoi
            var voronoiTimer = Stopwatch.StartNew();
            vornoiEdges = voronoi.GenerateEdgesFromDelaunay(triangulation);
            voronoiTimer.Stop();
        }

        /// <summary>
        /// Get all delaunay points as vectors.
        /// </summary>
        /// <returns></returns>
        public List<Vector2> GetAllVectors()
        {
            return ToVectors(this.points);
        }

        private List<Vector2> GetCircumCenterVectors(Point point)
        {
            // use a set to avoid duplicates
            HashSet<Vector2> circumCenterSet = new HashSet<Vector2>();

            foreach (Triangle triangle in point.AdjacentTriangles)
            {
                Point circumCenterPoint = new Point(triangle.Circumcenter.X, triangle.Circumcenter.Y);
                circumCenterSet.Add( ToVector(circumCenterPoint));

            }

            // convert to lsit
            List<Vector2> circumCenterList = new List<Vector2>();
            circumCenterList.AddRange(circumCenterSet);

            // ensure the points are in clockwise order
            circumCenterList.Sort(new ClockwiseComparerVector(circumCenterList));

            return circumCenterList;
        }

        public List<Vector2> GetCircumCenterPoints(int index)
        {
            Point point = ((List<Point>)points)[index];

            List<Vector2> circumCenterPoints = GetCircumCenterVectors(point);

            return circumCenterPoints;
        }

        public List<Vector2> GetCircumCenterPoints()
        {
            return triangulation.Select(item => ToVector( item.Circumcenter)).ToList();
        }

        public Vector2 GetVector( int index)
        {
            Point point = ((List<Point>)points)[index];

            return ToVector( point);
        }

        private List<Vector2> ToVectors(IEnumerable<Point> points)
        {
            return points.Select(item => ToVector( item)).ToList();
        }

        private Vector2 ToVector( Point point)
        {
            return new Vector2( (float) point.X, (float) point.Y);
        }

        public List<InteractiveDelaunayVoronoi.Triangle> GetDelaunayTriangles()
        {
            return triangulation.Select(item => new InteractiveDelaunayVoronoi.Triangle( //
                ToVector(item.Vertices[0]), //
                ToVector(item.Vertices[1]), //
                ToVector(item.Vertices[2]), //
                ToVector(item.Circumcenter), //
                item.RadiusSquared //
                )
            ).ToList();
        }
        
        public List<InteractiveDelaunayVoronoi.Edge> GetVoronoiEdges()
        {
            return vornoiEdges.Select(item => new InteractiveDelaunayVoronoi.Edge( //
                ToVector( item.Point1), //
                ToVector( item.Point2)) //
            ).ToList();
        }

        /// <summary>
        /// Get a list of all polygons per point. This contains duplicate edges if multiple points share the same edge.
        /// </summary>
        /// <returns></returns>
        public List<Vector2[]> GetAllVoronoiPolygons()
        {
            List<Vector2[]> allPolygons = new List<Vector2[]>();

            foreach ( Point point in points)
            {
                List<Vector2> currentPolygon = GetCircumCenterVectors(point);
                
                allPolygons.Add(currentPolygon.ToArray());
            }

            return allPolygons;
        }

        public Dictionary<int,Cell> GetAllVoronoiCellsMap(Vector2[] clipPolygon)
        {
            List<Point> allPointsList = (List<Point>)points;

            Dictionary<int, Cell> map = new Dictionary<int, Cell>();

            for( int i=0; i < allPointsList.Count; i++) 
            {
                Point point = allPointsList[i];

                Cell cell = GetVoronoiCell(point, clipPolygon);

                // cell can be null
                if (cell == null)
                {
                    UnityEngine.Debug.Log( "Cell is null for point index " + i);
                }

                map.Add(i, cell);

            }

            return map;
        }

        /// <summary>
        /// Get a list of all polygons per point. This contains duplicate edges if multiple points share the same edge.
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetAllVoronoiCells()
        {
            return GetAllVoronoiCells(null);
        }

        /// <summary>
        /// Get a list of all polygons per point. This contains duplicate edges if multiple points share the same edge.
        /// Optionally clip the points at the specified polygon.
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetAllVoronoiCells(Vector2[] clipPolygon)
        {
            List<Cell> allCells = new List<Cell>();

            foreach (Point point in points)
            {

                Cell cell = GetVoronoiCell(point, clipPolygon);

                if (cell == null)
                    continue;

                allCells.Add( cell);
            }

            return allCells;
        }

        /// <summary>
        /// Get the voronoi cell for the given point using the clip polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="clipPolygon"></param>
        /// <returns></returns>
        public Cell GetVoronoiCell( int index, Vector2[] clipPolygon)
        {
            Point point = ((List<Point>)points)[index];
            return GetVoronoiCell(point, clipPolygon);
        }

        /// <summary>
        /// Get the voronoi cell for the given point using the clip polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="clipPolygon"></param>
        /// <returns></returns>
        public Cell GetVoronoiCell( Point point, Vector2[] clipPolygon)
        {
            List<Vector2> currentPolygon = GetCircumCenterVectors(point);

            if (currentPolygon.Count == 0)
                return null;

            Cell cell;

            if (clipPolygon == null)
            {
                Vector2 centroid = PolygonUtils.GetMeanVector(currentPolygon.ToArray());
                cell = new Cell(currentPolygon.ToArray(), centroid, ToVector(point));
            }
            else
            {
                Vector2[] clippedPoints = SutherlandHodgman.GetIntersectedPolygon(currentPolygon.ToArray(), clipPolygon);

                if (clippedPoints.Length == 0)
                {
                    UnityEngine.Debug.Log("Clipping algorithm returned non-intersecting polygon. Skipping it.");
                    return null;
                }

                Vector2 centroid = PolygonUtils.GetMeanVector(clippedPoints);

                // create the cell including polygons and center point
                cell = new Cell(clippedPoints, centroid, ToVector(point));
            }

            return cell;
        }
      
        /// <summary>
        /// Perform Lloyd Relaxation. Move all points to the centroid of the voronoi cell
        /// </summary>
        public void RelaxTowardsCentroid( float speed, Vector2[] clipPolygon, double stopDistance)
        {
            List<Point> allPointsList = (List<Point>)points;

            for( int i=0; i < allPointsList.Count; i++)
            {
                Point point = allPointsList[i];

                Cell cell = GetVoronoiCell(point, clipPolygon);

                if (cell == null)
                    continue;

                // get distance, ie magnitude
                float distance = (cell.Centroid - cell.DelaunayPoint).magnitude;

                // stop at a given delta, otherwise we'd only get jitters
                if (distance < stopDistance)
                    continue;

                // get direction
                Vector2 direction = cell.Centroid - cell.DelaunayPoint;
                direction.Normalize();

                Vector2 relaxationStepPoint = cell.DelaunayPoint + direction * speed;
                Point relaxedPoint = new Point(relaxationStepPoint.x, relaxationStepPoint.y);

                allPointsList[i] = relaxedPoint;
            }
        }

        /// <summary>
        /// Perform Lloyd Relaxation. Move all points to the centroid of the voronoi cell
        /// </summary>
        public void Relax(Vector2[] clipPolygon)
        {
            List<Point> allPointsList = (List<Point>)points;

            for (int i = 0; i < allPointsList.Count; i++)
            {
                Point point = allPointsList[i];

                Cell cell = GetVoronoiCell(point, clipPolygon);

                if (cell == null)
                    continue;

                Point relaxedPoint = new Point(cell.Centroid.x, cell.Centroid.y);

                allPointsList[i] = relaxedPoint;
            }
        }

        /// <summary>
        /// Get the mean vector for the given cell
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static Vector2 GetMeanVector( Cell cell)
        {
            return PolygonUtils.GetMeanVector(cell.Vertices);
        }

        public Dictionary<int,List<Cell>> GetAllNeighbours(Vector2[] clipPolygon)
        {
            List<Point> allPointsList = (List<Point>)points;

            // create a map and initialize it with an empty list
            Dictionary<int, List<Cell>> neighbourMap = new Dictionary<int, List<Cell>>();

            for( int i=0; i < allPointsList.Count; i++) 
            {
                neighbourMap.Add(i, new List<Cell>());
            }

            Dictionary<int, Cell> cellMap = GetAllVoronoiCellsMap(clipPolygon);

            for (int i = 0; i < allPointsList.Count; i++)
            {
                Cell currentCell = cellMap[i];

                if (currentCell == null)
                    continue;

                for (int j = 0; j < allPointsList.Count; j++)
                {
                    // skip self
                    if (i == j)
                        continue;

                    Cell otherCell = cellMap[j];

                    if (otherCell == null)
                        continue;

                    int sharedVertexCount = 0;

                    foreach (Vector2 currentVector in currentCell.Vertices)
                    {
                        foreach (Vector2 otherVector in otherCell.Vertices)
                        {
                            if (currentVector == otherVector)
                            {
                                sharedVertexCount++;
                            }
                        }
                    }

                    if (sharedVertexCount > 1)
                    {
                        neighbourMap[i].Add(otherCell);
                    }
                }
            }

            return neighbourMap;
        }
       
        /// <summary>
        /// Get the point index for a given cell.
        /// TODO: store the index directly in the cell. Currently needed it only for testing
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public int GetPointIndex( Cell cell)
        {
            List<Vector2> allPointsList = ToVectors( points);

            for( int i=0; i < allPointsList.Count; i++)
            {
                if (allPointsList[i].x == cell.DelaunayPoint.x && allPointsList[i].y == cell.DelaunayPoint.y)
                    return i;
            }

            return -1;
        }
    }
}
