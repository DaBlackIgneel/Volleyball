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
    public bool faceNet;
    public bool goToRotation;
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
        FixRotation();
        Movement();
    }

    //checks to see if there is ground right below the player
    bool CheckGround()
    {
        //shoots a ray down from the foot of the player
        //if it collides with something then there is ground
        //otherwise there is no ground
        Debug.DrawLine(ground.position, ground.position + Vector3.down * .3f);
        RaycastHit hit;
        bool grounded = Physics.Raycast(new Ray(ground.position, Vector3.down),out hit, .4f);
        return hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground");
    }

    void FixRotation()
    {
        if (!relativeMovement && !goToRotation)
        {
            if (!faceNet)
            {
                if (!followBall)
                {
                    transform.localRotation = Quaternion.Euler(Vector3.up * Mathf.Atan2(movementDirection.y, movementDirection.x));
                }
                else
                {
                    Vector3 ballVelocity = Vector3.ProjectOnPlane(myPass.vBall.rb.velocity, Vector3.up);
                    Vector3 distance = transform.position - myPass.vBall.transform.position;
                    float ballAngle = ballVelocity.x > Mathf.Epsilon || ballVelocity.z > Mathf.Epsilon ?
                            Mathf.Atan2(ballVelocity.z, ballVelocity.x) * Mathf.Rad2Deg - 180 : (int)transform.eulerAngles.y;

                    float diffAngle = Vector3.Angle(ballVelocity, distance) * Mathf.Sign(distance.z);
                    float angle = ballVelocity.magnitude > Mathf.Epsilon ? ballAngle - diffAngle : ballAngle;
                    transform.localRotation = Quaternion.Euler(Vector3.up * angle);
                }
            }
            else
            {
                transform.eulerAngles = Vector3.up * (90 + 90 * (int)myPass.currentSide);
            }
        }
    }

    //Moves the player
    void Movement()
    {
        //only control your movement if you are touching the ground
        if (isGrounded)
        {
            //sets the desired speed whether walking or running
            float speed = isWalking ? walkSpeed : sprintSpeed;

            //calculates the desired movement that is tangent to the surface below
            Vector3 desiredMove;
            desiredMove = relativeMovement ? movementDirection.x * transform.right + movementDirection.y * transform.forward : movementDirection.x * Vector3.right + movementDirection.y * Vector3.forward;


            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, myCollider.radius, Vector3.down, out hitInfo,
                               myCollider.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            desiredMove = Vector3.ProjectOnPlane(desiredMove * speed, hitInfo.normal).normalized;
            //smoothly increase the speed to the desired speed
            Vector3 moveDir = Vector3.zero;
            moveDir = desiredMove * Mathf.Lerp(rb.velocity.magnitude, speed, 0.1f);
            moveDir.y = rb.velocity.y;

            jumping = false;
            //if you want to jump, then jump
            if (jump)
            {
                //do a power jump if you wanted to move and jump quickly
                if (Vector3.Project(moveDir, Vector3.up).magnitude > Vector3.Project(desiredMove, Vector3.up).magnitude * .1f && !relativeMovement)
                    moveDir = desiredMove;

                //add the initial jumpspeed
                moveDir.y = jumpSpeed;
                jump = false;
                jumping = true;
                
            }

            //move the player in the desired movement
            if (rb.velocity != moveDir)
            {
                rb.AddForce((moveDir - rb.velocity) * rb.mass, ForceMode.Impulse);
            }

        }

        isStopped = rb.velocity.magnitude < Mathf.Epsilon;
        isWalking = isStopped ? true : isWalking;
        //explode = false;

    }

    public void CalculatedMoveTowards(Vector3 targetPosition)
    {
        bool walking = Vector3.Project(targetPosition - transform.position, Vector3.up).magnitude < walkSpeed/2;
        MoveTowards(targetPosition, walking);
    }

    public void MoveTowards(Vector3 targetPosition, bool walking = false)
    {
        Vector3 direction = targetPosition - transform.position;
        
        direction.y = direction.z;
        direction.z = 0;
        isWalking = !isStopped? walking:true;
        float speed = isWalking ? walkSpeed : sprintSpeed;
        if (direction.magnitude > speed * .15f)
            Move(direction);
        else
            Stop();
    }

    public void MoveDirectlyTowards(Vector3 targetPosition, bool walking = false)
    {
        Vector3 direction = targetPosition - transform.position;

        direction.y = direction.z;
        direction.z = 0;
        isWalking = walking;
        Move(direction);
    }
    public void Move(Vector2 direction)
    {
        movementDirection = direction.normalized;
    }

    public void Stop()
    {
        movementDirection = Vector2.zero;
        Vector3 stopDir = Vector3.up * rb.velocity.y;
        if(!relativeMovement && isGrounded)
            rb.AddForce((stopDir - rb.velocity ) * rb.mass, ForceMode.Impulse);//*/
    }

}
