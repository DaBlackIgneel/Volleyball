using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters;

public enum Side { Left = -1, Right = 1}

public class SpecialAction : MonoBehaviour {

    public float power = 1;
    public float minimumPower = .25f;
    public int currentPosition = 1;
    public bool isPlayer = true;
    public Side VolleyballSide = Side.Left;
    public float tau = .5f;
    public float divisionFactor = 100f;
    PlayerMovement controller;
    MyMouseLook m_MouseLook;
    Transform myParent;
    bool block;
    bool hitting;
    bool slowDown;
    bool aimed;
    float hitCooldown = 1;
    float hitTime = 0;
    float slowTimeScale = .01f;
    public float lobConst = 2;
    public float smashConst = 2;
    float regFixedTimeDelta;
    public bool move;
    bool IgnoreCollision;
    Rigidbody ball;
    Rigidbody rb;
    CourtScript court;
    Transform arms;
    Camera tpc;
    Camera fpc;
    Vector3 aimDir;
    Vector2 tempMousePos;
	// Use this for initialization
	void Start () {
        
        myParent = transform.parent;
        if (myParent == null)
            myParent = transform;
        print(myParent.name);
        try
        {
            rb = myParent.GetComponent<Rigidbody>();
        }
        catch { }
        arms = myParent.Find("Arms");
        fpc = myParent.Find("FirstPersonCamera").GetComponent<Camera>();
        tpc = myParent.Find("ThirdPersonCamera").GetComponent<Camera>();
        controller = myParent.GetComponent<PlayerMovement>();
        regFixedTimeDelta = Time.fixedDeltaTime;
        if (!isPlayer)
        {
            controller.enabled = false;
            fpc.gameObject.SetActive(false);
            tpc.gameObject.SetActive(false);
        }
        
        if(isPlayer)
        tpc.GetComponent<MyMouseLook>().SetCursorLock(true);
        m_MouseLook = myParent.GetComponent<MyMouseLook>();
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Rigidbody>();
        court = GameObject.FindGameObjectWithTag("Court").GetComponent<CourtScript>();
        if (rb != null)
            rb.MovePosition(court.GetPosition(currentPosition, VolleyballSide));
        else
            myParent.position = court.GetPosition(currentPosition, VolleyballSide);
        hitTime = hitCooldown;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        
        Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;
        if (hitTime <= hitCooldown)
        {
            hitTime += Time.fixedDeltaTime;
            IgnoreCollision = true;
        }
        if (hitTime > hitCooldown && IgnoreCollision)
        {
            Physics.IgnoreCollision(ball.GetComponent<Collider>(), GetComponent<Collider>(), false);
            IgnoreCollision = false;
        }
        if (move)
        {
            move = false;
            rb.MovePosition(court.GetPosition(currentPosition, VolleyballSide));
        }

        if(!isPlayer)
        {
            ComputerMovement();
        }
        Block();
        if(hitting && !block  && hitTime > hitCooldown)
        {
            Aim();
        }
        if(slowDown)
        {
            Time.timeScale = slowTimeScale;
        }
        
        
    }

    void ComputerMovement()
    {
        //((int)VolleyballSide) * ball.velocity.z > 0 && 
        if (((int)VolleyballSide) * ball.transform.position.z > 0)
        {
            if(ball.transform.position.y > 1.25f && ball.transform.position.y < 1.75f)
            {
                float offset = .5f;
                MoveTowards(ball.position + ((int)VolleyballSide) * Vector3.forward * offset);
            }
        }
    }

    void MoveTowards(Vector3 position)
    {
        Vector3 myPosition = new Vector3(position.x, transform.position.y, position.z);
        rb.MovePosition(myPosition);
    }

    void Block()
    {
        if(isPlayer)
            block = (Input.GetAxisRaw("Block") != 0);
        arms.gameObject.SetActive(block);
        //fpc.enabled = block;
        //tpc.enabled = !block;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball" && hitTime > hitCooldown)
        {
            ball = other.GetComponent<Rigidbody>();
            hitting = true;
            slowDown = true;
            transform.parent = null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Ball" && hitTime > hitCooldown)
        {
            ReturnSpeedToNormal();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "Ball" && hitTime > hitCooldown)
        {
            ReturnSpeedToNormal();
        }
    }

    void Aim()
    {
        if((Input.GetButtonDown("Fire") || !isPlayer) && !aimed)
        {
            Ray mousePoint = new Ray(Vector3.zero,Vector3.forward * ((float)VolleyballSide) * -1 + Vector3.up);
            if(isPlayer)
                mousePoint = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector2 mousePos = Input.mousePosition;
            //controller.enabled = false;
            m_MouseLook.SetCursorLock(false);
            m_MouseLook.look = false;
            tpc.GetComponent<MyMouseLook>().look = false;
            aimDir = mousePoint.direction;
            aimDir = aimDir.normalized;
            tempMousePos = mousePos;
            aimed = true;
            ball.velocity = Vector3.zero;
        }
        if(aimed)
        {
            HitTheBall();

        }
    }

    void HitTheBall()
    {
        float power = this.power * .5f;
        if (Input.GetButtonUp("Fire") || !isPlayer)
        {
            //ball.velocity = Vector3.zero;
            float deltaMousePos = Input.mousePosition.y - tempMousePos.y;
            if (isPlayer)
            {
                power = (Mathf.Abs(deltaMousePos) / (Screen.height/2 * (1 / (1 - minimumPower))) + minimumPower) * this.power;
                if(deltaMousePos > 0)
                {
                    aimDir.y += deltaMousePos / (Screen.height * lobConst);
                    aimDir = aimDir.normalized;
                }
                else
                {
                    aimDir *= smashConst;
                }
                aimDir.y += Mathf.Sign(aimDir.y) * Mathf.Exp(Mathf.Abs(ball.transform.position.y) * -1 / tau) / divisionFactor;
            }
            if (aimDir.y > 0)
            {
                aimDir.y *= ball.mass;
            }
            
            
            ball.AddForce(aimDir * power);
            hitting = false;
            ReturnSpeedToNormal();
        }
        //print(power + ", Fire: " + Input.GetButtonUp("Fire"));
    }

    

    void ReturnSpeedToNormal()
    {
        if (slowDown)
        {
            hitting = false;
            slowDown = false;
            Time.timeScale = 1f;
            aimed = false;
            hitTime = 0;
            if (isPlayer)
            {
                ResetMovement();
            }
        }
    }

    void ResetMovement()
    {
        //controller.enabled = true;
        m_MouseLook.SetCursorLock(true);
        m_MouseLook.look = true;
        tpc.GetComponent<MyMouseLook>().look = true;
        
        if(myParent != transform)
        {
            transform.parent = myParent;
            transform.eulerAngles = myParent.eulerAngles;
        }
        Physics.IgnoreCollision(ball.GetComponent<Collider>(), GetComponent<Collider>(), true);

        // controller.enabled = true;
    }
}
