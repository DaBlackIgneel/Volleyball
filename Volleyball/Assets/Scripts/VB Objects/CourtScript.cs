using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CourtDimensions
{
    public float length;
    public float width;

    public CourtDimensions(float l, float w)
    {
        length = l;
        width = w;
    }

    public struct Rectangle
    {
        public Vector3 upperRightCorner;
        public Vector3 lowerLeftCorner;

        public Rectangle(Vector3 c1, Vector3 c2)
        {
            upperRightCorner = c1;
            lowerLeftCorner = c2;
        }

        public Vector3 Dimension()
        {
            return upperRightCorner - lowerLeftCorner;
        }

        public Vector3 Center()
        {
            return upperRightCorner - Dimension() * .5f;
        }
    }

    public enum Section {FrontLeft = 0, FrontMiddle = 1, FrontRight = 2, BackRight = 3, BackMiddle = 4, BackLeft = 5 }

    public Rectangle FrontLineRect(Side side)
    {
        return new Rectangle(new Vector3(width / 2 * (int)side, 0, length / 6.0f * (int)side), new Vector3(-width / 2 * (int)side, 0, 0));
    }

    public Rectangle BackLineRect(Side side)
    {
        return new Rectangle(new Vector3(width / 2 * (int)side, 0, length / 2.0f * (int)side), new Vector3(-width / 2 * (int)side, 0, length / 6.0f * (int)side));
    }

    public Vector3 FrontLine(Side side)
    {
        return new Vector3(width, 0, length / 6.0f * (int)side);
    }

    public Vector3 BackLine(Side side)
    {
        return new Vector3(width, 0, length / 2.0f * (int)side);
    }

    public Rectangle CourtSection(Side side, Section section)
    {
        int mySection = (int)section > 2 ? (5 - (int)section) : (int)section;
        Vector2 myLength = (int)section > 2 ? new Vector2(length / 2 , length / 6) : new Vector2(length / 6, 0);
        return new Rectangle(new Vector3((-width / 2 + width / 3 * (mySection + 1)) * (int)side, 0, myLength.x * (int)side),
                            new Vector3((-width / 2 + width / 3 * (mySection)) * (int)side, 0, myLength.y * (int)side));

    }
}

public class CourtScript : MonoBehaviour {
    
    public bool serve;
    public Side serveSide;
    public List<Side> sideInPlay;

    public Vector2 score;

    public bool rallyOver;
    public float waitTime = 5;
    public bool readyToServe;
    public Rules CourtRules { get { return courtRules; } }
    public bool onlyServes;
    public bool onlyReceives;
    public bool offline;
    public CourtDimensions dimensions;
    public Dictionary<Side,Dictionary<StrategyType,Strategy>> currentStrategy;

    public Dictionary<Side,List<SpecialAction>> Players
    {
        get { return players; }
    }
    public Dictionary<Side, StrategyType> Mode
    {
        get { return mode; }
    }

    public Strategy[] defaultDefenseStrategies;
    public Strategy[] defaultOffenseStrategies;

    public LocationRelation LocalRelate
    {
        get { return localRelate; }
    }

    public static int MaxNumberOfPlayers
    {
        get { return 6; }
    }

    public float NetHeight
    {
        get { return net.transform.position.y + net.transform.localScale.y/2; }
    }


    LocationRelation localRelate;
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
    Dictionary<Side,List<SpecialAction>> players;
    Dictionary<Side, StrategyType> mode;
    VolleyballScript ball;
    SpecialAction server;
    public Team LeftTeam;
    public Team RightTeam;

    public Dictionary<Side, Team> teams;

