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

    public class datasetModel
    {
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
        System.Random rand = new System.Random();
        double x = 0.9 - (rand.NextDouble() * 0.8);
        double y = 0.9 - (rand.NextDouble() * 0.8);
        double z = 17.5 - (rand.NextDouble() * 5.0);
        Vector3 camera_pos = cam.ViewportToWorldPoint(new Vector3((float)x, (float)y, (float)z));

        //Vector3 camera_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(12.5f, 17.5f)));
        Vector3 world_pos = new Vector3(camera_pos.x, camera_pos.y, camera_pos.z);
        return world_pos;
    }

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }

    public Vector3 GetRandomPosition(Vector3 min, Vector3 max)
    {
        return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
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
            // RGB image /////////////////////////////////////////////////////////
            GameObject modelRGB = Instantiate(modelList[idx].prefab, position, rotation) as GameObject;

            if (modelRGB.TryGetComponent<Renderer>(out Renderer mRendRGB))
            {
                Renderer modelRGBRender;
                modelRGBRender = mRendRGB;
                modelRGBRender.material.mainTexture = modelList[idx].textureRGB;
            }
            else
            {
                Renderer[] childRendsRGB = modelRGB.GetComponentsInChildren<Renderer>();
                foreach (Renderer childRendRGB in childRendsRGB)
                {
                    childRendRGB.material.mainTexture = modelList[idx].textureRGB;
                }
            }


            /// Transfer to rgb camera layer
            modelRGB.layer = LayerMask.NameToLayer("Default");

            // Mask image ///////////////////////////////////////////////////////
            GameObject modelMask = Instantiate(modelList[idx].prefab, position, rotation) as GameObject;

            /// Set texture
            
            if (modelMask.TryGetComponent<Renderer>(out Renderer mRend))
            {
                Renderer modelMaskRender;
                modelMaskRender = mRend;
                modelMaskRender.material.mainTexture = modelList[idx].textureMask;
                modelMaskRender.material.shader = Shader.Find("Unlit/Texture");
                //modelMask.AddComponent<MeshFilter>();
                modelMask.AddComponent<MeshCollider>();
            }
            else
            {
                Renderer[] childRends = modelMask.GetComponentsInChildren<Renderer>();
                foreach (Renderer childRend in childRends)
                {
                    childRend.material.mainTexture = modelList[idx].textureMask;
                    childRend.material.shader = Shader.Find("Unlit/Texture");
                }
                for (int i = 0; i < modelMask.transform.childCount; i++)
                {
                    GameObject childObj = modelMask.transform.GetChild(i).gameObject;
                    //childObj.gameObject.AddComponent<MeshFilter>();
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


    }

    void SaveCameraRGB(Camera cam, string filename)
    {
        int width = cam.pixelWidth;
        int height = cam.pixelHeight;
        int depth = 24;
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
        var bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filename, bytes);
        cam.targetTexture = originalTargetTexture;
         
    }

    void captureFrames(int frameID, string rgbFile, string maskFile)
    {

        var camRGB = GameObject.Find("RGBCamera").GetComponent<Camera>();
        SaveCameraRGB(camRGB, rgbFile);

        var camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();
        SaveMask(camAnnotation, maskFile);

    }

    int GetNumberOfInstances(int min, int max)
    {
        System.Random rand = new System.Random();
        min = Math.Max(1, min);
        max = Math.Min(10, max);

        return (rand.Next(min, max));
    }

    int[] GetObjectIndexes(int noInstances, int noObjects)
    {
        int[] idxList = new int[noInstances];
        for (int i = 0; i < idxList.Length; i += 1)
        {
            System.Random rand = new System.Random();
            idxList[i] = rand.Next(0, noObjects - 1);
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

    GameObject SpawnBackground()
    {
        GameObject background_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        background_plane.transform.position = new Vector3(0.0f, -100.0f, 0.0f);
        // TODO - Get the distance from the camera pose

        Camera cam = GameObject.Find("AnnotationCamera").GetComponent<Camera>();

        float distance = 120f;
        float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * cam.aspect;

        Vector3 plane_bounds = background_plane.GetComponent<Renderer>().bounds.extents;
        background_plane.transform.localScale = new Vector3(frustumWidth / plane_bounds.x / 2f, 0.0f, frustumHeight / plane_bounds.z / 2f);
        background_plane.name = "Background";
        Physics.SyncTransforms();

        return (background_plane);
    }

    void SetBackgroundTexture(GameObject background)
    {
        System.Random seed = new System.Random();
        int numFunctions = 4;

        int[] functionChoice = new int[3];
        Vector2[] origin = new Vector2[3];
        Vector2[] scale = new Vector2[3];
        float[] multiplier = new float[3];

        for (int i = 0; i < 3; i++)
        {
            functionChoice[i] = UnityEngine.Random.Range(0, numFunctions);
            origin[i] = new Vector2((float)seed.NextDouble(), (float)seed.NextDouble());
            scale[i] = new Vector2(UnityEngine.Random.Range(0.05f, 10f), UnityEngine.Random.Range(0.05f, 10f));
            multiplier[i] = 1.0f - 0.5f * (float)seed.NextDouble();
        }

        float width = 256;
        float height = 256;
        Texture2D perlinNoiseTexture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        Color[] pixels = new Color[perlinNoiseTexture.width * perlinNoiseTexture.height];

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
                        float val = Mathf.PerlinNoise(origin[p].x + (x / width) * scale[p].x, origin[p].y + y / height * scale[p].y);
                        //val = val * multiplier[p];
                        pixelVal[p] = val;

                    }
                    else if (functionChoice[p] == 1)
                    {
                        // sinusoid / cosine noise
                        float valx = 0.5f * Mathf.Sin(((origin[p].x + x / width) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].x);
                        float valy = 0.5f * Mathf.Cos(((origin[p].y + y / height) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].y);
                        float val = valx + valy;
                        //val = val * multiplier[p];
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
                        float valx = 0.5f * Mathf.Sin(((origin[p].x + x / width) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].x);
                        float valy = 0.5f * Mathf.Sin(((origin[p].y + y / height) * 2f * Mathf.PI) * 2f * Mathf.PI * scale[p].y);
                    }
                }
                //pixels[(int)y * perlinNoiseTexture.width + (int)x] = new Color(red * redMultiplier, green * greenMultiplier, blue * blueMultiplier);
                pixels[(int)y * perlinNoiseTexture.width + (int)x] = pixelVal;

            }
        }
        // Copy the pixel data to the texture and load it into the GPU.
        perlinNoiseTexture.SetPixels(pixels);
        perlinNoiseTexture.Apply();

        Material mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = (Texture)perlinNoiseTexture;
        background.GetComponent<Renderer>().material = mat;
    }

    private bool isVisible(GameObject go)
    {
        //bool visible = true;
        string go_root_name = go.transform.root.name;
        Vector3[] verts = new Vector3[0];
        Camera camMask = GameObject.Find("AnnotationCamera").GetComponent<Camera>();

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
            verts[i] = go.transform.TransformPoint(verts[i]);
            Vector2 vert_camera_pos = camMask.WorldToViewportPoint(verts[i]);
            if (vert_camera_pos.x > 1.0f || vert_camera_pos.y > 1.0f || vert_camera_pos.x < 0.0f || vert_camera_pos.y < 0.0f)
            {
                //visible = false;
                return false;
            }
        }
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
    }
        
    public static string GetBoundingBox(GameObject go)
    {
        Vector3[] verts = new Vector3[0];
        Camera camMask = GameObject.Find("AnnotationCamera").GetComponent<Camera>();

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
        //Vector2[] output = new Vector2[2] { min, max };

        int min_x, min_y, max_x, max_y;
        min_x = (int)min.x;
        min_y = (int)min.y;
        max_x = (int)max.x;
        max_y = (int)max.y;

        string serializedData = min_x.ToString() + " "
        + min_y.ToString() + " "
        + max_x.ToString() + " "
        + max_y.ToString() + "\n";

        return serializedData;
        //return output;
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
    }
    void Start()
    { 

        Camera camRGB = GameObject.Find("RGBCamera").GetComponent<Camera>();
        Camera camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();

        camRGB.enabled = false;
        camAnnotation.enabled = false;
    }

    
    void Update()
    {
        //if (Time.frameCount == 2)
        //{

        
        var watch = new Stopwatch();
        watch.Start();

        Camera camRGB = GameObject.Find("RGBCamera").GetComponent<Camera>();
        Camera camAnnotation = GameObject.Find("AnnotationCamera").GetComponent<Camera>();
        camRGB.enabled = true;
        camAnnotation.enabled = true;

        // Randomize screen resolution
        Screen.SetResolution(UnityEngine.Random.Range(400, 600), UnityEngine.Random.Range(400, 600), false);

        camRGB.enabled = false;
        camAnnotation.enabled = false;

        // Set background texture
        GameObject backgroundPlane = SpawnBackground();
        SetBackgroundTexture(backgroundPlane);

        // Spawn objects
        int noInstances = GetNumberOfInstances(3, 10);
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
            float randomScale = UnityEngine.Random.Range(0.5f, 5f);
            rgbObjects[i].transform.localScale = new Vector3(rgbObjects[i].transform.localScale.x * randomScale, rgbObjects[i].transform.localScale.y * randomScale, rgbObjects[i].transform.localScale.z * randomScale);
            maskObjects[i].transform.localScale = new Vector3(maskObjects[i].transform.localScale.x * randomScale, maskObjects[i].transform.localScale.y * randomScale, maskObjects[i].transform.localScale.z * randomScale);              

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

        // Check for occlusions and remove occluded objects
        for (int i = 0; i < rgbObjects.Count; i += 1) {
            List<int> indexesToDestroy = new List<int>();

            bool visible = isVisible(maskObjects[i]); // Sending in mask object as it has the mesh collider
            if (visible == false)
            {
                rgbObjects[i].transform.position = new Vector3(1000f, 0f, 0f);
                maskObjects[i].transform.position = new Vector3(1000f, 0f, 0f);
                Destroy(rgbObjects[i]);
                Destroy(maskObjects[i]);
                //indexesToDestroy.Add(i);
            }
            else
            {
                rgbObjectsTemp.Add(rgbObjects[i]);
                maskObjectsTemp.Add(maskObjects[i]);
                string bb = GetBoundingBox(maskObjects[i]);
            }
            Physics.SyncTransforms();
        }

        rgbObjects = rgbObjectsTemp;
        maskObjects = maskObjectsTemp;

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
            string bbox = maskObjects[i].name.Split('_')[0] + " " + GetBoundingBox(maskObjects[i]);
            using (StreamWriter writer = new StreamWriter(bboxFile, true))
            {
                writer.Write(bbox);
            }
        }
        

        captureFrames(Time.frameCount, rgbFile, maskFile);

        DestroyAllModels(rgbObjects);
        DestroyAllModels(maskObjects);
        DestroyAllModels(lights);
        Destroy(backgroundPlane);

        //Destroy all instances
        watch.Stop();
        print($"{Time.frameCount}: Done - Execution Time: {watch.ElapsedMilliseconds} ms");
        //}


    }

   
}
