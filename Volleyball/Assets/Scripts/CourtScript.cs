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
    [SerializeField]
    float currentTime;
    bool resetPositions;
    string LeftSideName = "LeftSide";
    string RightSideName = "RightSide";
    GameObject[] people;
    SpecialAction[] players;
    SpecialAction server;
    VolleyballScript ball;
    // Use this for initialization
    void Start ()
    {
        people = GameObject.FindGameObjectsWithTag("Player");
        currentTime = 0;
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<VolleyballScript>();
        players = new SpecialAction[people.Length];
        for(int i = 0; i < players.Length; i++)
        {
            players[i] = people[i].GetComponentInChildren<SpecialAction>();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(rallyOver)
        {
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

    public Vector3 GetPosition(int positionNumber, Side courtSide, SpecialAction currentPlayer)
    {
        string sideName = LeftSideName;
        if (courtSide == Side.Right)
            sideName = RightSideName;

        Vector3 position = Vector3.zero;
        if (positionNumber == 1 && serveSide == courtSide && serve)
        {
            //position.z += serveDistance * ((int)courtSide);
            //ball.transform.position = position - Vector3.forward * (int)courtSide * .75f + Vector3.up * 1.35f;
            position = transform.Find(sideName).Find("ServePosition").position;
            server = currentPlayer;
            server.EnableController();
            print(server.name);
        }
        else
        {
            position = transform.Find(sideName).Find(sideName + " (" + positionNumber + ")").position;
            
        }
        position += transform.position;
        position.y = 0;


        return position;
    }

    public void UpdateScore(Side wonVolley)
    {
        if (wonVolley == Side.Left)
            score.x++;
        else
            score.y++;
        EndRally();
        readyToServe = false;
    }

    void EndRally()
    {
        rallyOver = true;
        for (int i = 0; i < players.Length; i++)
        {
            players[i].EndRally();
        }
    }

    void BeginServe()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].enabled = true;
            players[i].resetPosition = true;
        }
        ball.transform.position = new Vector3(12, 0, 0);
        ball.Reset();
        readyToServe = true;
    }

    public Side OppositeSide(Side currentSide)
    {
        return (Side)((int)currentSide * -1);
    }

    public void GiveBallToServer()
    {
        rallyOver = false;
        ball.transform.position = server.transform.position + server.transform.parent.forward * .5f + Vector3.up * 1.35f;
        currentTime = 0;
    }
}
