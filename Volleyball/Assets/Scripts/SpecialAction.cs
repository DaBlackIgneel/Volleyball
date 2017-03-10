using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityStandardAssets.Characters;

public enum Side { Left = -1, Right = 1}

public struct PredictionPack
{
    public float time;
    public Vector3 position;

    public PredictionPack(float mTime, Vector3 mPosition)
    {
        time = mTime;
        position = mPosition;
    }
}


public class SpecialAction : MonoBehaviour {
    public GameObject BallLandingIndicator;

    public Vector3 aimSpot;
    //Vector3 spot;

    public float tester1;
    public float tester2;
    public float tester3;
    public float yScale;

    [Range(0, 2000)]
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
    public Team team;

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

    public Vector3 distanceToBallLanding;

    public PlayerMovement myMovement
    {
        get { return movement; }
    }

    public CourtScript Court
    {
        get { return court; }
    }

    public float MaxJumpHeight
    {
        get
        {
            float time = movement.jumpSpeed / Mathf.Abs(Physics.gravity.y);
            return time * movement.jumpSpeed + time * time * Physics.gravity.y * .5f + HEIGHT;
        }
    }

    public bool Hitting
    {
        get
        {
            return hitting;
        }
    }

    public float armWaveSensitivity = .5f;

    [System.NonSerialized]
    public bool goToBall;
    PlayerMovement movement;
    MyMouseLook m_MouseLook;
    Transform myParent;

    public bool block;
    bool blockOver;
    public bool squat;
    bool dive;
    bool hitting;
    bool aimed;

    float hitCooldown = 1;
    float hitCooldownTimer = 0;
    float normalHitCooldown = 1;
    float serveHitCooldown = .1f;
    float startHitTime;
    public float hitTime = 0;
    float maxHitTime = 5;

    bool slowDown;
    float slowTimeScale = .01f;

    float power = 1;
    
    float regFixedTimeDelta;
    [System.NonSerialized]
    public InGameUI gameUI;

    bool Shoot
    {
        set{ if(value){ ShootBall();}}
    }

    bool IgnoreCollision;    
    Vector3 pastBallVelocity;
    Rigidbody rb;
    [System.NonSerialized]
    public VolleyballScript vBall;

    GameObject aimSpotIndicator;

    CourtScript court;
    Transform arms;
    MyMouseLook rightArm;
    MyMouseLook leftArm;
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
    bool cancelAim;

    [SerializeField]
    [Range(0.01f, 2)]
    float lobScrollSpeed = .25f;

    ParticleSystem mySystem;
    ParticleSystem.Burst[] myBurst;
    ParticleSystem.Particle[] myParticle;

    CapsuleCollider myCollider;
    CapsuleCollider myTrigger;

    float squatThreshhold = 0.01f;
    Vector2 originMeshHeightY;
    Vector2 squatMeshHeightY;
    Vector2 originColliderHeightY;
    Vector2 squatColliderHeightY;
    Vector2 originTriggerHeightY;
    Vector2 squatTriggerHeightY;
    Vector3 originalGroundPosition;
    Vector3 squatGroundPosition;

    Vector3 originalPosition;

    Vector2 armWaveRotation;

    Vector3 myLastVelocity;
    bool hittable;
    bool hit;

    //computer variables
    bool decided;
    Vector3 destination;
    [SerializeField]
    int serveStage;
    bool smash;
    float waitServeCooldown = 1;
    float waitCount;
    float waitCooldown = 1f;
    float aimAngle = 0;
    [SerializeField]
    float aimHeight = 1.1f;
    bool runServe;
    [SerializeField]
    float servePower = .75f;
    delegate void ServeOfChoice();
    List<ServeOfChoice> myServes;
    int serveChoice;

    CapsuleCollider[] myColliders;
    Vector3 startingPosition;

    float HEIGHT = 2f;
    float SQUAT_HEIGHT = 1f;
    float WALK_LENGTH = 2;
    float jumpError = .1f;
    float rotationAngle;
    bool stop;
    bool printing;
    bool shootUp;
    float downArm = 0;
    float blockArmRotation;

    int movementIndex;
    bool dontHit;
    int hitMode = 0;
    bool changeHitMode = true;
    bool changeChangeHitMode = true;

    // Use this for initialization
    void Start () {
        myServes = new List<ServeOfChoice>();
        myServes.Add(NormalServe);
        myServes.Add(RunningServe);
        //gets the object over this object in the object heirarchy
        myParent = transform.parent;
        armWaveRotation = Vector2.zero;
        myColliders = myParent.parent.GetComponents<CapsuleCollider>();
        //the component that controlls the physics of this player
        rb = myParent.parent.GetComponent<Rigidbody>();

        //finds the arms used to block
        arms = myParent.parent.Find("Arms");
        rightArm = arms.Find("RightArmPivot").GetComponent<MyMouseLook>();
        leftArm = arms.Find("LeftArmPivot").GetComponent<MyMouseLook>();

        //finds the cameras for this player
        fpc = myParent.parent.Find("FirstPersonCamera").GetComponent<Camera>();
        tpc = myParent.parent.Find("ThirdPersonCamera").GetComponent<CameraFollow>();

        //finds the court which is used to refer to all the game specifics
        court = GameObject.FindGameObjectWithTag("CourtController").GetComponent<CourtScript>();

        //finds the ball and ball actiioins
        vBall = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();

        //Gets the movement part of the player
        movement = myParent.parent.GetComponent<PlayerMovement>();

        //Gets the script that controlls the horizontal movement of the camera
        m_MouseLook = myParent.parent.GetComponent<MyMouseLook>();

        //gets the component that controlls the vertical movement of the camera
        cameraLook = tpc.GetComponent<MyMouseLook>();

        //Gets the the component that controlls the collision bounds
        myCollider = myParent.parent.GetComponent<CapsuleCollider>();

        //gets the component that controlls the ball collisions
        //myTrigger = myParent.parent.GetComponent<CapsuleCollider>();
        //if (isPlayer)
            myTrigger = myColliders[myColliders.Length - 1];

        //if a person is controlling the player than lock the mouse to the center of the screen
        if (isPlayer)
            cameraLook.SetCursorLock(true);
        //if a computer is controlling the player turn off the cameras and the player movement controlled by 
        //the input
        else
        {
            movement.relativeMovement = false;
            fpc.gameObject.SetActive(false);
            tpc.gameObject.SetActive(false);
            m_MouseLook.enabled = false;
            cameraLook.enabled = false;
        }

        //store the current time it takes for fixed update to run once
        regFixedTimeDelta = Time.fixedDeltaTime;

        //when you first start there is no cooldown to hit the ball
        hitCooldownTimer = hitCooldown;

        //the component that controlls the amount of particles emmitted for the aiming
        mySystem = myParent.parent.GetComponentInChildren<ParticleSystem>();
        myBurst = new ParticleSystem.Burst[1];

        //the original heights for the mesh, collider, and collider for the ball
        originColliderHeightY = new Vector2(myCollider.height, myCollider.center.y);
        originMeshHeightY = new Vector2(transform.localScale.y, transform.localPosition.y);
        originTriggerHeightY = new Vector2(myTrigger.height, myTrigger.center.y);
        originalGroundPosition = movement.ground.localPosition;

        //the predefined crouch heights for the mesh, collider, and the collider for the ball
        squatColliderHeightY = new Vector2(1,1f);
        squatMeshHeightY = new Vector2(0.5f, 0.5f);
        //squatTriggerHeightY = new Vector2((originMeshHeightY.x - squatMeshHeightY.x + 2) * originTriggerHeightY.x, originTriggerHeightY.y - 1.5f);
        squatGroundPosition = originalGroundPosition + Vector3.up *.3f;
        squatTriggerHeightY = new Vector2(originTriggerHeightY.x, originTriggerHeightY.y-.75f);
        //gets a reference to the menu
        gameUI = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InGameUI>();

        originalPosition = transform.localPosition;

        myLastVelocity = Vector3.zero;
        startingPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!hitting)
        AimingDetail = aimingDetail;

