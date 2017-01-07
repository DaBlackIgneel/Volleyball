using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float speed = 2;
    public float sprintSpeed = 2;

    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Movement();
	}

    void Movement()
    {
        float runningSpeed = speed;
        float horizontal = Input.GetAxis("Horizontal");
        float verticle = Input.GetAxis("Vertical");
        Vector3 velocityForce = new Vector3(horizontal,0,verticle);
        rb.AddForce(velocityForce * runningSpeed * rb.mass);
    }

    void ChangeDirection()
    {

    }
}
