using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rules : MonoBehaviour {

    public static float maxNumberOfHits = 3;
    
    public bool DoubleTouch
    {
        get { return doubleTouch; }
    }
    public bool GroundedOut
    {
        get { return groundedOut; }
    }
    public bool NetServe
    {
        get { return netServe; }
    }
    public bool TouchNet
    {
        get { return touchNet; }
    }
    public bool MaxHitPerSide
    {
        get { return maxHitPerSide; }
    }


    bool doubleTouch;
    bool groundedOut;
    bool netServe;
    bool touchNet;
    bool maxHitPerSide;
    string message;
    CourtScript court;

	// Use this for initialization
	void Start () {

        //finds the court which is used to refer to all the game specifics
        court = GameObject.FindGameObjectWithTag("CourtController").GetComponent<CourtScript>();


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    #region Reports
    public void ReportDoubleTouch(Side playerSide)
    {
        //if the rule that states that you cannot hit the ball is enabled then update the score and
        //end the rally
        if(doubleTouch)
        {
            message = "A player touched the ball twice";
            court.UpdateScore(CourtScript.OppositeSide(playerSide), message);
        }
    }

    public void ReportGroundedOut(Vector3 position, bool outOfBounds, Side lastToTouchBall)
    {
        //if the rule that states that when the rally ends when the ball hits the ground is enabled
        //then update the score and end the rally;
        if (groundedOut)
        {
            if (outOfBounds)
            {
                message = "OUT";
                court.UpdateScore(CourtScript.OppositeSide(lastToTouchBall),message);
            }
            else
            {
                Vector3 relativePosToCourt = position - court.transform.position;
                Side sideHitIn;
                if (relativePosToCourt.z > 0)
                {
                    sideHitIn = Side.Right;
                }
                else
                {
                    sideHitIn = Side.Left;
                }
                message = "IN";
                court.UpdateScore(CourtScript.OppositeSide(sideHitIn), message);
            }
        }
    }

    public void ReportNetServe(Side playerSide)
    {
        //if the rule that states that you cannot hit the net with the serve is enabled then update
        //the score and end the rally
        if (netServe)
        {
            message = "The serve hit the net";
            court.UpdateScore(CourtScript.OppositeSide(playerSide), message);
        }
    }

    public void ReportTouchNet(Side playerSide)
    {
        //if the rule that states that you cannot hit the ball is enabled then update the score and
        //end the rally
        if (touchNet)
        {
            message = "A player touched the net";
            court.UpdateScore(CourtScript.OppositeSide(playerSide), message);
        }
    }

    public void ReportMaxNumberHit(Side playerSide)
    {
        //if the rule that states that you cannot hit the ball is enabled then update the score and
        //end the rally
        if (maxHitPerSide)
        {
            message = "One side hit the ball more than " + maxNumberOfHits + " times";
            court.UpdateScore(CourtScript.OppositeSide(playerSide), message);
        }
    }

    public void ReportCrossedLine(Vector3 position, float radius, Side playerSide)
    {
        message = "The server crossed the line while serving";
        Vector3 relativePosToCourt = position - court.transform.position;
        relativePosToCourt.z -= radius/2 * (float)playerSide;
        if (relativePosToCourt.z < 9 * (float)(playerSide))
        {
            court.UpdateScore(CourtScript.OppositeSide(playerSide), message);
        }
    }
    #endregion

    #region UI Methods
    public void EnableDoubleTouch(bool value)
    {
        doubleTouch = value;
    }

    public void EnableGroundOut(bool value)
    {
        groundedOut = value;
    }

    public void EnableNetServe(bool value)
    {
        netServe = value;
    }

    public void EnableTouchNet(bool value)
    {
        touchNet = value;
    }

    public void EnableMaxHitPerSide(bool value)
    {
        maxHitPerSide = value;
    }
    #endregion
}
