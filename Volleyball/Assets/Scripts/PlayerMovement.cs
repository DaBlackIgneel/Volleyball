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
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<CapsuleCollider>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        jump = Input.GetAxisRaw("Jump") == 1;
        isWalking = Input.GetAxisRaw("Sprint") == 1;
        isGrounded = CheckGround();
        Movement();

    }

    bool CheckGround()
    {
        return Physics.Raycast(new Ray(transform.position, Vector3.down), .1f);
    }

    void Movement()
    {
        
        if (isGrounded)
        {
            float speed = walkSpeed;
            if (isWalking)
                speed = sprintSpeed;
            float horizontal = Input.GetAxis("Horizontal");
            float verticle = Input.GetAxis("Vertical");
            Vector3 desiredMove = horizontal * transform.right + verticle * transform.forward;
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, myCollider.radius, Vector3.down, out hitInfo,
                               myCollider.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
            Vector3 moveDir = Vector3.zero;
            moveDir.x = desiredMove.x * Mathf.Lerp(rb.velocity.magnitude, speed, .1f);
            moveDir.z = desiredMove.z * Mathf.Lerp(rb.velocity.magnitude, speed, .1f);
            moveDir.y = rb.velocity.y;

            jumping = false;
            //moveDir.y = -stickToGroundForce;
            if(jump)
            {
                moveDir.y = jumpSpeed;
                jump = false;
                jumping = true;
            }
            rb.velocity = (moveDir);

        }
        
    }
}
