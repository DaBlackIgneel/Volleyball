using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle
{
    public float distance;
    public float height;

    public Obstacle(float distance, float height)
    {
        this.distance = distance;
        this.height = height;
    }

    public override string ToString()
    {
        System.Text.StringBuilder formattedString = new System.Text.StringBuilder();
        formattedString.Append("Distance to Obstacle: " + distance + "\n");
        formattedString.Append("Height of Obstacle: " + height + "\n");
        return formattedString.ToString();
    }
}

public class ShootCapsule
{
    public Vector3 powerVector;
    public float power;
    public Vector3 spin;
    public Vector3 aimDirection;
    public Vector3 aimSpot;
    public Vector3 aimSpotAddition;
    public Vector3 currentPosition;
    public List<Obstacle> obstacles;
    public float shotTime;
    public bool addSpin;
    private Vector3 additiveDirection;
    private Vector3 minimumDirection;

    public ShootCapsule()
    {
        obstacles = new List<Obstacle>();
        powerVector = Vector3.one;
        aimSpotAddition = Vector3.zero;
        power = 1;
        minimumDirection = Vector3.one * float.MinValue;
    }

    public ShootCapsule(Vector3? powerVector = null, float power = 1): this()
    {
        this.powerVector = powerVector ?? Vector3.one;
        this.power = power;
    }

    public Vector3 GetCalculatedPower()
    {
        return powerVector * power;
    } 

    public Vector3 GetCalculateShot()
    {
        return Vector3.Scale(GetCalculatedPower(), GetCalculatedDirection());
    }

    public Vector3 GetCalculatedDirection()
    {
        Vector3 calculatedDirection = aimDirection;
        if (calculatedDirection.x < minimumDirection.x)
            calculatedDirection.x = minimumDirection.x;
        if (calculatedDirection.y < minimumDirection.y)
            calculatedDirection.y = minimumDirection.y;
        if (calculatedDirection.z < minimumDirection.z)
            calculatedDirection.z = minimumDirection.z;
        return (additiveDirection + calculatedDirection).normalized;
    }

    public Vector3 GetCalculatedSpin(Vector3 forward)
    {
        Vector3 spin = Vector3.Scale(this.spin - Vector3.up, (new Vector3(1, -1, 1)));
        if (spin.y > 0)
            spin.y = 0;
        Vector3 direction = Vector3.Scale(forward, new Vector3(1, 0, 1));
        Vector3 calculatedSpin = Vector3.Cross(Vector3.up, direction).normalized * spin.x;
        calculatedSpin.y = spin.y;
        return calculatedSpin;
    }

    public Vector3 GetCalculatedAimSpot()
    {
        return aimSpot + aimSpotAddition;
    }

    public void SetAdditiveDirection(Vector3 addDirection)
    {
        additiveDirection = addDirection;
    }

    public void SetSpin(Vector3 spin)
    {
        this.spin = spin;
    }

    public void SetMinimumDirection(Vector3 minimumDir)
    {
        minimumDirection = minimumDir;
    }

    public void SetPower(float power)
    {
        this.power = power;
    }

    public void SetPowerVector(Vector3 powerVector)
    {
        this.powerVector = powerVector;
    }

    public void Reset()
    {
        powerVector = Vector3.one;
        power = 1;
        spin = Vector3.up;
        minimumDirection = Vector3.one * float.MinValue;
        additiveDirection = Vector3.zero;
        aimDirection = Vector3.one;
        shotTime = 1;
        obstacles.Clear();
    }

