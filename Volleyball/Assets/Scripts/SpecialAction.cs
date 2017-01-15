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
    public Side currentSide = Side.Left;

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
    public float AimingDetail { set {
            if (myAimingDetail != value)
            {
                myAimingDetail = value;
                myBurst[0].minCount = System.Convert.ToInt16( Mathf.Pow(myAimingDetail, 1.5f) * 200);
                myBurst[0].maxCount = System.Convert.ToInt16(Mathf.Pow(myAimingDetail, 1.5f) * 200);
                mySystem.emission.SetBursts(myBurst);
                if(hitting)
                {
                    StopAimingParticles();
                    StartAimingParticles();
                }
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
    float serveHitCooldown = .1f;
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
    Vector3 originAimDir;
    Vector2 originMousePos;
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

    Vector3 myLastVelocity;

    // Use this for initialization
    void Start () {
        
        //gets the object over this object in the object heirarchy
        myParent = transform.parent;

        //the component that controlls the physics of this player
        rb = myParent.GetComponent<Rigidbody>();

        //finds the arms used to block
        arms = myParent.Find("Arms");

        //finds the cameras for this player
        fpc = myParent.Find("FirstPersonCamera").GetComponent<Camera>();
        tpc = myParent.Find("ThirdPersonCamera").GetComponent<CameraFollow>();

        //finds the court which is used to refer to all the game specifics
        court = GameObject.FindGameObjectWithTag("CourtController").GetComponent<CourtScript>();

        //finds the ball and ball actiioins
        vBall = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();

        //Gets the movement part of the player
        controller = myParent.GetComponent<PlayerMovement>();

        //Gets the script that controlls the horizontal movement of the camera
        m_MouseLook = myParent.GetComponent<MyMouseLook>();

        //gets the component that controlls the vertical movement of the camera
        cameraLook = tpc.GetComponent<MyMouseLook>();

        //Gets the the component that controlls the collision bounds
        myCollider = myParent.GetComponent<CapsuleCollider>();

        //gets the component that controlls the ball collisions
        myTrigger = GetComponent<CapsuleCollider>();

        //if a person is controlling the player than lock the mouse to the center of the screen
        if (isPlayer)
            cameraLook.SetCursorLock(true);
        //if a computer is controlling the player turn off the cameras and the player movement controlled by 
        //the input
        else
        {
            controller.enabled = false;
            fpc.gameObject.SetActive(false);
            tpc.gameObject.SetActive(false);
        }

        //store the current time it takes for fixed update to run once
        regFixedTimeDelta = Time.fixedDeltaTime;

        //when you first start there is no cooldown to hit the ball
        hitTime = hitCooldown;

        //the component that controlls the amount of particles emmitted for the aiming
        mySystem = myParent.GetComponentInChildren<ParticleSystem>();
        myBurst = new ParticleSystem.Burst[1];

        //the original heights for the mesh, collider, and collider for the ball
        originColliderHeightY = new Vector2(myCollider.height, myCollider.center.y);
        originMeshHeightY = new Vector2(transform.localScale.y, transform.localPosition.y);
        originTriggerHeightY = new Vector2(myTrigger.height, myTrigger.center.y);

        //the predefined crouch heights for the mesh, collider, and the collider for the ball
        squatColliderHeightY = new Vector2(1, 0.5f);
        squatMeshHeightY = new Vector2(0.5f, 0.5f);
        squatTriggerHeightY = new Vector2((originMeshHeightY.x - squatMeshHeightY.x + 1) * originTriggerHeightY.x, originTriggerHeightY.y - .5f);

        //gets a reference to the menu
        gameUI = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InGameUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!hitting)
        AimingDetail = aimingDetail;

        //sets the cooldowns for hitting the ball
        //if you are serving, you have a smaller cooldown
        if (court.serve)
            hitCooldown = serveHitCooldown;
        else
            hitCooldown = normalHitCooldown;
    }

    //Fixed update is called several times a second
    void FixedUpdate ()
    {
        //if the rally is not over then preform all your normal actions
        if(!court.rallyOver)
        {
            //gets all the inputs for the player
            GetInput();

            //slows down the time when you are hitting the ball
            if (slowDown)
            {
                Time.timeScale = slowTimeScale;
            }

            //if you finished hitting the ball, then apply the force onto the ball
            if (shoot)
            {
                Shoot = true;
                shoot = false;
            }

            //allow for fixed update to run at normal speed even when you slow down time
            //this allows for you to controll your character in realtime otherwise the
            //controll is choppy
            Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;

            //increments the current time for the cooldown
            if (hitTime <= hitCooldown)
            {
                hitTime += Time.fixedDeltaTime;
            }

            //controlls the computer movement
            if (!isPlayer)
            {
                ComputerMovement();
            }

            //if you are in the process of hitting the ball, then allow the player to
            //aim and hit the ball
            if (hitting)
            {
                Aim();
            }

            //allow yourself to start blocking
            Block();
            
        }

        //when the rally's over then allow for all rally actions
        else
        {
            //moves the player to their starting positions;
            if (resetPosition)
            {
                resetPosition = false;
                rb.MovePosition(court.GetPosition(currentPosition, currentSide, this));
            }

            //if you are the server and its time to serve allow the ability to request the ball
            if (court.readyToServe && court.serveSide == currentSide && currentPosition == 1)
            {
                //left click the mouse to call for the ball
                if(Input.GetAxisRaw("Fire") == 1)
                {
                    hitTime = hitCooldown + 1;
                    court.GiveBallToServer();
                }
            }
        }

        //if your squatted then unsquat yourself
        Squat();
    }

    #region computerStuff
    //teleports to the location of the ball when it is on their side and it is at about head height
    void ComputerMovement()
    {
        //checks if the ball is currently on the same side as the person
        if (((int)currentSide) * vBall.transform.position.z > 0)
        {
            //if the ball is currently around head height
            //then teleports to the ball
            if(vBall.transform.position.y > 1.25f && vBall.transform.position.y < 1.75f)
            {
                float offset = .5f;
                MoveTowards(vBall.rb.position + ((int)currentSide) * Vector3.forward * offset);
            }
        }
    }

    //teleports to a position
    void MoveTowards(Vector3 position)
    {
        Vector3 myPosition = new Vector3(position.x, transform.position.y, position.z);
        rb.MovePosition(myPosition);
    }
    #endregion

    #region InputActions
    //turns the arms on and off when you want to block
    void Block()
    {
        arms.gameObject.SetActive(block);
    }

    //changes the height of the player when you want to squat and returns
    //the height back to normal when you don't want to squat
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

    //gets all the player input
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
    //turns the movement of the player on or off
    public void SetMovementEnabled(bool value)
    {
        controller.enabled = value;
    }

    //gives a reference to the current player movement
    public PlayerMovement GetController()
    {
        return controller;
    }

    //when the rally is over then unblock, unsquat, return back to normal speed
    //and stop the controll of movement
    public void EndRally()
    {
        block = false;
        squat = false;
        Block();
        Squat();
        ReturnSpeedToNormal();
        controller.enabled = false;
    }
    #endregion

    #region Shooting Ball
    //allows the player to aim the ball, and after aiming
    void Aim()
    {
        //make sure that the mouse is visible
        Cursor.visible = true;

        //if you are not finished aiming than aim the ball
        if(!aimed)
        {
            //set the default direction (mainly used by the computer)
            Ray mousePoint = new Ray(Vector3.zero,Vector3.forward * ((float)currentSide) * -1 + Vector3.up);

            //have the directon be where the mouse is currently pointing to
            if (isPlayer)
            {
                mousePoint = Camera.main.ScreenPointToRay(Input.mousePosition);

                //set the reference position used for the power/spin calculations
                Vector2 mousePos = Input.mousePosition;
                originMousePos = mousePos;
            }
            
            //normalize the current direction so that the magnitude == 1
            aimDir = mousePoint.direction;
            aimDir = aimDir.normalized;

            //set the current power to a dummy number so mainly used for the predictions
            power = MaxPower * minimumPower*1.5f;

            //reset the spin so that the previous hit has no affect on the current one
            ballSpin = Vector2.up;

            //if the aim direction is is pointed up, than add more power to the up direction
            //to add an arc
            if (aimDir.y > 0)
            {
                aimDir.y *= vBall.rb.mass;
            }

            //if you have left clicked then begin power calculations 
            //if you are a computer go straight to the power calculations
            if (Input.GetButtonDown("Fire") || !isPlayer)
            {
                if (isPlayer)
                    SetAimed(true);
                else
                    aimed = true;
                pastBallVelocity = vBall.rb.velocity;
                vBall.ResetMotion();
                originAimDir = aimDir;
            }
        }

        //if you are finished aiming, then start the power calculations
        if(aimed)
        {
            HitTheBall();
        }

        //predict where the ball will go
        if (isPlayer && hitting)
            PredictShot();
    }

    //calculates the power used to hit the ball
    void HitTheBall()
    {
        //the mouse position difference from the point where you aimed the ball,
        //and the current mouse position
        Vector2 deltaMousePos = (Vector2)Input.mousePosition - originMousePos;
        
        //reset the aim direction to the direction that you originally aimed at
        aimDir = originAimDir;

        float runHitConst = court.serve ? 1 : 10;
        
        //if you are the player then allow for you to change the power and spin the ball
        if (isPlayer)
        {
            //set the horizontal component of the spin based on the horizontal mouse position
            ballSpin.x = deltaMousePos.x / (Screen.width / 2) * maxSpin;
            
            //if the mouse's Y position is above the original mouse position, then do a lob pass
            if (deltaMousePos.y > 0)
            {
                //Allow the the adjustment of the arc by scrolling the mouse wheel
                lobAgjust += (Input.mouseScrollDelta.y) * lobScrollSpeed;
                if (lobAgjust < 0.01f)
                    lobAgjust = 0.01f;
                float myLob = lobConst * lobAgjust;
                
                //calculate the lob direction
                aimDir.y += deltaMousePos.y / (Screen.height / 2 * myLob) * vBall.rb.mass;
                aimDir = aimDir.normalized;

                //no floater of forward spin for a lob
                ballSpin.y = 1;
            }
            //if the mouse's Y position is above the original mouse position, then do a smash hit
            else
            {
                //add more force to the hit
                aimDir *= (smashConst + myLastVelocity.magnitude/ runHitConst);

                //use the mouse scroll to control the amount of forward spin on the ball
                if(ballSpin.y > 1.1f)
                    ballSpin.y -= (Input.mouseScrollDelta.y) * smashScrollSpeed * (1 + myLastVelocity.magnitude/ runHitConst);
                
                //when there no forward spin use the mouse scroll to control the strength of the
                //floater randomness
                else
                    ballSpin.y -= (Input.mouseScrollDelta.y) * smashScrollSpeed/10f;

                //set the max and min parts of the vertical spin
                if (ballSpin.y < 0)
                    ballSpin.y = 0;
                else if (ballSpin.y > (maxSpin* (1 + myLastVelocity.magnitude/ runHitConst) + 1))
                    ballSpin.y = maxSpin * (1 + myLastVelocity.magnitude/ runHitConst) + 1;
            }
            //when the ball is lower, add an inherent arc to the hit
            aimDir.y += Mathf.Sign(aimDir.y) * Mathf.Exp(Mathf.Abs(vBall.rb.transform.position.y) * -1 / tau) / divisionFactor;

            //calculate the power of the hit
            power = (Mathf.Abs(deltaMousePos.y) / (Screen.height / 2 * (1 / (1 - minimumPower))) + minimumPower) * MaxPower;
        }
        //if you are a computer always hit back at a constant power
        else
        {
            power = MaxPower * .75f;
        }
        //if you right clicked before you let go of the left click then cancel power calculations and 
        //go back to aiming
        if (Input.GetButtonDown("Cancel") && isPlayer)
        {
            SetAimed(false);
            vBall.rb.velocity = pastBallVelocity;
            return;
        }

        //if you let go of the left click then return speed back to normal and hit the ball
        if (Input.GetButtonUp("Fire") || !isPlayer)
        {
            ReturnSpeedToNormal();
            shoot = true;
        }
        
    }

    //set whether you have aimed or not, which will in turn adjust the 
    //camera movement accordingly
    void SetAimed(bool myAimed)
    {
        SetCameraMovement(!myAimed);
        lobAgjust = 1;
        aimed = myAimed;
    }

    //allow the cameras to move with the mouse
    public void SetCameraMovement(bool move)
    {
        m_MouseLook.SetCursorLock(move);
        m_MouseLook.look = move;
        cameraLook.look = move;
        tpc.allowZoom = move;
    }

    //add force and spin to the ball
    void ShootBall()
    {
        vBall.Shoot(aimDir * power,ballSpin, myParent);
    }


    //when the ball hits the collider that controls ball interactions then start the hitting the ball
    void OnTriggerEnter(Collider other)
    {
        //if the player is colliding with the ball and it the hit cooldown is over, and the rally 
        //is still ongoing then hit the ball
        if (other.tag == "Ball" && ((hitTime > hitCooldown && !court.rallyOver)) && !hitting)
        {

            hitting = true;
            slowDown = true;

            //makes it so that the player body doesn't rotate while your aiming
            if (isPlayer)
            {
                transform.parent = null;
                myLastVelocity = controller.rb.velocity;
                myLastVelocity.Scale(new Vector3(1, 0, 1));
                controller.rb.velocity = Vector3.zero;
                controller.enabled = false;
            }

            //if the court says that your ready to serve then tell them that you are currently
            //serving
            if (court.readyToServe)
                court.readyToServe = false;

            //tell the ball that you where the last player to hit it
            vBall.CollideWithPlayer(this);
        }
    }

    #endregion

    #region Reseting

    //returns the speed to normal and resets all hitting variabls
    void ReturnSpeedToNormal()
    {
        hitting = false;
        aimed = false;

        //returns time to normal
        slowDown = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;

        //starts the cooldown for hiting the ball
        hitTime = 0;

        //if the player is not a computer then enable movement
        if (isPlayer)
        {
            ResetMovement();
        }
    }

    //allows camera and player movement
    void ResetMovement()
    {
        //enable the control of movement
        if (isPlayer)
            controller.enabled = true;

        //stop using the particles for predicting where the ball will go
        StopAimingParticles();

        //allow the camera to move
        SetCameraMovement(true);

        //makes sure that the player body is in the center of where the camera rotates
        transform.eulerAngles = myParent.eulerAngles;
        transform.position = myParent.position + Vector3.up * .85f;
        transform.parent = myParent;
    }

    //stops player and camera movement
    public void Pause(bool pause)
    {
        controller.enabled = !pause;
        SetCameraMovement(!pause);
    }
    #endregion

    #region Prediction
    
    //starts the particle creations
    void StartAimingParticles()
    {
        if(mySystem.isStopped)
        {
            mySystem.Play();
            mySystem.emission.GetBursts(myBurst);

            myParticle = new ParticleSystem.Particle[myBurst[0].minCount];
        }

        //gets all the current particles that are alive
        if(mySystem.isPlaying)
        {
            mySystem.GetParticles(myParticle);
        }
    }

    //stops the particle creation and deletes all particles
    void StopAimingParticles()
    {
        if(mySystem.isPlaying)
        {
            mySystem.Clear();
            mySystem.Stop();
        }
    }


    //predicts where the ball will go
    void PredictShot()
    {
        //creates and gets the active particles
        StartAimingParticles();
        if(mySystem.particleCount > 0)
        {
            //increase the distance between the particles as the power increases
            float powerConst = 7 *power / MaxPower / 10 * 30/ mySystem.particleCount;

            float time = 0;

            //used to calculate the position of a floater hit (not always accurate
            Vector3 floaterRef = Vector3.zero;
            for(int i = 0; i < myParticle.Length; i++)
            {
                
                time = powerConst * regFixedTimeDelta;
                //calculate the the position of where the ball is going to be at a specific point in time for each particle
                myParticle[i].position = vBall.rb.position + PredictAddedForce(i * time) + PredictGravity(i * time) + PredictSpin(time, i, ref floaterRef);
            }

            //move the particles to the predicted position
            mySystem.SetParticles(myParticle, myParticle.Length);
        }

    }

    //predict the affect of the force onto the ball
    Vector3 PredictAddedForce(float time)
    {
        return (aimDir * power / vBall.rb.mass / 3.75f) * time;
    }

    //predict the affect of gravity on the ball
    Vector3 PredictGravity(float time)
    {
        return -10 * Vector3.Scale(Physics.gravity, Physics.gravity) * Mathf.Pow(time, 2);
    }

    //predict the affect of the spin on the ball
    Vector3 PredictSpin(float time, int i, ref Vector3 floaterRef)
    {
        
        Vector3 predictedSpin = Vector3.zero;
        //if the ball has spin predict where it will go
        if (ballSpin.y >= 1)
        {
            float rate = (1 - Mathf.Pow(vBall.SpinAddConst, time * i / regFixedTimeDelta)) / (1 - vBall.SpinAddConst) * 1.75f;
            predictedSpin = Vector3.Scale(ballSpin - Vector2.up, -Vector3.up * rate + Vector3.right * rate) * time * i;
        }
        //if the ball is a floater then predict where it will go
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
