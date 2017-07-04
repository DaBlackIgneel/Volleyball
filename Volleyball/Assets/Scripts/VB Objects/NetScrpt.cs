using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetScrpt : MonoBehaviour {
    CourtScript court;
    MeshFilter myRenderer;
    Mesh myMesh;
	// Use this for initialization
	void Start () {
        court = GameObject.FindGameObjectWithTag("CourtController").GetComponent<CourtScript>();
        myRenderer = GetComponent<MeshFilter>();
        myMesh = new Mesh();
        GenerateMesh(ref myMesh);
        myMesh.name = "SameSidedSquare";
        myRenderer.mesh = myMesh;
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

    void GenerateMesh(ref Mesh currentMesh)
    {
        Vector3[] verticies = new Vector3[24];
        Vector2[] uvs = new Vector2[verticies.Length];
        int[] triangles = new int[verticies.Length/2 * 3];


        //top square
        verticies[0] = new Vector3(0.5f, 0.5f, 0.5f);
        verticies[1] = new Vector3(0.5f, 0.5f, -0.5f);
        verticies[2] = new Vector3(-0.5f, 0.5f, -0.5f);
        verticies[3] = new Vector3(-0.5f, 0.5f, 0.5f);

        uvs[0] = new Vector2(1, 1);
        uvs[1] = new Vector2(0, 1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        //Bottom square
        verticies[4] = new Vector3(0.5f, 0.5f, 0.5f) * -1;
        verticies[5] = new Vector3(0.5f, 0.5f, -0.5f) * -1;
        verticies[6] = new Vector3(-0.5f, 0.5f, -0.5f) * -1;
        verticies[7] = new Vector3(-0.5f, 0.5f, 0.5f) * -1;

        uvs[4] = new Vector2(0, 0);
        uvs[5] = new Vector2(1, 0);
        uvs[6] = new Vector2(1, 1);
        uvs[7] = new Vector2(0, 1);

        triangles[6] = 4;
        triangles[7] = 7;
        triangles[8] = 6;

        triangles[9] = 4;
        triangles[10] = 6;
        triangles[11] = 5;

        //right square
        verticies[8] = new Vector3(0.5f, 0.5f, 0.5f);
        verticies[9] = new Vector3(0.5f, -0.5f, 0.5f);
        verticies[10] = new Vector3(0.5f, -0.5f, -0.5f);
        verticies[11] = new Vector3(0.5f, 0.5f, -0.5f);

        uvs[8] = new Vector2(1, 1);
        uvs[9] = new Vector2(0, 1);
        uvs[10] = new Vector2(0, 0);
        uvs[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 8;
        triangles[16] = 10;
        triangles[17] = 11;

        //left square
        verticies[12] = new Vector3(0.5f, 0.5f, 0.5f) * -1;
        verticies[13] = new Vector3(0.5f, -0.5f, 0.5f) * -1;
        verticies[14] = new Vector3(0.5f, -0.5f, -0.5f) * -1;
        verticies[15] = new Vector3(0.5f, 0.5f, -0.5f) * -1;

        uvs[12] = new Vector2(0, 0);
        uvs[13] = new Vector2(1, 0);
        uvs[14] = new Vector2(1, 1);
        uvs[15] = new Vector2(0, 1);

        triangles[18] = 12;
        triangles[19] = 15;
        triangles[20] = 14;

        triangles[21] = 12;
        triangles[22] = 14;
        triangles[23] = 13;

        //front square
        verticies[16] = new Vector3(0.5f, 0.5f, 0.5f);
        verticies[17] = new Vector3(-0.5f, 0.5f, 0.5f);
        verticies[18] = new Vector3(-0.5f, -0.5f, 0.5f);
        verticies[19] = new Vector3(0.5f, -0.5f, 0.5f);

        uvs[16] = new Vector2(1, 1);
        uvs[17] = new Vector2(0, 1);
        uvs[18] = new Vector2(0, 0);
        uvs[19] = new Vector2(1, 0);

        triangles[24] = 16;
        triangles[25] = 17;
        triangles[26] = 18;

        triangles[27] = 16;
        triangles[28] = 18;
        triangles[29] = 19;

        //back square
        verticies[20] = new Vector3(0.5f, 0.5f, 0.5f) * -1;
        verticies[21] = new Vector3(-0.5f, 0.5f, 0.5f) * -1;
        verticies[22] = new Vector3(-0.5f, -0.5f, 0.5f) * -1;
        verticies[23] = new Vector3(0.5f, -0.5f, 0.5f) * -1;

        uvs[20] = new Vector2(0, 0);
        uvs[21] = new Vector2(1, 0);
        uvs[22] = new Vector2(1, 1);
        uvs[23] = new Vector2(0, 1);

        triangles[30] = 20;
        triangles[31] = 23;
        triangles[32] = 22;

        triangles[33] = 20;
        triangles[34] = 22;
        triangles[35] = 21;

        currentMesh.vertices = verticies;
        currentMesh.uv = uvs;
        currentMesh.triangles = triangles; 
    }


    void Swap(ref Vector3 item)
    {
        float temp = item.x;
        item.x = item.y;
        item.y = item.z;
        item.z = temp;
    }
}
