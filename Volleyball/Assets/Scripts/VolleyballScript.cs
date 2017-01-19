using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { X, Y, Z }

public class VolleyballScript : MonoBehaviour {
    public bool grounded;
    public float SpinAddConst { get { return spinAddConst; } }
    public float MaxAdditions { get { return maxAdditions; } }


    [System.NonSerialized]
    public Rigidbody rb;
    SpecialAction previousPlayer;
    float samePlayerCount;
    float samePlayerCooldown = .1f;
    CourtScript court;
    int touches = 0;
    
    [SerializeField]
    Vector2 spin;

    [SerializeField]
    float torqueConst = 200;

    [SerializeField]
    float maxAngularVelocity = 300;

    [SerializeField]
    float spinAddConst = 1.01f;

    [SerializeField]
    int maxAdditions = 50;
    int currentAdditions = 0;

    [SerializeField]
    float floaterConst = 1;
    [SerializeField]
    float scrollSpeed = 0.25f;
    [SerializeField]
    float stepSpeed = 0.1f;
    float noisePosition = 0;

    bool endServe;
    bool lastHitByServer;

    Vector3 right;
	// Use this for initialization
	void Start () {
        
        //used to set the max amount of effect the spin has on the velocity of the ball
        currentAdditions = maxAdditions;

        //This is the setting when there is no spin on the ball
        //Vector2.up represents (0,1) so there is zero spin in the
        //X direction and 1 in the Y inbetween of a forward spin
        //and a no spin floater, so it represents zero spin in the y
        spin = Vector2.up;

        //the component that deals with the physics of the object
        rb = GetComponent<Rigidbody>();

        //this will refer back to game specific things
        court = GameObject.FindGameObjectWithTag("CourtController").GetComponent<CourtScript>();

        //this is used so that the ball will spin with a velocity of more than 1 rotation per second
        rb.maxAngularVelocity = maxAngularVelocity;
    }
	
	// Update is called once per frame
	void Update () {
        
        //if you go off the map respawn on the map
        if (transform.position.y < -5.0f || transform.position.y > 30)
        {
            rb.MovePosition(new Vector3(transform.position.x, 5, transform.position.z));
            rb.velocity = rb.velocity / 10f;
        }
	}

    void FixedUpdate()
    {
        //Adds the affect of spin onto the velocity of the ball
        AddSpin();

        //continue to change the air density map for the jump floater
        if(currentAdditions >= maxAdditions)
            noisePosition += scrollSpeed;

        //makes is so that the ball won't check for the same player collisions immediately
        if (samePlayerCount < samePlayerCooldown)
            samePlayerCount += Time.fixedDeltaTime;
    }


    

    //Adds an initial force to the ball, and sets up no spin
    public void Shoot(Vector3 force, Transform reference)
    {
        SetSpin(Vector2.up, reference);
        rb.AddForce(force,ForceMode.Impulse);
        if (endServe)
        {
            court.serve = false;
            lastHitByServer = true;
        }
    }

    //Adds an initial force to the ball, and sets up a spin
    public void Shoot(Vector3 force, Vector2 spin, Transform reference)
    {
        Shoot(force, reference);
        SetSpin(spin, reference);
    }

    //Adds a spin on the ball relative to the player, and 
    //adds the calculable spin to the ball
    void SetSpin(Vector2 spin, Transform reference)
    {
        this.spin = spin;
        right = reference.right; 
        currentAdditions = 0;

        rb.AddTorque((reference.up * spin.x * 1 + reference.right * spin.y) * torqueConst);
    }

    //adds spin or floater moveablilty to the ball
    void AddSpin()
    {
        //max amount of force the spin can add onto the ball
        if (currentAdditions < maxAdditions)
        {
            //initialize the spin velocity variable
            Vector3 spinAddition = Vector3.zero;

            //if your not doing a floater calculate the spin
            if (spin.y >= 1)
            {
                spinAddition.y = -SpinCalculator(ref spin.y, Direction.Y);
                spinAddition.x = SpinCalculator(ref spin.x, Direction.X);
            }
            //if you are doing a floater calculate the floater air density affect
            else if (spin.y < 1)
            {
                spinAddition.x = GetFloater(currentAdditions, Direction.X);
                spinAddition.y = GetFloater(currentAdditions, Direction.Y);
            }

            //add the spin vector to the current velocity
            //rb.velocity += spinAddition.y * Vector3.up + spinAddition.x * right;
            rb.AddForce((spinAddition.y * Vector3.up + spinAddition.x * right) * rb.mass, ForceMode.Impulse);
            currentAdditions++;
        }
    }

    float SpinCalculator(ref float mySpin, Direction dimension)
    {
        //exponentially increase the effect of the spin
        //exponent is very close to 1 so the effect is small but noticable
        //makes it so that at around the peak height of the ball, the spin
        //starts to make a big affect on the position of the ball
        if(dimension == Direction.Y)
        {
            mySpin--;
            mySpin *= spinAddConst;
            mySpin++;
            return (mySpin-1) / 100f;
        }
        else
        {
            mySpin *= spinAddConst;
            return mySpin / 100f;
        }
        
    }

