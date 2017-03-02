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
    public bool isStopped;
    public bool followBall;
    public Transform ground;
    public bool relativeMovement = true;
    [System.NonSerialized]
    public Rigidbody rb;
    CapsuleCollider myCollider;
    CollisionFlags m_CollisionFlags;
    SpecialAction myPass;
    Vector2 movementDirection;
	// Use this for initialization
	void Start () {
        myPass = GetComponentInChildren<SpecialAction>();
        //the component that controlls the physics of this object
        rb = GetComponent<Rigidbody>();

        //the component that sets the collision bounds
        myCollider = GetComponent<CapsuleCollider>();

        movementDirection = Vector2.zero;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
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
        Debug.DrawLine(ground.position, ground.position + Vector3.down * .3f);
        return Physics.Raycast(new Ray(ground.position, Vector3.down), .4f);
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
            //float horizontal = Input.GetAxis("Horizontal");
            //float verticle = Input.GetAxis("Vertical");

            //calculates the desired movement that is tangent to the surface below

            Vector3 desiredMove;
            desiredMove = relativeMovement? movementDirection.x * transform.right + movementDirection.y * transform.forward: movementDirection.x * Vector3.right + movementDirection.y * Vector3.forward;
            if (!relativeMovement)
            {
                if (!followBall)
                {
                    
                    transform.localRotation = Quaternion.Euler(Vector3.up * Mathf.Atan2(desiredMove.y, desiredMove.x));
                    Debug.DrawRay(transform.position, desiredMove);
                }
                else
                {
                    Vector3 ballVelocity = Vector3.ProjectOnPlane(myPass.vBall.rb.velocity, Vector3.up);
                    Vector3 distance = transform.position - myPass.vBall.transform.position;
                    float ballAngle = ballVelocity.x > Mathf.Epsilon || ballVelocity.z > Mathf.Epsilon? 
                            Mathf.Atan2(ballVelocity.z, ballVelocity.x)* Mathf.Rad2Deg - 180: (int)transform.eulerAngles.y;

                    float diffAngle = Vector3.Angle(ballVelocity, distance) * Mathf.Sign(distance.z);
                    float angle = ballVelocity.magnitude > Mathf.Epsilon? ballAngle - diffAngle: ballAngle;
                    transform.localRotation = Quaternion.Euler(Vector3.up * angle);
                }
            }
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
            if(rb.velocity != moveDir)
                rb.AddForce((moveDir - rb.velocity) * rb.mass,ForceMode.Impulse);

        }

        isStopped = rb.velocity.magnitude < Mathf.Epsilon;
        isWalking = isStopped ? true : isWalking;
        
    }

    public void MoveTowards(Vector3 targetPosition, bool walking = false)
    {
        Vector3 direction = targetPosition - transform.position;
        
        direction.y = direction.z;
        direction.z = 0;
        isWalking = !isStopped? walking:true;//direction.magnitude < 1;
        if (direction.magnitude > .1f)
            Move(direction);
        else
            Stop();
    }
    public void Move(Vector2 direction)
    {
        movementDirection = direction;
    }

    public void Stop()
    {
        movementDirection = Vector2.zero;
    }
    
}