        if (isPlayer)
        {
            gameUI.SetToggleMenu(Input.GetButtonDown("Menu"), this);
            
        }
        if(!isPlayer && BallLandingIndicator != null)
        {
            //spot = aimSpot;
            //spot.z *= (int)CourtScript.OppositeSide(currentSide);
            if (aimSpotIndicator == null)
                aimSpotIndicator = Instantiate(BallLandingIndicator, aimSpot, Quaternion.identity);
            else
                aimSpotIndicator.transform.position = aimSpot;
            aimSpotIndicator.GetComponent<MeshRenderer>().material.color = Color.green;
        }

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
        Movement();
        //if the rally is not over then preform all your normal actions
        if (!court.rallyOver)
        {
            //gets all the inputs for the player
            if(isPlayer)
                GetInput();

            //slows down the time when you are hitting the ball
            

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
            if (hitCooldownTimer <= hitCooldown)
            {
                hitCooldownTimer += regFixedTimeDelta;
            }
            //when the cooldown is over then allow player to hit the ball
            else
                //checks to see if player is in contact with ball
                CheckHit();

            
            
        }

        //when the rally's over then allow for all rally actions
        else
        {
            
            //moves the player to their starting positions;
            if (resetPosition)
            {
                ResetPosition();
            }

            //if you are the server and its time to allow the server to request the ball
            CheckServe();
        }

