using UnityEngine;

namespace InteractiveDelaunayVoronoi
{
    /// <summary>
    /// Triangle wrapper class to keep the code independent from the original DelaunayVoronoi implementation
    /// </summary>
    public class Triangle
    {
        public Vector2[] Vertices { get; }
        public Vector2 CircumCenter { get; }
        public double RadiusSquared;

        public Triangle(Vector2 vector1, Vector2 vector2, Vector2 vector3, Vector2 circumcenter, double radiusSquared)
        {
            Vertices = new Vector2[3] { vector1, vector2, vector3 };
            CircumCenter = circumcenter;
            RadiusSquared = radiusSquared;
        }
    }
}
