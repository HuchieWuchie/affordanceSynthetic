using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NewScript : MonoBehaviour
{
    private GameObject background_plane;
    private GameObject knife, knife_a;
    private Material new_mat;
    private static string textfile_path = "/home/daniel/unity_first_test/screenshots/boundingBoxes.txt";
    private static string img_path = "/home/daniel/unity_first_test/screenshots/img_";

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
       if (min.y > max.y) {
         float temp = max.y;
         max.y = min.y;
         min.y = temp;
       }
       Vector2[] output = new Vector2[2] {min, max};

       return output;
    }

    public void saveBoundingBox(Vector2[] boundingBox){
      int min_x, min_y, max_x, max_y;
      min_x = (int) boundingBox[0].x;
      min_y = (int) boundingBox[0].y;
      max_x = (int) boundingBox[1].x;
      max_y = (int) boundingBox[1].y;

      string serializedData = min_x.ToString() + " "
      + min_y.ToString() + " "
      + max_x.ToString() + " "
      + max_y.ToString() + "\n";

      using (StreamWriter writer = new StreamWriter(textfile_path, true)) {
        writer.Write(serializedData);
      }
    }

    public Vector3 GetRandomPose(){
      Vector3 camera_pos = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 15));;
      Vector3 world_pos = new Vector3(camera_pos.x, camera_pos.y, camera_pos.z);
      return world_pos;
    }

    private void DestroyAllModels(){
      GameObject[] gameobjects_list = FindObjectsOfType<GameObject>();
      foreach (GameObject go in gameobjects_list){
        if (go.name.ToString() !="Directional Light" &&
        go.name.ToString() != "Background" &&
        go.name.ToString() != "Main Camera"){
          Destroy(go);
        }
      }
    }

    private void SpawnModels(){
      GameObject knife_inst = Instantiate(knife, GetRandomPose(), Random.rotation);
      knife_inst.name = "knife";
      Vector2[] bb = GetBoundingBoxVertices(knife_inst.transform.GetChild(0).gameObject);
      saveBoundingBox(bb);

      GameObject knife_inst_a = Instantiate(knife_a, GetRandomPose(), Random.rotation);
      knife_inst_a.name = "knife_a";
      bb = GetBoundingBoxVertices(knife_inst_a.transform.GetChild(0).gameObject);
      saveBoundingBox(bb);
    }

    private void CaptureScreenshot(){
      int idx = Time.frameCount;
      string img_filename = idx.ToString() + ".png";
      Debug.Log(img_path + img_filename);
      //ScreenCapture.CaptureScreenshot(img_path);
    }

    private IEnumerator pacer(){
      yield return SpawnModels();
      yield return CaptureScreenshot();
      yield return DestroyAllModels();
    }


    void Awake() {
      background_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
      background_plane.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
      background_plane.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
      new_mat = Resources.Load("Bricks Textures/Bricks Texture 01/Bricks Texture 01") as Material;
      background_plane.GetComponent<Renderer>().material = new_mat;
      //background_plane.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
      background_plane.name = "Background";

    	knife = Resources.Load("Models/1_knife1/OBJ/Kitchenknife_lowpoly") as GameObject;
    	knife_a = Resources.Load("Models/1_knife1_a/OBJ/Kitchenknife_lowpoly") as GameObject;
    }

    void Start(){

    }

    // Update is called once per frame
    void Update(){
      //SpawnModels();
      //CaptureScreenshot();
      //DestroyAllModels();
      StartCoroutine(pacer());
    }
}
