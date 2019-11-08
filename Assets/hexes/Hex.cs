namespace hexes
{
    using UnityEngine;

    [ExecuteInEditMode]
    public class Hex : MonoBehaviour
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Material material;
        public Vector2 gridSize;
    }
}