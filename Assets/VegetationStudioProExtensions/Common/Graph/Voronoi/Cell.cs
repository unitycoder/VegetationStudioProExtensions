using System.Windows;
using UnityEngine;

namespace InteractiveDelaunayVoronoi
{
    /// <summary>
    /// A cell containing the site and 
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// The vertices which build the voronoi cell
        /// </summary>
        public Vector2[] Vertices { get; }

        /// <summary>
        /// The center point around which the vertices are distributed
        /// </summary>
        public Vector2 Centroid { get; }
        
        /// <summary>
        /// The point that was used for the Delaunay Triangulation
        /// </summary>
        public Vector2 DelaunayPoint { get; }

        public Cell(Vector2[] vertices, Vector2 centroid, Vector2 delaunayPoint)
        {
            Vertices = vertices;
            Centroid = centroid;
            DelaunayPoint = delaunayPoint;
        }
    }
}
