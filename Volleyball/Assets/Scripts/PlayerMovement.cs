using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    float speed = 2;
    float sprintSpeed = 2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Movement();
	}

    void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float verticle = Input.GetAxis("Verticle");
        Vector3 velocityForce = new Vector3(horizontal,0,verticle);
    }

    void ChangeDirection()
    {

    }
}
