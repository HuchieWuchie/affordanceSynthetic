using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NewScript : MonoBehaviour
{
    private static int background_index = 0;
    private GameObject knife, knife_a, background_plane;
    private static string textfile_path = "/home/daniel/unity_first_test/screenshots/boundingBoxes.txt";
    //private static string img_path = "/home/daniel/unity_first_test/screenshots/";
    //private Camera main_cam;

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
      // TODO - Use camera y position as the base for ViewportToWorldPoint
      Vector3 camera_pos = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 15));
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
      float scale;
      bool visible;

      GameObject knife_inst = Instantiate(knife, GetRandomPose(), Random.rotation);
      //GameObject knife_inst = Instantiate(knife, new Vector3(0.0f, 2.5f, 0.0f), Random.rotation);
      knife_inst.name = "knife";
      knife_inst.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
      scale = Random.Range(1.0f, 4.0f);
      knife_inst.transform.localScale = Vector3.Scale(new Vector3 (scale, scale, scale), knife_inst.transform.localScale);
      Physics.SyncTransforms();
      visible = isVisible(knife_inst.transform.GetChild(0).gameObject);
      if(visible == true){
        Debug.Log("Knife is visible");
        Vector2[] bb = GetBoundingBoxVertices(knife_inst.transform.GetChild(0).gameObject);
        saveBoundingBox(bb);
      } else {
        Debug.Log("Knife is not visible. Deleting");
        Destroy(knife_inst);
      }


      GameObject knife_inst_a = Instantiate(knife_a, GetRandomPose(), Random.rotation);
      //GameObject knife_inst_a = Instantiate(knife_a, new Vector3(5.0f, 2.5f, 0.0f), Random.rotation);
      knife_inst_a.name = "knife_a";
      knife_inst_a.transform.GetChild(0).gameObject.name = "default_a";
      knife_inst_a.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
      scale = Random.Range(1.0f, 4.0f);
      knife_inst_a.transform.localScale = Vector3.Scale(new Vector3 (scale, scale, scale), knife_inst_a.transform.localScale);
      visible = isVisible(knife_inst_a.transform.GetChild(0).gameObject);
      if(visible == true){
        Debug.Log("Knife_a is visible");
        Vector2[] bb = GetBoundingBoxVertices(knife_inst_a.transform.GetChild(0).gameObject);
        saveBoundingBox(bb);
      } else {
        Debug.Log("Knife_a is not visible. Deleting.");
        Destroy(knife_inst_a);
      }


    }


    void SaveCameraRGB(string filename) {
        var cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        int width = cam.pixelWidth;
        int height = cam.pixelHeight;
        int depth = 24;
        bool needsRescale = false;
        bool supportsAntialiasing = false;

        var format = RenderTextureFormat.Default;
        var readWrite = RenderTextureReadWrite.Default;
        var antiAliasing = (supportsAntialiasing) ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;

        var finalRT = RenderTexture.GetTemporary(width, height, depth, format, readWrite, antiAliasing);
        var renderRT = (!needsRescale) ? finalRT :
            RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, depth, format, readWrite, antiAliasing);
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        var prevActiveRT = RenderTexture.active;
        var prevCameraRT = cam.targetTexture;

        // render to offscreen texture (readonly from CPU side)
        RenderTexture.active = renderRT;
        var originalTargetTexture = cam.targetTexture;
        cam.targetTexture = renderRT;

        cam.Render();

        // read offsreen texture contents into the CPU readable texture
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();

        // encode texture into PNG
        var bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filename, bytes);
        cam.targetTexture = originalTargetTexture;
    }

    private bool isVisible(GameObject go){
      bool visible = true;
      string go_root_name = go.transform.root.name;
      //Debug.Log(go.transform.root.name);
      Mesh mesh = go.GetComponent<MeshFilter>().mesh;
      Vector3[] verts = mesh.vertices;
      for (int i = 0; i < verts.Length; i++) {
        verts[i] = go.transform.TransformPoint(verts[i]);
        Vector2 vert_camera_pos = Camera.main.WorldToViewportPoint(verts[i]);
        if (vert_camera_pos.x > 1.0f || vert_camera_pos.y > 1.0f || vert_camera_pos.x < 0.0f || vert_camera_pos.y < 0.0f){
          visible = false;
          return visible;
        }
      }
      foreach (Vector3 vert in verts){
        RaycastHit hit;
        Vector3 direction = vert - Camera.main.transform.position;
        if (Physics.Raycast(Camera.main.transform.position, direction, out hit)){
          //Debug.Log(go.transform.root.name + " occluded by " + hit.transform.root.name);
          if (hit.transform.root.name == go_root_name){
            Debug.DrawRay(Camera.main.transform.position, direction, Color.blue, 240.0f);
            continue;
          } else {
            visible = false;
            Debug.DrawRay(Camera.main.transform.position, direction, Color.red, 240.0f);
            Debug.Log(go.transform.root.name + " occluded by " + hit.transform.root.name);
          }
        } else { Debug.DrawRay(Camera.main.transform.position, direction, Color.green, 240.0f); }
      }
      return visible;
    }


    private void ChangeBackgroundTexture(){
      int idx = background_index;
      string to_load = "Backgrounds/img_" + idx.ToString();
      Texture2D tex = Resources.Load(to_load) as Texture2D;
      Material mat = new Material(Shader.Find("Unlit/Texture"));
      mat.mainTexture = (Texture)tex;
      background_plane.GetComponent<Renderer>().material = mat;
      background_index++;
    }

    private void SpawnBackground(){
      background_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
      background_plane.transform.position = new Vector3(0.0f, -100.0f, 0.0f);
      // TODO - Get the distance from the camera pose
      float distance = 120f;
      float frustumHeight = 2.0f * distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
      float frustumWidth = frustumHeight * Camera.main.aspect;
      //Renderer rend = background_plane.GetComponent<Renderer>();
      Vector3 plane_bounds = background_plane.GetComponent<Renderer>().bounds.extents;
      background_plane.transform.localScale = new Vector3(frustumWidth/plane_bounds.x/2f, 0.0f, frustumHeight/plane_bounds.z/2f);
      background_plane.name = "Background";
      Physics.SyncTransforms();
      Vector3 plane_collider_bounds = background_plane.GetComponent<Collider>().bounds.extents;
      Debug.Log("Collider bounds " + plane_collider_bounds.ToString());
    }

    private void SetupCamera(){
      Camera.main.transform.Rotate(90, 0, 0);
    	Camera.main.transform.position = new Vector3(0, 20, 0);
    }

    void Awake() {
      SetupCamera();
      SpawnBackground();

    	knife = Resources.Load("Models/1_knife1/OBJ/Kitchenknife_lowpoly") as GameObject;
    	knife_a = Resources.Load("Models/1_knife1_a/OBJ/Kitchenknife_lowpoly") as GameObject;

      if(File.Exists(textfile_path)){
        File.Delete(textfile_path);
      }
    }

    void Start(){
      //ChangeBackgroundTexture();
      SpawnModels();
    }

    // Update is called once per frame
    void Update(){
      /*
      ChangeBackgroundTexture();
      SpawnModels();
      int frame_id = Time.frameCount;
      if (frame_id < 50){
        string img_filename = "img_" + Time.frameCount.ToString() + ".png";
        string img_full_path = img_path + img_filename;

        SaveCameraRGB(img_full_path);
        DestroyAllModels();
      }
      else {
        Debug.Log("Frame_id > 30");
      }
      */


    }
}
