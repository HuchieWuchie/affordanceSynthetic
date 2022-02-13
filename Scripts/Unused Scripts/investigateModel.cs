using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class investigateModel : MonoBehaviour
{
    private struct pixel
    {
      public int x, y;
    }

    private struct boundingBox
    {
      public string name;
      public int min_x;
      public int min_y;
      public int max_x;
      public int max_y;
    }

    private void SetupCamera()
    {
      Camera.main.transform.Rotate(90, 0, 0);
      Camera.main.transform.position = new Vector3(0, 20, 0);
    }

    public Vector3 GetRandomPose()
    {
      // TODO - Use camera y position as the base for ViewportToWorldPoint
      Vector3 camera_pos = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 15));
      Vector3 world_pos = new Vector3(camera_pos.x, camera_pos.y, camera_pos.z);
      return world_pos;
    }

    private List<GameObject> spawnModels()
    {
      List<GameObject> models = new List<GameObject>();
      bool visible = true;
      GameObject spoon = Resources.Load("Models/6_spoon_5/source/model") as GameObject;
      GameObject shears = Resources.Load("Models/4_shears_2/source/model") as GameObject;

      GameObject shears_inst = Instantiate(shears, GetRandomPose(), Random.rotation);
      shears_inst.name = "shears_inst";
      for (int i = 0; i < shears_inst.transform.childCount; i++)
      {
        GameObject childObj = shears_inst.transform.GetChild(i).gameObject;
        childObj.AddComponent<MeshCollider>();
        //Debug.Log("ChildObj " + childObj.GetComponent<Collider>().bounds.ToString());
      }
      models.Add(shears_inst);


      GameObject spoon_inst_1 = Instantiate(spoon, GetRandomPose(), Random.rotation);
      spoon_inst_1.name = "spoon_inst_1";
      //spoon_inst_1.transform.GetChild(0).gameObject.AddComponent<MeshFilter>();
      spoon_inst_1.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
      //visible = isVisible(spoon_inst_1.transform.GetChild(0).gameObject);
      if (visible == false){
        Debug.Log("spoon_inst_1 isnt visible");
      }
      models.Add(spoon_inst_1);


      GameObject spoon_inst_2 = Instantiate(spoon, GetRandomPose(), Quaternion.identity);
      spoon_inst_2.name = "spoon_inst_2";
      //spoon_inst_2.transform.GetChild(0).gameObject.AddComponent<MeshFilter>();
      spoon_inst_2.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
      //visible = isVisible(spoon_inst_2.transform.GetChild(0).gameObject);
      if (visible == false){
        Debug.Log("spoon_inst_2 isnt visible");
      }
      models.Add(spoon_inst_2);

      return models;


      //GameObject knife_inst = Instantiate(knife, new Vector3(0.0f, 2.5f, 0.0f), Random.rotation);
      /*
      spoon_inst_1.name = "spoon_inst_1";
      knife_inst.transform.GetChild(0).gameObject.AddComponent<MeshFilter>();
      scale = Random.Range(1.0f, 4.0f);
      spoon_inst_1.transform.localScale = Vector3.Scale(new Vector3 (scale, scale, scale), knife_inst.transform.localScale);
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
      */
    }

    private bool doIntersect(Bounds[] bounds)
    {
        if (bounds[0].Intersects(bounds[1])){
          return true;
        }
      return false;
    }

    private bool isVisible(GameObject go)
    {
      bool visible = true;
      //Debug.Log("Trigger On : " + m_ObjectCollider.isTrigger);
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
            //Debug.Log(go.transform.root.name + " occluded by " + hit.transform.root.name);
          }
        } else { Debug.DrawRay(Camera.main.transform.position, direction, Color.green, 240.0f); }
      }
      return visible;
    }

    public List<GameObject> spawnDistractors()
    {
      //Camera camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();
      System.Random rnd = new System.Random();
      //int numberOfDistractotsToSpawn = ;
      List<int> distractorsToSpawn = new List<int>();
      List<GameObject> distractors = new List<GameObject>();

      for (int i = 0; i < 15; i++){
        distractorsToSpawn.Add((int)rnd.Next(5));
      }

      //print(distractorsToSpawn);
      int id = 0;
      foreach (int distractor_id in distractorsToSpawn){
        float scale_x = UnityEngine.Random.Range(0.5f,3f);
        float scale_y = UnityEngine.Random.Range(0.5f,3f);
        float scale_z = UnityEngine.Random.Range(0.5f,3f);
        Vector3 rand_scale = new Vector3 (scale_x, scale_y, scale_z);
        Color rand_color = new Color (UnityEngine.Random.Range(0F, 1F),
                                      UnityEngine.Random.Range(0F, 1F),
                                      UnityEngine.Random.Range(0F, 1F),
                                      UnityEngine.Random.Range(0F, 1F));

        switch (distractor_id)
        {
          case 0:
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.name = "cube_" + id.ToString();
            cube.transform.position =GetRandomPose();
            cube.transform.rotation = UnityEngine.Random.rotation;
            cube.transform.localScale = rand_scale;
            cube.GetComponent<MeshRenderer>().material.color = rand_color;
            distractors.Add(cube);
            break;
          case 1:
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.name = "sphere_" + id.ToString();
            sphere.transform.position =GetRandomPose();
            sphere.transform.rotation = UnityEngine.Random.rotation;
            sphere.transform.localScale = rand_scale;
            sphere.GetComponent<MeshRenderer>().material.color = rand_color;
            distractors.Add(sphere);
            break;
          case 2:
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.name = "capsule_" + id.ToString();
            capsule.transform.position =GetRandomPose();
            capsule.transform.rotation = UnityEngine.Random.rotation;
            capsule.transform.localScale = rand_scale;
            capsule.GetComponent<MeshRenderer>().material.color = rand_color;
            distractors.Add(capsule);
            break;
          case 4:
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.name = "cylinder_" + id.ToString();
            cylinder.transform.position =GetRandomPose();
            cylinder.transform.rotation = UnityEngine.Random.rotation;
            cylinder.transform.localScale = rand_scale;
            cylinder.GetComponent<MeshRenderer>().material.color = rand_color;
            distractors.Add(cylinder);
            break;
        }
        id++;
      }

      return distractors;
    }

    private List<int> checkForCollision(List<GameObject> objects){
      List<Collider> colliders = new List<Collider>();
      List<int> toDestroy = new List<int>() {0,1};
      foreach (GameObject go in objects)
      {
        if (go.TryGetComponent<Collider>(out Collider goC))
        {
          colliders.Add(goC);
        }
        else
        {
          for (int i = 0; i < go.transform.childCount; i++)
          {
              colliders.Add(go.transform.GetChild(i).gameObject.GetComponent<Collider>());
          }
        }
      }
      Debug.Log("Colliders.Count " + colliders.Count.ToString());

      for (int i=0; i < colliders.Count; i++){
        string go_name = colliders[i].gameObject.transform.root.name;
        GameObject go = colliders[i].gameObject;
        Bounds bounds = colliders[i].bounds;
        for (int j = i; j < colliders.Count; j++){
          if (bounds.Intersects(colliders[j].bounds)){
            if (go_name == colliders[j].gameObject.transform.root.name){
              continue;
            } else {
              go.transform.position = new Vector3(0f, 0f, 100f);
              Debug.Log("Occlusion");
            }
          }
        }
      }


      //Debug.Log("Colliders Count " + colliders.Count.ToString());
      return toDestroy;
    }

    private Dictionary<pixel, int> buildImg()
    {
      Dictionary<pixel, int> img = new Dictionary<pixel, int>();
      //Hashtable img = new Hashtable(); Screen.height Screen.width
      pixel px = new pixel();
      for (int i = 0; i < Screen.height; i++){
        px.x = i;
        for (int j = 0; j < Screen.width; j++){
          px.y = j;
          img.Add(px, 0);
        }
      }

      return img;
    }

    private static boundingBox GetBoundingBox(GameObject go)
    {
        Vector3[] verts = new Vector3[0];
        Camera camMask = GameObject.Find("Main Camera").GetComponent<Camera>();

        // grap mesh filter of parent if available otherwise of children
        if (go.TryGetComponent<MeshFilter>(out MeshFilter mMeshF))
        {
            verts = mMeshF.mesh.vertices;
        }
        else
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                //print("We are inside child mesh filter");
                GameObject childObj = go.transform.GetChild(i).gameObject;
                Mesh mesh = childObj.GetComponent<MeshFilter>().mesh;
                verts = verts.Concat(mesh.vertices).ToArray();
            }

        }


        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = camMask.WorldToScreenPoint(go.transform.TransformPoint(verts[i]));
            //make sure we dont go off screen
            if (verts[i].x < 0 || verts[i].x > Screen.width || verts[i].y < 0 || verts[i].y > Screen.height)
            {
                verts[i] = verts[0];
            }
        }
        Vector2 min = verts[0];
        Vector2 max = verts[0];
        foreach (Vector2 v in verts)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        min.y = Screen.height - min.y;
        max.y = Screen.height - max.y;
        if (min.y > max.y)
        {
            float temp = max.y;
            max.y = min.y;
            min.y = temp;
        }

        boundingBox bb = new boundingBox();
        bb.name = go.transform.root.name;
        bb.min_x = (int)min.x;
        bb.min_y = (int)min.y;
        bb.max_x = (int)max.x;
        bb.max_y = (int)max.y;

        return bb;
    }

    private static void saveBoundingBoxes(List<boundingBox> boxes){
      ScreenCapture.CaptureScreenshot("/home/daniel/unity_first_test/temp/bb.png");
      string bboxFile = "/home/daniel/unity_first_test/temp/bb.txt";

      if (File.Exists(bboxFile)) {
          File.Delete(bboxFile);
      }

      foreach (boundingBox box in boxes) {
        string serializedData = box.min_x.ToString() + " "
        + box.min_y.ToString() + " "
        + box.max_x.ToString() + " "
        + box.max_y.ToString() + "\n";
        using (StreamWriter writer = new StreamWriter(bboxFile, true))
                {
                    writer.Write(serializedData);
                }
      }


    }

    private static bool doOverlap(pixel l1, pixel r1, pixel l2, pixel r2)
    {
        // If one rectangle is on left side of other
        if (l1.x >= r2.x || l2.x >= r1.x)
        {
            //Debug.Log("IF1");
            return false;
        }

        // If one rectangle is above other
        if (r1.y >= l2.y || r2.y >= l1.y)
        {
            //Debug.Log("IF2");
            return false;
        }
        return true;
    }

    private void RemoveOcclusion(List<boundingBox> boxes)
    {
      Debug.Log("RemoveOcclusion");
      int loop_counter = 0;
      pixel min_bb1 = new pixel(),
            max_bb1 = new pixel(),
            min_bb2 = new pixel(),
            max_bb2 = new pixel();

      for (int i = 0; i < boxes.Count; i++){
        min_bb1.x = boxes[i].min_x;
        min_bb1.y = boxes[i].min_y;
        max_bb1.x = boxes[i].max_x;
        max_bb1.y = boxes[i].max_y;
        for (int j = i; j < boxes.Count; j++){
          loop_counter++;
          min_bb2.x = boxes[j].min_x;
          min_bb2.y = boxes[j].min_y;
          max_bb2.x = boxes[j].max_x;
          max_bb2.y = boxes[j].max_y;

          bool overlap = doOverlap(min_bb1, max_bb1, min_bb2, max_bb2);
          Debug.Log("Overlap " + overlap.ToString());
          overlap = doOverlap(min_bb2, max_bb2, min_bb1, max_bb1);
          Debug.Log("Opposite Overlap " + overlap.ToString());

          /*
          if (min_bb1.x >= max_bb2.x || min_bb2.x >= max_bb1.x) {
              Debug.Log("Continue");
              continue;
          } else if (max_bb1.y >= min_bb2.y || max_bb2.y >= min_bb1.y){
              Debug.Log("Continue");
              continue;
          } else {
            Debug.Log("Occlusion detected");
            GameObject temp_go = GameObject.Find(boxes[j].name);
            temp_go.transform.position = new Vector3 (0f, 0f, 100f);
            boxes.RemoveAt(j);
          }
          */
        }
      }
      Debug.Log("loop_counter " + loop_counter.ToString());
    }

    private static void doOverlap_v2(List<boundingBox> boxes){
      int loop_counter = 0;
      List<int> range_x_bb1 = new List<int>(), range_y_bb1 = new List<int>(),
            range_x_bb2 = new List<int>(), range_y_bb2 = new List<int>();



      for (int i = 0; i < boxes.Count; i++){
        //range_x_bb1.Clear();
        //range_y_bb1.Clear();
        Debug.Log("For loop I");
        //range_x_bb1 = Enumerable.Range(boxes[i].min_x, boxes[i].max_x).ToList();
        //range_y_bb1 = Enumerable.Range(boxes[i].min_y, boxes[i].max_y).ToList();
          for (int j = 0; j < boxes.Count; j++){
            Debug.Log("For loop J");
            if (j == i) {
              continue;
            }
            Debug.Log("i " + i.ToString() + " j " + j.ToString());
            //range_x_bb2.Clear();
            //range_y_bb2.Clear();
            loop_counter++;
            Debug.Log("range");
            //range_x_bb2 = Enumerable.Range(boxes[j].min_x, boxes[j].max_x).ToList();
            //range_y_bb2 = Enumerable.Range(boxes[j].min_y, boxes[j].max_y).ToList();
            Debug.Log("bool");
            //bool x_overlap = range_x_bb1.First() < range_x_bb2.Last() && range_x_bb2.First() < range_x_bb1.Last();
            bool x_overlap = boxes[i].min_x < boxes[j].max_x && boxes[j].min_x < boxes[i].max_x;
            //bool y_overlap = range_y_bb1.First() < range_y_bb2.Last() && range_y_bb2.First() < range_y_bb1.Last();
            bool y_overlap = boxes[i].min_y < boxes[j].max_y && boxes[j].min_y < boxes[i].max_y;
            if (x_overlap && y_overlap){
              Debug.Log("if");
              GameObject go = GameObject.Find(boxes[j].name);
              float go_z_coordinate = (float) go.transform.position.z;
              if (go_z_coordinate == 100f) {
                Debug.Log("Already moved");
                continue;
              } else {
                go.transform.position = new Vector3(0f,0f,100f);
                boundingBox new_bb = new boundingBox();
                Debug.Log("New bb");
                new_bb = GetBoundingBox(go);
                Debug.Log("Rewriting");
                boxes[j] = new_bb;
                //boxes[j].min_x = new_bb.min_x;
                //boxes[j].min_y = new_bb.min_y;
                //boxes[j].max_x = new_bb.max_x;
                //boxes[j].max_y = new_bb.max_y;
              }
              //boxes.RemoveAt(j);
              //boxes.Remove(boxes[j])
            } else {
              continue;
            }
            //Debug.Log(loop_counter.ToString() + " x_overlap " + x_overlap.ToString());
            //Debug.Log(loop_counter.ToString() + " y_overlap " + y_overlap.ToString());
            //bool overlap = a.start < b.end && b.start < a.end;
        }
      }


    }

    // Start is called before the first frame update
    void Start()
    {
      SetupCamera();
      //Dictionary<pixel, int> img = buildImg();
      //Debug.Log("img.Count " + img.Count.ToString());
      List<GameObject> models = spawnModels();
      List<boundingBox> boxes = new List<boundingBox>();
      foreach(GameObject go in models){
        boxes.Add(GetBoundingBox(go));
      }
      //Debug.Log("Boxes.Count " + boxes.Count.ToString());
      //RemoveOcclusion(boxes);
      //print(boxes[0].name);
      //boxes.Clear();

      List<GameObject> distractors = spawnDistractors();
      foreach(GameObject go in distractors){
        boxes.Add(GetBoundingBox(go));
      }
      Debug.Log("Boxes.Count " + boxes.Count.ToString());
      saveBoundingBoxes(boxes);
      doOverlap_v2(boxes);
      Debug.Log("Boxes.Count " + boxes.Count.ToString());
      //RemoveOcclusion(boxes);

      //List<GameObject> all_gameobjects = models.Concat(distractors).ToList();
      //List<int> toRemove = checkForCollision(all_gameobjects);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
