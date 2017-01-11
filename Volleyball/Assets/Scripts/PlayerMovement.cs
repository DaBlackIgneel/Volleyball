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
    CharacterController controller;
    CollisionFlags m_CollisionFlags;

	// Use this for initialization
	void Start () {
        controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        jump = Input.GetAxisRaw("Jump") == 1;
        Movement();

    }

    void Movement()
    {
        float speed = walkSpeed;
        if (Input.GetAxisRaw("Sprint") == 1)
            speed = sprintSpeed;
        float horizontal = Input.GetAxis("Horizontal");
        float verticle = Input.GetAxis("Vertical");
        Vector3 desiredMove = horizontal* transform.right + verticle * transform.forward;
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position,controller.radius, Vector3.down, out hitInfo,
                           controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        Vector3 moveDir = Vector3.zero;
        moveDir.x = desiredMove.x * Mathf.Lerp(controller.velocity.magnitude, speed, .1f);
        moveDir.z = desiredMove.z * Mathf.Lerp(controller.velocity.magnitude, speed, .1f);
        moveDir.y = controller.velocity.y;
        if (controller.isGrounded)
        {
            jumping = false;
            moveDir.y = -stickToGroundForce;
            if(jump)
            {
                moveDir.y = jumpSpeed;
                jump = false;
                jumping = true;
            }

        }
        else
        {
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }
        
        /*
        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        */


        m_CollisionFlags = controller.Move(moveDir  * Time.fixedDeltaTime);
    }
}