    UnityEngine.UI.Text[] scoreDisplay;
    Vector2 scoreIndex;
    UnityEngine.UI.Text displayText;
    [System.NonSerialized]
    public GameObject net;
    // Use this for initialization
    void Start()
    {
        dimensions = new CourtDimensions(18, 9);

        //find all players in the game
        people = GameObject.FindGameObjectsWithTag("Player");

        localRelate = GetComponent<LocationRelation>();

        net = GameObject.FindGameObjectWithTag("Net");

        displayText = GameObject.FindGameObjectWithTag("DisplayText").GetComponent<UnityEngine.UI.Text>();

        GameObject[] scoreObj = GameObject.FindGameObjectsWithTag("Score");
        scoreDisplay = new UnityEngine.UI.Text[scoreObj.Length];
        for (int i = 0; i < scoreObj.Length; i++)
        {
            scoreDisplay[i] = scoreObj[i].GetComponent<UnityEngine.UI.Text>();
        }

        if (scoreDisplay.Length > 1)
        {
            if (scoreDisplay[0].name.IndexOf("Left") > 0)
            {
                scoreIndex.x = 0;
                scoreIndex.y = 1;
            }
            else
            {
                scoreIndex.x = 1;
                scoreIndex.y = 0;
            }
        }

        //Gets the rules of the game
        courtRules = GetComponent<Rules>();

        //used for a pause in the game 
        //example: after a rally is over the is a small wait time of a 
        //few seconds then someone begins the serve
        currentTime = 0;

        //find the only ball in the game
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();

        //get access to the special actions of the players
        players = new Dictionary<Side, List<SpecialAction>>();
        teams = new Dictionary<Side, Team>();
        for (int i = 0; i < people.Length; i++)
        {
            SpecialAction tempPerson = people[i].GetComponentInChildren<SpecialAction>();
            if (!players.ContainsKey(tempPerson.currentSide))
            {
                teams.Add(tempPerson.currentSide, new Team(tempPerson.currentSide));
                players.Add(tempPerson.currentSide, new List<SpecialAction>());
                sideInPlay.Add(tempPerson.currentSide);
            }
            players[tempPerson.currentSide].Add(tempPerson);
            teams[tempPerson.currentSide].AddPlayer(tempPerson);
        }
        /*LeftTeam = new Team(players[Side.Left]);
        LeftTeam.SetSide(Side.Left);
        RightTeam = new Team(players[Side.Right]);
        RightTeam.SetSide(Side.Right);
        LeftTeam.SwapPositions(1, LeftTeam.numberOfActivePlayers);*/
        currentStrategy = new Dictionary<Side, Dictionary<StrategyType, Strategy>>();
        mode = new Dictionary<Side, StrategyType>();
        foreach (Side tempSide in sideInPlay)
        {
            //Side tempSide = (Side)i;
            currentStrategy.Add(tempSide, new Dictionary<StrategyType, Strategy>());
            currentStrategy[tempSide].Add(StrategyType.Defense, defaultDefenseStrategies[players[tempSide].Count - 1]);
            currentStrategy[tempSide].Add(StrategyType.Offense, defaultOffenseStrategies[players[tempSide].Count - 1]);
            teams[tempSide].AddStrategy(defaultDefenseStrategies);
            teams[tempSide].AddStrategy(defaultOffenseStrategies);
            mode.Add(tempSide, StrategyType.Defense);
            /*if (i == -1)
            {
                LeftTeam.AddStrategy(defaultDefenseStrategies);
                LeftTeam.AddStrategy(defaultOffenseStrategies);
                RightTeam.AddStrategy(defaultDefenseStrategies);
                RightTeam.AddStrategy(defaultOffenseStrategies);
            }*/
        }

        /*
        mode.Add(Side.Left, StrategyType.Defense);
        mode.Add(Side.Right, StrategyType.Defense);
        */
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateScore();
        //when the rally is over wait a few seconds to start the serve
		if(rallyOver)
        {
            //only transport the ball to the server in 1 instance
            if (currentTime < waitTime + 100)
            {
                if (currentTime >= waitTime)
                {
                    serve = true;
                    currentTime = waitTime + 101;
                    BeginServe();
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

    void UpdateScore()
    {
        scoreDisplay[(int)scoreIndex.x].text = ((int)score.x).ToString();
        scoreDisplay[(int)scoreIndex.y].text = ((int)score.y).ToString();
    }

    void UpdateStrategyPositions()
    {
        try
        {
            foreach (Side tempSide in sideInPlay)
            {
                for (int p = 0; p < teams[tempSide].numberOfActivePlayers; p++)
                {
                    GetCourtPosition(p + 1, teams[tempSide].side).position = Vector3.Scale(teams[tempSide].CurrentStrategy.DefaultPositions(p), new Vector3((int)(teams[tempSide].side), 1, (int)(teams[tempSide].side)));
                }
            }
            /*
            for (int p = 0; p < RightTeam.numberOfActivePlayers; p++)
            {
                GetCourtPosition(p + 1, RightTeam.side).position = Vector3.Scale(RightTeam.CurrentStrategy.DefaultPositions(p), new Vector3((int)(RightTeam.side), 1, (int)(RightTeam.side)));
            }
            for (int p = 0; p < LeftTeam.numberOfActivePlayers; p++)
            {
                GetCourtPosition(p + 1, LeftTeam.side).position = Vector3.Scale(LeftTeam.CurrentStrategy.DefaultPositions(p), new Vector3((int)(LeftTeam.side), 1, (int)(LeftTeam.side)));
            }*/
        }
        catch (System.Exception e)
        {
            print(e.Message);
        }
        
    }

    public Transform GetCourtPosition(int positionNumber, Side courtSide)
    {
        string sideName = courtSide == Side.Right? RightSideName : LeftSideName;
        return transform.Find(sideName).Find(sideName + " (" + positionNumber + ")");
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
            position = GetCourtPosition(positionNumber,courtSide).position;
            
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
        foreach(Side side in sideInPlay)
        {
            teams[side].EndRally();
        }
        /*LeftTeam.EndRally();
        RightTeam.EndRally();*/
    }


    //sets up the court for the serve
    void BeginServe()
    {
        //go through each player and allow special actions and move them to their starting positions
        foreach(Side side in sideInPlay)
        {
            teams[side].ResetTeamPositions(false);
        }
        /*LeftTeam.ResetTeamPositions(false);
        RightTeam.ResetTeamPositions(false);*/

        //move the ball off to the side
        ball.transform.position = new Vector3(12, 0, 0);
        ball.Reset();

        //declare that your ready to serve
        readyToServe = true;

        

        if(onlyReceives)
        {
            SetAllDefense();
            serve = false;
            readyToServe = false;
            rallyOver = false;
            currentTime = 0;
            ball.transform.position = localRelate.RandomSectionLocation(serveSide, CourtDimensions.Section.BackMiddle) + Vector3.up * 20;//localRelate.AimSpot(serveSide) + Vector3.up * 20;
        }
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

    public void SetMode(Side side)
    {
        foreach(Side tempSide in sideInPlay)
        {
            if(tempSide == side)
            {
                mode[tempSide] = StrategyType.Offense;
                teams[tempSide].SetMode(StrategyType.Offense);
            } 
            else
            {
                mode[tempSide] = StrategyType.Defense;
                teams[tempSide].SetMode(StrategyType.Defense);
            }
        }
        
        /*mode[side] = StrategyType.Offense;
        mode[OppositeSide(side)] = StrategyType.Defense;
        if (LeftTeam.side == side)
            LeftTeam.SetMode(StrategyType.Offense);
        else
            LeftTeam.SetMode(StrategyType.Defense);
        if (RightTeam.side == side)
            RightTeam.SetMode(StrategyType.Offense);
        else
            RightTeam.SetMode(StrategyType.Defense);
            */
        UpdateStrategyPositions();
    }

    public void SetAllDefense()
    {
        foreach(Side side in sideInPlay)
        {
            mode[side] = StrategyType.Defense;
            teams[side].SetMode(StrategyType.Defense);
        }
        /*mode[Side.Left] = StrategyType.Defense;
        mode[Side.Right] = StrategyType.Defense;
        LeftTeam.SetMode(StrategyType.Defense);
        RightTeam.SetMode(StrategyType.Defense);*/
        UpdateStrategyPositions();
    }

    //given a side return the opposite side
    public static Side OppositeSide(Side currentSide)
    {
        return (Side)((int)currentSide * -1);
    }

    public static Transform GetHighestParent(Transform current)
    {
        if (current.parent == null)
            return current;
        else
            return GetHighestParent(current.parent);
    }

    public static SpecialAction FindPlayerFromCollision(Transform current)
    {
        return GetHighestParent(current).Find("PlayerPivot/PlayerBody").GetComponent<SpecialAction>();
    }

    public static Side FloatToSide(float ff)
    {
        int iSide = (int)Mathf.Sign(ff);
        return (Side)iSide;
    }


}
