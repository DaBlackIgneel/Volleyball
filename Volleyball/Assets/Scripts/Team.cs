using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public List<SpecialAction> players;
    public List<SpecialAction> blockers;
    public Dictionary<int, Dictionary<StrategyType, List<Strategy>>> currentStrategies;
    public StrategyType currentMode;
    public Dictionary<StrategyType, int> strategyIndex;
    public string name;
    public int numberOfActivePlayers { get { return players.Count; } }
    public Side side;
    public Texture logo;
    public Transform[] positionLocations;
    public CourtScript court;

    public Strategy CurrentStrategy
    { get { return currentStrategies[numberOfActivePlayers][currentMode][strategyIndex[currentMode]]; } }

    public Team()
    {
        players = new List<SpecialAction>();
        blockers = new List<SpecialAction>();
        currentStrategies = new Dictionary<int, Dictionary<StrategyType, List<Strategy>>>();
        strategyIndex = new Dictionary<StrategyType, int>();
        strategyIndex.Add(StrategyType.Offense, 0);
        strategyIndex.Add(StrategyType.Defense, 0);
        currentMode = StrategyType.Defense;
        side = Side.Right;
        name = "UnrivaledSuperHottie";
        positionLocations = new Transform[CourtScript.MaxNumberOfPlayers];
    }

    public Team(Side currentSide) : this()
    {
        side = currentSide;
    }

    public Team(string myName) : this()
    {
        name = myName;
    }

    public Team(params SpecialAction[] p) : this()
    {
        foreach (SpecialAction myP in p)
        {
            AddPlayer(myP);
        }
    }

    public Team(IEnumerable<SpecialAction> p) : this()
    {
        foreach (SpecialAction myP in p)
        {
            AddPlayer(myP);
        }
    }

    public bool AddPlayer()
    {
        if (numberOfActivePlayers < CourtScript.MaxNumberOfPlayers)
        {
            SpecialAction sp = ((GameObject)GameObject.Instantiate(Resources.Load("Player"))).GetComponentInChildren<SpecialAction>();
            sp.currentSide = side;
            sp.currentPosition = numberOfActivePlayers + 1;
            sp.isPlayer = false;
            sp.team = this;
            players.Add(sp);
            return true;
        }
        return false;
    }

    public bool AddPlayer(SpecialAction sp)
    {
        if (numberOfActivePlayers < CourtScript.MaxNumberOfPlayers)
        {
            sp.currentSide = side;
            sp.currentPosition = numberOfActivePlayers + 1;
            sp.team = this;
            players.Add(sp);
            return true;
        }
        return false;
    }

    public SpecialAction RemovePlayer()
    {
        SpecialAction removeP = null;
        for (int i = 0; i < numberOfActivePlayers; i++)
        {
            removeP = players.Find(x => x.currentPosition == numberOfActivePlayers - i);
            if (!removeP.isPlayer)
            {
                SpecialAction other = null;
                for (int b = i-1; b >= 0; b--)
                {
                    other = players.Find(x => x.currentPosition == numberOfActivePlayers - b);
                    other.currentPosition--;
                }
                if(!players.Remove(removeP))
                    MonoBehaviour.print("Didn't remove player for some reason");
                break;
            }
        }
        foreach (SpecialAction sp in players)
            MonoBehaviour.print(sp.currentPosition);
        return removeP.isPlayer ? null : removeP;
        
    }

    public void AddBlocker(SpecialAction p)
    {
        if (!blockers.Contains(p))
            blockers.Add(p);
    }

    public void RemoveBlocker(SpecialAction p)
    {
        if (blockers.Contains(p))
            blockers.Remove(p);
    }

    public void AddStrategy(Strategy s)
    {
        if (!currentStrategies.ContainsKey(s.numOfPlayers))
            currentStrategies.Add(s.numOfPlayers, new Dictionary<StrategyType, List<Strategy>>());
        if (!currentStrategies[s.numOfPlayers].ContainsKey(s.type))
            currentStrategies[s.numOfPlayers].Add(s.type, new List<Strategy>());
        currentStrategies[s.numOfPlayers][s.type].Add(s);
    }

    public void AddStrategy(Strategy[] st)
    {
        foreach (Strategy s in st)
        {
            AddStrategy(s);
        }
    }

    public Strategy GetStrategy(StrategyType st)
    {
        return currentStrategies[numberOfActivePlayers][st][0];
    }

    public Strategy GetStrategy(StrategyType st, int index)
    {
        return currentStrategies[numberOfActivePlayers][st][index];
    }

    public int NumberOfStrategies(StrategyType st)
    {
        return currentStrategies[numberOfActivePlayers][st].Count;
    }

    public void ResetTeamPositions(bool rotatePlayers)
    {
        if (rotatePlayers)
            RotatePositions();
        foreach (SpecialAction p in players)
        {
            p.ResetPosition();
        }
    }

    public void EndRally()
    {
        foreach (SpecialAction p in players)
        {
            p.EndRally();
        }
    }

    public void SwapPositions(int p1, int p2)
    {
        SpecialAction sp1 = players.Find(x => x.currentPosition == p1);
        SpecialAction sp2 = players.Find(x => x.currentPosition == p2);
        sp1.currentPosition = p2;
        sp2.currentPosition = p1;
    }

    public void SwitchSide()
    {
        SetSide(CourtScript.OppositeSide(side));
    }

    public void SetSide(Side mySide)
    {
        foreach (SpecialAction p in players)
        {
            p.currentSide = mySide;
        }
        side = mySide;
    }

    public void SubstitutePlayer(SpecialAction sub, int pos)
    {
        SpecialAction temp = players[pos - 1];
        sub = players[pos - 1];
        sub.currentPosition = temp.currentPosition;
        temp.myParent.parent.gameObject.SetActive(false);
        if (!sub.myParent.parent.gameObject.activeInHierarchy)
            sub.myParent.parent.gameObject.SetActive(true);
    }

    public Vector3 GetBlockSide(SpecialAction p, Vector3 goToLocation)
    {
        List<SpecialAction> sortedBlockers = blockers;
        sortedBlockers.Sort((x, y) => (goToLocation.x - x.transform.position.x).CompareTo(goToLocation.x - y.transform.position.x));
        int index = sortedBlockers.FindIndex(x => x == p);
        float offset;

        float playerSize = 1f;
        if (sortedBlockers.Count % 2 == 1)
            offset = ((float)index - Mathf.Floor(sortedBlockers.Count / 2f)) * playerSize;//.75 is the width of the player
        else
            offset = ((float)(index * 2f + 1) / (sortedBlockers.Count * 2f) - 1f / 2) * sortedBlockers.Count * playerSize;
        return Vector3.right * -offset;
    }

    void RotatePositions()
    {
        foreach (SpecialAction p in players)
        {
            p.currentPosition = (p.currentPosition + 1) % (numberOfActivePlayers);
            if (p.currentPosition == 0)
                p.currentPosition = numberOfActivePlayers;
        }
    }

    public void SetMode(StrategyType mode)
    {
        currentMode = mode;
        blockers.Clear();
    }

    public override string ToString()
    {
        System.Text.StringBuilder formattedString = new System.Text.StringBuilder();
        formattedString.Append("**********Team: " + name + "**********\n");
        formattedString.Append("Current Strategy: " + CurrentStrategy.name + "\n");
        formattedString.Append("Number of Players: " + numberOfActivePlayers + "\n");
        formattedString.Append("Side: " + side + "\n");
        formattedString.Append("Current Mode: " + currentMode + "\n");
        return formattedString.ToString();
    }

    public bool isBlocker(int position)
    {
        return Mathf.Abs(positionLocations[position - 1].position.z) < Mathf.Abs(court.dimensions.FrontLine(side).z);
    }
}