namespace vm
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    
    // [ExecuteInEditMode]
    public class HexCreator : EditorWindow
    {
        public MeshFilter meshFilter;
        public Mesh mesh;
        public Material material;
        public float a;
        public float b;
        public float c;
        
        // void Update()
        // {
        //     if(Input.GetKeyDown(KeyCode.C))
        //         CreateMesh();
        // }

        [MenuItem("Tools/Hexes")]
        static void Init()
        {
            var w = EditorWindow.CreateInstance<HexCreator>();
            
            w.a = 1f;
            w.b = Mathf.Sin(Mathf.PI/3);
            w.c = 0.5f;
            w.Show();
        }

        void OnGUI()
        {
            if(GUILayout.Button("Create"))
                CreateMesh();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
        }

        void CreateMesh()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var center = Vector3.zero;

            var v1 = CreateVerts(center);
            var t1 = GetTris(v1);
            vertices.AddRange(v1);
            triangles.AddRange(t1);

            var v2 = CreateVerts(new Vector3(center.x-b, center.y, center.z+a+c));
            var t2 = GetTris(v2, vertices.Count);
            vertices.AddRange(v2);
            triangles.AddRange(t2);

            var v3 = CreateVerts(new Vector3(center.x+b, center.y, center.z+a+c));
            var t3 = GetTris(v3, vertices.Count);
            vertices.AddRange(v3);
            triangles.AddRange(t3);

            GameObject g = new GameObject("Hex");
            g.transform.position = center;
            meshFilter = g.AddComponent<MeshFilter>();
            g.AddComponent<MeshRenderer>();
            meshFilter.mesh = mesh = new Mesh();
            mesh.name = "Hex";
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }

        int[] GetTris(Vector3[] verts, int n = 0)
        {
            var triangles = new int[18];
            var j=0;
            for(int i=2; i<verts.Length; i++) {
                triangles[j++] = i-1 + n;
                triangles[j++] = i + n;
                triangles[j++] = 0 + n;
            }
            triangles[j++] = verts.Length-1 + n;
            triangles[j++] = 1 + n;
            triangles[j++] = 0 + n;
            return triangles;
        }

        Vector3[] CreateVerts(Vector3 center)
        {
            var v = new Vector3[7];
            v[0] = center;
            v[1] = new Vector3(center.x, center.y, center.z+a);
            v[2] = new Vector3(center.x+b, center.y, center.z+c);
            v[3] = new Vector3(center.x+b, center.y, center.z-c);
            v[4] = new Vector3(center.x, center.y, center.z-a);
            v[5] = new Vector3(center.x-b, center.y, center.z-c);
            v[6] = new Vector3(center.x-b, center.y, center.z+c);
            return v;
        }
    }
}