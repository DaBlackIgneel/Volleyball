using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveCollision : MonoBehaviour {

    public SpecialAction user;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        user.OnMyTriggerEnter(other);
    }
}
