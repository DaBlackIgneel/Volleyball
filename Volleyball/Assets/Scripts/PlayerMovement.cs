using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MyMouseLook))]
public class PlayerMovement : MonoBehaviour {

    public float walkSpeed = 2;
    public float sprintSpeed = 4;
    public float jumpSpeed = 6;
    public bool jumping = false;
    public float gravityMultiplier = 2;
    public bool jump = false;
    public float stickToGroundForce = 10;
    public bool isGrounded;
    public bool isWalking;
    [System.NonSerialized]
    public Rigidbody rb;
    CapsuleCollider myCollider;
    CollisionFlags m_CollisionFlags;

	// Use this for initialization
	void Start () {
        //the component that controlls the physics of this object
        rb = GetComponent<Rigidbody>();

        //the component that sets the collision bounds
        myCollider = GetComponent<CapsuleCollider>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //checks if you want to jump
        jump = Input.GetAxisRaw("Jump") == 1;

        //checks if you want to walk/run
        isWalking = Input.GetAxisRaw("Sprint") != 1;

        //checks if you are touching the ground
        isGrounded = CheckGround();

        //move the player
        Movement();
    }

    //checks to see if there is ground right below the player
    bool CheckGround()
    {
        //shoots a ray down from the foot of the player
        //if it collides with something then there is ground
        //otherwise there is no ground
        return Physics.Raycast(new Ray(transform.position, Vector3.down), .1f);
    }

    //Moves the player
    void Movement()
    {
        //only control your movement if you are touching the ground
        if (isGrounded)
        {
            //sets the desired speed whether walking or running
            float speed = walkSpeed;
            if (!isWalking)
                speed = sprintSpeed;

            //gets the horizontal and vertical inputs
            float horizontal = Input.GetAxis("Horizontal");
            float verticle = Input.GetAxis("Vertical");

            //calculates the desired movement that is tangent to the surface below
            Vector3 desiredMove = horizontal * transform.right + verticle * transform.forward;
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, myCollider.radius, Vector3.down, out hitInfo,
                               myCollider.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            //smoothly increase the speed to the desired speed
            Vector3 moveDir = Vector3.zero;
            moveDir.x = desiredMove.x * Mathf.Lerp(rb.velocity.magnitude, speed, .1f);
            moveDir.z = desiredMove.z * Mathf.Lerp(rb.velocity.magnitude, speed, .1f);
            moveDir.y = rb.velocity.y;

            jumping = false;
            //if you want to jump, then jump
            if(jump)
            {
                //add the initial jumpspeed
                moveDir.y = jumpSpeed;
                jump = false;
                jumping = true;
            }

            //move the player in the desired movement
            rb.velocity = (moveDir);

        }
        
    }
}
