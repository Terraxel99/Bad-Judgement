﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Movement : MonoBehaviour
{
    #region Members

    #region Sounds members

    [SerializeField] private AudioClip sonDePasSurRue; //Initialisation des sons de pas 
    [SerializeField] private AudioClip sonDePasSurBois;
    [SerializeField] private AudioClip sonDePasSurterre;
    [SerializeField] private AudioClip jumpOnGrass; // Initialisation de sons de Jump
    [SerializeField] private AudioClip jumpOnStreet;
    [SerializeField] private AudioClip jumpOnWood;
    [SerializeField] private AudioSource personnage; // Source pour les bruits de pas normaux
    [SerializeField] private AudioSource piedjumpPersonnage; // Source pour les pruits de sauts
    [Range(0f, 1f)] // Permet de regler le volume via un bouton de reglage dans Unity
    private float volumeDesSonsDePas = 0.2F;
    private char tagNew = '\0'; // donne la valeur null du char, cette variable permet de faire en sorte dans l'algorithme de continuer un son de pas même lorsqu'on entre en collision avec un autre objet non tagué (exemple : un mur)

    #endregion Sounds members

    private float forwardSpeed = 4.2F;
    private float backwardSpeed;
    private float sideSpeed = 2.15F; //Speeds
    //private float strafeSpeed; //We'll be able to strafe fast. => WIP (2.88 KMH).
    private float runMultiplier = 1.6F; //If the player wants to run, his forward speed will be multiplicated by 1.6

    private float jumpForce = 4.5F; //Force of the jump that the character will have

    private float normalCrouchDeltaH = 0.6F;
    private float onTheKneesCrouchDeltaH = 0.35F; //The height the character will lose while crouching

    private bool wantsToRun; //To know is the character wants to run
    private bool characterCanJump; //Useful for the jump move

    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private CapsuleCollider playerCollider; //Getting thos components via editor

    //List<string> animParametersList; //New list w/ names of booleans to handle animations

    FatigueSys fatigue;

    #endregion

    #region Properties & readonly

    public static bool characterIsMoving { get; private set; }
    public static bool characterIsJumping { get; private set; } //Properties accessible in readonly in other scripts
    public static bool characterIsCrouched { get; private set; }
    public static bool characterIsGrounded { get; private set; }

    public static bool characterIsWalkingFwd { get; private set; }
    public static bool characterIsIdle { get; private set; }

    #endregion

    #region Start and Update

    // Use this for initialization
    void Start()
    {
        backwardSpeed = (0.66F * forwardSpeed); //After real tests, reverse speed is 2/3 times of forward speed.
        characterIsCrouched = false;
        characterIsGrounded = true;

        #region sounds
        personnage.volume = volumeDesSonsDePas; // Permet de reglez les sons de pas
        piedjumpPersonnage.volume = volumeDesSonsDePas; // permet de regler les son de jumps
        #endregion 

        characterIsIdle = true;
        characterIsWalkingFwd = false;

        this.fatigue = new FatigueSys();

        //this.animParametersList = AnimatorHandling.GetParameterNames(this.anim);
        //We send our animator to get a whole list of the animator's parameters. This will allow us to disable all the bools we don't need in only one line !
    }

    // Update is called once per frame
    void Update()
    {
        #region Ground Move

        if (characterIsGrounded) this.Move();

        #endregion

        #region Jump

        //Checking for Jump :
        //If player wants to jump AND that character can jump AND that character isn't crouched :
        if (Input.GetButtonDown("Jump") && this.characterCanJump && !(characterIsCrouched))
        {
            Jump(); //Makes the character jump
            characterIsJumping = true; //Setting the property for other scripts
        }
        else characterIsJumping = false; //Setting property for other scripts

        #endregion

        #region Crouch

        if (Input.GetKeyDown(KeyCode.C)) Crouch(this.normalCrouchDeltaH);
        //Doing two != types of crouching depending on the key pressed
        if (Input.GetKeyDown(KeyCode.B)) Crouch(this.onTheKneesCrouchDeltaH);

        #endregion
    }

    #endregion

    #region Moving Methods

    private void Jump()
    {
        #region sound
        Sounds.JumpSound(piedjumpPersonnage); // Permet de jouer les sons de saut
        #endregion sound

        Vector3 jump = new Vector3(0F, jumpForce, 0F); //Making the jump by setting the velocity to the jump force
        //playerRigidbody.velocity += jump;
        playerRigidbody.AddForce(jump, ForceMode.VelocityChange);
        this.characterCanJump = false; //Telling that the player may not jump

        //In the sounds part, there's the method that handles OnCollisionEnter event. I just added this.characterCanJump = true
        //So that when the player touches the ground again, he can jump.
    }

    private void Move() //Fully reworked to correct collision bugs
    {
        Vector3 currentVelocity = playerRigidbody.velocity; //Getting current velocity
        Vector3 targetSpeed = new Vector3(Input.GetAxis("Horizontal"), currentVelocity.y, Input.GetAxis("Vertical")); //Calculating new velocity


        if (targetSpeed.x != 0 || targetSpeed.z != 0)
        {
            bool wantsToRun = Input.GetKey(KeyCode.LeftShift); //Checking if player pressed Lshift (means that he wants to run)

            if (targetSpeed.z > 0) targetSpeed.z *= forwardSpeed; //If going forward multiplying by forward speed
            else targetSpeed.z *= backwardSpeed; //If going backwards, multiplying by backwards speed that is lower than forward one
            targetSpeed.x *= sideSpeed; //Assigning speeds to each component of the moving Vector

            targetSpeed = transform.TransformDirection(targetSpeed); //Doing a transformDirection to be able to turn the axes
            //Have to keep this after applying speeds so that we don't have speeds/direction problems

            if (wantsToRun && this.fatigue.isAbleToRun()) //If player wants to run, we increase movement speed by a number that'll change depending on exhaust
            {
                targetSpeed.x *= runMultiplier;
                targetSpeed.z *= runMultiplier;
                this.fatigue.Running();
            }

            Vector3 deltaMove = targetSpeed - currentVelocity;
            
            //Not doing the difference between actual velocity and new one would provoke a kind of acceleration which we don't want

            playerRigidbody.AddForce(deltaMove, ForceMode.VelocityChange); //Applying that force to the player. Multiplying by 50 (float) to get something strong enough.

            #region sound
            Sounds.FootSteepsSound(personnage); // Permet de jouer les sons de pas
            #endregion  
        }
    }

    private void Crouch(float deltaHeight)
    {
        if (characterIsCrouched) deltaHeight *= -(1.0F);
        //If the player is crouched then we make the height higher and not lower

        this.playerCollider.height -= deltaHeight;
        this.playerCollider.center -= new Vector3(0F, (deltaHeight / 2), 0F);

        characterIsCrouched = InvertBool(characterIsCrouched);
    }

    #endregion

    #region Animation Methods

    public void Dying()
    {

    }

    #endregion

    #region Other Methods 

    private bool InvertBool(bool toInvert)
    {
        if (toInvert) toInvert = false;
        else toInvert = true;

        return toInvert;
    }

    #endregion

    #region Sounds detection area

    private void OnCollisionEnter(Collision collision) // Permet d'evaluer le son a jouer en fonction du type de sol rencontré /!\ On a besoin d'un rigibody et d'une box Colider /!\
    {
        this.characterCanJump = true;

        if ((collision.collider.CompareTag("Wood") || (tagNew == 'w' && !collision.collider.CompareTag("Wood"))) && !collision.collider.CompareTag("Street") && !collision.collider.CompareTag("Grass")) // OPTIMISATION : tag == "something"  allocates memory, CompareTag does not, i've changes this , sources : https://forum.unity.com/threads/making-stepssounds-by-using-oncollisionenter-or-raycasts-optimizations-question.518865/#post-3402025 ,https://answers.unity.com/questions/200820/is-comparetag-better-than-gameobjecttag-performanc.html
        {
            Sounds.DeclareSonDemarche(personnage, sonDePasSurBois, piedjumpPersonnage, jumpOnWood, tagNew, 'w');
        }
        else if ((collision.collider.CompareTag("Street") || (tagNew == 's' && !collision.collider.CompareTag("Street"))) && !collision.collider.CompareTag("Wood") && !collision.collider.CompareTag("Grass")) // Toute ces conditions permette de jouer le sons même en etant en colision avec d'autres objet non tagué (exemple : un mur, un ventilateur, ect...) 
        {
            Sounds.DeclareSonDemarche(personnage, sonDePasSurRue, piedjumpPersonnage, jumpOnStreet, tagNew, 's');
        }
        else if ((collision.collider.CompareTag("Grass") || (tagNew == 'g' && !collision.collider.CompareTag("Wood"))) && !collision.collider.CompareTag("Street") && !collision.collider.CompareTag("Wood"))
        {
            Sounds.DeclareSonDemarche(personnage, sonDePasSurterre, piedjumpPersonnage, jumpOnGrass, tagNew, 'g');
        }
    }

    #endregion

}