    public override string ToString()
    {
        System.Text.StringBuilder formattedString = new System.Text.StringBuilder();
        formattedString.Append("********SHOOT_INFO********\n");
        formattedString.Append("Aim Direction: " + GetCalculatedDirection() + "\n");
        formattedString.Append("Aim Spot: " + aimSpot + "\n");
        formattedString.Append("Current Position: " + currentPosition + "\n");
        formattedString.Append("Power: " + GetCalculatedPower() + "\n");
        formattedString.Append("Spin: " + spin + "\n");
        formattedString.Append("ShootTime: " + shotTime + "\n");

        formattedString.Append("Obstacles:  ");
        foreach(Obstacle obstacle in obstacles)
        {
            formattedString.Append(obstacle);
        }
        formattedString.Append("**************************");
        return formattedString.ToString();
    }
}


public class PlayerShoot : MonoBehaviour, IInitializable
{

    [Range(0, 25)]
    public float MaxPower = 12.5f;
    [Range(0.001f, .99f)]
    public float minimumPower = .1f;
    [Range(.001f, 2)]
    public float lobConst = .45f;
    [Range(1, 4)]
    public float smashConst = 2;

    public float lobAgjust = 1f;
    public float maxSpin = 5;

    public float tau = .5f;
    public float divisionFactor = 100f;

    public float timeConstant = 1f;

    [SerializeField]
    Vector2 ballSpin;
    [SerializeField]
    [Range(0.1f, 4)]
    float smashScrollSpeed = .5f;
    [SerializeField]
    [Range(0.01f, 2)]
    float lobScrollSpeed = .25f;
    SpecialAction player;

    public Vector3 vecPower;
    [SerializeField]
    float power = 1;


    public void Initialize(SpecialAction player)
    {
        this.player = player;
    }

