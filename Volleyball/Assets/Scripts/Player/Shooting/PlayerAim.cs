using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour, IInitializable
{

    public Vector3 aimSpot;
    [SerializeField]
    Vector3 aimDir;
    Vector3 originAimDir;
    SpecialAction player;

    public void Initialize(SpecialAction player)
    {
        this.player = player;
    }

    public Vector3 UserAim()
    {
        //make sure that the mouse is visible
        Cursor.visible = true;

        //have the directon be where the mouse is currently pointing to
        Ray mousePoint = Camera.main.ScreenPointToRay(Input.mousePosition);

        //normalize the current direction so that the magnitude == 1
        aimDir = mousePoint.direction;
        aimDir = aimDir.normalized;
        
        return aimDir;
    }
}
