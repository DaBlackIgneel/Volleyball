using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtScript : MonoBehaviour {
    
    public bool serve;
    public Side serveSide;

    public Vector2 score;

    public bool rallyOver;
    public float waitTime = 5;
    public bool readyToServe;
    public Rules CourtRules { get { return courtRules; } }
    public bool onlyServes;
    public bool onlyReceives;

    float currentTime;
    float normalServeWaitTime = 5;
    float quickServeWaitTime = 1;
    bool resetPositions;
    string LeftSideName = "LeftSide";
    string RightSideName = "RightSide";

    float displayTimer = 1;
    float displayTimerStep = .01f;

    Rules courtRules;
    GameObject[] people;
    SpecialAction[] players;
    VolleyballScript ball;
    SpecialAction server;
    UnityEngine.UI.Text displayText;
    [System.NonSerialized]
    public GameObject net;
    // Use this for initialization
    void Start ()
    {
        //find all players in the game
        people = GameObject.FindGameObjectsWithTag("Player");

        net = GameObject.FindGameObjectWithTag("Net");

        displayText = GameObject.FindGameObjectWithTag("DisplayText").GetComponent<UnityEngine.UI.Text>();

        //Gets the rules of the game
        courtRules = GetComponent<Rules>();

        //used for a pause in the game 
        //example: after a rally is over the is a small wait time of a 
        //few seconds then someone begins the serve
        currentTime = 0;

        //find the only ball in the game
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();

        //get access to the special actions of the players
        players = new SpecialAction[people.Length];
        for(int i = 0; i < players.Length; i++)
        {
            players[i] = people[i].GetComponentInChildren<SpecialAction>();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        //when the rally is over wait a few seconds to start the serve
		if(rallyOver)
        {
            //only transport the ball to the server in 1 instance
            if (currentTime < waitTime + 100)
            {
                if (currentTime >= waitTime)
                {
                    serve = true;
                    BeginServe();
                    currentTime = waitTime + 101;
                }
                currentTime += Time.deltaTime;
            }
        }
	}

    void FixedUpdate()
    {

        FadeDisplay();

    }

    void FadeDisplay()
    {
        if (displayTimer > Mathf.Epsilon)
        {
            Color temp = displayText.color;
            temp.a = displayTimer;
            displayText.color = temp;
            displayTimer -= displayTimerStep;
        }
    }

    //get the starting position of each player
    public Vector3 GetPosition(int positionNumber, Side courtSide, SpecialAction currentPlayer)
    {
        //get the string version of the current player
        string sideName = LeftSideName;
        if (courtSide == Side.Right)
            sideName = RightSideName;

        //initialize the position variable
        Vector3 position = Vector3.zero;

        //if the current player is up to serve than transport him to the serving position
        if (positionNumber == 1 && serveSide == courtSide && serve)
        {
            position = transform.Find(sideName).Find("ServePosition").position;
            server = currentPlayer;
            //server.SetMovementEnabled(true);
            
        }
        //if the current player is not serving than transport them to their corresponding positions
        else
        {
            position = transform.Find(sideName).Find(sideName + " (" + positionNumber + ")").position;
            
        }
        if (currentPlayer.isPlayer)
        {
            currentPlayer.SetMovementEnabled(true);
        }
        position += transform.position;
        position.y = 0;

        return position;
    }

    //increments the score and starts the sequence to begin the next serve
    public void UpdateScore(Side wonVolley)
    {
        //increment the score for the corresponding side
        if (wonVolley == Side.Left)
            score.x++;
        else
            score.y++;

        //Makes sure to stop the game
        EndRally();

        //declare that the your not ready to serve
        readyToServe = false;
    }

    public void UpdateScore(Side wonVolley, string reason)
    {
        if (!rallyOver)
        {
            Display(reason);
            UpdateScore(wonVolley);
        }
    }

    //stops each player from continuing the game
    void EndRally()
    {
        //declare that the rally is over
        rallyOver = true;

        //goes through each player and stops their movement and their special actions
        for (int i = 0; i < players.Length; i++)
        {
            players[i].EndRally();
        }
    }


    //sets up the court for the serve
    void BeginServe()
    {
        //go through each player and allow special actions and move them to their starting positions
        for (int i = 0; i < players.Length; i++)
        {
            players[i].enabled = true;
            players[i].resetPosition = true;
        }

        //move the ball off to the side
        ball.transform.position = new Vector3(12, 0, 0);
        ball.Reset();

        //declare that your ready to serve
        readyToServe = true;
    }

    //given a side return the opposite side
    public Side OppositeSide(Side currentSide)
    {
        return (Side)((int)currentSide * -1);
    }

    //transport the ball to the server
    public void GiveBallToServer()
    {
        //declare that a new rally has begun
        rallyOver = false;

        //move ball to the server
        ball.transform.position = server.transform.position + server.transform.parent.forward * .5f + Vector3.up * 1f;
        currentTime = 0;
    }

    public void Display(string message)
    {
        displayText.text = message;
        displayTimer = 1;
    }

    public void SetOnlyServes(bool value)
    {
        onlyServes = value;
        if (value)
        {
            waitTime = quickServeWaitTime;
            courtRules.EnableGroundOut(true);
            serveSide = Side.Left;
        }
            

    }
    public void SetOnlyReceives(bool value)
    {
        onlyReceives = value;
        if (value)
        {
            waitTime = quickServeWaitTime;
            courtRules.EnableGroundOut(true);
            serveSide = Side.Right;
        }
    }
}
