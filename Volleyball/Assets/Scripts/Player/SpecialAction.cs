using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityStandardAssets.Characters;

public enum Side { Left = -1, Right = 1}
public enum HitMode { Ready = 0, Aim = 1, Transition = 2, CalculatePower = 3}
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
public struct BallTarget
{
    public bool goToBall;
    public int touchCount;
    public BallTarget(bool b, int tc)
    {
        goToBall = b;
        touchCount = tc;
    }
}

public class SpecialAction : MonoBehaviour {
    public GameObject BallLandingIndicator;

    public bool showAimSpot;

    private Vector3 aimSpot;

    [Range(0, 2000)]
    public float MaxPower = 1000;

    [Range(1,6)]
    public int currentPosition = 1;
    public bool isPlayer = true;
    public Side currentSide = Side.Left;

    public bool resetPosition;
    public Team team;

    public PlayerMovement myMovement{ get{ return movement;}}

    public CourtScript Court{ get{ return court; }}

    public PlayerSquat mySquat { get { return playerSquat; } }

    public PlayerAim myAim { get { return playerAim; } }

    public PlayerArmMovement myArmMovement { get { return playerArm;  } } 

    public float MaxJumpHeight
    {
        get
        {
            float time = movement.jumpSpeed / Mathf.Abs(Physics.gravity.y);
            return time * movement.jumpSpeed + time * time * Physics.gravity.y * .5f + HEIGHT;
        }
    }

    public bool Hitting{ get{ return hitting;} }

    public float Height { get { return HEIGHT; } }
    //[System.NonSerialized]
    BallTarget ballTarget;
    PlayerMovement movement;
    MyMouseLook m_MouseLook;
    [System.NonSerialized]
    public Transform myParent;
    
    bool hitting;

    float hitCooldown = 1;
    public float hitCooldownTimer = 0;
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

    bool IgnoreCollision;    
    Vector3 pastBallVelocity;
    Rigidbody rb;
    [System.NonSerialized]
    public VolleyballScript vBall;

    GameObject aimSpotIndicator;

    CourtScript court;
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
    bool cancelAimEnd;

    [SerializeField]
    [Range(0.01f, 2)]
    float lobScrollSpeed = .25f;

    Vector3 originalPosition;

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
    [SerializeField]
    float servePower = .75f;
    delegate void ServeOfChoice();
    List<ServeOfChoice> myServes;
    int serveChoice;

    Vector3 ballStartingPosition;

    float HEIGHT = 2f;
    float SQUAT_HEIGHT = 1f;
    float WALK_LENGTH = 2;
    float jumpError = .1f;
    float rotationAngle;
    bool stop;
    bool printing;
    bool shootUp;
    bool running=false;

    int movementIndex;
    bool dontHit;
    HitMode hitMode = HitMode.Ready;
    bool changeHitMode = true;
    bool changeChangeHitMode = true;

    PlayerAim playerAim;
    PlayerShoot playerShoot;
    PlayerSquat playerSquat;
    PlayerArmMovement playerArm;
    ShootCapsule shootInfo;
    DrawPath shotPathDrawer;
    bool showBallPath;

    public SpecialAction passPlayer;

    float computerServeRunTimeOffset = 0.5f;
    float computerServeJumpTimeOffset = 0.75f;

    //public bool shouldPrintGoToBall;
    