    //keeps track of collisions with players
    public void CollideWithPlayer(SpecialAction currentPlayer)
    {
        //if the last player the ball collided with is the same player its colliding with now
        if (previousPlayer == currentPlayer)
        {
            //checks if the last time you collided with the player was within a reasonable
            //amount of time
            if (samePlayerCount >= samePlayerCooldown)
            {
                //if you are the server then declare that the serve is now over
                //when the player hits the ball
                if (court.serve && currentPlayer.currentPosition == 1)
                {
                    endServe = true;
                }
                //if your not the server than its a penalty
                else
                {
                    court.CourtRules.ReportDoubleTouch(currentPlayer.currentSide);
                }
            }
        }
        //if the current player colliding with the ball a different player
        else
        {
            //if the player was on the same side as the last player then increase the 
            //amount of times that the side has touched the ball
            if (previousPlayer != null && currentPlayer.currentSide == previousPlayer.currentSide)
                touches++;
            //if the player was on a different side then reset the amount of times that
            //the side has touched the ball
            else
                touches = 1;
            if(touches > Rules.maxNumberOfHits)
            {
                court.CourtRules.ReportMaxNumberHit(currentPlayer.currentSide);
            }
            if (lastHitByServer)
                lastHitByServer = false;
        }
        //no more spin calculations
        currentAdditions = maxAdditions;

        //current player is now the last player to hit the ball
        previousPlayer = currentPlayer;

        //reset the same player cooldown
        samePlayerCount = 0;
    }

    //stops all motion
    public void ResetMotion()
    {
        //no more spin 
        spin = Vector2.up;

        //stop the ball
        rb.velocity = Vector3.zero;

        //stop the actual rotation of the ball
        rb.angularVelocity = Vector3.zero;
    }

    //deletes record of last player to hit the ball 
    //and stops all motion of the ball
    public void Reset()
    {
        previousPlayer = null;
        ResetMotion();
        samePlayerCount = 0;
        endServe = false;

    }

    //gets the affect of the air density on the velocity of the ball
    public float GetFloater(float noiseOffset, Direction dimension)
    {
        return GetNoise(noiseOffset * stepSpeed, dimension) * (1-spin.y) * floaterConst;
    }

    //gets the affect of the air density on the velocity of the ball
    //used by the prediction particles for the aiming function in SpecialAction
    public float GetFloater(float noiseOffset, float spiny, Direction dimension)
    {
        return GetNoise(noiseOffset * stepSpeed, dimension) * (1 - spiny) * floaterConst;
    }

    //finds the current position of the PerlinNoise, then returns the
    //differential for each dimension of the PerlinNoise at the specified
    //position
    float GetNoise(float noiseOffset, Direction dimension)
    {
        Vector2 offset;
        if (dimension == Direction.X)
            offset = Vector2.right * noiseOffset;
        else
            offset = Vector2.up * noiseOffset;
        return PerlinNoiseDerivative(offset, dimension);
    }

    //uses the perlin noise to model the affect of the air density on the 
    //velocity of the ball.  Uses the one dimensional differential of the
    //perlin noise for the velocity of the ball
    float PerlinNoiseDerivative(Vector2 noiseOffset, Direction dimension)
    {
        //finds the dimension that you want to find the differential of
        Vector2 stepBack;
        if (dimension == Direction.X)
            stepBack = Vector2.right * stepSpeed;
        else
            stepBack = Vector2.up * stepSpeed;

        //finds the offset of the previous noisePosition
        stepBack = noiseOffset - stepBack;

        //calculate the change per unit of the Perlin Noise at the noise offset position
        float derivative = (Mathf.PerlinNoise(noisePosition + noiseOffset.x, noiseOffset.y) - Mathf.PerlinNoise(noisePosition + stepBack.x, stepBack.y))/stepSpeed;
        if (dimension == Direction.Y)
            derivative /= 2;
        return derivative; 
    }

    //on collision with anything reset the spin of the ball
    //collisions with the objects other than players
    void OnCollisionEnter(Collision other)
    {
        

        //if currently colliding with the player then go through all the player collision checks
        if (other.transform.tag == "Player")
        {
            SpecialAction currentPlayer = other.transform.GetComponentInChildren<SpecialAction>();
            if(currentPlayer != null)
                CollideWithPlayer(currentPlayer);
            return;
        }
        //if the ball that was served hit the net the report a net serve
        else if(other.transform.tag == "Net")
        {
            if(lastHitByServer)
            {
                court.CourtRules.ReportNetServe(previousPlayer.currentSide);
            }
        }
        //will eventually be used for ending a rally
        else if (!court.rallyOver)
        {
            if (previousPlayer != null)
            {
                court.CourtRules.ReportGroundedOut(rb.position, other.transform.tag != "Court", previousPlayer.currentSide);
            }
            currentAdditions = maxAdditions;
        }
    }

    //reset the spin of the ball
    void ZeroMotion()
    {
        spin = Vector2.up;
        currentAdditions = maxAdditions;
    }
}
