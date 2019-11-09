namespace hexes
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor.Formats.Fbx.Exporter;
    using Autodesk.Fbx;
    
    // [ExecuteInEditMode]
    [CustomEditor(typeof(Hex))]
    public class HexCreator : Editor
    {
        float a;
        float b;
        float c;
        Hex hex;
        Vector3[][] cents;
        Vector2Int ah;     // Active hex coordinates
        
        public HexCreator()
        {
            a = 1f;
            b = Mathf.Sin(Mathf.PI/3);
            c = 0.5f;
        }

        [MenuItem("Tools/Hexes")]
        static void Init()
        {
            var g = (new GameObject("Hex"));
            g.transform.position = Vector3.zero;
            var h = g.AddComponent<Hex>();
            var mf = g.AddComponent<MeshFilter>();
            h.mesh = mf.mesh = new Mesh();
            // g.AddComponent<MeshRenderer>().material = h.material;
            g.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Default-Diffuse");
        }

        protected virtual void OnSceneGUI()
        {
            if(hex == null) hex = target as Hex;
            Handles.color = Color.black;
            Handles.DrawPolyLine(new Vector3(-0.5f*hex.gridSize.x, 0, 0.5f*hex.gridSize.y), new Vector3(0.5f*hex.gridSize.x, 0, 0.5f*hex.gridSize.y), new Vector3(0.5f*hex.gridSize.x, 0, -0.5f*hex.gridSize.y), new Vector3(-0.5f*hex.gridSize.x, 0, -0.5f*hex.gridSize.y), new Vector3(-0.5f*hex.gridSize.x, 0, 0.5f*hex.gridSize.y));
            cents = GetHexCenters(hex.transform.position);
            var e = Event.current;
            if(e.type == EventType.KeyDown) {
                switch(e.keyCode) {
                    case KeyCode.W:
                        if(ah.x < this.cents.Length-1) {
                            ah.x += 1;
                            if(this.cents[ah.x].Length <= ah.y)
                                ah.y = this.cents[ah.x].Length-1;
                        }
                        break;
                    case KeyCode.S:
                        if(ah.x>0) {
                            ah.x -= 1;
                            if(this.cents[ah.x].Length <= ah.y)
                                ah.y = this.cents[ah.x].Length-1;
                        }
                        break;
                    case KeyCode.D:
                        if(ah.y < this.cents[ah.x].Length-1)
                            ah.y += 1;
                        break;
                    case KeyCode.A:
                        if(ah.y>0)
                            ah.y -= 1;
                        break;
                    case KeyCode.Space:
                        AddHexToMesh(cents[ah.x][ah.y]);
                        break;
                }
                SceneView.RepaintAll();
            }
            for(int i=0; i<cents.Length; i++) {
                for(int j=0; j<cents[i].Length; j++) {
                    if(i==ah.x && j==ah.y) {
                        Handles.color = Color.green;
                        Handles.SphereHandleCap(0, cents[i][j], Quaternion.identity, 0.2f, EventType.Repaint);
                        Handles.DrawPolyLine(GetHexPoly(cents[i][j]));
                        Handles.color = Color.black;
                    }
                    else {
                        Handles.SphereHandleCap(0, cents[i][j], Quaternion.identity, 0.2f, EventType.Repaint);
                        Handles.DrawPolyLine(GetHexPoly(cents[i][j]));
                    }
                }
            }
            GUILayout.BeginArea(new Rect(0, 0, 80, Screen.height));
            Handles.BeginGUI();
            if(GUILayout.Button("Refresh"))
                Refresh();
            if(GUILayout.Button("Save Mesh"))
                SaveMesh();
            if(GUILayout.Button("Load Mesh"))
                Load();
            Handles.EndGUI();
            GUILayout.EndArea();
        }

        void AddHexToMesh(Vector3 center)
        {
            List<Vector3> verts = new List<Vector3>(hex.vertices);
            List<int> tris = new List<int>(hex.triangles);
            var v2 = CreateVerts(center);
            var t2 = GetTris(v2, verts.Count);
            verts.AddRange(v2);
            tris.AddRange(t2);
            var mesh = hex.mesh;
            mesh.name = "Hex";
            mesh.vertices = hex.vertices = verts.ToArray();
            mesh.triangles = hex.triangles = tris.ToArray();
            mesh.RecalculateNormals();
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

        Vector3[][] GetHexCenters(Vector3 center)
        {
            int xn = (int)(hex.gridSize.x/(4*b));
            var xs = xn*2*b;
            xn = 2*xn+1;
            var yn = (int)(hex.gridSize.y/(3*a));
            var ys = -1*yn*1.5f*a;
            var x_out = yn%2==1;
            xs = x_out ? -1*(xs+b) : -1*xs;
            yn = 2*yn + 1;
            var c = new Vector3[yn][];
            List<Vector3> centers = new List<Vector3>();
            for(int i=0; i<yn; i++) {
                var n = x_out?xn+1:xn;
                c[i] = new Vector3[n];
                for(int j=0; j<n; j++)
                    c[i][j] = new Vector3(xs+b*2*j, 0, ys+a*1.5f*i);
                x_out = !x_out;
                xs = x_out ? xs-b : xs+b;
            }
            return c;
        }

        void SaveMesh()
        {
            string dir = "./Assets/Resources/Hexes";
            Directory.CreateDirectory(dir);
            ModelExporter.ExportObject(dir+"/"+hex.name+".fbx", hex);
            var g2 = Instantiate<GameObject>(Resources.Load<GameObject>("Hexes/"+hex.name));
            var h2 = g2.AddComponent<Hex>();
            h2.name = hex.name;
            h2.triangles = hex.triangles;
            h2.vertices = hex.vertices;
            h2.gridSize = hex.gridSize;
            PrefabUtility.SaveAsPrefabAsset(g2, Path.GetFullPath(dir+"/"+hex.name+".prefab"));
            DestroyImmediate(g2);
        }

        void Load()
        {
            var g = Instantiate<GameObject>(Resources.Load<GameObject>("Hexes/Hex"));
        }

        void Refresh()
        {
            Debug.Log("refresh");
            hex.mesh = hex.GetComponent<MeshFilter>().mesh;
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