    // Use this for initialization
    void Awake () {
        myServes = new List<ServeOfChoice>();

        //gets the object over this object in the object heirarchy
        myParent = transform.parent;
        //the component that controlls the physics of this player
        rb = myParent.parent.GetComponent<Rigidbody>();

        ballTarget = new BallTarget(false, -99);

        //the component to draw the path of where you will shoot;
        shotPathDrawer = GetComponent<DrawPath>();
        shootInfo = new ShootCapsule();
        passPlayer = null;
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
        
        //store the current time it takes for fixed update to run once
        regFixedTimeDelta = Time.fixedDeltaTime;

        //when you first start there is no cooldown to hit the ball
        hitCooldownTimer = hitCooldown;

        //gets a reference to the menu
        gameUI = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InGameUI>();

        originalPosition = transform.localPosition;

        myLastVelocity = Vector3.zero;
        ballStartingPosition = Vector3.zero;

        SetPlayer(isPlayer);

        #region Getting_Player_Components 
        try
        {
            playerAim = GetComponent<PlayerAim>();
            if (playerAim == null)
                playerAim = gameObject.AddComponent<PlayerAim>();
        }
        catch (System.Exception e)
        {
            playerAim = gameObject.AddComponent<PlayerAim>();
        }
        finally
        {
            playerAim.Initialize(this);
        }

        try
        {
            playerShoot = GetComponent<PlayerShoot>();
            if (playerShoot == null)
                playerShoot = gameObject.AddComponent<PlayerShoot>();
        }
        catch (System.Exception e)
        {
            playerShoot = gameObject.AddComponent<PlayerShoot>();
        }
        finally
        {
            playerShoot.Initialize(this);
        }
        
        try
        {
            playerSquat = GetComponent<PlayerSquat>();
            if (playerSquat == null)
                playerSquat = gameObject.AddComponent<PlayerSquat>();
        }
        catch (System.Exception e)
        {
            playerSquat = gameObject.AddComponent<PlayerSquat>();
        }
        finally
        {
            playerSquat.Initialize(this);
        }

        try
        {
            playerArm = GetComponent<PlayerArmMovement>();
            if (playerArm == null)
                playerArm = gameObject.AddComponent<PlayerArmMovement>();
        }
        catch (System.Exception e)
        {
            playerArm = gameObject.AddComponent<PlayerArmMovement>();
        }
        finally
        {
            playerArm.Initialize(this);
        }
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayer)
        {
            gameUI.SetToggleMenu(Input.GetButtonDown("Menu"), this);
        }
        

        if (showAimSpot)
            ShowAimSpot();

        if (showBallPath)
            PredictShot();
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

            //allow for fixed update to run at normal speed even when you slow down time
            //this allows for you to controll your character in realtime otherwise the
            //control is choppy
            Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;

