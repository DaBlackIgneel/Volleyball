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
        return new Rectangle(new Vector3(width / 2 * (float)side, 0, length / 6.0f * (float)side), new Vector3(-width / 2 * (float)side, 0, 0));
    }

    public Rectangle BackLineRect(Side side)
    {
        return new Rectangle(new Vector3(width / 2 * (float)side, 0, length / 2.0f * (float)side), new Vector3(-width / 2 * (float)side, 0, length / 6.0f * (float)side));
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

    public GameObject playerPrefab;
    LocationRelation localRelate;
    float currentTime;
    float normalServeWaitTime = 2;
    float quickServeWaitTime = 1;
    bool dontRotate;
    bool rotatePositions;
    string LeftSideName = "LeftSide";
    string RightSideName = "RightSide";

    float displayTimer = 1;
    float displayTimerStep = .5f;

    Rules courtRules;
    
    Dictionary<Side,List<SpecialAction>> players;
    Dictionary<Side, StrategyType> mode;
    VolleyballScript ball;
    SpecialAction server;

    public Dictionary<Side, Team> teams;
    public List<Vector3> scoredPoints;

    UnityEngine.UI.Text[] scoreDisplay;
    Vector2 scoreIndex;
    UnityEngine.UI.Text displayText;
    public UnityEngine.UI.Dropdown teamNameDropDown;
    [System.NonSerialized]
    public GameObject net;
    // Use this for initialization
    void Start()
    {
        dimensions = new CourtDimensions(18, 9);

        scoredPoints = new List<Vector3>();

        //find all players in the game
        GameObject[] people = GameObject.FindGameObjectsWithTag("Player");

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
                teams[tempPerson.currentSide].name = tempPerson.currentSide.ToString() + "_Team";
            }
            players[tempPerson.currentSide].Add(tempPerson);
            teams[tempPerson.currentSide].AddPlayer(tempPerson);
        }

        currentStrategy = new Dictionary<Side, Dictionary<StrategyType, Strategy>>();
        mode = new Dictionary<Side, StrategyType>();
        foreach (Side tempSide in sideInPlay)
        {
            currentStrategy.Add(tempSide, new Dictionary<StrategyType, Strategy>());
            currentStrategy[tempSide].Add(StrategyType.Defense, defaultDefenseStrategies[players[tempSide].Count - 1]);
            currentStrategy[tempSide].Add(StrategyType.Offense, defaultOffenseStrategies[players[tempSide].Count - 1]);
            teams[tempSide].AddStrategy(defaultDefenseStrategies);
            teams[tempSide].AddStrategy(defaultOffenseStrategies);
            teams[tempSide].court = this;
            mode.Add(tempSide, StrategyType.Defense);
            for(int i = 0; i < MaxNumberOfPlayers; i++)
            {
                teams[tempSide].positionLocations[i] = GetCourtPosition(i + 1, tempSide);
            }
        }
        PopulateTeamDropDown();
        //UpdateStrategyPositions();
    }
	
    void PopulateTeamDropDown()
    {
        List<string> teamNames = new List<string>();
        foreach(Side t in teams.Keys)
        {
            teamNames.Add(teams[t].name);
        }
        teamNameDropDown.ClearOptions();
        teamNameDropDown.AddOptions(teamNames);
    }

	// Update is called once per frame
	void Update ()
    {
        UpdateScore();
        FadeDisplay();
        //when the rally is over wait a few seconds to start the serve
        if (rallyOver)
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
        UpdateDefenseStrategyPositions();
    }

    void FadeDisplay()
    {
        if (displayTimer > Mathf.Epsilon)
        {
            Color temp = displayText.color;
            temp.a = displayTimer;
            displayText.color = temp;
            displayTimer -= displayTimerStep * Time.deltaTime;
        }
    }

    void UpdateScore()
    {
        scoreDisplay[(int)scoreIndex.x].text = ((int)score.x).ToString();
        scoreDisplay[(int)scoreIndex.y].text = ((int)score.y).ToString();
    }

    void UpdateDefenseStrategyPositions()
    {
        foreach (Side tempSide in sideInPlay)
        {
            if(teams[tempSide].currentMode == StrategyType.Defense && FloatToSide(Mathf.Sign(ball.transform.position.z)) == OppositeSide(tempSide))
            {
                float size = 9;
                float ballx = Mathf.Abs(ball.transform.position.x);
                Vector3 position = Vector3.zero;
                Vector3 difference = Vector3.zero;
                float zNum = Mathf.Abs(ball.transform.position.z) - 3;
                if (zNum < 0)
                    zNum = 0;
                if(ballx > size / 2)
                {
                    ballx = size / 2;
                }
                float positionRation = (ballx / (size / 2)) * ((size - zNum)/size) ;
                BallSimulation ballSide = (BallSimulation)(Mathf.Sign(ball.transform.position.x) * (float)OppositeSide(tempSide));
                for (int p = 0; p < teams[tempSide].numberOfActivePlayers; p++)
                {
                    if (!serve && !rallyOver && !readyToServe)
                    {
                        difference = positionRation * (teams[tempSide].CurrentStrategy.DefensePositions(ballSide, p) - teams[tempSide].CurrentStrategy.DefensePositions(BallSimulation.Center, p));
                        position = difference + teams[tempSide].CurrentStrategy.DefensePositions(BallSimulation.Center, p);
                    }
                    else
                    {
                        position = teams[tempSide].CurrentStrategy.DefensePositions(BallSimulation.Center, p);
                    }
                    position = Vector3.Scale(position, new Vector3((int)(teams[tempSide].side)*-1, 1, (int)(teams[tempSide].side)));
                    GetCourtPosition(p + 1, teams[tempSide].side).position = position;
                }
            }
        }
    }

    void UpdateOffenseStrategyPositions()
    {
        foreach (Side tempSide in sideInPlay)
        {
            if(teams[tempSide].currentMode == StrategyType.Offense)
            {
                for (int p = 0; p < teams[tempSide].numberOfActivePlayers; p++)
                {
                    GetCourtPosition(p + 1, teams[tempSide].side).position = Vector3.Scale(teams[tempSide].CurrentStrategy.DefaultPositions(p), new Vector3((int)(teams[tempSide].side), 1, (int)(teams[tempSide].side)));
                }
            }
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

        rotatePositions = wonVolley != serveSide && !dontRotate;
        if (!onlyReceives && !onlyReceives)
        {
            serveSide = wonVolley;
            waitTime = normalServeWaitTime;
        }
        else
        {
            waitTime = quickServeWaitTime;
        }
            
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
    }

    //sets up the court for the serve
    void BeginServe()
    {
        //declare that your ready to serve
        readyToServe = true;

        SetAllDefense();
        UpdateDefenseStrategyPositions();
        
        //go through each player and allow special actions and move them to their starting positions
        foreach (Side side in sideInPlay)
        {
            teams[side].ResetTeamPositions(rotatePositions && serveSide == side);
        }

        //move the ball off to the side
        ball.transform.position = new Vector3(12, 0, 0);
        ball.Reset();
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
            courtRules.EnableGroundOut(true);
            serveSide = Side.Left;
        }
            

    }
    public void SetOnlyReceives(bool value)
    {
        onlyReceives = value;
        if (value)
        {
            courtRules.EnableGroundOut(true);
            serveSide = Side.Right;
        }
    }
    public void SetDontRotate(bool value)
    {
        dontRotate = value;
    }
    public void FromMenuAddPlayer()
    {
        foreach (Side t in teams.Keys)
        {
            if(teams[t].name == teamNameDropDown.options[teamNameDropDown.value].text)
            {
                AddPlayer(t);
                break;
            }
        }
    }
    public void FromMenuRemovePlayer()
    {
        foreach (Side t in teams.Keys)
        {
            if (teams[t].name == teamNameDropDown.options[teamNameDropDown.value].text)
            {
                RemovePlayer(t);
                break;
            }
        }
    }

    public void AddPlayer(Side tempSide)
    {
        SpecialAction tempPlayer = Instantiate<GameObject>(playerPrefab).GetComponentInChildren<SpecialAction>();
        if (teams[tempSide].AddPlayer(tempPlayer))
        {
            players[tempSide].Add(tempPlayer);
            
            tempPlayer.SetPlayer(false);
            tempPlayer.myParent.parent.position = dimensions.length / 4 * Vector3.forward * (float)tempSide + transform.position + (tempPlayer.myParent.parent.position.y + 2) * Vector3.up;
        }
        else
        {
            print("Can't add cause too many players");
            Destroy(tempPlayer.myParent.parent);
        }
        
    }

    public void RemovePlayer(Side tempSide)
    {
        SpecialAction rp = teams[tempSide].RemovePlayer();
        if (rp != null)
        {
            print("removed Player");
            players[tempSide].Remove(rp);
            Destroy(rp.myParent.parent.gameObject);
        }
        else
            print("Couldn't remove player");
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
        UpdateOffenseStrategyPositions();
    }

    public void SetAllDefense()
    {
        foreach(Side side in sideInPlay)
        {
            mode[side] = StrategyType.Defense;
            teams[side].SetMode(StrategyType.Defense);
        }
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
