using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeObjectTexture : MonoBehaviour
{
    private int frame_index = 0;
    private GameObject knife;
    private Texture2D mask;
    
    void Start()
    {
	//mask.LoadImage("Models/Kitchen Knife/Texture/KnifeTexture_masked.jpg");
	//mask = Resources.Load("Models/Kitchen Knife/Texture/KnifeTexture_masked") as Texture2D;
	if (mask) {
		Debug.Log(mask.name);
	} else {
		Debug.Log("No mask texture found");	
	}

	knife = GameObject.Find("Kitchenknife_lowpoly(Clone)");
	if (knife) {
		Debug.Log(knife.name);
	} else {
		Debug.Log("No game object knife found");	
	}
    }

    // Update is called once per frame
    void Update()
    {
	++frame_index;
	if (frame_index == 100){
		ScreenCapture.CaptureScreenshot("/home/daniel/unity_first_test/screenshots/whatever");
		Debug.Log("Screenshot captured");
		//frame_index = 0;
		
	}
	if (frame_index == 300){
		Debug.Log("Texture statement");
		//knife.GetComponentInChildren<Renderer>().material.mainTexture = mask;
		//knife.GetComponent<Renderer>().material.mainTexture = mask;
		//knife.texture.mainTexture = mask;
	}
    }
}
