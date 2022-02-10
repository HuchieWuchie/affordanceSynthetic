using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewScript : MonoBehaviour
{
    private GameObject background_plane;
    private GameObject knife, knife_a;
    private Material new_mat;
    private static Vector3 gizmo_center, gizmo_size;
    //private Rect rect;
    //private List<GameObject> gameobjects_list = new List<GameObject>();
    //private Renderer rend;
    //private  gameobjects_list;

    void OnDrawGizmos() {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
        Gizmos.DrawWireCube(gizmo_center, gizmo_size);
    }

    public static Vector2 WorldToGUIPoint(Vector3 world) {
      Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
      screenPoint.y = (float) Screen.height - screenPoint.y;
      return screenPoint;
    }

    public static Vector2[] GetBoundingBox(GameObject go) {
      Bounds b = go.GetComponent<Renderer>().bounds;
      Vector3 center = go.GetComponent<Renderer>().bounds.center;
      Vector3 extents = go.GetComponent<Renderer>().bounds.extents;
      gizmo_center = center;
      gizmo_size = 2*extents;
      //Debug.Log("Bounds " + b);

      float x_min, x_max, y_min, y_max, z_min, z_max;
      x_min = center.x - extents.x;
      x_max = center.x + extents.x;
      y_min = center.y - extents.y;
      y_max = center.y + extents.y;
      z_min = center.z - extents.z;
      z_max = center.z + extents.z;

      Vector2[] extentPoints = new Vector2[8] {
           WorldToGUIPoint(new Vector3(x_min, y_min, z_min)),
           WorldToGUIPoint(new Vector3(x_min, y_min, z_max)),
           WorldToGUIPoint(new Vector3(x_min, y_max, z_min)),
           WorldToGUIPoint(new Vector3(x_max, y_min, z_min)),
           WorldToGUIPoint(new Vector3(x_min, y_max, z_max)),
           WorldToGUIPoint(new Vector3(x_max, y_max, z_min)),
           WorldToGUIPoint(new Vector3(x_max, y_min, z_max)),
           WorldToGUIPoint(new Vector3(x_max, y_max, z_max))
       };

       Vector2 min = extentPoints[0];
       Vector2 max = extentPoints[0];
       foreach (Vector2 v in extentPoints) {
           min = Vector2.Min(min, v);
           max = Vector2.Max(max, v);
       }
       Vector2[] output = new Vector2[2] {min, max};

       return output;
    }

    //public static Vector3[] GetBoundingBoxVertices(GameObject go) {
    public static Vector2[] GetBoundingBoxVertices(GameObject go) {
      Mesh mesh = go.GetComponent<MeshFilter>().mesh;
      //print(mesh.isReadable);
      Vector3[] verts = mesh.vertices;

      for (int i = 0; i < verts.Length; i++) {
           verts[i] = Camera.main.WorldToScreenPoint(go.transform.TransformPoint(verts[i]));
           //make sure we dont go off screen
           if (verts[i].x < 0 || verts[i].x > Screen.width || verts[i].y < 0 || verts[i].y > Screen.height) {
               verts[i] = verts[0];
           }
       }
       Vector2 min = verts[0];
       Vector2 max = verts[0];
       foreach (Vector2 v in verts) {
           min = Vector2.Min(min, v);
           max = Vector2.Max(max, v);
       }
       min.y = Screen.height - min.y;
       max.y = Screen.height - max.y;
       Vector2[] output = new Vector2[2] {min, max};

       return output;

       /*Rect currBox = new Rect {
            xMin = verts[0].x,
            xMax = verts[0].x,
            yMin = verts[0].y,
            yMax = verts[0].y
        };

        //find min and max screen space values
        for (int i = 0; i < verts.Length; i++) {
            currBox.xMin = currBox.xMin < verts[i].x ? currBox.xMin : verts[i].x;
            currBox.xMax = currBox.xMax > verts[i].x ? currBox.xMax : verts[i].x;
            currBox.yMin = currBox.yMin < verts[i].y ? currBox.yMin : verts[i].y;
            currBox.yMax = currBox.yMax > verts[i].y ? currBox.yMax : verts[i].y;
        }

        currBox.yMin = Screen.height - currBox.yMin;
        currBox.yMax = Screen.height - currBox.yMax;*/
    }


    void Awake() {
      //Debug.Log("NewScript Start");
      background_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
      background_plane.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
      background_plane.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
      new_mat = Resources.Load("Bricks Textures/Bricks Texture 01/Bricks Texture 01") as Material;
      background_plane.GetComponent<Renderer>().material = new_mat;
      //background_plane.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
      background_plane.name = "plane";
      //Debug.Log("Background created");

    	knife = Resources.Load("Models/1_knife1/OBJ/Kitchenknife_lowpoly") as GameObject;
    	knife_a = Resources.Load("Models/1_knife1_a/OBJ/Kitchenknife_lowpoly") as GameObject;
    }

    void Start(){
      //https://answers.unity.com/questions/1414048/destroy-specific-gameobject-with-name.html
      var gameobjects_list = FindObjectsOfType<GameObject>();
      ScreenCapture.CaptureScreenshot("/home/daniel/unity_first_test/screenshots/whatever");

      Vector3 camera_pos = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 15));;
      Vector3 world_pos = new Vector3(camera_pos.x, camera_pos.y, camera_pos.z);
      GameObject knife_inst = Instantiate(knife, world_pos, Quaternion.identity);
      knife_inst.name = "knife";
      Vector2[] boundingBox = GetBoundingBox(knife_inst.transform.GetChild(0).gameObject);
      Debug.Log("Min " + boundingBox[0]);
      Debug.Log("Max " + boundingBox[1]);
      Debug.Log("========================");
      //boundingBox[0].y = (float) Screen.height - boundingBox[0].y;
      //boundingBox[1].y = (float) Screen.height - boundingBox[1].y;
      //Debug.Log("Min after invertin y_axis " + boundingBox[0]);
      //Debug.Log("Max after invertin y_axis" + boundingBox[1]);

      camera_pos = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 15));;
      world_pos = new Vector3(camera_pos.x, camera_pos.y, camera_pos.z);
      GameObject knife_inst_a = Instantiate(knife_a, world_pos, Random.rotation);
      knife_inst_a.name = "knife_a";
      Vector2[] bb = GetBoundingBoxVertices(knife_inst_a.transform.GetChild(0).gameObject);
      Debug.Log("Min " + bb[0]);
      Debug.Log("Max " + bb[1]);
      ScreenCapture.CaptureScreenshot("/home/daniel/unity_first_test/screenshots/whatever");
      Debug.Log("Screenshot captured");
    }

    // Update is called once per frame
    void Update(){

    }
}
