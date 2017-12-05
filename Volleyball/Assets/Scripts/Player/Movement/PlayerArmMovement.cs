using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArmMovement : MonoBehaviour, IInitializable
{
    public bool block;
    bool blockOver;

    public float armSpreadSensitivity = .5f;
    public Vector2 armMovementSensitivity;
    float downArm = 0;
    public float blockArmRotation;

    public Vector2 armWaveRotation;
    Transform arms;
    MyMouseLook rightArm;
    MyMouseLook leftArm;
    SpecialAction player;
    
    // Use this for initialization
    void Start () {
        
    }

    public void Initialize(SpecialAction player)
    {
        this.player = player;
        armWaveRotation = Vector2.zero;

        //finds the arms used to block
        arms = player.myParent.parent.Find("Arms");
        rightArm = arms.Find("RightArmPivot").GetComponent<MyMouseLook>();
        leftArm = arms.Find("LeftArmPivot").GetComponent<MyMouseLook>();

        if(armMovementSensitivity == null || armMovementSensitivity == Vector2.zero)
            armMovementSensitivity = new Vector2(leftArm.sensitivityX, leftArm.sensitivityY);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Block()
    {
        arms.gameObject.SetActive(block);
        UpdateArmMovementSensitivity();
        if (block)
        {
            if (player.isPlayer)
            {
                player.SetCameraMovement(false);
                SetArmMovement(true);
                player.SetCursorLock(true);
                armWaveRotation.x -= (Input.mouseScrollDelta.y) * armSpreadSensitivity;
            }
            else
            {
                /*if (Mathf.Abs(leftArm.transform.localEulerAngles.z) < 45 && Mathf.Abs(rightArm.transform.localEulerAngles.z) < 45)
                {
                    downArm-= 5f;
                    print(downArm + ", " + leftArm.transform.localEulerAngles.z + " , " + rightArm.transform.localEulerAngles.z);
                    if (downArm < -30)
                        downArm = -30;
                    leftArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY,0,downArm);
                    rightArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY,0, downArm);
                }*/
                if (player.myMovement.rb.velocity.y > 0)
                    downArm -= 1.75f;
                else
                {
                    if (downArm < 0)
                        downArm += 1.75f;
                }
                if (downArm < -40)
                    downArm = -40;
                leftArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY, blockArmRotation, downArm);
                rightArm.SetRotation(MyMouseLook.RotationAxes.MouseXAndY, blockArmRotation, downArm);
            }

            blockOver = true;
            if (armWaveRotation.x > 15)
                armWaveRotation.x = 15;
            if (armWaveRotation.x < -45)
                armWaveRotation.x = -45;
            player.hitCooldownTimer = 0;
            armWaveRotation.y = -armWaveRotation.x;
            blockArmRotation = 0;
            leftArm.OffsetRotation = Quaternion.AngleAxis(armWaveRotation.x, leftArm.XDirection);
            rightArm.OffsetRotation = Quaternion.AngleAxis(armWaveRotation.y, rightArm.XDirection);
        }
        else
        {
            if (blockOver)
            {
                if (player.isPlayer)
                {
                    SetArmMovement(false);
                    player.SetCameraMovement(true);
                }
                else
                {
                    rightArm.Reset();
                    leftArm.Reset();
                }
                blockOver = false;
                downArm = 0;
                leftArm.OffsetRotation = Quaternion.AngleAxis(0, leftArm.XDirection);
                rightArm.OffsetRotation = Quaternion.AngleAxis(0, rightArm.XDirection);
                armWaveRotation = Vector2.zero;
            }
        }
    }

    void UpdateArmMovementSensitivity()
    {
        leftArm.SetSensitivityX(armMovementSensitivity.x);
        leftArm.SetSensitivityY(armMovementSensitivity.y);
        rightArm.SetSensitivityX(armMovementSensitivity.x);
        rightArm.SetSensitivityY(armMovementSensitivity.y);
    }

    public void SetArmMovement(bool move)
    {
        player.SetCursorLock(true);
        rightArm.look = move;
        leftArm.look = move;
        if (!move)
        {
            rightArm.Reset();
            leftArm.Reset();
        }
    }
}
