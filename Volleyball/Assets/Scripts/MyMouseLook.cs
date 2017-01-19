﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMouseLook : MonoBehaviour
{

    public bool look = true;

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public Direction xAxisOfRotation = Direction.Y;
    public Direction yAxisOfRotation = Direction.X;
    public bool InvertX;
    public bool InvertY;

    Vector3 xDirection;
    Vector3 yDirection;

    [Range(0.01f,1)]
    public float smoothX = .1f;
    [Range(0.01f, 1)]
    public float smoothY = .1f;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationX = 0F;
    float rotationY = 0F;

    Quaternion originalRotation;
    Quaternion myRotation;
    void Update()
    {
        switch (xAxisOfRotation)
        {
            case Direction.X:
                xDirection = Vector3.left;
                break;
            case Direction.Y:
                xDirection = Vector3.up;
                break;
            case Direction.Z:
                xDirection = Vector3.forward;
                break;
        }
        switch (yAxisOfRotation)
        {
            case Direction.X:
                yDirection = Vector3.left;
                break;
            case Direction.Y:
                yDirection = Vector3.up;
                break;
            case Direction.Z:
                yDirection = Vector3.forward;
                break;
        }
        xDirection *= InvertX ? -1 : 1;
        yDirection *= InvertY ? -1 : 1;
        if (look)
        {
            if (axes == RotationAxes.MouseXAndY)
            {
                // Read the mouse input axis
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, xDirection);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, yDirection);
                myRotation = Quaternion.Lerp(transform.localRotation, originalRotation * xQuaternion * yQuaternion,(smoothX + smoothY)/2);
            }
            else if (axes == RotationAxes.MouseX)
            {
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationX = ClampAngle(rotationX, minimumX, maximumX);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, xDirection);
                myRotation = Quaternion.Lerp(transform.localRotation, originalRotation * xQuaternion, smoothX);
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, yDirection);
                myRotation = Quaternion.Lerp(transform.localRotation, originalRotation * yQuaternion, smoothY);
                
            }
        }
        transform.localRotation = myRotation;
    }

    void Start()
    {
        originalRotation = transform.localRotation;
    }

    public void Reset()
    {
        myRotation = originalRotation;
        transform.localRotation = myRotation;
        rotationX = 0;
        rotationY = 0;
    }

    public void SetSensitivityX(float input)
    {
        sensitivityX = input;
    }

    public void SetSensitivityY(float input)
    {
        sensitivityY = input;
    }
    public void SetCursorLock(bool state)
    {
        if(state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}