    public ShootCapsule UserCalculatePower(Vector2 prevMousePos, ShootCapsule shotInfo, Vector3 myLastVelocity)
    {
        //the mouse position difference from the point where you aimed the ball,
        //and the current mouse position
        Vector2 deltaMousePos = (Vector2)Input.mousePosition - prevMousePos;
        Vector3 minDir = Vector3.one * float.MinValue;
        Vector3 addDir = Vector3.zero;
        float runHitConst = player.Court.serve ? 4 : 10;
        runHitConst = deltaMousePos.magnitude > 10 ? runHitConst : 1;
        
        //set the horizontal component of the spin based on the horizontal mouse position
        ballSpin.x = deltaMousePos.x / (Screen.width / 2) * maxSpin;

        //calculate the power of the hit
        power = ((Mathf.Abs(deltaMousePos.y) / (Screen.height / 2) * (1 - minimumPower)) + minimumPower) * MaxPower;
        if (power > MaxPower)
            power = MaxPower;

        //if the mouse's Y position is above the original mouse position, then do a lob pass
        if (deltaMousePos.y > 0)
        {
            //Allow the the adjustment of the arc by scrolling the mouse wheel
            lobAgjust -= (Input.mouseScrollDelta.y) * lobScrollSpeed;
            if (lobAgjust < 0.01f)
                lobAgjust = 0.01f;
            float myLob = lobConst * lobAgjust;

            //calculate the lob direction
            addDir.y = 1 + deltaMousePos.y / (Screen.height / 2) * myLob;

            minDir.y = 0;
            //powerVector = powerVector.normalized * Vector3.one.magnitude;
            //no floater of forward spin for a lob
            ballSpin.y = 1;
        }
        //if the mouse's Y position is above the original mouse position, then do a smash hit
        else
        {
            //add more force to the hit
            power *= (smashConst + myLastVelocity.magnitude / runHitConst);

            //use the mouse scroll to control the amount of forward spin on the ball
            if (ballSpin.y > 1.1f)
                ballSpin.y -= (Input.mouseScrollDelta.y) * smashScrollSpeed * (1 + myLastVelocity.magnitude / runHitConst);

            //when there no forward spin use the mouse scroll to control the strength of the
            //floater randomness
            else
                ballSpin.y -= (Input.mouseScrollDelta.y) * smashScrollSpeed / 10f;

            //set the max and min parts of the vertical spin
            if (ballSpin.y < 0)
                ballSpin.y = 0;
            else if (ballSpin.y > (maxSpin * (1 + myLastVelocity.magnitude / runHitConst) + 1))
                ballSpin.y = maxSpin * (1 + myLastVelocity.magnitude / runHitConst) + 1;
            shotInfo.powerVector = Vector3.one;
        }
        //when the ball is lower, add an inherent arc to the hit
        addDir += Vector3.up * Mathf.Exp(Mathf.Abs(player.vBall.rb.transform.position.y) * -1 / tau) / divisionFactor;

        shotInfo.SetPower(power);
        shotInfo.SetAdditiveDirection(addDir);
        shotInfo.SetSpin(ballSpin);
        shotInfo.SetMinimumDirection(minDir);
        vecPower = shotInfo.powerVector;
        return shotInfo;
    }
    public ShootCapsule UserPassToPlayer(Vector2 prevMousePos, ShootCapsule shotInfo, SpecialAction passPlayer)
    {
        //the mouse position difference from the point where you aimed the ball,
        //and the current mouse position
        Vector2 deltaMousePos = (Vector2)Input.mousePosition - prevMousePos;
        shotInfo.SetSpin(Vector3.up);
        shotInfo.shotTime = Strategy.PassSpeedToFloat(PassSpeed.Slow) - Mathf.Abs(deltaMousePos.y / (Screen.height / 2)) * (Strategy.PassSpeedToFloat(PassSpeed.Slow) - Strategy.PassSpeedToFloat(PassSpeed.SuperQuick));
        float horizontalSpeedAimSpotConst = Strategy.PassSpeedToFloat(PassSpeed.SuperQuick) * 2 / (Strategy.PassSpeedToFloat(PassSpeed.SuperQuick) + shotInfo.shotTime);
        shotInfo.aimSpot = passPlayer.myMovement.transform.position + Vector3.up * passPlayer.mySquat.HEIGHT 
            + Vector3.Scale(passPlayer.myMovement.rb.velocity * shotInfo.shotTime , (new Vector3(horizontalSpeedAimSpotConst,1, horizontalSpeedAimSpotConst))) 
            + 0.5f*Physics.gravity * Mathf.Pow(shotInfo.shotTime, 2);
        if (shotInfo.aimSpot.y < passPlayer.Height)
            shotInfo.aimSpot.y = passPlayer.Height;
        shotInfo.aimSpotAddition += Vector3.up * Input.mouseScrollDelta.y * lobScrollSpeed;
        Vector3 distance = shotInfo.GetCalculatedAimSpot() - player.vBall.transform.position;

        float horizontalSpeed = (Vector3.ProjectOnPlane(distance, Vector3.up).magnitude - shotInfo.spin.x * .5f * Mathf.Pow(shotInfo.shotTime, 2)) / shotInfo.shotTime;
        float verticalSpeed = (-.5f * (Physics.gravity.y - shotInfo.spin.y + 1) * shotInfo.shotTime * shotInfo.shotTime + distance.y) / shotInfo.shotTime;
        Vector3 velocity = (Vector3.ProjectOnPlane(distance, Vector3.up).normalized * horizontalSpeed + Vector3.up * verticalSpeed);
        shotInfo.SetPower(velocity.magnitude);
        shotInfo.aimDirection = velocity.normalized;
        return shotInfo;
    }


