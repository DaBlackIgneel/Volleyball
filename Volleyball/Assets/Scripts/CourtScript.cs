using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtScript : MonoBehaviour {
    string LeftSideName = "LeftSide";
    string RightSideName = "RightSide";
    public bool serve;
    public float serveDistance = 3;
	// Use this for initialization
	void Start ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public Vector3 GetPosition(int positionNumber, Side courtSide)
    {
        string sideName = LeftSideName;
        if (courtSide == Side.Right)
            sideName = RightSideName;
        Vector3 position = transform.Find(sideName).Find(sideName + " (" + positionNumber + ")").position;
        if (positionNumber == 1 && serve)
            position.z += serveDistance * ((int)courtSide);
        position += transform.position;
        position.y = 0;
        print(position);
        return position;
    }
}
