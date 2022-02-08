using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NewScript : MonoBehaviour
{
    private GameObject knife, knife_a, background_plane;
    private static string textfile_path = "/home/daniel/unity_first_test/screenshots/boundingBoxes.txt";
    private static string img_path = "/home/daniel/unity_first_test/screenshots/";
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
      //GameObject knife_inst = Instantiate(knife, new Vector3(0.0f, 0.5f, 0.2f), Random.rotation);
      knife_inst.name = "knife";
      knife_inst.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
      float scale = Random.Range(1.0f, 4.0f);
      knife_inst.transform.localScale = Vector3.Scale(new Vector3 (scale, scale, scale), knife_inst.transform.localScale);
      bool visible = isVisible(knife_inst.transform.GetChild(0).gameObject);
      if(visible == true){
        Debug.Log("Knife is visible");
        Vector2[] bb = GetBoundingBoxVertices(knife_inst.transform.GetChild(0).gameObject);
        saveBoundingBox(bb);
      } else { Debug.Log("Knife is not visible"); }


      GameObject knife_inst_a = Instantiate(knife_a, GetRandomPose(), Random.rotation);
      //GameObject knife_inst_a = Instantiate(knife_a, new Vector3(0.0f, 0.5f, 0.2f), Random.rotation);
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
      } else { Debug.Log("Knife_a is not visible"); }
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
      string go_root_name = go.transform.root.name;
      //Debug.Log(go.transform.root.name);
      Mesh mesh = go.GetComponent<MeshFilter>().mesh;
      Vector3[] verts = mesh.vertices;
      for (int i = 0; i < verts.Length; i++) {
        verts[i] = go.transform.TransformPoint(verts[i]);
      }
      bool visible = true;
      foreach (Vector3 vert in verts){
        RaycastHit hit;
        Vector3 direction = vert - Camera.main.transform.position;
        if (Physics.Raycast(Camera.main.transform.position, direction, out hit)){
          //Debug.Log(go.transform.root.name + " occluded by " + hit.transform.root.name);
          if (hit.transform.root.name == go_root_name){
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

    private static void ExecPythonScript(){
      System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
      psi.FileName = "/usr/bin/python3";
      var script = "/home/daniel/unity_first_test/generate_background.py";
      psi.Arguments = $"\"{script}\"";
      psi.UseShellExecute = false;
      psi.CreateNoWindow = true;
      psi.RedirectStandardOutput = true;
      psi.RedirectStandardError = true;
      using(System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi)) {
        Debug.Log("Launched the python script");
      }
    }

    private void ChangeBackground(){
      ExecPythonScript();
      Texture2D tex = Resources.Load("Backgrounds/img") as Texture2D;
      //Debug.Log(tex);
      Material mat = new Material(Shader.Find("Unlit/Texture"));
      mat.mainTexture = (Texture)tex;
      background_plane.GetComponent<Renderer>().material = mat;
    }

    void Awake() {
      background_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
      background_plane.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
      float distance = Vector3.Distance(background_plane.transform.position, Camera.main.transform.position);
      Debug.Log("distance" + distance.ToString());
      var frustumHeight = 2.0f * distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
      Debug.Log("frustumHeight" + frustumHeight.ToString());
      float frustumWidth = frustumHeight * Camera.main.aspect;
      Debug.Log("frustumWidth" + frustumHeight.ToString());
      background_plane.transform.localScale = new Vector3(frustumWidth, 0.1f, frustumHeight/2f);
      background_plane.name = "Background";

      /*
      Vector3 temp = new Vector3(-frustumWidth/2.0f, 0.0f, -frustumHeight/2.0f);
      GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cube.transform.position = temp;
      Vector3 plane_scale = new Vector3(frustumWidth/2.0f, 0.01f, frustumHeight/2.0f);
      */


      //Material new_mat = Resources.Load("Bricks Textures/Bricks Texture 01/Bricks Texture 01") as Material;
      //background_plane.GetComponent<Renderer>().material = new_mat;
      //background_plane.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
      ChangeBackground();


    	knife = Resources.Load("Models/1_knife1/OBJ/Kitchenknife_lowpoly") as GameObject;
    	knife_a = Resources.Load("Models/1_knife1_a/OBJ/Kitchenknife_lowpoly") as GameObject;

      if(File.Exists(textfile_path)){
        File.Delete(textfile_path);
      }
    }

    void Start(){
      SpawnModels();
    }

    // Update is called once per frame
    void Update(){
      //Debug.Log(Camera.main.aspect);
      /*
      int frame_id = Time.frameCount;
      if (frame_id < 30){
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
