using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters;

public enum Side { Left = -1, Right = 1}

public class SpecialAction : MonoBehaviour {
    [Range(500, 2000)]
    public float MaxPower = 1000;
    [Range(0.001f,.99f)]
    public float minimumPower = .25f;

    [Range(1,6)]
    public int currentPosition = 1;
    public bool isPlayer = true;
    public Side VolleyballSide = Side.Left;

    public float tau = .5f;
    public float divisionFactor = 100f;

    [Range(.001f, 2)]
    public float lobConst = 2;
    [Range(1, 4)]
    public float smashConst = 2;

    public bool resetPosition;
    public bool shoot;
    public float squatSpeed;
    public float serveLob = .5f;

    PlayerMovement controller;
    MyMouseLook m_MouseLook;
    Transform myParent;

    bool block;
    bool squat;
    bool hitting;
    bool aimed;

    float hitCooldown = 1;
    [SerializeField] float hitTime = 0;
    bool slowDown;
    float slowTimeScale = .01f;

    float power = 1;
    
    float regFixedTimeDelta;

    bool Shoot
    {
        set{ if(value){ ShootBall();}}
    }

    bool IgnoreCollision;    
    Vector3 pastBallVelocity;
    Rigidbody rb;
    VolleyballScript vBall;

    CourtScript court;
    Transform arms;
    Camera tpc;
    Camera fpc;
    MyMouseLook cameraLook;

    Vector3 aimDir;
    Vector3 tempAimDir;
    Vector2 tempMousePos;

    ParticleSystem mySystem;
    ParticleSystem.Particle[] myParticle;

    CapsuleCollider myCollider;
    CapsuleCollider myTrigger;

    float squatThreshhold = 0.05f;
    Vector2 originMeshHeightY;
    Vector2 squatMeshHeightY;
    Vector2 originColliderHeightY;
    Vector2 squatColliderHeightY;
    Vector2 originTriggerHeightY;
    Vector2 squatTriggerHeightY;

    // Use this for initialization
    void Start () {
        
        myParent = transform.parent;

        rb = myParent.GetComponent<Rigidbody>();
        arms = myParent.Find("Arms");
        fpc = myParent.Find("FirstPersonCamera").GetComponent<Camera>();
        tpc = myParent.Find("ThirdPersonCamera").GetComponent<Camera>();
        court = GameObject.FindGameObjectWithTag("Court").GetComponent<CourtScript>();
        vBall = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();
        controller = myParent.GetComponent<PlayerMovement>();
        m_MouseLook = myParent.GetComponent<MyMouseLook>();
        myCollider = myParent.GetComponent<CapsuleCollider>();
        myTrigger = GetComponent<CapsuleCollider>();
        cameraLook = tpc.GetComponent<MyMouseLook>();
        
        if (isPlayer)
            tpc.GetComponent<MyMouseLook>().SetCursorLock(true);
        else
        {
            controller.enabled = false;
            fpc.gameObject.SetActive(false);
            tpc.gameObject.SetActive(false);
        }

        regFixedTimeDelta = Time.fixedDeltaTime;

        hitTime = hitCooldown;

        mySystem = myParent.GetComponentInChildren<ParticleSystem>();
        myParticle = new ParticleSystem.Particle[30];

        originColliderHeightY = new Vector2(myCollider.height, myCollider.center.y);
        originMeshHeightY = new Vector2(transform.localScale.y, transform.localPosition.y);
        originTriggerHeightY = new Vector2(myTrigger.height, myTrigger.center.y);
        squatColliderHeightY = new Vector2(1, 0.5f);
        squatMeshHeightY = new Vector2(0.5f, 0.5f);
        squatTriggerHeightY = new Vector2((originMeshHeightY.x - squatMeshHeightY.x + 1) * originTriggerHeightY.x, originTriggerHeightY.y - .5f);
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if(!court.rallyOver)
        {
            GetInput();
            if (slowDown)
            {
                Time.timeScale = slowTimeScale;
            }
            if (shoot)
            {
                Shoot = true;
                shoot = false;
            }
            Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;
            if (hitTime <= hitCooldown)
            {
                hitTime += Time.fixedDeltaTime;
                IgnoreCollision = true;
            }
            if (hitTime > hitCooldown && IgnoreCollision)
            {
                //Physics.IgnoreCollision(ball.GetComponent<Collider>(), GetComponent<Collider>(), false);
                IgnoreCollision = false;
            }
            if (!isPlayer)
            {
                ComputerMovement();
            }

            if (hitting)
            {
                Aim();
            }
            Block();
            Squat();
        }
        else
        {
            if (resetPosition)
            {
                resetPosition = false;
                rb.MovePosition(court.GetPosition(currentPosition, VolleyballSide, this));
            }
            if (court.readyToServe)
            {
                if(Input.GetAxisRaw("Fire") == 1)
                {
                    hitTime = hitCooldown + 1;
                    court.GiveBallToServer();
                }
            }
        }        
    }

