using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour {
    float offset;
	// Use this for initialization
	void Start () {
        offset = transform.position.z;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position = new Vector3(transform.position.x, transform.position.y, offset);
	}
    void Zoom()
    {
        offset += Input.mouseScrollDelta.y;
    }
}
