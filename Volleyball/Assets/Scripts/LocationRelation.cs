using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationRelation : MonoBehaviour {

    public CourtDimensions.Section tempSection = CourtDimensions.Section.BackLeft;
    public Side tempSide = Side.Left;
    public Vector3 aimSpot;
    public Dictionary<Side, SpecialAction> ClosestPlayerToBall
    {
        get { return ballHandler; }
    }

    public float TimeTillBallReachesLocation
    {
        get {
            ballTime = Mathf.Abs((aimSpot.x - vBall.transform.position.x) / vBall.rb.velocity.x);
            return ballTime; }
    }

    float ballTime;
    CourtScript court;
    VolleyballScript vBall;
    CourtDimensions.Rectangle aimingSection;
    GameObject temp;
    Dictionary<Side, SpecialAction> ballHandler;

    Random rand;
    // Use this for initialization
    void Start () {
        court = GameObject.FindGameObjectWithTag("CourtController").GetComponent<CourtScript>();
        vBall = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();
        rand = new Random();
        aimingSection = court.dimensions.CourtSection(tempSide, tempSection);
        temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ballHandler = new Dictionary<Side, SpecialAction>();
        StartCoroutine("UpdateClosestPlayerToBall");
    }
	
	// Update is called once per frame
	void Update () {
        aimingSection = court.dimensions.BackLineRect(tempSide);
        temp.transform.localScale = aimingSection.Dimension() + Vector3.up * .1f;
        temp.transform.position = aimingSection.Center();

    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    void FixedUpdate()
    {
        
    }

    public SpecialAction FindClosestPlayerToBall(Side tempSide)
    {
        foreach (SpecialAction p in court.Players[tempSide])
        {
            Vector3 distance = MovePositionAtMaxHeight(2, p.myMovement) - p.myMovement.transform.position;
            p.distanceToBallLanding = distance;
            try
            {
                if(ballHandler[tempSide].distanceToBallLanding.magnitude > p.distanceToBallLanding.magnitude && (IsValidPlayer(p)))
                {
                    ballHandler[tempSide] = p;
                }
            }
            catch(System.NullReferenceException e)
            {
                ballHandler[tempSide] = p;
                print(e.Message);
            }
            catch(KeyNotFoundException e)
            {
                ballHandler.Add(tempSide, p);
                print(e.Message);
            }
        }
        return ballHandler[tempSide];
    }

    IEnumerator UpdateClosestPlayerToBall()
    {
        for (;;)
        {
            FindClosestPlayerToBall(Side.Left);
            FindClosestPlayerToBall(Side.Right);
            yield return new WaitForSeconds(.05f);
        }
    }

    public Vector3 AimSpot(SpecialAction player)
    {
        try
        {
            return Vector3.Scale(court.currentStrategy[player.currentSide][StrategyType.Offense].GetPassLocation(player.vBall.GetSideTouches(player.currentSide), player), new Vector3((int)player.currentSide, 1,(int)player.currentSide));
        }
        catch
        {
            return aimingSection.upperRightCorner - new Vector3(Random.Range(0, aimingSection.Dimension().x), -.2f, Random.Range(0, aimingSection.Dimension().z));
        }
        
    }
    public Pass AimSpotInfo(SpecialAction player)
    {
        try
        {
            return court.currentStrategy[player.currentSide][StrategyType.Offense].GetPassInformation(player.vBall.GetSideTouches(player.currentSide), player);
        }
        catch
        {
            return new Pass(PassType.Location,aimingSection.upperRightCorner - new Vector3(Random.Range(0, aimingSection.Dimension().x), -.2f, Random.Range(0, aimingSection.Dimension().z)));
        }

    }

    public static Path GetMovement(SpecialAction player)
    {
        return player.Court.currentStrategy[player.currentSide][StrategyType.Offense].movementPath[player.currentPosition-1];
    }

    public static Vector3 StrategyLocationToCourt(Vector3 loc, Side currentSide)
    {
        return Vector3.Scale(loc, new Vector3((int)currentSide, 1, (int)currentSide));
    }

    public static Vector3 PathLocationToCourt(Vector3 loc, Side currentSide)
    {
        return Vector3.Scale(loc, new Vector3(-(int)currentSide, 1, (int)currentSide));
    }

    public bool IsValidPlayer(SpecialAction player)
    {
        return player.vBall.PreviousPlayer != player || player.Court.Players[player.currentSide].Count == 1;
    }

    public Vector3 MovePositionAtMaxHeight(float maxHeight, PlayerMovement caller)
    {
        Vector3 predictedPosition = Vector3.zero;

        try
        {
            Vector3 distance = vBall.transform.position - caller.transform.position;
            Vector3 ballVelocity = vBall.rb.velocity;
            float speed = caller.sprintSpeed;
            float direction = Mathf.Asin((distance.z / distance.x * ballVelocity.x - ballVelocity.z) / Mathf.Sqrt(speed * speed + Mathf.Pow(distance.z / distance.x * speed, 2))) + Mathf.Atan(distance.z / distance.x);
            float time = distance.x / (speed * Mathf.Cos(direction) - ballVelocity.x);
            Vector3 ballPosition = vBall.transform.position + ballVelocity * time + .5f * Physics.gravity * time * time;
            if (ballPosition.y > PredictBallPosition(maxHeight).position.y)
            {
                predictedPosition = PredictBallPosition(maxHeight).position;
            }
            else if(ballPosition.y > .2f)
            {
                predictedPosition = ballPosition;
            }
            else
            {
                predictedPosition = PredictBallLanding();
            }
        }
        catch (System.Exception e)
        {
            predictedPosition = PredictBallLanding();
            print(e.Message);
        }
        return predictedPosition;
    }
    public Vector3 PredictBallLanding()
    {
        return PredictBallPosition(.1f).position;
    }

    public PredictionPack PredictBallPosition(float targetHeight)
    {
        Vector3 landing;

        float time = (-vBall.rb.velocity.y - Mathf.Sqrt(vBall.rb.velocity.y * vBall.rb.velocity.y - 4 * -9.8f * .5f * (vBall.transform.position.y - targetHeight))) / (-9.8f);
        landing = vBall.rb.velocity * time + vBall.transform.position + 0.5f * Physics.gravity * time * time;


        return new PredictionPack(time, landing);
    }

    public float PredictMovementTime(Vector3 targetPosition, PlayerMovement caller)
    {
        float fallTime = (-caller.rb.velocity.y - Mathf.Sqrt(caller.rb.velocity.y * caller.rb.velocity.y - 4 * -9.8f * .5f * (caller.transform.position.y))) / (-9.8f);
        Vector3 startPos = caller.transform.position + fallTime * caller.rb.velocity;
        Vector3 distance = targetPosition - startPos;
        distance.y = 0;
        //direction.Normalize();
        float time = distance.magnitude / caller.sprintSpeed + fallTime;
        return time;
    }
}