    void ComputerMovement()
    {
        //((int)VolleyballSide) * ball.velocity.z > 0 && 
        if (((int)VolleyballSide) * vBall.transform.position.z > 0)
        {
            if(vBall.transform.position.y > 1.25f && vBall.transform.position.y < 1.75f)
            {
                float offset = .5f;
                MoveTowards(vBall.rb.position + ((int)VolleyballSide) * Vector3.forward * offset);
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
        arms.gameObject.SetActive(block);
        //fpc.enabled = block;
        //tpc.enabled = !block;
    }

    void Squat()
    {
        //print(Input.GetAxis("Squat"));
        float hittingOffset;
        hittingOffset = hitting ? myParent.transform.position.y : 0;
        if (squat)
        {
            if(Mathf.Abs(myCollider.height - squatColliderHeightY.x) > squatThreshhold)
            {
                myCollider.height = Mathf.Lerp(myCollider.height, squatColliderHeightY.x, squatSpeed);
                myCollider.center = Vector3.Lerp(myCollider.center, new Vector3(myCollider.center.x, squatColliderHeightY.y, myCollider.center.z), squatSpeed);
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, squatMeshHeightY.x, transform.localScale.z), squatSpeed);
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, squatMeshHeightY.y + hittingOffset, transform.localPosition.z), squatSpeed);
                myTrigger.height = Mathf.Lerp(myTrigger.height, squatTriggerHeightY.x, squatSpeed);
                myTrigger.center = Vector3.Lerp(myTrigger.center, new Vector3(myTrigger.center.x, squatTriggerHeightY.y, myTrigger.center.z), squatSpeed);
            }
            
        }
        else
        {
            if (Mathf.Abs(myCollider.height - originColliderHeightY.x) > squatThreshhold)
            {
                myCollider.height = Mathf.Lerp(myCollider.height, originColliderHeightY.x, squatSpeed);
                myCollider.center = Vector3.Lerp(myCollider.center, new Vector3(myCollider.center.x, originColliderHeightY.y, myCollider.center.z), squatSpeed);
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, originMeshHeightY.x, transform.localScale.z), squatSpeed);
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, originMeshHeightY.y + hittingOffset, transform.localPosition.z), squatSpeed);
                myTrigger.height = Mathf.Lerp(myTrigger.height, originTriggerHeightY.x, squatSpeed);
                myTrigger.center = Vector3.Lerp(myTrigger.center, new Vector3(myTrigger.center.x, originTriggerHeightY.y, myTrigger.center.z), squatSpeed);
            }
                
        }
    }

    void GetInput()
    {
        if (isPlayer)
        {
            block = (Input.GetAxisRaw("Block") != 0);
            squat = (Input.GetAxis("Squat") > 0.75f);
        }
            
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball" && ((hitTime > hitCooldown  && !court.rallyOver) || court.readyToServe))
        {
            hitting = true;
            slowDown = true;
            if (isPlayer)
            {
                transform.parent = null;
                controller.rb.velocity = Vector3.zero;
                controller.enabled = false;
            }
            vBall.CollideWithPlayer(gameObject);
            if (court.readyToServe)
                court.readyToServe = false;
        }
    }

    public void EnableController()
    {
        controller.enabled = true;
    }

    public void EndRally()
    {
        block = false;
        squat = false;
        Block();
        Squat();
        if (slowDown)
        {
            
            ReturnSpeedToNormal();
            //this.enabled = false;
        }
        controller.enabled = false;
    }

    /*void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Ball" && hitting)
        {
            ReturnSpeedToNormal();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "Ball" && hitting)
        {
            ReturnSpeedToNormal();
        }
    }
    */
    void Aim()
    {
        Cursor.visible = true;
        if(!aimed)
        {
            
            Ray mousePoint = new Ray(Vector3.zero,Vector3.forward * ((float)VolleyballSide) * -1 + Vector3.up);
            if(isPlayer)
            {
                mousePoint = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector2 mousePos = Input.mousePosition;
                tempMousePos = mousePos;
            }
                
            
            aimDir = mousePoint.direction;
            aimDir = aimDir.normalized;
            power = MaxPower * minimumPower;
            if (isPlayer)
            {
                
                /*float deltaMousePos = Input.mousePosition.y - tempMousePos.y;
                if (deltaMousePos > 0)
                {
                    aimDir.y += deltaMousePos / (Screen.height / 2 * lobConst);
                    aimDir = aimDir.normalized;
                }
                else
                {
                    aimDir *= smashConst;
                }
                aimDir.y += Mathf.Sign(aimDir.y) * Mathf.Exp(Mathf.Abs(ball.transform.position.y) * -1 / tau) / divisionFactor;*/
            }
            if (aimDir.y > 0)
            {
                aimDir.y *= vBall.rb.mass;
            }
            if (Input.GetButtonDown("Fire") || !isPlayer)
            {
                m_MouseLook.SetCursorLock(false);
                m_MouseLook.look = false;
                cameraLook.look = false;
                aimed = true;
                pastBallVelocity = vBall.rb.velocity;
                vBall.rb.velocity = Vector3.zero;
                tempAimDir = aimDir;
            }
        }
        if(aimed)
        {
            HitTheBall();

        }
        if (isPlayer && hitting)
            PredictShot();
    }

    void HitTheBall()
    {
        
        float deltaMousePos = Input.mousePosition.y - tempMousePos.y;
        aimDir = tempAimDir;
        if (isPlayer)
        {

            if (deltaMousePos > 0)
            {
                float myLob = court.serve ? lobConst * serveLob : lobConst;
                aimDir.y += deltaMousePos / (Screen.height / 2 * myLob) * vBall.rb.mass;
                aimDir = aimDir.normalized;
            }
            else
            {
                aimDir *= smashConst;
            }
            aimDir.y += Mathf.Sign(aimDir.y) * Mathf.Exp(Mathf.Abs(vBall.rb.transform.position.y) * -1 / tau) / divisionFactor;            
            power = (Mathf.Abs(deltaMousePos) / (Screen.height / 2 * (1 / (1 - minimumPower))) + minimumPower) * MaxPower;
        }
        else
        {
            power = MaxPower * .75f;
        }
        if (Input.GetButtonDown("Cancel") && isPlayer)
        {
            m_MouseLook.SetCursorLock(true);
            m_MouseLook.look = true;
            cameraLook.look = true;
            aimed = false;
            vBall.rb.velocity = pastBallVelocity;
            return;
        }
        if (Input.GetButtonUp("Fire") || !isPlayer)
        {
            ReturnSpeedToNormal();
            shoot = true;
        }
        
    }

    void ShootBall()
    {
        vBall.rb.AddForce(aimDir * power);
    }

    void ReturnSpeedToNormal()
    {
        hitting = false;
        slowDown = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;
        aimed = false;
        hitTime = 0;
        if (isPlayer)
        {
            ResetMovement();
        }
    }

    void ResetMovement()
    {
        if (isPlayer)
        {
            controller.enabled = true;
            
        }
        
        mySystem.Clear();
        mySystem.Stop();
        m_MouseLook.SetCursorLock(true);
        m_MouseLook.look = true;
        cameraLook.look = true;

        print("Hello3 " + myParent.name);
        transform.eulerAngles = myParent.eulerAngles;
        transform.position = myParent.position + Vector3.up * .85f;
        transform.parent = myParent;
        print(transform.parent);
        //Physics.IgnoreCollision(ball.GetComponent<Collider>(), GetComponent<Collider>(), true);

        // controller.enabled = true;
    }

    void PredictShot()
    {
        if(mySystem.isStopped)
        {
            mySystem.Play();
        }
        if(mySystem.isPlaying)
        {
            mySystem.GetParticles(myParticle);
        }

        if(mySystem.particleCount > 0)
        {
            float powerConst = 7 *power / MaxPower;
            for(int i = 0; i < myParticle.Length; i++)
            {
                myParticle[i].position = vBall.rb.position + (aimDir * power / vBall.rb.mass/3.75f)* (i * powerConst* regFixedTimeDelta / 10)
                        - 10*Vector3.Scale(Physics.gravity, Physics.gravity) * Mathf.Pow(i * powerConst * regFixedTimeDelta /10,2);
            }
            mySystem.SetParticles(myParticle, myParticle.Length);
        }

    }
}
