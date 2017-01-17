using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassDownEvents : MonoBehaviour {

    SpecialAction myPass;

	// Use this for initialization
	void Start () {
        myPass = GetComponentInChildren<SpecialAction>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball")
            myPass.MyTriggerEnter(other);
    }
}
