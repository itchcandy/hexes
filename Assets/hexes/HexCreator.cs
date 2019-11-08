namespace hexes
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    
    // [ExecuteInEditMode]
    [CustomEditor(typeof(Hex))]
    public class HexCreator : Editor
    {
        MeshFilter meshFilter;
        Mesh mesh;
        Material material;
        float a;
        float b;
        float c;
        Vector2 size;
        GameObject gameObject;
        Hex hex;
        Vector3[] g_centers = new Vector3[0];
        
        [MenuItem("Tools/Hexes")]
        static void Init()
        {
            var t = (new GameObject("Hex"));
            t.transform.position = Vector3.zero;
            t.gameObject.AddComponent<Hex>();
        }

        public HexCreator()
        {
            a = 1f;
            b = Mathf.Sin(Mathf.PI/3);
            c = 0.5f;
        }

        Vector3[] GetHexPoly(Vector3 center)
        {
            var v = new Vector3[7];
            v[0] = new Vector3(center.x, center.y, center.z+a);
            v[1] = new Vector3(center.x+b, center.y, center.z+c);
            v[2] = new Vector3(center.x+b, center.y, center.z-c);
            v[3] = new Vector3(center.x, center.y, center.z-a);
            v[4] = new Vector3(center.x-b, center.y, center.z-c);
            v[5] = new Vector3(center.x-b, center.y, center.z+c);
            v[6] = new Vector3(center.x, center.y, center.z+a);
            return v;
        }

        Vector3[] GetHexCenters()
        {
            var xn = hex.gridSize.x/(4*b);
            var xs = xn*2*b;
            xn = 2*xn+1;
            var yn = hex.gridSize.y/(3*a);
            var ys = -1*yn*1.5f*b;
            var x_out = yn%2==1;
            xs = x_out ? -1*(xs-b) : -1*xs;
            yn = 2*yn + 1;
            List<Vector3> centers = new List<Vector3>();
            for(int i=0; i<yn; i++) {
                for(int j=0; j<(x_out?xn+1:xn); j++)
                    centers.Add(new Vector3(xs+b*2*j, 0, ys+a*1.5f*i));
                x_out = !x_out;
                xs = x_out ? xs-b : xs+b;
            }
            return centers.ToArray();
        }

        protected virtual void OnSceneGUI()
        {
            if(hex == null) hex = target as Hex;
            Handles.color = Color.black;
            Handles.Label(Vector3.zero, "hello there");
            Handles.DrawPolyLine(hex.vertices);
            Handles.DrawPolyLine(new Vector3(-0.5f*hex.gridSize.x, 0, 0.5f*hex.gridSize.y), new Vector3(0.5f*hex.gridSize.x, 0, 0.5f*hex.gridSize.y), new Vector3(0.5f*hex.gridSize.x, 0, -0.5f*hex.gridSize.y), new Vector3(-0.5f*hex.gridSize.x, 0, -0.5f*hex.gridSize.y), new Vector3(-0.5f*hex.gridSize.x, 0, 0.5f*hex.gridSize.y));
            var c = GetHexCenters();
            Handles.Label(c[0], c[0].x.ToString() + "," + c[0].z.ToString());
            for(int i=0; i<c.Length; i++) {
                Handles.DrawPolyLine(GetHexPoly(c[i]));
            }

            Handles.BeginGUI();
            if(GUILayout.Button("Refresh"))
                Refresh();
            if(GUILayout.Button("Create mesh"))
                CreateMesh();
            Handles.EndGUI();
        }

        void Refresh()
        {
            Debug.Log("refresh");
            Debug.Log(target.name);
        }

        void CreateMesh()
        {
            if(hex == null) hex = target as Hex;
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

            GameObject g = hex.gameObject;
            g.transform.position = center;
            meshFilter = g.GetComponent<MeshFilter>();
            if(meshFilter == null)
                meshFilter = g.AddComponent<MeshFilter>();
            var mr = g.GetComponent<MeshRenderer>();
            if(mr == null)
                mr = g.AddComponent<MeshRenderer>();
            mr.material = hex.material;
            meshFilter.mesh = mesh = new Mesh();
            mesh.name = "Hex";
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            hex.triangles = triangles.ToArray();
            hex.vertices = vertices.ToArray();
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