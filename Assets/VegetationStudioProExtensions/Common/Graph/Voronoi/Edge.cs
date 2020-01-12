using UnityEngine;

namespace InteractiveDelaunayVoronoi
{
    /// <summary>
    /// Edge wrapper class to keep the code independent from the original DelaunayVoronoi implementation
    /// </summary>
    public class Edge
    {
        public Vector2 Vector1 { get; }
        public Vector2 Vector2 { get; }

        public Edge(Vector2 vector1, Vector2 vector2)
        {
            Vector1 = vector1;
            Vector2 = vector2;
        }
    }
}