            //increments the current time for the cooldown
            if (hitCooldownTimer <= hitCooldown)
            {
                hitCooldownTimer += regFixedTimeDelta;
            }
            //when the cooldown is over then allow player to hit the ball
            else
            {    
                //checks to see if player is in contact with ball
                CheckHit();
            }
        }

        //when the rally's over then allow for all rally actions
        else
        {
            //moves the player to their starting positions;
            if (resetPosition)
            {
                ResetPosition();
            }

            
        }
        //if you are the server and its time to allow the server to request the ball
        CheckServe();
        //if your squatted then unsquat yourself
        playerSquat.Squat();
        //allow yourself to start blocking
        playerArm.Block();
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
        if (court.serve && court.serveSide == currentSide && currentPosition == 1)
        {
            if (isPlayer)
            {
                if (court.readyToServe) { 
                    //left click the mouse to call for the ball
                    if (Input.GetAxisRaw("Fire") == 1)
                    {
                        hitTime = 999;
                        court.GiveBallToServer();
                    }
                }
            }
            else
            {
                PickServe();
            }
        }
    }

    #region computerStuff

    //teleports to the location of the ball when it is on their side and it is at about head height
    void ComputerMovement()
    {
        if((court.serve && !court.readyToServe && court.serveSide != currentSide) || !court.serve)
        {
            //if your on defense, then move defensively
            if (team.currentMode == StrategyType.Defense)
            {
                movementIndex = 0;
                dontHit = false;
                DefensiveMovement();
            }
            //Otherwise you must be on offense, so follow the offense strategy
            else
            {
                OffensiveMovement();
            }
        }
    }

    void OffensiveMovement()
    {
        //Gets the movement path from the offense strategy
        Path myPath = LocationRelation.GetMovement(this);
        
        //reset variables
        movement.goToRotation = false;
        playerSquat.squat = false;
        dontHit = false;
        if (movement.isGrounded)
            playerArm.block = false;
        else
            movement.jump = false;

        //If there is no path for the player to follow
        if (myPath.size == 0)
        {
            //Get the ball if you were the intended receiver
            if (ShouldGoToBall())
            {
                GetBall();
            }
            //Otherwise go to the offensive position
            else
                ReturnToPosition();
        }
        //If there is a path, then follow the path
        else
        {
            Vector3 movementLocation = movementIndex < myPath.size? LocationRelation.PathLocationToCourt(myPath.points[movementIndex], currentSide): LocationRelation.PathLocationToCourt(myPath.points[movementIndex-1], currentSide);
            //if you haven't reached the end of the path, then continue following it
            if (movementIndex < myPath.size)
            {
                if (Vector3.ProjectOnPlane(transform.position - movementLocation, Vector3.up).magnitude < movement.sprintSpeed / 10f)
                    movementIndex++;
                else
                    movement.MoveTowards(movementLocation,myPath.walkToThisPoint[movementIndex]);
            }
            //if you've reached the end of the path, then stop
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
                    //if your supposed to jump, then jump
                    if (   (myPath.jumpTime == JumpTime.EndOfPath && movementIndex == myPath.size - 1 && Vector3.ProjectOnPlane(transform.position - movementLocation, Vector3.up).magnitude < .5f)
                        || (myPath.jumpTime == JumpTime.BeforeSetterReceivesBall && court.LocalRelate.TimeTillBallReachesLocation < myOffset && court.LocalRelate.TimeTillBallReachesLocation > -1f)
                        || (myPath.jumpTime == JumpTime.BeforeAttackerReceivesBall && vBall.GetSideTouches(currentSide) == 2))
                    {
                        if (myPath.stopJump)
                            Stop();
                        movement.jump = true;
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
        //goToBall = false;
        bool shouldBlock = team.isBlocker(currentPosition) 
            && Mathf.Abs(transform.position.z) < court.dimensions.FrontLine(currentSide).z * (float)currentSide/2
            && !vBall.LastHitByServer;

        //if you are the closest player to the ball and it's on your side, then get the ball
        if (ShouldMove())
        {
            if(movement.isGrounded)
                GetBall();
        }
        //otherwise follow the defensive movement
        else
        {
            LocationRelation.BallToPlayer personBallIsGoingTo = court.LocalRelate.personBallIsGoingTo;
            if (shouldBlock)
            {
                movement.faceNet = true;
                if(CourtScript.FloatToSide(vBall.transform.position.z) != currentSide)
                {
                    if (vBall.GetSideTouches(CourtScript.OppositeSide(currentSide)) > 1)
                    {
                        if (personBallIsGoingTo != null)
                        {
                            //If the ball is going to a person and at least two people have touched
                            //the ball, then block them
                            if (BlockPlayer(personBallIsGoingTo))
                                return;
                        }
                        else
                        {
                            //If the ball isn't going to a person, and the ball is going to go over
                            //the net, then block the ball.
                            if (BlockBallGoingOverNet())
                                return;
                        }
                    }
                    //If the ball is going over the net even though less than two people have 
                    //hit the ball, then block it.
                    else
                        if (BlockBallGoingOverNet())
                            return;
                }
            }

            //Follow the defensive formation
            ReturnToPosition();
        }
    }

    bool BlockPlayer(LocationRelation.BallToPlayer personBallIsGoingTo)
    {
        if (personBallIsGoingTo.angle < 12)
        {
            SpecialAction followPlayer = personBallIsGoingTo.player;
            if (Mathf.Abs(Vector3.Project(followPlayer.transform.position - transform.position, Vector3.up).x) < .75f)
            {
                float timeTillMeet = Vector3.Project(followPlayer.transform.position - vBall.transform.position, Vector3.up).magnitude / Vector3.Project(followPlayer.rb.velocity - vBall.rb.velocity, Vector3.up).magnitude;
                return ConditionalBlock(timeTillMeet);
                
            }
        }
        return false;
    }

    bool BlockBallGoingOverNet()
    {
        if (CourtScript.FloatToSide(vBall.rb.velocity.z) == currentSide)
        {
            float timeTillBallAtNet = -vBall.transform.position.z / vBall.rb.velocity.z;
            Vector3 ballAtNet = vBall.transform.position + vBall.rb.velocity * timeTillBallAtNet + .5f * Physics.gravity * Mathf.Pow(timeTillBallAtNet, 2);
            if (ballAtNet.y > court.NetHeight)
            {
                if (Mathf.Abs(Vector3.Project(ballAtNet - transform.position, Vector3.up).x) < .5f)
                {
                    return ConditionalBlock(timeTillBallAtNet);
                }
            }
        }
        return false;
    }

    bool ConditionalBlock(float time)
    {
        if (time < (-movement.jumpSpeed / Physics.gravity.y) * .75f)
        {
            if (movement.isGrounded)
                movement.jump = true;
            if(Mathf.Abs(transform.position.z) > 0)
            {
                Vector3 position = court.GetPosition(currentPosition, currentSide, this);
                position.z = 0;
                movement.MoveDirectlyTowards(position);
            }
            playerArm.block = true;
            playerArm.armWaveRotation.x = 2.5f;
            return true;
        }
        return false;
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
        playerSquat.squat = false;
        if (movement.isGrounded)
            playerArm.block = false;
        Vector3 position = court.GetPosition(currentPosition, currentSide, this);
        movement.CalculatedMoveTowards(position);
        //movement.MoveTowards(position, Vector3.Project(position - transform.position, Vector3.up).magnitude < WALK_LENGTH);
    }

    bool IsSupposedToGetBall()
    {
        return court.LocalRelate.ClosestPlayerToBall[currentSide] != null 
                && court.LocalRelate.ClosestPlayerToBall[currentSide].player == this;
    }

    void GetBall()
    {
        movement.goToRotation = false;
        movement.faceNet = false;
        if (movement.isGrounded)
            playerArm.block = false;
        Vector3 landing = court.LocalRelate.MovePositionAtMaxHeight(MaxJumpHeight - 2*jumpError, movement);
        Vector3 ballDistance = vBall.transform.position - landing;
        Vector3 distance = Vector3.ProjectOnPlane(landing - movement.transform.position, Vector3.up);
        bool shouldSquat = landing.y <= SQUAT_HEIGHT + .5f && vBall.transform.position.y < (1f - vBall.rb.velocity.y / 2f) && distance.magnitude < WALK_LENGTH * 3 / 2f && (!movement.jumping);
        bool shouldWalk = (distance - distance.y * Vector3.up).magnitude < WALK_LENGTH && (!movement.jumping);
        playerSquat.squat = shouldSquat;
        if (landing.y > HEIGHT && !movement.jumping)
        {
            float time = (-vBall.rb.velocity.y - Mathf.Sqrt(Mathf.Pow(vBall.rb.velocity.y, 2) - 4 * Physics.gravity.y * ballDistance.y)) / (2 * Physics.gravity.y);
            float jumpDistance = time * movement.jumpSpeed + .5f * time * time * Physics.gravity.y + HEIGHT;
            float maxJumpHeightTime = -movement.jumpSpeed / Physics.gravity.y;
            float timeTillPlayerReachesLanding = Vector3.Project(landing - transform.position, Vector3.up).magnitude / movement.sprintSpeed;
            float distanceToLanding = Vector3.Distance(vBall.rb.velocity * time + vBall.transform.position + Physics.gravity * time * time / 2, landing);
            if (time < maxJumpHeightTime && time > maxJumpHeightTime * .5f && vBall.rb.velocity.magnitude < 20)//jumpDistance < landing.y + jumpError && distanceToLanding < 2 && jumpDistance > landing.y && movement.isGrounded)
            {
                movement.jump = true;
            }

        }
        if (!movement.isGrounded)
            movement.jump = false;

        movement.MoveTowards(landing, shouldWalk);
        aimSpot = landing;
    }

    void Stop()
    {
        movement.Stop();
        playerSquat.squat = false;
    }

    bool ShouldGoToBall()
    {
        //if(shouldPrintGoToBall)
            //print("Should go to ball: " + ballTarget.goToBall + ", " + ballTarget.touchCount + "==" + vBall.GetSideTouches(currentSide));
        ballTarget.goToBall = ballTarget.goToBall && ballTarget.touchCount == vBall.GetSideTouches(currentSide);

        return ballTarget.goToBall;
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
            ComputerCalulateAndHit();
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
            serveChoice = Mathf.RoundToInt(Random.Range(0f, myServes.Count-1));
        }
        if(decided)
        {
            //toss the ball in the air
            if (waitCount > waitServeCooldown)
                ComputerServe();
            else
            {
                waitCount += Time.fixedDeltaTime;
            }
        }
    }

    void ComputerServe()
    {
        //Move back
        if (serveStage == 0)
        {
            TeleportTo(transform.position - Vector3.forward * ((float)CourtScript.OppositeSide(currentSide)) * movement.sprintSpeed * computerServeRunTimeOffset);
            aimAngle = Random.Range(15, 25) * Mathf.Deg2Rad;
            serveStage = 1;
            waitCount = 0;
            movement.faceNet = true;
            movement.goToRotation = false;
        }
        //Get ball
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
                serveStage = 3;
                shootInfo = playerShoot.FowardTossUp(computerServeRunTimeOffset, computerServeJumpTimeOffset, computerServeJumpTimeOffset + movement.jumpSpeed/(-Physics.gravity.y));
                HitBall();
                running = false;
            }
        }
        //Run towards ball and jump
        else if (serveStage == 3)
        {
            float timeTillBallIsAtJumpHeight = (-vBall.rb.velocity.y - Mathf.Sqrt(Mathf.Pow(vBall.rb.velocity.y, 2) - 4 * Physics.gravity.y * .5f * (vBall.rb.position.y - MaxJumpHeight))) / (Physics.gravity.y);
            float timeForPlayerToReachJumpHeight = -movement.jumpSpeed / Physics.gravity.y;
            float timeTillPlayerReachesBall = Vector3.Project(vBall.rb.velocity * timeTillBallIsAtJumpHeight + vBall.rb.position - transform.position, Vector3.up).magnitude / movement.sprintSpeed;
            if (timeTillBallIsAtJumpHeight < timeForPlayerToReachJumpHeight && vBall.rb.velocity.y < 0 && Mathf.Abs(rb.velocity.y) < .15f && rb.position.y < .15f)
            {
                movement.jump = true;
            }
            if (timeTillPlayerReachesBall < timeTillBallIsAtJumpHeight || running)
            {
                movement.MoveDirectlyTowards(vBall.transform.position);
                running = true;
            }
            if (hittable)
                serveStage++;
        }
        //Hit ball over net
        else if (serveStage == 4)
        {
            if(hittable)
            {
                decided = false;
                serveStage = 5;
                shootInfo = playerAim.ComputerAimFor(court.dimensions.CourtSection(CourtScript.OppositeSide(currentSide), CourtDimensions.Section.BackLeft)); //court.dimensions.BackLineRect(CourtScript.OppositeSide(currentSide)));
                aimSpot = shootInfo.aimSpot;
                myLastVelocity = rb.velocity;
                ComputerCalulateAndHit();
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
                ComputerCalulateAndHit();
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
            ComputerCalulateAndHit();

        }
    }
    #endregion

    void ComputerAim()
    {
        shootInfo = playerAim.ComputerAim();
        aimSpot = shootInfo.aimSpot;
        ballSpin = Vector3.up;
        hitting = true;
    }

    void ComputerCalulateAndHit()
    {
        shootInfo = playerShoot.ComputerCalculateShot(shootInfo, myLastVelocity);
        HitBall();
    }
    #endregion

    public void SetPlayer(bool rplayer)
    {
        //if a person is controlling the player than lock the mouse to the center of the screen
        if (rplayer)
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
        //print("Current player named \"" + myParent.parent.gameObject.name + "\" is currently a " + rplayer + " player");
        isPlayer = rplayer;
    }

    #region InputActions

    //gets all the player input
    void GetInput()
    {
        if (isPlayer)
        {
            playerArm.block = (Input.GetAxisRaw("Block") != 0);
            playerSquat.squat = (Input.GetAxis("Squat") > 0.75f);
            stop = (Input.GetButtonDown("Cancel") || Input.GetButton("Cancel")) && !hitting;
            hit = Input.GetButtonDown("Fire") || Input.GetButton("Fire");
            if (hitting || hittable)
            {
                if ((Input.GetButtonDown("Fire") || Input.GetButton("Fire")) && changeHitMode)
                {
                    hitMode++;
                    changeHitMode = false;
                }
                if ((Input.GetButtonUp("Fire") || !Input.GetButton("Fire")) && !changeHitMode)
                {
                    changeHitMode = true;
                }
                if (Input.GetButtonDown("Cancel"))
                {
                    if(hitMode == HitMode.CalculatePower)
                    {
                        hitMode = HitMode.Transition;
                        cancelAim = true;
                    }
                    else if(hitMode == HitMode.Aim)
                    {
                        ReturnSpeedToNormal();
                    }
                }
                if ((Input.GetButtonDown("Pass")) && !court.rallyOver)
                {
                    GetPassPlayer();
                }

            }
            else
            {
                passPlayer = null;
            }
            shootUp = Input.GetMouseButton(2);
            //checks if your running
            movement.isWalking = Input.GetAxisRaw("Sprint") != 1;
            //checks if you want to jump
            movement.jump = Input.GetAxisRaw("Jump") == 1;
            //gameUI.SetToggleMenu(Input.GetButtonDown("Menu"), this);
        }
            
    }

    void GetPassPlayer()
    {
        RaycastHit hit;
        Ray ray = tpc.camera.ScreenPointToRay(Input.mousePosition);
        ray.origin = ray.origin + ray.direction * (tpc.transform.position - transform.position).magnitude;
        if (Physics.Raycast(ray, out hit, 30, LayerMask.GetMask("Player")))
        {
            SpecialAction temp = CourtScript.FindPlayerFromCollision(hit.transform);//.GetComponentInChildren<SpecialAction>();
            if (temp.currentSide == currentSide)
            {
                passPlayer = temp;
                return;
            }
        }
        passPlayer = null;
    }
    #endregion

    #region court methods
    //turns the movement of the player on or off
    public void SetMovementEnabled(bool value)
    {
        movement.enabled = value;
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
        playerArm.block = false;
        playerSquat.squat = false;
        playerArm.Block();
        playerSquat.Squat();
        ReturnSpeedToNormal();
        movement.Stop();
        movement.enabled = false;
    }
    #endregion

    #region Shooting Ball
    void ShowAimSpot()
    {
        if (aimSpotIndicator == null)
            aimSpotIndicator = Instantiate(BallLandingIndicator, aimSpot, Quaternion.identity);
        else
            aimSpotIndicator.transform.position = aimSpot;
        if (!isPlayer && BallLandingIndicator != null)
        {
            aimSpotIndicator.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

    void HitBallStraightUp()
    {
        shootInfo.power = MaxPower / 3;
        shootInfo.spin = Vector3.up;
        shootInfo.aimDirection = Vector3.up;
        HitBall();
    }

    void HitBall()
    {
        playerShoot.ShootBall(vBall, shootInfo);
        if (passPlayer != null)
            passPlayer.SetAsIntendedTarget();
        ReturnSpeedToNormal();
    }

    //allows the player to aim the ball, and after aiming
    void Aim()
    {
        //make sure that the mouse is visible
        Cursor.visible = true;

        //get the position of the ball for the shoot path simulation
        ballStartingPosition = vBall.rb.position;

        //if you are not finished aiming than aim the ball
        if (hitMode == HitMode.Aim)//!aimed)
        {
            //Reset the power direction;
            shootInfo = playerShoot.ResetPower();
            
            aimDir = playerAim.UserAim();
            shootInfo.aimDirection = aimDir;
            //set the reference position used for the power/spin calculations
            originMousePos = Input.mousePosition;
        }
        else if (hitMode == HitMode.Transition)
        {
            if (!cancelAim)
            {
                SetAimed(true);

                //store the old ball velocity so that when you cancel your shot, the ball continues moving
                pastBallVelocity = vBall.rb.velocity;

                //stop the ball from moving
                vBall.ResetMotion();
                hitMode = HitMode.CalculatePower;
            }
            else
            {
                SetAimed(false);

                //store the old ball velocity so that when you cancel your shot, the ball continues moving
                vBall.rb.velocity = pastBallVelocity;

                hitMode = HitMode.Aim;
                cancelAim = false;
            }
        }
        //if you are finished aiming, then start the power calculations
        else if (hitMode == HitMode.CalculatePower)//aimed)
        {
            //calculates the power used to hit the ball
            if (passPlayer == null)
            {
                shootInfo = playerShoot.UserCalculatePower(originMousePos, shootInfo, myLastVelocity);
                aimSpot = PredictLanding();
            }
            else
            {
                shootInfo = playerShoot.UserPassToPlayer(originMousePos, shootInfo, passPlayer);
                aimSpot = shootInfo.aimSpot;
            }
            //if you let go of the left click then return speed back to normal and hit the ball
            if (!hit || !isPlayer)
            {
                HitBall();
            }
        }

        //predict where the ball will go
        if (hitting)
            showBallPath = true;
    }

    //set whether you have aimed or not, which will in turn adjust the 
    //camera movement accordingly
    void SetAimed(bool myAimed)
    {
        SetCameraMovement(!myAimed);
        //aimed = myAimed;
    }

    //allow the cameras to move with the mouse
    public void SetCameraMovement(bool move)
    {
        m_MouseLook.SetCursorLock(move);
        m_MouseLook.look = move;
        cameraLook.look = move;
        tpc.allowZoom = move;
    }

    public void SetCursorLock(bool look)
    {
        m_MouseLook.SetCursorLock(look);
    }
    
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
            }
        }
    }

    public void SetAsIntendedTarget()
    {
        ballTarget.goToBall = true;
        ballTarget.touchCount = vBall.GetSideTouches(currentSide);
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
        //aimed = false;
        smash = false;
        hittable = false;
        showBallPath = false;
        passPlayer = null;

        //returns time to normal
        slowDown = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = regFixedTimeDelta * Time.timeScale;

        //starts the cooldown for hiting the ball
        hitTime = 0;
        hitMode = HitMode.Ready;
        hitCooldownTimer = 0;
        ballTarget.goToBall = false;

        //Clear the information used for shooting the ball
        shootInfo.Reset();

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
        movement.enabled = true;

        //stop using the particles for predicting where the ball will go
        shotPathDrawer.ClearParticles();
        shotPathDrawer.ClearFunction();

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
    //predicts where the ball will go
    void PredictShot()
    {
        Function shotFunction = new Function(null, shootInfo.GetCalculateShot());
        Function spinFunction = new Function(null, null, shootInfo.GetCalculatedSpin(transform.forward));
        shotPathDrawer.SetPathSpread(shootInfo.power / playerShoot.MaxPower * 7*30);
        shotPathDrawer.SetFunction(shotFunction);
        if (!shotPathDrawer.GetFunction().Contains(Function.Gravity()))
        {
            shotPathDrawer.AddGravity();
            
        }
        shotPathDrawer.AddFunction(spinFunction);
        shotPathDrawer.particleStartPosition = ballStartingPosition;
        //print(shotPathDrawer.GetFunction());
        shotPathDrawer.SimulatePath(this);
    }

    Vector3 PredictLanding(float height)
    {
        Function function = shotPathDrawer.GetFunction().Flatten();
        //print(function);
        Vector3 landing = Vector3.zero;

        float time = (-function.GetVelocity().y - Mathf.Sqrt(Mathf.Pow(function.GetVelocity().y,2) - 4 * function.GetAcceleration().y * 0.5f * (function.GetPosition().y - height))) / (function.GetAcceleration().y);
        landing = 0.5f * function.GetAcceleration() * Mathf.Pow(time, 2) + function.GetVelocity() * time + function.GetPosition();
        //print(landing);
        return landing + vBall.transform.position;
    }

    Vector3 PredictLanding()
    {
        return PredictLanding(0.1f);
    }
    #endregion

    #region Hittable

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
    #endregion
}

