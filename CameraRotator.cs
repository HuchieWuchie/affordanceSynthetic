using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    //public float speed = 30;

    void Awake()
    {
      transform.Rotate(90, 0, 0);
    	//transform.Translate(0, 0, 0);
      transform.position = new Vector3(0, 20, 0);
    }

    // Update is called once per frame
    void Update()
    {
	//transform.Rotate(speed * Time.deltaTime, 0, 0);
    }

}
