﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    #region Members

    private Camera cam; //Current camera (it's a children component, this script has to be applied to the player !!!!!)
    [SerializeField] private Camera normalCam; //Normal camera which isn't free moveable
    [SerializeField] private Camera freeCam; //Reference to the free cam that hasn't a gun attached on it. Disabled by default

    [SerializeField] private float minVertical = 70.0F;
    [SerializeField] private float maxVertical = -55.0F; //Maximum head angle is 55 degrees. Modifiable here.

    [SerializeField] private float maxLeftRightFreeCamAngle = 70.0F;

    private Vector3 verticalEulerVector; //Vector to handle vertical rotation of the camera
    private Vector3 horizontalEulerVector; //Vector to handle the horizontal rotation of the camera (IF FREE CAMERA MODE ACTIVE)

    #endregion
    
    #region Properties

    public bool isVerticalAxisInverted { get; private set; } //For you LERUTH // MERCI!!

    public bool isFreeCamActive { get; private set; }

    public float horizontalSensitivity { get; set; }
    public float verticalSensitivity { get; set; }

    #endregion

    #region Start and Update

    // Use this for initialization
    void Start()
    {
        cam = normalCam; //Starting with the main cam which is the FPS one
                           //As project I'd like to put a second cam which could be changed to TPS if we press a button

        //Initializing properties :
        this.isVerticalAxisInverted = false;
        this.horizontalSensitivity = 6.0F;
        this.verticalSensitivity = 6.0F;

        verticalEulerVector = Vector3.zero;
        horizontalEulerVector = Vector3.zero;
    }


    // Update is called once per frame
    void Update()
    {
        float yGameAxis = Input.GetAxis("Mouse X"); //The horizontal (X) mouse axis matches with the ingame y rotation axis
        float xGameAxis = Input.GetAxis("Mouse Y"); //The vertical (Y) mouse axis matches with the ingame x rotation axis

        if (yGameAxis != 0)
        {
            if (isFreeCamActive) MoveCamHoriz(yGameAxis); //Rotating the camera because of freecam mode
            else MoveCamHoriz(yGameAxis); //Rotating the player on its Y-axis.
        }
        MoveCamVertically(xGameAxis); //Moving the camera vertically (X axis)

        if (Input.GetKeyDown(KeyCode.LeftAlt)) ChangeFreeCamOrNot();
        //Setting Camera to free if alt is pressed, reverting action by pressing a second time

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Mouse0)) this.InvertAxis();
        //If LeftAlt + LeftClick => Inverting Y axis
    }

    #endregion

    #region Cam Methods

    private void InvertAxis()
    {
        if (this.isVerticalAxisInverted) this.isVerticalAxisInverted = false;
        else this.isVerticalAxisInverted = true;
    }

    private void MoveCamVertically(float xGameAxis)
    {
        this.cam.transform.localEulerAngles = verticalEulerVector; //Refreshing cam movement every frame

        float verticalRotation = xGameAxis * verticalSensitivity; //Applying sensitivity
        verticalEulerVector.x += verticalRotation; //Adding the mouse axis to the actual rotation

        if (verticalEulerVector.x >= minVertical) verticalEulerVector.x = minVertical;
        if (verticalEulerVector.x <= maxVertical) verticalEulerVector.x = maxVertical;
    }

    private void MoveCamHoriz(float yGameAxis)
    {
        yGameAxis *= horizontalSensitivity; //Applying vertical sensitivity

        this.transform.Rotate(0F, yGameAxis, 0F); //Horizontal rotate (applied to the player)
    }

    //WIP :
    private void MoveCamHorizIfFreeCam(float yGameAxis)
    {
        this.cam.transform.localEulerAngles = horizontalEulerVector; //Refreshing camera horizontal rotation every frame

        float horizontalRotation = yGameAxis * horizontalSensitivity; //Applying sensitivity
        horizontalEulerVector.y += horizontalRotation; //Adding the mouse rotation to the actual rotation

        //Clamping cam to a certain angle
        //if (verticalEulerVector.y >= maxLeftRightFreeCamAngle) verticalEulerVector.y = maxLeftRightFreeCamAngle;
        //if (verticalEulerVector.y <= -(maxLeftRightFreeCamAngle)) verticalEulerVector.y = -maxLeftRightFreeCamAngle;
    }

    private void ChangeFreeCamOrNot()
    {
        if (isFreeCamActive) //If we want to have the normal camera
        {
            this.normalCam.enabled = true;
            this.cam = normalCam;
            this.freeCam.enabled = false;

            this.isFreeCamActive = false;
        }
        else //If we want to have the free camera mode
        {
            this.freeCam.enabled = true;
            this.cam = freeCam;
            this.normalCam.enabled = false;

            this.isFreeCamActive = true;
        }
    }

    #endregion

}
