using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;


public class scenemanager : MonoBehaviour
{

    string datasetDir;
    string trainDir;
    string trainRGBDir;
    string trainBboxDir;
    string trainMaskDir;

    string testDir;
    string testRGBDir;
    string testBboxDir;
    string testMaskDir;

    List<GameObject> rgbObjects;
    List<GameObject> maskObjects;
    List<GameObject> lights;

    List<datasetModel> modelList;

    Camera camRGB;
    Camera camAnnotation;

    GameObject backgroundPlane;
    Texture2D backgroundTexture;
    Material backgroundMaterial;
    Renderer backgroundRenderer;

    RenderTexture renderTex;
    Texture2D screenshotTex;

    public class datasetModel
    {
        public GameObject prefabRGB;
        public GameObject prefab;
        public int label;
        public int objectVersion;
        public string name;
        public string textureRGBPath;
        public string textureMaskPath;
        public Texture textureRGB;
        public Texture textureMask;
        public Vector3 defaultScale;
        public string modelPath;

    }

    public List<datasetModel> GetDatasetModels(string rootDir)
    {
        List<datasetModel> mList = new List<datasetModel>();

        string rootDirUnity = Path.Combine("Assets" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar, rootDir);
        string[] dirs = Directory.GetDirectories(rootDirUnity);
        foreach (string subdir in dirs)
        {
            datasetModel m = new datasetModel();
            string modelDir = Path.GetFileName(subdir);
            string[] subDirInfo = modelDir.Split('_');
            m.label = Int32.Parse(subDirInfo[0]);
            m.objectVersion = Int32.Parse(subDirInfo[2]);
            m.name = subDirInfo[1];

            string modelDirPath = Path.Combine(rootDir, modelDir);
            string textureDirPath = Path.Combine(modelDirPath, "textures" + Path.DirectorySeparatorChar);
            m.textureRGBPath = Path.Combine(textureDirPath, "rgb");
            m.textureMaskPath = Path.Combine(textureDirPath, "annotated");

            m.textureRGB = Resources.Load(m.textureRGBPath) as Texture;
            m.textureMask = Resources.Load(m.textureMaskPath) as Texture;

            m.modelPath = Path.Combine(modelDirPath, "source" + Path.DirectorySeparatorChar + "model");
            m.prefab = Resources.Load(m.modelPath) as GameObject;
            mList.Add(m);

        }

        return mList;
    }
    public Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(12.5f, 17.5f)));
        return world_pos;
    }

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }

    void ScaleToMaxExtent(float maxExtent, GameObject rgbInstance, GameObject maskInstance)
    {
        float[] sizeArray = { 0, 0, 0 };
        if (rgbInstance.TryGetComponent<Renderer>(out Renderer mRendRGB))
        {
            sizeArray[0] = mRendRGB.bounds.extents.x;
            sizeArray[1] = mRendRGB.bounds.extents.y;
            sizeArray[2] = mRendRGB.bounds.extents.z;
        }
        else
        {
            Renderer[] childRendsRGB = rgbInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer childRendRGB in childRendsRGB)
            {
                if (childRendRGB.bounds.extents.x > sizeArray[0])
                {
                    sizeArray[0] = childRendRGB.bounds.extents.x;
                }
                if (childRendRGB.bounds.extents.y > sizeArray[1])
                {
                    sizeArray[1] = childRendRGB.bounds.extents.y;
                }
                if (childRendRGB.bounds.extents.z > sizeArray[2])
                {
                    sizeArray[2] = childRendRGB.bounds.extents.z;
                }
            }
        }

        float scale = maxExtent / Mathf.Max(sizeArray);
        rgbInstance.transform.localScale = new Vector3(rgbInstance.transform.localScale.x * scale, rgbInstance.transform.localScale.y * scale, rgbInstance.transform.localScale.z * scale);
        maskInstance.transform.localScale = new Vector3(maskInstance.transform.localScale.x * scale, maskInstance.transform.localScale.y * scale, maskInstance.transform.localScale.z * scale);


    }

    public (List<GameObject>, List<GameObject>) InstantiateModels(List<datasetModel> modelList, int[] idxList){

        List<GameObject> rgbObjects = new List<GameObject>();
        List<GameObject> maskObjects = new List<GameObject>();

        Vector3 position = new Vector3(0.0f, 0.5f, -0.5f);
        Quaternion rotation = new Quaternion(1.0f, 1.0f, 0.0f, 1.0f);
        int identifier = 0;

        foreach (int idx in idxList)
        {

            bool randomTexture = true;
            if (UnityEngine.Random.Range(0f, 1f) < 0.2f)
            {
                randomTexture = false;
            }

            // RGB image /////////////////////////////////////////////////////////
            GameObject modelRGB = Instantiate(modelList[idx].prefab, position, rotation) as GameObject;
            /*
            GameObject modelRGB = new GameObject();
            try
            {
                modelRGB = Instantiate(modelList[idx].prefab, position, rotation) as GameObject;
            }
            catch(ArgumentException e)
            {
                print(modelList[idx].name);
                print(modelList[idx].modelPath);
            }
            */

            if (modelRGB.TryGetComponent<Renderer>(out Renderer mRendRGB))
            {
                if(randomTexture == true)
                {
                    Texture2D tex = new Texture2D(128, 128);
                    tex = RandomizeTexture(ref tex);
                    mRendRGB.material.mainTexture = tex;
                    Destroy(tex);
                }
                else
                {
                    mRendRGB.material.mainTexture = modelList[idx].textureRGB;
                }

            }
            else
            {
                Renderer[] childRendsRGB = modelRGB.GetComponentsInChildren<Renderer>();
                foreach (Renderer childRendRGB in childRendsRGB)
                {
                    if(randomTexture == true)
                    {
                        Texture2D tex = new Texture2D(96, 96);
                        tex = RandomizeTexture(ref tex);
                        childRendRGB.material.mainTexture = tex;
                        Destroy(tex);
                    }
                    else
                    {
                        childRendRGB.material.mainTexture = modelList[idx].textureRGB;
                    }

                }
            }


            /// Transfer to rgb camera layer
            modelRGB.layer = LayerMask.NameToLayer("Default");

            // Mask image ///////////////////////////////////////////////////////
            GameObject modelMask = Instantiate(modelList[idx].prefab, position, rotation) as GameObject;

            /// Set texture

            if (modelMask.TryGetComponent<Renderer>(out Renderer mRend))
            {
                mRend.material.mainTexture = modelList[idx].textureMask;
                //UnityEngine.Debug.Log("TextureMask " + modelList[idx].textureMask.ToString());
                mRend.material.shader = Shader.Find("Unlit/Texture");
                if (modelMask.GetComponent<MeshFilter>() == null)
                {
                    modelMask.AddComponent<MeshFilter>();
                }
                
                modelMask.AddComponent<MeshCollider>();
            }
            else
            {
                Renderer[] childRends = modelMask.GetComponentsInChildren<Renderer>();
                foreach (Renderer childRend in childRends)
                {
                    childRend.material.mainTexture = modelList[idx].textureMask;
                    //UnityEngine.Debug.Log("TextureMask " + modelList[idx].textureMask.ToString());
                    childRend.material.shader = Shader.Find("Unlit/Texture");
                }
                for (int i = 0; i < modelMask.transform.childCount; i++)
                {
                    GameObject childObj = modelMask.transform.GetChild(i).gameObject;
                    if (childObj.gameObject.GetComponent<MeshFilter>() == null)
                    {
                        childObj.gameObject.AddComponent<MeshFilter>();
                    }
                    childObj.gameObject.AddComponent<MeshCollider>();
                    childObj.layer = LayerMask.NameToLayer("Mask");
                }

            }
            modelMask.transform.name = modelList[idx].label.ToString() + "_" + "model_mask" + identifier.ToString();
            modelRGB.transform.name = modelList[idx].label.ToString() + "_" + "model_RGB" + identifier.ToString();

            identifier++;


            /// Transfer to mask camera layer
            modelMask.layer = LayerMask.NameToLayer("Mask");

            maskObjects.Add(modelMask);
            rgbObjects.Add(modelRGB);

        }

        return (rgbObjects, maskObjects);

    }

    void SaveMask(Camera cam, string filename)
    {
        /*
        int width = cam.pixelWidth;
        int height = cam.pixelHeight;
        int depth = 8;
        bool needsRescale = false;
        bool supportsAntialiasing = false;

        var format = RenderTextureFormat.Default;
        var readWrite = RenderTextureReadWrite.Default;
        var antiAliasing = (supportsAntialiasing) ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;

        var finalRT =
            RenderTexture.GetTemporary(width, height, depth, format, readWrite, antiAliasing);
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
        var bytes = tex.EncodeToTGA();
        File.WriteAllBytes(filename, bytes);

        cam.targetTexture = originalTargetTexture;
        Destroy(tex);
        //Destroy(renderRT);
        //Destroy(finalRT);
        */

        RenderTexture rt = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 8);
        cam.targetTexture = rt;

        cam.Render();
        RenderTexture.active = rt;
        screenshotTex.Resize(cam.pixelWidth, cam.pixelHeight);
        screenshotTex.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);
        screenshotTex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(rt);

        rt = null;
        Destroy(rt);

        byte[] bytes = screenshotTex.EncodeToTGA();
        System.IO.File.WriteAllBytes(filename, bytes);
    }

    void SaveCameraRGB(Camera cam, string filename)
    {

        RenderTexture rt = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 24);
        cam.targetTexture = rt;

        cam.Render();
        RenderTexture.active = rt;
        screenshotTex.Resize(cam.pixelWidth, cam.pixelHeight);
        screenshotTex.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(rt);

        rt = null;
        Destroy(rt);

        byte[] bytes = screenshotTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);
    }

    void captureFrames(int frameID, string rgbFile, string maskFile)
    {

        //var camRGB = GameObject.Find("RGBCamera").GetComponent<Camera>();
        SaveCameraRGB(camRGB, rgbFile);

        //var camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();
        SaveMask(camAnnotation, maskFile);

    }

    int GetNumberOfInstances(int min, int max)
    {
        System.Random rand = new System.Random();
        min = Math.Max(1, min);

        return (rand.Next(min, max));
    }

    int[] GetObjectIndexes(int noInstances, int noObjects)
    {
        int[] idxList = new int[noInstances];
        for (int i = 0; i < idxList.Length; i += 1)
        {
            System.Random rand = new System.Random();
            //idxList[i] = rand.Next(0, noObjects - 1);
            idxList[i] = (int) UnityEngine.Random.Range(0, noObjects - 1);
        }

        return (idxList);
    }

    private void DestroyAllModels(List<GameObject> instances)
    {
        foreach (GameObject go in instances)
        {
            Destroy(go);
        }
    }

    public List<GameObject> InstantiateLights(int maxNumberLights)
    {
        System.Random rand = new System.Random();
        int numLights = rand.Next(1, maxNumberLights);
        List<GameObject> lights = new List<GameObject>();
        Vector3 position = new Vector3(0.0f, 0.5f, -0.5f);
        Quaternion rotation = new Quaternion(-1.0f, 1.0f, 0.0f, 1.0f);

        for (int num = 0; num < numLights; num += 1)
        {
            // Make a game object
            GameObject lightGameObject = new GameObject("light" + num.ToString());

            // Add the light component
            Light lightComp = lightGameObject.AddComponent<Light>();
            lightComp.type = LightType.Directional;

            //GameObject lightInst = Instantiate(lightGameObject, position, rotation);

            lights.Add(lightGameObject);
            //Destroy(lightGameObject);
        }
        return (lights);
    }

    GameObject ResizeBackgroundToCameraView(ref GameObject background_plane, Camera cam)
    {

        // TODO - Get the distance from the camera pose

        float distance = 120f;
        float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * cam.aspect;

        //Vector3 plane_bounds = background_plane.GetComponent<Renderer>().bounds.extents;
        //background_plane.transform.localScale = new Vector3(frustumWidth / plane_bounds.x / 2f, 0.0f, frustumHeight / plane_bounds.z / 2f);
        background_plane.transform.localScale = new Vector3(frustumWidth / 5.0f / 2f, 0.0f, frustumHeight / 5.0f / 2f);
        Physics.SyncTransforms();

        return (background_plane);
    }

    Texture2D RandomizeTexture(ref Texture2D randomizedTexture)
    {
        System.Random seed = new System.Random();
        int numFunctions = 5;

        int[] functionChoice = new int[3];
        Vector2[] origin = new Vector2[3];
        Vector2[] scale = new Vector2[3];
        float[] multiplier = new float[3];

        for (int i = 0; i < 3; i++)
        {
            functionChoice[i] = UnityEngine.Random.Range(0, numFunctions);
            origin[i] = new Vector2(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
            scale[i] = new Vector2(UnityEngine.Random.Range(0.05f, 10f), UnityEngine.Random.Range(0.05f, 10f));
            multiplier[i] = UnityEngine.Random.Range(0.5f, 1.0f);
        }

        int width = randomizedTexture.width;
        int height = randomizedTexture.height;
        float width_f = (float)width;
        float height_f = (float)height;
        //Texture2D randomizedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        //randomizedTexture.width = width;
        //randomizedTexture.height = height;
        //randomizedTexture.format = TextureFormat.RGB24;
        Color[] pixels = new Color[width * height];

        // For each pixel in the texture...
        for (float y = 0; y < height; y += 1)
        {
            for (float x = 0; x < width; x += 1)
            {
                Color pixelVal = new Color();
                pixelVal[3] = 1f; // set the alpha channel to 1

                // for each color channel
                for (int p = 0; p < 3; p++)
                {
                    if(functionChoice[p] == 0)
                    {
                        // perlin noise
                        float val = Mathf.PerlinNoise(origin[p].x + (x / width_f) * scale[p].x, origin[p].y + y / height_f * scale[p].y);
                        pixelVal[p] = val;

                    }
                    else if (functionChoice[p] == 1)
                    {
                        // sinusoid / cosine noise
                        float valx = 0.5f * Mathf.Sin(((origin[p].x + x / width_f) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].x);
                        float valy = 0.5f * Mathf.Cos(((origin[p].y + y / height_f) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].y);
                        float val = valx + valy;
                        pixelVal[p] = val;
                    }

                    else if (functionChoice[p] == 2)
                    {
                        float val = (float)seed.NextDouble();
                        pixelVal[p] = val;
                    }

                    else if (functionChoice[p] == 3)
                    {
                        // sinusoid noise
                        float valx = 0.5f * Mathf.Sin(((origin[p].x + x / width_f) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].x);
                        float valy = 0.5f * Mathf.Sin(((origin[p].y + y / height_f) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].y);
                    }
                    else if (functionChoice[p] == 4)
                    {
                        // checkerboard pattern
                        float checker_widthX = (0.5f * width_f) / scale[p].x;
                        float checker_widthY = (0.5f * width_f) / scale[p].y;
                        float xNorm = (x % (checker_widthX * 2.0f)) / (checker_widthX * 2.0f);
                        float yNorm = (y % (checker_widthY * 2.0f)) / (checker_widthY * 2.0f);
                        //float offset = bin_discretizer * origin[p].x; // if offset = 0 start checkerboard pattern in top left corner.

                        float valX = 1;
                        if (xNorm < 0.5f)
                        {
                            valX = 0;
                        }
                        float valY = 1;
                        if (yNorm < 0.5f)
                        {
                            valY = 0f;
                        }
                        float val = 0.5f;
                        if (valX == valY)
                        {
                            val = 1f;
                        }
                        pixelVal[p] = multiplier[p] * val;
                    }
                }

                pixels[(int)y * width + (int)x] = pixelVal;

            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        randomizedTexture.SetPixels(pixels);
        randomizedTexture.Apply();
        //texIn = randomizedTexture;
        //Destroy(randomizedTexture);

        //return (texIn);
         return (randomizedTexture);

    }

    Vector3[] GetMeshVertices(GameObject go)
    {
        Vector3[] verts = new Vector3[0];

        // grap mesh filter of parent if available otherwise of children
        if (go.TryGetComponent<MeshFilter>(out MeshFilter mMeshF))
        {
            Vector3[] verts_local = mMeshF.mesh.vertices;
            for (int j = 0; j < verts_local.Length; j++)
            {
                verts_local[j] = go.transform.TransformPoint(verts_local[j]);
            }
            verts = verts_local;
        }

        else
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject childObj = go.transform.GetChild(i).gameObject;
                try
                {
                    Vector3[] verts_local = childObj.GetComponent<MeshFilter>().mesh.vertices;
                    for (int j = 0; j < verts_local.Length; j++)
                    {
                        verts_local[j] = childObj.transform.TransformPoint(verts_local[j]);
                    }
                    verts = verts.Concat(verts_local).ToArray();
                }
                catch (NullReferenceException e)
                {
                    print(go.name);
                    print(go.transform.name);
                    verts = verts.Concat(childObj.GetComponent<MeshFilter>().mesh.vertices).ToArray();
                }
            }

        }
        return verts;
    }

    private bool VisibleInCamera(Vector4 bbox)
    {
        if (bbox.x > Screen.width || bbox.x < 0) // x_min
        {
            return (false);
        }
        if (bbox.z > Screen.width || bbox.z < 0) // x_max
        {
            return (false);
        }
        if (bbox.y > Screen.height || bbox.y < 0) // y_min
        {
            return (false);
        }
        if (bbox.w > Screen.height || bbox.w < 0) // y_max
        {
            return (false);
        }
        return (true);
    }

    private bool isVisible(GameObject go)
    {

        //bool visible = true;
        string go_root_name = go.transform.root.name;


        return false; // added for debugging
        /*

        foreach (Vector3 vert in verts)
        {
            RaycastHit hit;
            Vector3 direction = vert - camMask.transform.position;
            if (Physics.Raycast(camMask.transform.position, direction, out hit))
            {
                //UnityEngine.Debug.Log(go.transform.root.name + " occluded by " + hit.transform.root.name);
                if (hit.transform.root.name == go_root_name)
                {
                    //UnityEngine.Debug.DrawRay(camMask.transform.position, direction, Color.blue, 240.0f);
                    continue;
                }
                else
                {
                    //visible = false;
                    //UnityEngine.Debug.DrawRay(camMask.transform.position, direction, Color.red, 240.0f);
                    return false;
                    //Debug.Log(go.transform.root.name + " occluded by " + hit.transform.root.name);
                }
            }
        }
        return true;
        */
    }

    Vector4 GetBoundingBoxInCamera(GameObject go, Camera cam)
    {
        Vector3[] verts = GetMeshVertices(go);

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = cam.WorldToScreenPoint(verts[i]);
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

        int min_x, min_y, max_x, max_y;
        min_x = (int)min.x;
        min_y = (int)min.y;
        max_x = (int)max.x;
        max_y = (int)max.y;

        return new Vector4(min_x, min_y, max_x, max_y);
    }

    private bool BBoxIntersects(Vector4 a, Vector4 b)
    {
        // Assuming Vector4 {x_min, y_min, x_max, y_max)
        float xLeft = Mathf.Max(a.x, b.x);
        float yTop = Mathf.Max(a.y, b.y);
        float xRight = Mathf.Min(a.z, b.z);
        float yBottom = Mathf.Min(a.w, b.w);
        if ( xRight < xLeft || yBottom < yTop)
        {
            return (false);
        }
        return (true);
    }

    private List<GameObject> spawnDistractors(Camera cam)
    {
      //Camera camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();
      //System.Random rnd = new System.Random();
      //int numberOfDistractotsToSpawn = ;
      List<int> distractorsToSpawn = new List<int>();
      List<GameObject> distractors = new List<GameObject>();
      int numberOfDistractotsToSpawn = UnityEngine.Random.Range(5, 10);
      for (int i = 0; i < 15; i++){
        distractorsToSpawn.Add(UnityEngine.Random.Range(0, 5));
      }

      //print(distractorsToSpawn);
      int id = 0;
      foreach (int distractor_id in distractorsToSpawn) {
        float scale_x = UnityEngine.Random.Range(0.5f,5f);
        float scale_y = UnityEngine.Random.Range(0.5f,5f);
        float scale_z = UnityEngine.Random.Range(0.5f,5f);
        Vector3 rand_scale = new Vector3 (scale_x, scale_y, scale_z);
        Color rand_color = new Color (UnityEngine.Random.Range(0F, 1F),
                                      UnityEngine.Random.Range(0F, 1F),
                                      UnityEngine.Random.Range(0F, 1F),
                                      UnityEngine.Random.Range(0F, 1F));
        int colourOrTexture = UnityEngine.Random.Range(0,100);

        switch (distractor_id)
        {
          case 0:
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.name = "cube_" + id.ToString();
            go.transform.position = GetRandomPositionInCamera(cam);
            go.transform.rotation = UnityEngine.Random.rotation;
            go.transform.localScale = rand_scale;
            if (colourOrTexture <= 50){
              Texture2D tex = new Texture2D(128, 128);
              tex = RandomizeTexture(ref tex);
              go.GetComponent<MeshRenderer>().material.mainTexture = tex;
              Destroy(tex);
            } else {
              go.GetComponent<MeshRenderer>().material.color = rand_color;
            }
            distractors.Add(go);
            break;

          case 1:
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.name = "sphere_" + id.ToString();
            go.transform.position = GetRandomPositionInCamera(cam);
            go.transform.rotation = UnityEngine.Random.rotation;
            go.transform.localScale = rand_scale;
            if (colourOrTexture <= 50){
              Texture2D tex = new Texture2D(128, 128);
              tex = RandomizeTexture(ref tex);
              go.GetComponent<MeshRenderer>().material.mainTexture = tex;
              Destroy(tex);
            } else {
              go.GetComponent<MeshRenderer>().material.color = rand_color;
            }
            distractors.Add(go);
            break;

          case 2:
            go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.name = "capsule_" + id.ToString();
            go.transform.position = GetRandomPositionInCamera(cam);
            go.transform.rotation = UnityEngine.Random.rotation;
            go.transform.localScale = rand_scale;
            if (colourOrTexture <= 50){
              Texture2D tex = new Texture2D(128, 128);
              tex = RandomizeTexture(ref tex);
              go.GetComponent<MeshRenderer>().material.mainTexture = tex;
              Destroy(tex);
            } else {
              go.GetComponent<MeshRenderer>().material.color = rand_color;
            }
            distractors.Add(go);
            break;

          case 3:
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.transform.name = "cylinder_" + id.ToString();
            go.transform.position = GetRandomPositionInCamera(cam);
            go.transform.rotation = UnityEngine.Random.rotation;
            go.transform.localScale = rand_scale;
            if (colourOrTexture <= 50){
              Texture2D tex = new Texture2D(128, 128);
              tex = RandomizeTexture(ref tex);
              go.GetComponent<MeshRenderer>().material.mainTexture = tex;
              Destroy(tex);
            } else {
              go.GetComponent<MeshRenderer>().material.color = rand_color;
            }
            distractors.Add(go);
            break;
        }
        id++;
      }
      return distractors;
    }

    void Awake()
    {
        modelList = GetDatasetModels("Models");
        Screen.SetResolution(480, 480, false);

        datasetDir = "dataset";
        trainDir = Path.Combine(datasetDir, "train");
        trainRGBDir = Path.Combine(trainDir, "images");
        trainBboxDir = Path.Combine(trainDir, "object_labels");
        trainMaskDir = Path.Combine(trainDir, "masks");

        testDir = Path.Combine(datasetDir, "test");
        testRGBDir = Path.Combine(testDir, "images");
        testBboxDir = Path.Combine(testDir, "object_labels");
        testMaskDir = Path.Combine(testDir, "masks");

        System.IO.Directory.CreateDirectory(datasetDir);

        System.IO.Directory.CreateDirectory(trainDir);
        System.IO.Directory.CreateDirectory(trainBboxDir);
        System.IO.Directory.CreateDirectory(trainRGBDir);
        System.IO.Directory.CreateDirectory(trainMaskDir);

        System.IO.Directory.CreateDirectory(testDir);
        System.IO.Directory.CreateDirectory(testBboxDir);
        System.IO.Directory.CreateDirectory(testRGBDir);
        System.IO.Directory.CreateDirectory(testMaskDir);

        camRGB = GameObject.Find("RGBCamera").GetComponent<Camera>();
        camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();

        backgroundMaterial = new Material(Shader.Find("Unlit/Texture"));
        backgroundTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);

        renderTex = new RenderTexture(camRGB.pixelWidth, camRGB.pixelHeight, 24);
        screenshotTex = new Texture2D(camRGB.pixelWidth, camRGB.pixelHeight, TextureFormat.RGB24, false);

    }
    void Start()
    {

        Camera camRGB = GameObject.Find("RGBCamera").GetComponent<Camera>();
        Camera camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();

        camRGB.enabled = false;
        camAnnotation.enabled = false;

        backgroundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        backgroundPlane.transform.position = new Vector3(0.0f, -100.0f, 0.0f);
        backgroundPlane.name = "Background";
        backgroundPlane = ResizeBackgroundToCameraView(ref backgroundPlane, camAnnotation);
        backgroundTexture = RandomizeTexture(ref backgroundTexture);
        backgroundRenderer = backgroundPlane.GetComponent<Renderer>();
        backgroundRenderer.material = backgroundMaterial;

    }


    void Update()
    {

        var watch = new Stopwatch();
        watch.Start();

        camRGB.enabled = true;
        camAnnotation.enabled = true;

        // Randomize screen resolution
        Screen.SetResolution(UnityEngine.Random.Range(400, 600), UnityEngine.Random.Range(400, 600), false);

        camRGB.enabled = false;
        camAnnotation.enabled = false;


        // Set background texture
        backgroundPlane = ResizeBackgroundToCameraView(ref backgroundPlane, camAnnotation);
        backgroundTexture = RandomizeTexture(ref backgroundTexture);
        backgroundMaterial.mainTexture = backgroundTexture;


       // Spawn objects
       int noInstances = GetNumberOfInstances(3, 25);
       int[] indexList = GetObjectIndexes(noInstances, modelList.Count);


        (rgbObjects, maskObjects) = InstantiateModels(modelList, indexList);
        for (int i = 0; i < rgbObjects.Count; i += 1)
        {
            // Scale to default scale
            ScaleToMaxExtent(1f, rgbObjects[i], maskObjects[i]);

            // Translate each object to a position in camera view
            Vector3 randomPosition = GetRandomPositionInCamera(camRGB);
            rgbObjects[i].transform.position = randomPosition;
            maskObjects[i].transform.position = randomPosition;

            // Randomize each objects orientation
            Quaternion randomRotation = UnityEngine.Random.rotation;
            rgbObjects[i].transform.rotation = randomRotation;
            maskObjects[i].transform.rotation = randomRotation;

            // Scale the objects randomly
            float randomScale = UnityEngine.Random.Range(1.0f, 5f);
            rgbObjects[i].transform.localScale = new Vector3(rgbObjects[i].transform.localScale.x * randomScale, rgbObjects[i].transform.localScale.y * randomScale, rgbObjects[i].transform.localScale.z * randomScale);
            maskObjects[i].transform.localScale = new Vector3(maskObjects[i].transform.localScale.x * randomScale, maskObjects[i].transform.localScale.y * randomScale, maskObjects[i].transform.localScale.z * randomScale);

            // Randomly morph the x y and z scales.
            float x = UnityEngine.Random.Range(0.75f, 1.25f);
            float y = UnityEngine.Random.Range(0.75f, 1.25f);
            float z = UnityEngine.Random.Range(0.75f, 1.25f);
            Vector3 morphVector = new Vector3(x, y, z);
            Vector3 localScale = rgbObjects[i].transform.localScale;
            Vector3 newLocalScale = new Vector3(morphVector.x * localScale.x, morphVector.y * localScale.y, morphVector.z * localScale.z);

            rgbObjects[i].transform.localScale = newLocalScale;
            maskObjects[i].transform.localScale = newLocalScale;
        }
        // Randomize lights and illumination

        lights = InstantiateLights(3);
        foreach (GameObject lightObj in lights)
        {
            // Set color and intensity
            Light lightComp = lightObj.GetComponent<Light>();
            UnityEngine.Color randomLightColor = new UnityEngine.Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
            lightComp.color = randomLightColor;
            lightComp.intensity = UnityEngine.Random.Range(1f, 4f);

            // Set the position (or any transform property)
            Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-20f, 20f));
            lightObj.transform.position = randomPosition;

            // set orientation
            Vector3 randomRotatoinEulerAngles = new Vector3(UnityEngine.Random.Range(-180f, 180f), UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(-180f, 180f));
            Quaternion randomRotation = Quaternion.Euler(randomRotatoinEulerAngles);
            lightObj.transform.rotation = randomRotation;
        }

        // Synchronize physics and update colliders
        Physics.SyncTransforms();

        List<GameObject> rgbObjectsTemp = new List<GameObject>();
        List<GameObject> maskObjectsTemp = new List<GameObject>();
        List<Vector4> bboxs = new List<Vector4>();

        // Check for occlusions and remove occluded objects
        for (int i = 0; i < rgbObjects.Count; i += 1) {

            bool noOcclusion = true;
            Vector4 bbox = GetBoundingBoxInCamera(maskObjects[i], camAnnotation); // Sending in mask object as it has the mesh collider
            if (VisibleInCamera(bbox) == true)
            {
                // check for occlusion by other objects

                for (int j = 0; j < bboxs.Count; j++)
                {
                    // Check if boxes intersects
                    bool intersects = BBoxIntersects(bboxs[j], bbox);
                    if (intersects == true)
                    {
                        noOcclusion = false;
                    }
                }
            }
            else
            {
                noOcclusion = false;
            }

            if (noOcclusion == true)
            {
                rgbObjectsTemp.Add(rgbObjects[i]);
                maskObjectsTemp.Add(maskObjects[i]);
                bboxs.Add(bbox);
            }
            else
            {
                rgbObjects[i].transform.position = new Vector3(1000f, 0f, 0f);
                maskObjects[i].transform.position = new Vector3(1000f, 0f, 0f);
                Destroy(rgbObjects[i]);
                Destroy(maskObjects[i]);
            }
            Physics.SyncTransforms();
        }
        rgbObjects = rgbObjectsTemp;
        maskObjects = maskObjectsTemp;

        List<GameObject> distractors = new List<GameObject>();
        distractors = spawnDistractors(camAnnotation);
        List<Vector4> bboxsDistractors = new List<Vector4>();
        //spawn distractors
        for (int i = 0; i < distractors.Count; i += 1) {
            bool noOcclusion = true;
            Vector4 bboxDistractor = GetBoundingBoxInCamera(distractors[i], camAnnotation); // Sending in mask object as it has the mesh collider

            for (int j = 0; j < bboxs.Count; j++)
            {
                // Check if boxes intersects
                bool intersects = BBoxIntersects(bboxs[j], bboxDistractor);
                if (intersects == true)
                {
                    noOcclusion = false;
                }
            }

            if (noOcclusion == false)
            {
                distractors[i].transform.position = new Vector3(1000f, 0f, 0f);
            }
            Physics.SyncTransforms();
        }

        if (maskObjects.Count > 0 && rgbObjects.Count > 0)
        {
            // TO DO: Add distractors

            camRGB.Render();
            camAnnotation.Render();

            string rgbFile = Path.Combine(trainRGBDir, Time.frameCount.ToString() + ".png");
            string maskFile = Path.Combine(trainMaskDir, Time.frameCount.ToString() + ".tga");
            string bboxFile = Path.Combine(trainBboxDir, Time.frameCount.ToString() + ".txt");

            if (File.Exists(bboxFile))
            {
                File.Delete(bboxFile);
            }


            for (int i = 0; i < maskObjects.Count; i++)
            {
                string line = maskObjects[i].name.Split('_')[0] + " " + bboxs[i].x.ToString() + " " + bboxs[i].y.ToString() + " " + bboxs[i].z.ToString() + " " + bboxs[i].w.ToString() + "\n";
                using (StreamWriter writer = new StreamWriter(bboxFile, true))
                {
                    writer.Write(line);
                }

            }

            captureFrames(Time.frameCount, rgbFile, maskFile);
        }

        DestroyAllModels(rgbObjects);
        DestroyAllModels(maskObjects);
        DestroyAllModels(distractors);
        DestroyAllModels(lights);

        watch.Stop();
        print($"{Time.frameCount}: Done - Execution Time: {watch.ElapsedMilliseconds} ms");




    }


}