        //if your squatted then unsquat yourself
        Squat();
        //allow yourself to start blocking
        Block();
    }

    void Movement()
    {
        if (isPlayer)
        {
            if (!stop)
                movement.Move(Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxis("Vertical"));
            else
            {
                movement.Stop();
            }
        }
        else
        {
            ComputerMovement();
        }
    }

    void CheckHit()
    {
        if (hittable && !hitting)
        {
            if (isPlayer)
            {
                if (Input.GetButton("Fire") || Input.GetButtonDown("Fire"))
                {
                    HitSetUp();
                }
            }
            else
            {
                if(!dontHit)
                    DecideShot();
            }
        }
        if (hitting)
        {
            if (isPlayer)
            {
                Aim();
                hitTime = Time.realtimeSinceStartup - startHitTime;

            }
            else
                DecideShot();
        }
        else
        {
            hitTime = 0;
        }
        if (hitTime >= maxHitTime && !court.readyToServe && hitTime < 900)
        {
            EndHit();
        }
    }

    void CheckServe()
    {
        if (court.readyToServe && court.serveSide == currentSide && currentPosition == 1)
        {
            if (isPlayer)
            {
                //left click the mouse to call for the ball
                if (Input.GetAxisRaw("Fire") == 1)
                {
                    hitTime = 999;
                    court.GiveBallToServer();
                }
            }
            else
            {
                PickServe();
            }
        }
    }

    #region computerStuff


    

    /*Vector3 MovePositionAtMaxHeight(float maxHeight)
    {
        Vector3 predictedPosition = Vector3.zero;
        
        try
        {
            Vector3 distance = vBall.transform.position - movement.transform.position;
            Vector3 ballVelocity = vBall.rb.velocity;
            float speed = movement.sprintSpeed;
            float direction = Mathf.Asin((distance.z/distance.x * ballVelocity.x - ballVelocity.z)/Mathf.Sqrt(speed * speed + Mathf.Pow(distance.z/distance.x * speed,2))) + Mathf.Atan(distance.z/distance.x);
            float time = distance.x / (speed * Mathf.Cos(direction) - ballVelocity.x);
            Vector3 ballPosition = vBall.transform.position + ballVelocity * time + .5f * Physics.gravity * time * time;
            if(ballPosition.y > PredictBallPosition(maxHeight).position.y)
            {
                predictedPosition = PredictBallPosition(maxHeight).position;
            }
            else
            {
                predictedPosition = PredictBallLanding();
            }
        }
        catch(System.Exception e)
        {
            predictedPosition = PredictBallLanding();
            print(e.Message);
        }
        return predictedPosition;
    }*/

    //teleports to the location of the ball when it is on their side and it is at about head height
    void ComputerMovement()
    {
        /*checks if the ball is currently on the same side as the person
        if (((int)currentSide) * vBall.transform.position.z > 0)
        {
            //if the ball is currently around head height
            //then teleports to the ball
            if(vBall.transform.position.y > 1.25f && vBall.transform.position.y < 1.75f)
            {
                float offset = .5f;
                MoveTowards(vBall.rb.position + ((int)currentSide) * Vector3.forward * offset);
            }
        }*/
        if((court.serve && !court.readyToServe && court.serveSide != currentSide) || !court.serve)
        {
            if (team.currentMode == StrategyType.Defense)
            {
                movementIndex = 0;
                dontHit = false;
                DefensiveMovement();
            }
            else
            {
                OffensiveMovement();
            }
        }
        else
        {
            if (decided)
            {
                ComputerActions();
            }
        }
        
    }

    void OffensiveMovement()
    {
        Path myPath = LocationRelation.GetMovement(this);
        movement.goToRotation = false;
        squat = false;

        if (movement.isGrounded)
            block = false;
        else
            movement.jump = false;
        dontHit = !goToBall;
        if (myPath.size == 0)
        {
            if (goToBall)
            {
                
                movement.MoveTowards(court.LocalRelate.aimSpot);
            }
            else
                ReturnToPosition();
        }
        else
        {
            Vector3 movementLocation = movementIndex < myPath.size? LocationRelation.PathLocationToCourt(myPath.points[movementIndex], currentSide): LocationRelation.PathLocationToCourt(myPath.points[movementIndex-1], currentSide);
            if (movementIndex < myPath.size)
            {
                if (Vector3.ProjectOnPlane(transform.position - movementLocation, Vector3.up).magnitude < .2f)
                    movementIndex++;
                else
                    movement.MoveTowards(movementLocation,myPath.walkToThisPoint[movementIndex]);
            }
            else
            {
                movement.Stop();
            }
            if (movement.isGrounded)
            {
                float jumpHighestTime = Mathf.Abs(movement.jumpSpeed / Physics.gravity.y);
                float myOffset = myPath.timeOffset > jumpHighestTime ? jumpHighestTime : myPath.timeOffset;
                if (myPath.shouldJump)
                {
                    if (myPath.jumpTime == JumpTime.EndOfPath && movementIndex == myPath.size - 1 && Vector3.ProjectOnPlane(transform.position - movementLocation, Vector3.up).magnitude < .5f)
                    {
                        if (myPath.stopJump)
                            Stop();
                        movement.jump = true;
                    }
                    else if (court.LocalRelate.TimeTillBallReachesLocation < myOffset && court.LocalRelate.TimeTillBallReachesLocation > -1f)
                    {
                        if (myPath.jumpTime == JumpTime.BeforeSetterReceivesBall && vBall.GetSideTouches(currentSide) == 1)
                        {
                            if (myPath.stopJump)
                                Stop();
                            movement.jump = true;
                        }
                        else if (myPath.jumpTime == JumpTime.BeforeAttackerReceivesBall && vBall.GetSideTouches(currentSide) == 2)
                        {
                            if (myPath.stopJump)
                                Stop();
                            movement.jump = true;
                        }
                    }
                }
            }
            if (movement.transform.position.y > .5f)
            {
                Stop();
                movement.jump = false;
            }
        }
    }

    void DefensiveMovement()
    {
        goToBall = false;
        bool shouldBlock = team.GetStrategy(StrategyType.Defense).myPosition[currentPosition - 1] == DefensePosition.Blocker;
        if (ShouldMove() && movement.isGrounded)
            GetBall();
        else
        {
            LocationRelation.BallToPlayer personBallIsGoingTo = court.LocalRelate.personBallIsGoingTo;
            if (shouldBlock)
            {
                team.AddBlocker(this);
                movement.faceNet = true;

                //movement.transform.eulerAngles = Vector3.up * rotationAngle;
                if (vBall.GetSideTouches(CourtScript.OppositeSide(currentSide)) > 1)
                {

                    Vector3 followPlayer = vBall.transform.position;
                    if (personBallIsGoingTo != null && personBallIsGoingTo.angle < 12)
                        followPlayer = personBallIsGoingTo.player.transform.position;
                    followPlayer.z = 0.5f * (float)currentSide;
                    followPlayer.y = transform.position.y;
                    //print("Original" + currentPosition + ": " + followPlayer);
                    followPlayer += team.GetBlockSide(this, followPlayer);
                    //print("New" + currentPosition + ": " + followPlayer);
                    //bool walking = Mathf.Abs(followPlayer.x - transform.position.x) < 5;
                    movement.MoveTowards(followPlayer);//, walking);
                    
                    if(personBallIsGoingTo != null)
                    {
                        float minimumAirTime = 0.25f;
                        float maximumAirTime = Mathf.Abs(personBallIsGoingTo.player.movement.jumpSpeed / Physics.gravity.y) - .05f;
                        float jumpTimeInAir = personBallIsGoingTo.player.movement.jumpSpeed * minimumAirTime + Physics.gravity.y * minimumAirTime * minimumAirTime / 2;
                        float maxJumpTimeInAir = personBallIsGoingTo.player.movement.jumpSpeed * maximumAirTime + Physics.gravity.y * maximumAirTime * maximumAirTime / 2;
                        if (!personBallIsGoingTo.player.movement.isGrounded && personBallIsGoingTo.player.movement.transform.position.y > jumpTimeInAir && movement.isGrounded)
                        {
                            if (Vector3.ProjectOnPlane(followPlayer - transform.position, Vector3.up).magnitude < 2)
                            {
                                if (Mathf.Abs(followPlayer.x - transform.position.x) < .2f || personBallIsGoingTo.player.movement.transform.position.y > maxJumpTimeInAir)
                                {
                                    Stop();
                                    movement.jump = true;
                                    block = true;
                                    armWaveRotation.x = 2.5f;

                                    if (personBallIsGoingTo.player.movement.transform.position.y > maxJumpTimeInAir)
                                    {
                                        blockArmRotation = -1 * (Mathf.Atan2(2, followPlayer.x - transform.position.x) * Mathf.Rad2Deg - 90);
                                        armWaveRotation.x = -5f;
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {
                                movement.jump = true;
                                block = true;
                                blockArmRotation = -1 * (Mathf.Atan2(1, followPlayer.x - transform.position.x) * Mathf.Rad2Deg - 90);
                                armWaveRotation.x = -5;
                            }
                        }
                        //movement.MoveTowards(followPlayer, personBallIsGoingTo.movement.isWalking);
                        
                        //print("moving towards player");
                    }
                    if (!movement.isGrounded)
                        movement.jump = false;
                }
                else
                {
                    //if(!movement.isGrounded)
                        //movement.transform.eulerAngles = Vector3.up * rotationAngle;
                    ReturnToPosition();
                }
            }
            else
            {
                ReturnToPosition();
            }
        }
    }

    bool ShouldMove()
    {
        Vector3 ballPos = court.LocalRelate.PredictBallLanding();
        bool shouldBlock = team.GetStrategy(StrategyType.Defense).myPosition[currentPosition -1] == DefensePosition.Blocker && team.currentMode == StrategyType.Defense;
        return ((CourtScript.FloatToSide(ballPos.z) == currentSide && !shouldBlock) || CourtScript.FloatToSide(vBall.transform.position.z) == currentSide )&& IsSupposedToGetBall() && ((court.LocalRelate.personBallIsGoingTo == null && vBall.GetSideTouches(CourtScript.OppositeSide(currentSide)) < 2) || (court.LocalRelate.personBallIsGoingTo != null && court.LocalRelate.personBallIsGoingTo.player.currentSide == currentSide));
    }

    void ReturnToPosition()
    {
        movement.faceNet = true;
        squat = false;
        if (movement.isGrounded)
            block = false;
        movement.MoveTowards(court.GetPosition(currentPosition, currentSide, this));

    }

    bool IsSupposedToGetBall()
    {
        return court.LocalRelate.ClosestPlayerToBall[currentSide] == this;
    }

    void GetBall()
    {
        //PredictionPack pp = PredictBallPosition(HEIGHT);
        //float moveTime = PredictMovementTime(pp.position);
        movement.goToRotation = false;
        movement.faceNet = false;
        if (movement.isGrounded)
            block = false;
        Vector3 landing = court.LocalRelate.MovePositionAtMaxHeight(MaxJumpHeight - 2*jumpError, movement);
        Vector3 ballDistance = vBall.transform.position - landing;
        if (landing.y > HEIGHT && !movement.jumping)
        {
            float time = (-vBall.rb.velocity.y - Mathf.Sqrt(Mathf.Pow(vBall.rb.velocity.y, 2) - 4 * Physics.gravity.y * ballDistance.y)) / (2 * Physics.gravity.y);
            float jumpDistance = time * movement.jumpSpeed + .5f * time * time * Physics.gravity.y + HEIGHT;
            float distanceToLanding = Vector3.Distance(vBall.rb.velocity * time + vBall.transform.position + Physics.gravity * time * time / 2, landing);
            if (jumpDistance < landing.y + jumpError && distanceToLanding < 2 && jumpDistance > landing.y && movement.isGrounded)
                movement.jump = true;
            
        }
        if (!movement.isGrounded)
            movement.jump = false;
        Vector3 distance = landing - movement.transform.position;
        distance = Vector3.ProjectOnPlane(distance, Vector3.up);
        bool shouldSquat = landing.y <= SQUAT_HEIGHT + .5f && vBall.transform.position.y < (1f - vBall.rb.velocity.y / 2f) && distance.magnitude < WALK_LENGTH * 3 / 2f && (!movement.jumping);
        bool shouldWalk = (distance - distance.y * Vector3.up).magnitude < WALK_LENGTH && (!movement.jumping);// && vBall.transform.position.y > 4;
        if (shouldWalk && movement.jump)
            Stop();
        else
            movement.MoveTowards(landing, shouldWalk);
        squat = shouldSquat;
        if (!isPlayer)
        {
            if (GameObject.Find(BallLandingIndicator.name) == null)
                BallLandingIndicator = Instantiate(BallLandingIndicator, landing, Quaternion.identity);
            else
                BallLandingIndicator.transform.position = landing;
        }
    }

    void Stop()
    {
        movement.Stop();
        squat = false;
    }

    //teleports to a position
    void TeleportTo(Vector3 position)
    {
        Vector3 myPosition = new Vector3(position.x, transform.position.y, position.z);
        rb.MovePosition(myPosition);
    }

    void DecideShot()
    {
        if(court.serve && currentPosition == 1 & court.serveSide == currentSide)
        {
            if (hittable && !hitting)
                hitting = true;
            PickServe();
        }
        else if(((!court.readyToServe && court.serveSide != currentSide)|| !court.serve)&& !hitting)
        {
            ComputerAim();
            ComputerHit();
        }
    }

    #region ComputerServe
    void PickServe()
    {
        if(court.readyToServe && !decided)
        {
            serveStage = 0;
            decided = true;
            waitCount = 0;
            runServe = false;
            serveChoice = Mathf.RoundToInt(Random.Range(0f, myServes.Count-1));
            
        }
        if(decided)
        {
            //toss the ball in the air
            if(waitCount > waitServeCooldown)
                myServes[serveChoice]();
            else
            {
                waitCount += Time.fixedDeltaTime;
            }
        }
            
        
    }

    void RunningServe()
    {
        if (serveStage == 0)
        {
            TeleportTo(transform.position - transform.forward * 2f);
            aimAngle = Random.Range(15, 25) * Mathf.Deg2Rad;
            serveStage = 1;
        }
        else if(serveStage == 1)
        {
            hitCooldownTimer = hitCooldown + 1;
            court.GiveBallToServer();
            serveStage = 2;
        }
        //toss the ball up in the air
        else if (serveStage == 2)
        {
            if (hitting)
            {
                originAimDir = Vector3.up * 1.1f + transform.forward;
                originAimDir = originAimDir.normalized;
                power = MaxPower * 0.5f;
                ballSpin = Vector2.up;
                serveStage = 3;
                smash = false;
                ComputerHit();
                runServe = true;
            }
        }
        else if (serveStage == 4)
        {
            if(hittable)
            {
                originAimDir = (Mathf.Cos(aimAngle) * transform.forward - Mathf.Sin(aimAngle) * transform.right) * Mathf.Sqrt(5) - Vector3.up * 0.15f;
                originAimDir = originAimDir.normalized;
                power = MaxPower * .5f;
                ballSpin = Vector2.up * (maxSpin * (1 + myLastVelocity.magnitude) + 1);
                decided = false;
                serveStage = 5;
                smash = true;
                ComputerHit();
                runServe = false;
            }
        }
    }

    void FloaterServe()
    {
        if (serveStage == 0)
        {
            hitCooldownTimer = hitCooldown + 1;
            court.GiveBallToServer();
            aimAngle = Random.Range(10, 20) * Mathf.Deg2Rad;
            serveStage = 1;
        }
        //toss the ball up in the air
        else if (serveStage == 1)
        {
            if (hitting)
            {
                originAimDir = Vector3.up;
                power = MaxPower * .5f;
                ballSpin = Vector2.up;
                serveStage = 2;
                smash = false;
                ComputerHit();
            }
        }
        else if (serveStage == 3)
        {
            originAimDir = (Mathf.Cos(aimAngle) * transform.forward - Mathf.Sin(aimAngle) * transform.right) * Mathf.Sqrt(5) + transform.up * .71f*1.75f;
            originAimDir = originAimDir.normalized;
            power = MaxPower * .75f;
            ballSpin = Vector2.up * Random.Range(.1f, 0f);
            decided = false;
            serveStage = 4;
            smash = true;
            ComputerHit();

        }
    }

    void NormalServe()
    {
        
        if(serveStage == 0)
        {
            hitCooldownTimer = hitCooldown + 1;
            court.GiveBallToServer();
            aimAngle = Random.Range(10, 20) * Mathf.Deg2Rad;
            serveStage = 1;
        }
        //toss the ball up in the air
        else if (serveStage == 1)
        {
            if(hittable)
            {
                hitting = true;
                originAimDir = Vector3.up;
                power = MaxPower * .5f;
                ballSpin = Vector2.up;
                serveStage = 2;
                smash = false;
                ComputerHit();
            }
        }
        else if(serveStage == 3)
        {
            if(hittable)
            {
                originAimDir = (Mathf.Cos(aimAngle) * transform.forward - Mathf.Sin(aimAngle) * transform.right) * Mathf.Sqrt(5) + transform.up * .6f;
                originAimDir = originAimDir.normalized;
                power = MaxPower*.5f;
                ballSpin = Vector2.up;
                decided = false;
                serveStage = 4;
                smash = true;
                ComputerHit();
            }
        }
    }

    void SpinServe()
    {

        if (serveStage == 0)
        {
            hitCooldownTimer = hitCooldown + 1;
            court.GiveBallToServer();
            aimAngle = Random.Range(10, 20) * Mathf.Deg2Rad;
            serveStage = 1;
        }
        //toss the ball up in the air
        else if (serveStage == 1)
        {
            if (hitting)
            {
                originAimDir = Vector3.up;
                power = MaxPower * .5f;
                ballSpin = Vector2.up;
                serveStage = 2;
                smash = false;
                ComputerHit();
            }
        }
        else if (serveStage == 3)
        {
            originAimDir = (Mathf.Cos(aimAngle) * transform.forward - Mathf.Sin(aimAngle) * transform.right) * Mathf.Sqrt(5) + transform.up;//* .85f;
            originAimDir = originAimDir.normalized;
            power = MaxPower * .9f;
            ballSpin = Vector2.up * Random.Range(1.1f,maxSpin + 1) + Vector2.right *  Random.Range(maxSpin, maxSpin);
            decided = false;
            serveStage = 4;
            smash = true;
            ComputerHit();

        }
    }
    #endregion

    void ComputerAim()
    {
        hitting = true;
        goToBall = false;
        Pass aimSpotInfo = court.LocalRelate.AimSpotInfo(this);
        float time = ((float)aimSpotInfo.speed) / 4f;
        if (aimSpotInfo.position <= CourtScript.MaxNumberOfPlayers)
        {
            if (aimSpotInfo.type == PassType.Location)
                aimSpot = LocationRelation.StrategyLocationToCourt(aimSpotInfo.location, currentSide);
            else
            {
                SpecialAction player = team.players.Find(x => x.currentPosition == aimSpotInfo.position);
                aimSpot = Vector3.ProjectOnPlane(player.transform.position + player.movement.rb.velocity * time, Vector3.up) + (HEIGHT + player.movement.transform.position.y) * Vector3.up + aimSpotInfo.offset;
                if (!player.movement.isGrounded)
                {
                    aimSpot += Vector3.up * (player.movement.rb.velocity.y * time + .5f * Physics.gravity.y * time * time + .5f);
                    
                }
                player.goToBall = true;
                if (CourtScript.FloatToSide(aimSpot.z) != currentSide)
                    aimSpot.z = 0.25f * (float)currentSide;
            }
            if (aimSpot.y < HEIGHT)
                aimSpot.y = HEIGHT;
            
        }
        else
        {
            aimSpot = court.LocalRelate.AimSpot(CourtScript.OppositeSide(currentSide));


            //SpecialAction player = court.Players[CourtScript.OppositeSide(currentSide)].Find(x => x.currentPosition == 1);
            //aimSpot = Vector3.ProjectOnPlane(player.transform.position + player.movement.rb.velocity * time, Vector3.up) + (HEIGHT + player.movement.transform.position.y) * Vector3.up;// + aimSpotInfo.offset;
        }

        court.LocalRelate.aimSpot = aimSpot;
        Vector3 distance = aimSpot - vBall.transform.position;
        Vector3 velocity = Vector3.zero ;
        float horizontalSpeed = Vector3.ProjectOnPlane(distance, Vector3.up).magnitude / time * vBall.rb.mass;
        float verticalSpeed = (-.5f * Physics.gravity.y * time * time + distance.y) / time;
        velocity = (Vector3.ProjectOnPlane(distance, Vector3.up).normalized * horizontalSpeed + Vector3.up * verticalSpeed);
        Vector3 ballAtNet = (vBall.transform.position - Vector3.up * (vBall.transform.localScale.y + .1f) + velocity * (-vBall.transform.position.z / velocity.z) + Physics.gravity * Mathf.Pow((-vBall.transform.position.z / velocity.z), 2) * .5f);
        if (CourtScript.FloatToSide(aimSpot.z) != currentSide &&  ballAtNet.y < court.NetHeight)
        {
            Vector3 distanceBallNet = vBall.rb.position - Vector3.up * vBall.transform.localScale.y- court.net.transform.position;
            Vector3 distanceBallSpot = vBall.rb.position - aimSpot;
            float Psin = 4.9f * 4 * (distanceBallSpot.y * -distanceBallNet.x - distanceBallNet.y * -distanceBallSpot.x)/((9.8f*9.8f -1) * (-distanceBallNet.x + distanceBallSpot.x));
            float angle = Mathf.Asin(Mathf.Sqrt(Psin)) * Mathf.Rad2Deg;
            //print(Psin);
            //if(Psin > 0)
            //{
                //print("angle: " + angle);
            //}
            //verticalSpeed = Mathf.Sin(Mathf.Deg2Rad * angle) * horizontalSpeed / Mathf.Cos(Mathf.Deg2Rad * -angle);
            //velocity.y = verticalSpeed;

        }
        power = velocity.magnitude<= MaxPower*2? velocity.magnitude:MaxPower*2;
        originAimDir = velocity.normalized;
        smash = false;
        ballSpin = Vector3.up;
        hitting = true;

    }

    void ComputerHit()
    {
        aimDir = originAimDir;
        float runHitConst = court.serve ? 2 : 10;
        if (smash)
        {
            aimDir *= (smashConst + myLastVelocity.magnitude / runHitConst);
        }
        else
        {
            aimDir.y *= vBall.rb.mass;

        }
        ReturnSpeedToNormal();
        ShootBall();
    }

    void ComputerActions()
    {
        if(court.serve)
        {
            if (vBall.rb.position.y > 6 && vBall.rb.position.y < 8 && vBall.rb.velocity.y < 0)
            {
                rb.velocity = Vector3.up * 6;
                if (!runServe)
                    serveStage = 3;
                else
                    serveStage = 4;
            }
            if(runServe)
            {
                rb.rotation = currentSide == Side.Left ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
            }
            if (runServe && !hitting && vBall.rb.velocity.y < 4.5f && vBall.transform.position.y > 4 &&  serveStage >= 3)
            {
                rb.velocity = Vector3.Scale(rb.velocity, new Vector3(0, 1, 0)) + transform.forward * 7;
            }
        }
    }

    #endregion

    #region InputActions
    //turns the arms on and off when you want to block
    void Block()
    {
        arms.gameObject.SetActive(block);
        if (block)
        {
            if (isPlayer)
            {
                SetCameraMovement(false);
                SetArmMovement(true);
                armWaveRotation.x -= (Input.mouseScrollDelta.y) * armWaveSensitivity;
            }
            else
            {
                /*if (Mathf.Abs(leftArm.transform.localEulerAngles.z) < 45 && Mathf.Abs(rightArm.transform.localEulerAngles.z) < 45)
                {
                    downArm-= 5f;
                    print(downArm + ", " + leftArm.transform.localEulerAngles.z + " , " + rightArm.transform.localEulerAngles.z);
                    if (downArm < -30)
                        downArm = -30;
                    leftArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY,0,downArm);
                    rightArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY,0, downArm);
                }*/
                if (movement.rb.velocity.y > 0)
                    downArm -= 1.75f;
                else
                {
                    if (downArm < 0)
                        downArm += 1.75f;
                }
                if (downArm < -40)
                    downArm = -40;
                leftArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY, blockArmRotation, downArm);
                rightArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY, blockArmRotation, downArm);
            }
            
            
            blockOver = true;
            if (armWaveRotation.x > 15)
                armWaveRotation.x = 15;
            if (armWaveRotation.x < -45)
                armWaveRotation.x = -45;
            hitCooldownTimer = 0;
            armWaveRotation.y = -armWaveRotation.x;
            blockArmRotation = 0;
            leftArm.OffsetRotation = Quaternion.AngleAxis(armWaveRotation.x, leftArm.XDirection);
            rightArm.OffsetRotation = Quaternion.AngleAxis(armWaveRotation.y, rightArm.XDirection);
            //rb.velocity = rb.velocity.y * Vector3.up;
        }
        else
        {
            if(blockOver)
            {
                if (isPlayer)
                {
                    SetArmMovement(false);
                    SetCameraMovement(true);
                }
                else
                {
                    rightArm.Reset();
                    leftArm.Reset();
                }
                blockOver = false;
                downArm = 0;
                leftArm.OffsetRotation = Quaternion.AngleAxis(0, leftArm.XDirection);
                rightArm.OffsetRotation = Quaternion.AngleAxis(0, rightArm.XDirection);
                armWaveRotation = Vector2.zero;
            }
        }
    }

    //changes the height of the player when you want to squat and returns
    //the height back to normal when you don't want to squat
    void Squat()
    {
        //print(Input.GetAxis("Squat"));
        float hittingOffset;
        hittingOffset = hitting ? myParent.transform.position.y : 0;
        if(dive || (squat && !movement.isWalking))
        {
            Dive();
        }
        else
        {
            if (squat)
            {
                if(myCollider.center.y > squatColliderHeightY.y)
                {
                    myCollider.height =  squatColliderHeightY.x;
                    myCollider.center = new Vector3(myCollider.center.x, squatColliderHeightY.y, myCollider.center.z);
                    transform.localScale = new Vector3(transform.localScale.x, squatMeshHeightY.x, transform.localScale.z);
                    transform.localPosition = new Vector3(transform.localPosition.x, squatMeshHeightY.y + hittingOffset, transform.localPosition.z);
                    myTrigger.height = squatTriggerHeightY.x;
                    myTrigger.center = new Vector3(myTrigger.center.x, squatTriggerHeightY.y, myTrigger.center.z);
                    movement.ground.localPosition = squatGroundPosition;
                }
                //else if (Mathf.Abs(myCollider.height - squatColliderHeightY.x) > squatThreshhold)
                {
                    myCollider.height = Mathf.Lerp(myCollider.height, squatColliderHeightY.x, squatSpeed);
                    myCollider.center = Vector3.Lerp(myCollider.center, new Vector3(myCollider.center.x, /*squatColliderHeightY.y*/1, myCollider.center.z), squatSpeed);
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, squatMeshHeightY.x, transform.localScale.z), squatSpeed);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, squatMeshHeightY.y + hittingOffset, transform.localPosition.z), squatSpeed);
                    myTrigger.height = Mathf.Lerp(myTrigger.height, squatTriggerHeightY.x, squatSpeed);
                    myTrigger.center = Vector3.Lerp(myTrigger.center, new Vector3(myTrigger.center.x, squatTriggerHeightY.y, myTrigger.center.z), squatSpeed);
                    movement.ground.localPosition = Vector3.Lerp(movement.ground.localPosition, squatGroundPosition, squatSpeed);

                }

            }
            else
            {

                if (myCollider.center.y < originColliderHeightY.y)
                {
                    myCollider.height = originColliderHeightY.x;
                    myCollider.center = new Vector3(myCollider.center.x, originColliderHeightY.y, myCollider.center.z);
                    transform.localScale = new Vector3(transform.localScale.x, originMeshHeightY.x, transform.localScale.z);
                    transform.localPosition = new Vector3(transform.localPosition.x, originMeshHeightY.y + hittingOffset, transform.localPosition.z);
                    myTrigger.height = originTriggerHeightY.x;
                    myTrigger.center = new Vector3(myTrigger.center.x, originTriggerHeightY.y, myTrigger.center.z);
                    movement.ground.localPosition = originalGroundPosition;
                }
                else if (Mathf.Abs(myCollider.height - originColliderHeightY.x) > squatThreshhold)
                {
                    myCollider.height = Mathf.Lerp(myCollider.height, originColliderHeightY.x, squatSpeed);
                    myCollider.center = Vector3.Lerp(myCollider.center, new Vector3(myCollider.center.x, originColliderHeightY.y, myCollider.center.z), squatSpeed);
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, originMeshHeightY.x, transform.localScale.z), squatSpeed);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, originMeshHeightY.y + hittingOffset, transform.localPosition.z), squatSpeed);
                    myTrigger.height = Mathf.Lerp(myTrigger.height, originTriggerHeightY.x, squatSpeed);
                    myTrigger.center = Vector3.Lerp(myTrigger.center, new Vector3(myTrigger.center.x, originTriggerHeightY.y, myTrigger.center.z), squatSpeed);
                    movement.ground.localPosition = Vector3.Lerp(movement.ground.localPosition, originalGroundPosition, squatSpeed);
                }

            }
        }
        
    }

    void Dive()
    {
        float hittingOffset;
        hittingOffset = hitting ? myParent.transform.position.y : 0;
        if (squat && !movement.isWalking)
        {
            
            float relativeAngle = (Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg - myParent.parent.eulerAngles.y);
            relativeAngle -= ((int)(relativeAngle / 360)) * 360;
            relativeAngle *= -1;
            Vector3 tempAxis = Vector3.up *  relativeAngle + Vector3.right * (Mathf.Atan2(rb.velocity.x, rb.velocity.z)) * Mathf.Rad2Deg + Vector3.forward * myParent.parent.eulerAngles.y;
            Vector3 rotationAxis = Vector3.right * Mathf.Cos(relativeAngle * Mathf.Deg2Rad) + Vector3.forward * Mathf.Sin(relativeAngle * Mathf.Deg2Rad);
            myParent.localEulerAngles = (rotationAxis).normalized * 60;
            dive = true;
        }
        else
        {
            myParent.localEulerAngles = Vector3.zero;
            
            if(myParent.localEulerAngles.x + myParent.localEulerAngles.z < 1f)
            {
                dive = false;
            }
        }
        /*if (Mathf.Abs(myCollider.height - originColliderHeightY.x) > squatThreshhold)
        {
            myCollider.height = Mathf.Lerp(myCollider.height, originColliderHeightY.x, squatSpeed);
            myCollider.center = Vector3.Lerp(myCollider.center, new Vector3(myCollider.center.x, originColliderHeightY.y, myCollider.center.z), squatSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, originMeshHeightY.x, transform.localScale.z), squatSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, originMeshHeightY.y + hittingOffset, transform.localPosition.z), squatSpeed);
            myTrigger.height = Mathf.Lerp(myTrigger.height, originTriggerHeightY.x, squatSpeed);
            myTrigger.center = Vector3.Lerp(myTrigger.center, new Vector3(myTrigger.center.x, originTriggerHeightY.y, myTrigger.center.z), squatSpeed);
        }*/
    }

    //gets all the player input
    void GetInput()
    {
        if (isPlayer)
        {
            block = (Input.GetAxisRaw("Block") != 0);
            squat = (Input.GetAxis("Squat") > 0.75f);
            stop = (Input.GetButtonDown("Cancel") || Input.GetButton("Cancel")) && !hitting;
            hit = Input.GetButtonDown("Fire") || Input.GetButton("Fire");
            if (hitting || hittable)
            {
                if ((Input.GetButtonDown("Fire") || Input.GetButton("Fire")) && changeHitMode)// && changeChangeHitMode)
                {
                    hitMode++;
                    changeHitMode = false;
                    //changeChangeHitMode = false;
                }
                if ((Input.GetButtonUp("Fire") || !Input.GetButton("Fire")) && !changeHitMode)// && !changeChangeHitMode)
                {
                    changeHitMode = true;
                    //StartCoroutine("EnableChangeHitMode", true);
                    //changeChangeHitMode = true;
                }
            }
            shootUp = Input.GetMouseButton(2);
            //checks if your running
            movement.isWalking = Input.GetAxisRaw("Sprint") != 1;
            //checks if you want to jump
            movement.jump = Input.GetAxisRaw("Jump") == 1;
            //gameUI.SetToggleMenu(Input.GetButtonDown("Menu"), this);
        }
            
    }
    #endregion

    IEnumerator EnableChangeHitMode(bool value)
    {
        yield return new WaitForSeconds(0.1f);
        changeHitMode = value;
        yield return null;
    }


    #region court methods
    //turns the movement of the player on or off
    public void SetMovementEnabled(bool value)
    {
        movement.enabled = value;
    }

    //gives a reference to the current player movement
    public PlayerMovement GetController()
    {
        return movement;
    }

    public void ResetPosition()
    {
        resetPosition = false;
        rb.MovePosition(court.GetPosition(currentPosition, currentSide, this));
        decided = false;
        movement.enabled = true;
        movement.Stop();
        hitCooldownTimer = hitCooldown + 1;
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
        movement.Stop();
        movement.enabled = false;
    }
    #endregion

    #region Shooting Ball
    
    void HitBallStraightUp()
    {
        originAimDir = Vector3.up;
        power = MaxPower/3;
        ballSpin = Vector3.up;
        ComputerHit();
    }
    
    //allows the player to aim the ball, and after aiming
    void Aim()
    {
        //make sure that the mouse is visible
        Cursor.visible = true;
        startingPosition = vBall.rb.position;
        //if you are not finished aiming than aim the ball
        if (hitMode == 1)//!aimed)
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

            if(cancelAim && !hit)
            {
                cancelAim = false;
            }

            //if you have left clicked then begin power calculations 
            //if you are a computer go straight to the power calculations
            /*if ((Input.GetButtonDown("Fire") || Input.GetButton("Fire") hit hitMode == 2)&& !cancelAim || !isPlayer)
            {
                if (isPlayer)
                    SetAimed(true);
                else
                    aimed = true;
                pastBallVelocity = vBall.rb.velocity;
                vBall.ResetMotion();
                originAimDir = aimDir;
            }*/
        }
        if ((hitMode == 2) && !cancelAim || !isPlayer)
        {
            if (isPlayer)
                SetAimed(true);
            else
                aimed = true;
            pastBallVelocity = vBall.rb.velocity;
            vBall.ResetMotion();
            originAimDir = aimDir;
            hitMode = 3;
        }
        //if you are finished aiming, then start the power calculations
        if (hitMode == 3)//aimed)
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

        float runHitConst = court.serve ? 4 : 10;
        
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
        //if you right clicked before you let go of the left click then cancel power calculations and 
        //go back to aiming
        if (Input.GetButtonDown("Cancel") && isPlayer)
        {
            SetAimed(false);
            vBall.rb.velocity = pastBallVelocity;
            cancelAim = true;
            hitMode = 1;
            return;
        }

        //if you let go of the left click then return speed back to normal and hit the ball
        if (!hit || !isPlayer)//Input.GetButtonUp("Fire") || !isPlayer || !Input.GetButton("Fire"))
        {
            ShootBall();
            ReturnSpeedToNormal();
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

    public void SetArmMovement(bool move)
    {
        m_MouseLook.SetCursorLock(true);
        rightArm.look = move;
        leftArm.look = move;
        if(!move)
        {
            rightArm.Reset();
            leftArm.Reset();
        }
    }

    //add force and spin to the ball
    void ShootBall()
    {
        
        vBall.rb.velocity = Vector3.zero;
        vBall.Shoot(aimDir * power,ballSpin, myParent);
        vBall.CollideWithPlayer(this);
        rb.velocity = Vector3.zero;
        //if the court says that your ready to serve then tell them that you are currently
        //serving
        if (court.readyToServe)
            court.readyToServe = false;
    }

    //void OnTriggerEnter(Collider other)
    public void OnMyTriggerEnter(Collider other)
    {
        //if the player is colliding with the ball and it the hit cooldown is over, and the rally 
        //is still ongoing then hit the ball
        if(other.tag == "Ball")
        {
            if (!court.rallyOver && !hitting)
            {
                hittable = true;
                myLastVelocity = rb.velocity;
                if (court.readyToServe)
                {
                    HitSetUp();
                    rb.velocity = Vector3.zero;
                }
                if(shootUp)
                {
                    HitBallStraightUp();
                }

                //makes it so that the player body doesn't rotate while your aiming
                /*if (isPlayer)
                {
                    transform.parent = null;
                    movement.enabled = false;

                }*/



                //tell the ball that you where the last player to hit it

            }

        }
    }

    void HitSetUp()
    {
        hitting = true;
        slowDown = true;
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;
        myLastVelocity = rb.velocity;
        myLastVelocity.Scale(new Vector3(1.1f, 0, 1.1f));
        hitTime = 0;
        startHitTime = Time.realtimeSinceStartup;
    }

    void OnTriggerExit()
    {
        if(!hitting)
        {
            hittable = false;
        }
    }

    void EndHit()
    {
        hittable = false;
        if(hitting)
        {
            hitting = false;
            hitTime = 0;
            hitCooldownTimer = 0;
            ReturnSpeedToNormal();
        }
    }
    #endregion

    #region Reseting



    //returns the speed to normal and resets all hitting variabls
    void ReturnSpeedToNormal()
    {
        hitting = false;
        aimed = false;
        smash = false;
        hittable = false;

        //returns time to normal
        slowDown = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;

        //starts the cooldown for hiting the ball
        hitTime = 0;
        hitMode = 0;
        hitCooldownTimer = 0;

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
        //if (isPlayer)
            movement.enabled = true;

        //stop using the particles for predicting where the ball will go
        StopAimingParticles();

        //allow the camera to move
        SetCameraMovement(true);

        //makes sure that the player body is in the center of where the camera rotates
        transform.eulerAngles = myParent.eulerAngles;
        transform.position = myParent.position + originalPosition;
        transform.parent = myParent;
    }

    //stops player and camera movement
    public void Pause(bool pause)
    {
        movement.enabled = !pause;
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
            //float powerConst = 7 *power / MaxPower / 10 * 30/ mySystem.particleCount;
            float powerConst = power / MaxPower * 7 * 30 / mySystem.particleCount;
            float time = 0;

            //used to calculate the position of a floater hit (not always accurate
            Vector3 floaterRef = Vector3.zero;
            for(int i = 0; i < myParticle.Length; i++)
            {
                
                time = powerConst * regFixedTimeDelta;
                //calculate the the position of where the ball is going to be at a specific point in time for each particle
                myParticle[i].position = startingPosition + PredictAddedForce(i * time) + PredictGravity(i * time);// + PredictSpin(time, i, ref floaterRef);
                myParticle[i].remainingLifetime = 100000;
            }

            //move the particles to the predicted position
            mySystem.SetParticles(myParticle, myParticle.Length);
        }

    }

    //predict the affect of the force onto the ball
    Vector3 PredictAddedForce(float time)
    {
        return aimDir * power / vBall.rb.mass * time;
        //return (Vector3.Scale(aimDir,new Vector3(1,1/yScale,1)) * power / vBall.rb.mass / tester1) * time; //3.75f
    }

    //predict the affect of gravity on the ball
    Vector3 PredictGravity(float time)
    {
        return Physics.gravity * Mathf.Pow(time, 2) * .5f;
        //return -tester2 * Vector3.Scale(Physics.gravity, Physics.gravity) * Mathf.Pow(time, 2); //-10
    }

    //predict the affect of the spin on the ball
    Vector3 PredictSpin(float time, int i, ref Vector3 floaterRef)
    {

        Vector3 predictedSpin = Vector3.zero;
        //if the ball has spin predict where it will go
        if (ballSpin.y >= 1)
        {
            Vector3 tempSpin = (ballSpin.y - 1) * -Vector3.up + ballSpin.x * transform.right;
            float iteration = time * i / regFixedTimeDelta;

            float rate;
            if (iteration < 50)
            {
                rate = (1 - Mathf.Pow(vBall.SpinAddConst, iteration)) / (1 - vBall.SpinAddConst) - 1f;
                floaterRef.x += Mathf.Pow(vBall.SpinAddConst, iteration) * (iteration - 1) * regFixedTimeDelta;
                floaterRef.y = time * (i) / regFixedTimeDelta;
            }
            else
            {
                rate = (1 - Mathf.Pow(vBall.SpinAddConst, floaterRef.y)) / (1 - vBall.SpinAddConst) - 1f;
                //rate += (iteration - 50) * Mathf.Pow(vBall.SpinAddConst, 50);
                //floaterRef.x += Mathf.Pow(vBall.SpinAddConst, 50) * (iteration - 1) * regFixedTimeDelta;
            }

            float floaterConst = (1 - Mathf.Exp(-1 * 4f * (power / MaxPower))) * (1 + Mathf.Pow(power / MaxPower, 2));
            predictedSpin = tempSpin * Mathf.Abs(time * i * rate - floaterRef.x * floaterConst)/100;// - floaterRef.x/100); //- (rate * tempSpin - vBall.SpinAddConst * tempSpin);
            int myIteration = iteration < 50? (int)iteration: 50;
            //float maxIteration = iteration < 50 ? iteration : 50;
            float geometricSum = (1 - Mathf.Pow(vBall.SpinAddConst, myIteration)) / (1 - vBall.SpinAddConst);
            predictedSpin = (tempSpin * geometricSum + tempSpin * Mathf.Pow(vBall.SpinAddConst, myIteration) * (iteration - myIteration));// * regFixedTimeDelta;
            //float rate = (1 - Mathf.Pow(vBall.SpinAddConst, time * i / regFixedTimeDelta)) / (1 - vBall.SpinAddConst);
            //predictedSpin = ((ballSpin.y - 1) * -Vector3.up + ballSpin.x * transform.right ) * Mathf.Pow(time * i, 2) * rate / (100 * vBall.rb.mass);
            //predictedSpin = (-Vector3.up * (ballSpin.y - 1) + myParent.right * ballSpin.x) * rate * time * i / vBall.rb.mass; //Vector3.Scale(ballSpin.y * Vector3.up + ballSpin.x * myParent.right - Vector3.up, -Vector3.up * rate + myParent.right * rate) * time * i;
        }
        //if the ball is a floater then predict where it will go
        else
        {
            predictedSpin = (Vector3.up * vBall.GetFloater(time * i / regFixedTimeDelta, ballSpin.y, Direction.Y) + Vector3.right * vBall.GetFloater(time * i / regFixedTimeDelta, ballSpin.y, Direction.X)) * time * vBall.rb.mass;
            predictedSpin += floaterRef;
            floaterRef = predictedSpin;
        }
        return predictedSpin;
    }
    #endregion

    public bool GetHittable()
    {
        return hittable;
    }

    public float GetHitTimer()
    {
        float myTimer = hitTime >= 900? 1 : 1 - hitTime / maxHitTime;
        
        if(myTimer < 0)
        {
            myTimer = 0;
        }
        return myTimer;
    }
    
}

