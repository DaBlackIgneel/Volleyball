using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetScrpt : MonoBehaviour {
    CourtScript court;
    MeshFilter myRenderer;
	// Use this for initialization
	void Start () {
        court = GameObject.FindGameObjectWithTag("CourtController").GetComponent<CourtScript>();
        myRenderer = GetComponent<MeshFilter>();
        for(int i = 0; i < myRenderer.mesh.uv.Length; i++)
        {
            
            print(myRenderer.mesh.uv[i] + "," + myRenderer.mesh.vertices[i]);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            court.CourtRules.ReportTouchNet(CourtScript.FindPlayerFromCollision(other.transform).currentSide);
        }
    }
}