    /**
     *Calculates all the information needed in order to shoot the ball at a specific spot
     * @param shotInfo - it needs the following properties populated...
     *          -bool addSpin = used to tell if spin should be added to the shot.
     *          -Vector3 aimSpot = the spot your aiming for.
     *          -float shotTime = time need for the ball to get the the spot your aiming for.
     *          -Vector3 currentPosition = position of the object your shooting.
     * 
     * @returns A ShootCapsule containing the spin of the ball, the power, and direction need to shoot the ball.
     */
    public ShootCapsule ComputerCalculateShot(ShootCapsule shotInfo)
    {
        return ComputerCalculateShot(shotInfo, Vector3.zero);
    }
    public ShootCapsule ComputerCalculateShot(ShootCapsule shotInfo, Vector3 myLastVelocity)
    {
        Vector3 distance = shotInfo.aimSpot - shotInfo.currentPosition;
        Vector3 velocity = Vector3.zero;
        float runHitConst = player.Court.serve ? 4 : 10;
        //Add spin if you are hitting the ball over the net.
        if (shotInfo.addSpin)
        {
            int maxPoss = 16;
            int sign = Random.Range((int)(maxPoss/4 + maxPoss/8)+1, maxPoss+1);
            sign = (sign-1) / (maxPoss / 2);
            sign *= Random.Range(0, 2) * 2 - 1;
            float maxPossibleSpin = maxSpin * (1 + myLastVelocity.magnitude / runHitConst);
            float floater = 0;//player.Court.serve ? Random.Range(0.5f,1) : 0;
            shotInfo.SetSpin(new Vector3(/*Random.Range(maxPossibleSpin * sign * .75f, maxPossibleSpin * sign)*/ 0, Mathf.Abs((int)((sign + 2)/2)) * Random.Range(maxPossibleSpin * .5f, maxPossibleSpin) + 1, 0));
        }
        else
        {
            shotInfo.SetSpin(Vector3.up);
        }

        //Calculate the speed at which the ball needs to move in order to go to the spot your aiming for.
        float horizontalSpeed = (Vector3.ProjectOnPlane(distance, Vector3.up).magnitude - shotInfo.spin.x * .5f * Mathf.Pow(shotInfo.shotTime,2)) / shotInfo.shotTime;
        float verticalSpeed = (-.5f * (Physics.gravity.y - shotInfo.spin.y + 1) * shotInfo.shotTime * shotInfo.shotTime + distance.y) / shotInfo.shotTime;
        velocity = (Vector3.ProjectOnPlane(distance, Vector3.up).normalized * horizontalSpeed + Vector3.up * verticalSpeed);

        //The location at which the ball would cross the net.
        Vector3 ballAtNet = player.vBall.transform.position + velocity * (-player.vBall.transform.position.z / velocity.z) + (Physics.gravity + Vector3.up * (- shotInfo.spin.y + 1)) * Mathf.Pow((-player.vBall.transform.position.z / velocity.z), 2) * .5f;
        //if your aiming to hit the ball over the net and you normal hit would cause the ball to hit the net,
        //then lob the ball over the net.
        if (CourtScript.FloatToSide(shotInfo.aimSpot.z) != player.currentSide && ballAtNet.y < player.Court.NetHeight + .1f * shotInfo.spin.y/10 + .243/2f)
        {
            //The position the ball should be at in order to barely go over the net.

            ballAtNet.y = player.Court.NetHeight + player.vBall.transform.localScale.y / 2 + .1f * shotInfo.spin.y / 10 + .243f / 2 + Random.Range(0,.8f) * shotInfo.shotTime;

            //Calculate the distance to the net and to the spot your aiming for.
            float distTillNet = (ToHorizontalPlane(ballAtNet) - ToHorizontalPlane(shotInfo.currentPosition)).magnitude * ((float)player.currentSide);
            float distTillAimSpot = (ToHorizontalPlane(shotInfo.aimSpot) - ToHorizontalPlane(shotInfo.currentPosition)).magnitude * ((float)player.currentSide);

            //Find the ratio between the time the ball goes over the net, and the time that the ball hits the
            //spot your aiming for.
            float timeRatio = distTillNet / distTillAimSpot;
            float timeTillSpot = Mathf.Sqrt((shotInfo.aimSpot.y * timeRatio - ballAtNet.y - shotInfo.currentPosition.y * (timeRatio - 1))/((9.8f + shotInfo.spin.y-1) * 0.5f * timeRatio * (timeRatio -1)));

            //Calculate the speed that the ball will need to move in order to barely go over the net and hit 
            //the spot your aiming for.
            horizontalSpeed = (float)player.currentSide * distTillAimSpot / timeTillSpot;
            verticalSpeed = (shotInfo.aimSpot.y - shotInfo.currentPosition.y + (9.8f + shotInfo.spin.y-1) *0.5f* Mathf.Pow(timeTillSpot, 2)) / timeTillSpot;
            velocity = (Vector3.ProjectOnPlane(distance, Vector3.up).normalized * horizontalSpeed + Vector3.up * verticalSpeed);

            //Only allow downward spin if your going to lob it over the net.
            shotInfo.SetSpin(Vector3.up * shotInfo.spin.y);
        }
        
        shotInfo.aimDirection = velocity.normalized;
        power = /*velocity.magnitude <= MaxPower * 2 ?*/ velocity.magnitude;// : MaxPower * 2;
        shotInfo.power = power;
        ballSpin = Vector3.up;
        return shotInfo;
    }

