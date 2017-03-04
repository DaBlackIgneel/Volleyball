using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[System.Serializable]
public class ListListPass
{
    public int size;
    public int capacity;

    [SerializeField]
    ListPass[] lcontent;

    public ListListPass()
    {
        size = 0;
        capacity = 10;
        lcontent = new ListPass[capacity];
    }

    public ListListPass(int c)
    {
        size = 0;
        capacity = c;
        lcontent = new ListPass[capacity];
    }
    public void Add(ListPass item)
    {
        if (size < capacity)
        {
            lcontent[size] = item;
            size++;
        }
        else
            throw new System.OverflowException("ListListPass has reached full capacity");
    }

    public ListPass this[int i]
    {
        get
        { return lcontent[i]; }
        set
        { lcontent[i] = value; }
    }
}

[System.Serializable]
public class ListPass
{
    public int size;
    public int capacity;

    [SerializeField]
    Pass[] content;

    public ListPass()
    {
        size = 0;
        capacity = 10;
        content = new Pass[capacity];
    }

    public ListPass(int c)
    {
        size = 0;
        capacity = c;
        content = new Pass[capacity];
    }

    public void Add(Pass item)
    {
        if (size < capacity)
        {
            content[size] = item;
            size++;
        }
        else
            throw new System.OverflowException("ListPass has reached full capacity");
    }

    public void Remove(int index)
    {
        if (size != 0 && index < size)
        {
            for (int i = index; i < size; i++)
            {
                content[i] = content[i + 1];
            }
            size--;
        }
        else
        {
            throw new System.OverflowException("Cannot remove item at given index");
        }
    }

    public Pass this[int i]
    {
        get
        { return content[i]; }
        set
        { content[i] = value; }
    }

}

[System.Serializable]
public struct Pass
{
    public PassType type;
    public int position;
    public Vector3 location;

    public Pass(PassType t, int pos)
    {
        type = t;
        position = pos;
        location = Vector3.zero;
    }

    public Pass(PassType t, Vector3 loc)
    {
        type = t;
        location = loc;
        position = 0;
    }
}

public enum PassType { Person = 0, Location = 1 }
public enum StrategyType { Offense = 0, Defense = 1 }
[CreateAssetMenu(fileName = "New Strategy", menuName = "Strategy", order = 3)]
public class Strategy : ScriptableObject {

    public StrategyType type;
    public int numOfPlayers;

    


    [SerializeField]
    private Vector3[] defaultPositions;

    public Vector3 defensePassToLocation;
    public ListListPass myPass;

	// Use this for initialization
	void Start () {
	}
	

	// Update is called once per frame
	void Update () {
		
	}

    public Vector3 DefaultPositions(int position)
    {
        return ScaleForNormalCourt(defaultPositions[position]);
    }

    public Vector3 GetPassLocation(int iteration, SpecialAction player)
    {
        if (type == StrategyType.Defense)
            return ScaleForNormalCourt(defensePassToLocation);
        else
        {
            
            return CalcPassLocation(iteration, player);
        }
    }

    Vector3 CalcPassLocation(int iteration, SpecialAction player, int i = 0)
    {
        if(myPass[iteration][i].type == PassType.Person)
        {
            int position = myPass[iteration][i].position <= numOfPlayers ? myPass[iteration][i].position : numOfPlayers;
            SpecialAction tempPlayer = player.Court.Players[player.currentSide].Find(x => x.currentPosition == position);
            if(!player.Court.LocalRelate.IsValidPlayer(tempPlayer))
            {
                return CalcPassLocation(iteration, player, i++);
            }
            else
                return tempPlayer.transform.position + Vector3.up * 2;
        }
        else
        {
            return myPass[iteration][0].location;
        }
    }

    public Vector3 ScaleForNormalCourt(Vector3 input)
    {
        return Vector3.Scale(input, (-Vector3.right + Vector3.forward) * 9 + Vector3.up);
    }
}
