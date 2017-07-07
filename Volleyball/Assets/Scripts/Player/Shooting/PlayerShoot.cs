using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootCapsule
{
    public Vector3 powerVector;
    public float power;
    public Vector3 spin;
    public Vector3 aimDirection;

    private Vector3 additiveDirection;
    private Vector3 minimumDirection;

    public ShootCapsule(Vector3? powerVector = null, float power = 1)
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

    public ShootCapsule ResetPower()
    {
        ShootCapsule resetShootCapsule = new ShootCapsule(null, MaxPower * minimumPower);
        return resetShootCapsule;
    }

    public void ShootBall(VolleyballScript vBall, ShootCapsule shotInfo)
    {
        vBall.rb.velocity = Vector3.zero;
        vBall.Shoot(shotInfo.GetCalculateShot() * vBall.rb.mass, ballSpin, player.myParent);
        vBall.CollideWithPlayer(player);
        //if the court says that your ready to serve then tell them that you are currently
        //serving
        if (player.Court.readyToServe)
            player.Court.readyToServe = false;
    }
}
