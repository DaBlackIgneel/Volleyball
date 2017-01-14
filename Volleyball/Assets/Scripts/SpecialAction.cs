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
    public float lobAgjust = 1f;
    public float maxSpin = 5;

    [Range(0.2f,1)]
    public float aimingDetail = .5f;
    private float myAimingDetail;
    float AimingDetail { set {
            if (myAimingDetail != value)
            {
                myAimingDetail = value;
                myBurst[0].minCount = System.Convert.ToInt16( Mathf.Pow(myAimingDetail, 1.5f) * 200);
                myBurst[0].maxCount = System.Convert.ToInt16(Mathf.Pow(myAimingDetail, 1.5f) * 200);
                mySystem.emission.SetBursts(myBurst);
            }
        } }


    PlayerMovement controller;
    MyMouseLook m_MouseLook;
    Transform myParent;

    bool block;
    bool squat;
    bool hitting;
    bool aimed;

    float hitCooldown = 1;
    float normalHitCooldown = 1;
    float serveHitCooldown = .2f;
    [SerializeField] float hitTime = 0;
    bool slowDown;
    float slowTimeScale = .01f;

    float power = 1;
    
    float regFixedTimeDelta;
    InGameUI gameUI;

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
    CameraFollow tpc;
    Camera fpc;
    MyMouseLook cameraLook;

    Vector3 aimDir;
    Vector3 tempAimDir;
    Vector2 tempMousePos;
    [SerializeField]
    Vector2 ballSpin;
    [SerializeField]
    [Range(0.1f, 4)]
    float smashScrollSpeed = .5f;

    [SerializeField]
    [Range(0.01f, 2)]
    float lobScrollSpeed = .25f;

    ParticleSystem mySystem;
    ParticleSystem.Burst[] myBurst;
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
        tpc = myParent.Find("ThirdPersonCamera").GetComponent<CameraFollow>();
        court = GameObject.FindGameObjectWithTag("Court").GetComponent<CourtScript>();
        vBall = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();
        controller = myParent.GetComponent<PlayerMovement>();
        m_MouseLook = myParent.GetComponent<MyMouseLook>();
        myCollider = myParent.GetComponent<CapsuleCollider>();
        myTrigger = GetComponent<CapsuleCollider>();
        cameraLook = tpc.GetComponent<MyMouseLook>();
        
        if (isPlayer)
            cameraLook.SetCursorLock(true);
        else
        {
            controller.enabled = false;
            fpc.gameObject.SetActive(false);
            tpc.gameObject.SetActive(false);
        }

        regFixedTimeDelta = Time.fixedDeltaTime;

        hitTime = hitCooldown;

        mySystem = myParent.GetComponentInChildren<ParticleSystem>();
        myBurst = new ParticleSystem.Burst[1];

        originColliderHeightY = new Vector2(myCollider.height, myCollider.center.y);
        originMeshHeightY = new Vector2(transform.localScale.y, transform.localPosition.y);
        originTriggerHeightY = new Vector2(myTrigger.height, myTrigger.center.y);
        squatColliderHeightY = new Vector2(1, 0.5f);
        squatMeshHeightY = new Vector2(0.5f, 0.5f);
        squatTriggerHeightY = new Vector2((originMeshHeightY.x - squatMeshHeightY.x + 1) * originTriggerHeightY.x, originTriggerHeightY.y - .5f);

        gameUI = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InGameUI>();
    }
	
    void Update()
    {
        if (!hitting)
            AimingDetail = aimingDetail;
        if (court.serve)
            hitCooldown = serveHitCooldown;
        else
            hitCooldown = normalHitCooldown;
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
        Squat();
    }

    #region computerStuff
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
    #endregion

    #region InputActions
    void Block()
    {
        arms.gameObject.SetActive(block);
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
            gameUI.SetToggleMenu(Input.GetButtonDown("Menu"), this);
        }
            
    }
    #endregion

    #region court methods
    public void EnableController()
    {
        controller.enabled = true;
    }

    public PlayerMovement GetController()
    {
        return controller;
    }

    public void EndRally()
    {
        block = false;
        squat = false;
        Block();
        Squat();
        //if (slowDown)
        //{
            
            ReturnSpeedToNormal();
            //this.enabled = false;
        //}
        controller.enabled = false;
    }
    #endregion

    #region Shooting Ball
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
            power = MaxPower * minimumPower*2;
            ballSpin = Vector2.up;
            if (aimDir.y > 0)
            {
                aimDir.y *= vBall.rb.mass;
            }
            if (Input.GetButtonDown("Fire") || !isPlayer)
            {
                SetAimed(true);
                pastBallVelocity = vBall.rb.velocity;
                vBall.ResetMotion();
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
            float deltaMousePosX = Input.mousePosition.x - tempMousePos.x;
            ballSpin.x = deltaMousePosX / (Screen.width / 2) * maxSpin;
            
            if (deltaMousePos > 0)
            {
                lobAgjust += (Input.mouseScrollDelta.y) * lobScrollSpeed;
                if (lobAgjust < 0.01f)
                    lobAgjust = 0.01f;
                float myLob = lobConst * lobAgjust;
                
                aimDir.y += deltaMousePos / (Screen.height / 2 * myLob) * vBall.rb.mass;
                aimDir = aimDir.normalized;
                ballSpin.y = 1;
            }
            else
            {
                aimDir *= smashConst;
                if(ballSpin.y > 1.1f)
                    ballSpin.y -= (Input.mouseScrollDelta.y) * smashScrollSpeed;
                else
                    ballSpin.y -= (Input.mouseScrollDelta.y) * smashScrollSpeed/5f;
                if (ballSpin.y < 0)
                    ballSpin.y = 0;
                else if (ballSpin.y > maxSpin + 1)
                    ballSpin.y = maxSpin + 1;
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
            SetAimed(false);
            vBall.rb.velocity = pastBallVelocity;
            return;
        }
        if (Input.GetButtonUp("Fire") || !isPlayer)
        {
            ReturnSpeedToNormal();
            shoot = true;
        }
        
    }

    void SetAimed(bool myAimed)
    {
        m_MouseLook.SetCursorLock(!myAimed);
        m_MouseLook.look = !myAimed;
        cameraLook.look = !myAimed;
        tpc.allowZoom = !myAimed;
        lobAgjust = 1;
        aimed = myAimed;
    }

    void ShootBall()
    {
        vBall.Shoot(aimDir * power,ballSpin, myParent);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball" && ((hitTime > hitCooldown && !court.rallyOver)) && !hitting)
        {
            hitting = true;
            slowDown = true;
            if (isPlayer)
            {
                transform.parent = null;
                controller.rb.velocity = Vector3.zero;
                controller.enabled = false;
            }
            if (court.readyToServe)
                court.readyToServe = false;
            vBall.CollideWithPlayer(this);
        }
    }

    #endregion

    #region Reseting
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
        SetAimed(false);

        transform.eulerAngles = myParent.eulerAngles;
        transform.position = myParent.position + Vector3.up * .85f;
        transform.parent = myParent;
    }

    public void Pause(bool pause)
    {
        controller.enabled = !pause;
        SetAimed(pause);
    }
    #endregion


    #region Prediction
    void PredictShot()
    {
        if(mySystem.isStopped)
        {
            mySystem.Play();
            mySystem.emission.GetBursts(myBurst);
            
            myParticle = new ParticleSystem.Particle[myBurst[0].minCount];
        }
        if(mySystem.isPlaying)
        {
            mySystem.GetParticles(myParticle);
        }

        if(mySystem.particleCount > 0)
        {
            float powerConst = 7 *power / MaxPower / 10 * 30/ mySystem.particleCount;
            float time = 0;
            Vector3 floaterRef = Vector3.zero;
            for(int i = 0; i < myParticle.Length; i++)
            {
                
                time = powerConst * regFixedTimeDelta;
                myParticle[i].position = vBall.rb.position + PredictAddedForce(i * time) + PredictGravity(i * time) + PredictSpin(time, i, ref floaterRef);
            }
            mySystem.SetParticles(myParticle, myParticle.Length);
        }

    }

    Vector3 PredictAddedForce(float time)
    {
        return (aimDir * power / vBall.rb.mass / 3.75f) * time;
    }

    Vector3 PredictGravity(float time)
    {
        return -10 * Vector3.Scale(Physics.gravity, Physics.gravity) * Mathf.Pow(time, 2);
    }

    Vector3 PredictSpin(float time, int i, ref Vector3 floaterRef)
    {
        
        Vector3 predictedSpin = Vector3.zero;
        if (ballSpin.y >= 1)
        {
            float rate = (1 - Mathf.Pow(vBall.SpinAddConst, time * i / regFixedTimeDelta)) / (1 - vBall.SpinAddConst) * 1.75f;
            predictedSpin = Vector3.Scale(ballSpin - Vector2.up, -Vector3.up * rate + Vector3.right * rate) * time * i;
        }
        else
        {
            predictedSpin = (Vector3.up * vBall.GetFloater(time * i / regFixedTimeDelta, ballSpin.y, Direction.Y) + Vector3.right * vBall.GetFloater(time * i/ regFixedTimeDelta, ballSpin.y, Direction.X)) * time*40;
            predictedSpin += floaterRef;
            floaterRef = predictedSpin;
        }
        return predictedSpin;
    }
    #endregion
}
