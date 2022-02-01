using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewScript : MonoBehaviour
{
    private GameObject background_plane;
    private GameObject knife, knife_a;
    private Material new_mat;
    private List<GameObject> gameobjects_list = new List<GameObject>();
    //private  gameobjects_list;

    void Awake() {
      Debug.Log("NewScript Start");
      background_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
      background_plane.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
      background_plane.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
      new_mat = Resources.Load("Bricks Textures/Bricks Texture 01/Bricks Texture 01") as Material;
      background_plane.GetComponent<Renderer>().material = new_mat;
      background_plane.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
      background_plane.name = "plane";
      Debug.Log("Background created");

    	knife = Resources.Load("Models/1_knife1/OBJ/Kitchenknife_lowpoly") as GameObject;
    	knife_a = Resources.Load("Models/1_knife1_a/OBJ/Kitchenknife_lowpoly") as GameObject;
    	Debug.Log("GameObject knife reached");

    	/*Vector3 myVector;
    	myVector = new Vector3(0.0f, 0.5f, 0.0f);
    	Quaternion myQuat;
    	myQuat = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);*/
      float pos_x = Random.Range(0.05f, 0.95f);
      float pos_y = Random.Range(0.0f, 20.0f);
      float pos_z = Random.Range(0.05f, 0.95f);
      Vector3 pos = new Vector3(pos_x, pos_y, pos_z);
      //Vector3 p = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));
      pos = Camera.main.ViewportToWorldPoint(pos);
      GameObject temp = Instantiate(knife, pos, Random.rotation);
      temp.name = "knife";

      pos_x = Random.Range(0.05f, 0.95f);
      pos_y = Random.Range(5f, 20f);
      pos_z = Random.Range(0.05f, 0.95f);
      pos = new Vector3(pos_x, pos_y, pos_z);
      pos = Camera.main.ViewportToWorldPoint(pos);
    	temp =Instantiate(knife_a, pos, Random.rotation);
      temp.name = "knife_a";

    }

    void Start(){
      //https://answers.unity.com/questions/1414048/destroy-specific-gameobject-with-name.html
      var gameobjects_list = FindObjectsOfType<GameObject>();
      /*foreach( GameObject x in gameobjects_list) {
        if (x.name == "plane") {
          Debug.Log("Not deleting Plane");
        } else {
          Destroy(x);
          Debug.Log("Deleted " + x.ToString());
        }
      }*/

      //Destroy(GameObject.Find("knife"), 10);
      //Destroy(GameObject.Find("knife_a"), 10);

    }

    // Update is called once per frame
    void Update(){}
}