    /**
     * Calculates all information needed to toss the ball straight up.
     * @returns shootCapsule containing the spin of the ball, the power, and direction need to toss ball
     * straight up.
     */
    public ShootCapsule TossUp()
    {
        ShootCapsule shotInfo = new ShootCapsule();
        shotInfo.aimDirection = Vector3.up;
        shotInfo.power = MaxPower * .5f;
        shotInfo.spin = Vector3.up;
        return shotInfo;
    }

    /**
     * Calculates all information needed to toss the ball foward for a running jump.
     * @returns shootCapsule containing the spin of the ball, the power, and direction need to toss ball
     * foward.
     */
    public ShootCapsule FowardTossUp()
    {
        ShootCapsule shotInfo = new ShootCapsule();
        shotInfo.aimDirection = (Vector3.up * 1.1f + transform.forward).normalized;
        shotInfo.power = MaxPower * 0.5f;
        shotInfo.spin = Vector3.up;
        return shotInfo;
    }
    public ShootCapsule FowardTossUp(float runDelay, float jumpDelay, float meetTime)
    {
        ShootCapsule shotInfo = new ShootCapsule();
        float horizontalSpeed = -1 * (Vector3.Project(player.vBall.transform.position - player.transform.position, Vector3.up).magnitude + (player.myMovement.sprintSpeed+0.5f) * runDelay - (player.myMovement.sprintSpeed + 0.5f) * meetTime) / meetTime;
        float verticalSpeed =  (player.vBall.transform.position.y  - (player.transform.position.y + player.Height) + player.myMovement.jumpSpeed * (meetTime - jumpDelay) + 0.5f * Physics.gravity.y * (-2 * jumpDelay * meetTime + jumpDelay * jumpDelay)) / meetTime;
        Vector3 velocity = horizontalSpeed * player.transform.forward + Vector3.up * verticalSpeed;
        shotInfo.aimDirection = velocity.normalized;//(Vector3.up * 1.1f + transform.forward).normalized;
        shotInfo.power = velocity.magnitude;
        shotInfo.spin = Vector3.up;
        return shotInfo;
    }

    public ShootCapsule ResetPower()
    {
        ShootCapsule resetShootCapsule = new ShootCapsule(null, MaxPower * minimumPower);
        return resetShootCapsule;
    }

    public void ShootBall(VolleyballScript vBall, ShootCapsule shotInfo)
    {
        //Shoot the ball.
        vBall.Shoot(shotInfo, player.myParent);

        //Tell the ball that you just were hit by this player.
        vBall.CollideWithPlayer(player);
       
        //If the court says that your ready to serve then tell them that you are currently
        //serving
        if (player.Court.readyToServe)
            player.Court.readyToServe = false;
    }

    private Vector3 ToHorizontalPlane(Vector3 vector)
    {
        return Vector3.ProjectOnPlane(vector, Vector3.up);
    }
}
