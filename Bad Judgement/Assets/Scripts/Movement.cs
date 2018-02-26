﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    #region Members

    private float forwardSpeed;
    private float backwardSpeed;
    private float sideSpeed; //Speeds
    //private float strafeSpeed; //We'll be able to strafe fast. => WIP (2.88 KMH).

    private float jumpForce; //Force of the jump that the character will have

    private bool characterCanJump; //Useful for the jump move

    [SerializeField]private Rigidbody playerRigidbody;

    #endregion

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
    private char tag = '\0'; // donne la valeur null du char, cette variable permet de faire en sorte dans l'algorithme de continuer un son de pas même lorsqu'on entre en collision avec un autre objet non tagué (exemple : un mur)

    #endregion Sounds members

    #region Properties

    public bool characterIsMoving { get; private set; }
    public bool characterIsJumping { get; private set; } //Properties accessible in readonly in other scripts

    #endregion

    #region Start and Update

    // Use this for initialization
    void Start()
    {
        //These speeds are based on my real tests and are in KMH :
        forwardSpeed = 4.2F;
        backwardSpeed = (0.66F * forwardSpeed); //After real tests, reverse speed is 2/3 times of forward speed.
        sideSpeed = 2.15F;
        jumpForce = 4.5F;

        #region sounds
        personnage.volume = volumeDesSonsDePas; // Permet de reglez les sons de pas
        piedjumpPersonnage.volume = volumeDesSonsDePas; // permet de regler les son de jumps
        #endregion sounds

        //To be moved later :
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //To be moved :
        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
        if (Cursor.lockState == CursorLockMode.None && Input.GetKey(KeyCode.Mouse0)) Cursor.lockState = CursorLockMode.Locked;
        //Locks the cursor on the window

        //========================================================================================
        //Checking for Ground Moving :
        float xAxis = Input.GetAxis("Horizontal") * sideSpeed * Time.deltaTime;
        float zAxis = Input.GetAxis("Vertical") * Time.deltaTime;

        if (zAxis != 0 || xAxis != 0) //If the player is moving :
        {
            Move(zAxis, xAxis); //We're moving on X and Z
            characterIsMoving = true; //If the player moves, then we say to the property to be true
        }
        else characterIsMoving = false; //If the player moves, then we say to the property to be true
        //Other ppl might want to use that property in other scripts

        //========================================================================================

        //Checking for Jump :

        if (Input.GetButtonDown("Jump") && this.characterCanJump) //If player wants to jump & that character can jump
        {
            Jump(); //Makes the character jump
            this.characterIsJumping = true; //Setting the property for other scripts
        }
        else this.characterIsJumping = false; //Setting property for other scripts
    }

    #endregion

    #region Moving Methods

    private void Jump()
    {
        #region sound
        Sounds.JumpSound(piedjumpPersonnage); // Permet de jouer les sons de saut
        #endregion sound

        Vector3 jump = new Vector3(0F, jumpForce, 0F); //Making the jump by setting the velocity to the jump force
        playerRigidbody.velocity += jump;

        this.characterCanJump = false; //Telling that the player may not jump
        //In the sounds part, there's the method that handles OnCollisionEnter event. I just added this.characterCanJump = true
        //So that when the player touches the ground again, he can jump.
    }

    private void Move(float zAxis, float xAxis)
    {
        #region sound
        Sounds.FootSteepsSound(personnage); // Permet de jouer les sons de pas
        #endregion sound

        if (zAxis < 0) zAxis *= backwardSpeed;
        else zAxis *= forwardSpeed; //Forward speed != than backward speed

        Vector3 movement = new Vector3(xAxis, 0F, zAxis);
        //X is the strafe and Z is forward/backward

        this.transform.Translate(movement); //Making the move
    }

    #endregion

    #region Sounds detecion area

    private void OnCollisionEnter(Collision collision) // Permet d'evaluer le son a jouer en fonction du type de sol rencontré /!\ On a besoin d'un rigibody et d'une box Colider /!\
    {
        this.characterCanJump = true;

        if ((collision.collider.CompareTag("Wood") || (tag == 'w' && !collision.collider.CompareTag("Wood"))) && !collision.collider.CompareTag("Street") && !collision.collider.CompareTag("Grass")) // OPTIMISATION : tag == "something"  allocates memory, CompareTag does not, i've changes this , sources : https://forum.unity.com/threads/making-stepssounds-by-using-oncollisionenter-or-raycasts-optimizations-question.518865/#post-3402025 ,https://answers.unity.com/questions/200820/is-comparetag-better-than-gameobjecttag-performanc.html
        {
            personnage.clip = sonDePasSurBois; // Ici le son va alors devenir celui d'un bruit de pas sur le bois, pour les autres ca va jouer les son que l'on aura alors importé aussi lorsque la surface change
            piedjumpPersonnage.clip = jumpOnWood; // Pareil pour les jumps 
            if (tag != 'w') tag = 'w';
        }
        else if ((collision.collider.CompareTag("Street") || (tag == 's' && !collision.collider.CompareTag("Street"))) && !collision.collider.CompareTag("Wood") && !collision.collider.CompareTag("Grass")) // Toute ces conditions permette de jouer le sons même en etant en colision avec d'autres objet non tagué (exemple : un mur, un ventilateur, ect...) 
        {
            personnage.clip = sonDePasSurRue;
            piedjumpPersonnage.clip = jumpOnStreet;
            if (tag != 's') tag = 's';
        }
        else if ((collision.collider.CompareTag("Grass") || (tag == 'g' && !collision.collider.CompareTag("Wood"))) && !collision.collider.CompareTag("Street") && !collision.collider.CompareTag("Wood"))
        {
            personnage.clip = sonDePasSurterre;
            piedjumpPersonnage.clip = jumpOnGrass;
            if (tag != 'g') tag = 'g';
        }
    }

    #endregion Sounds detecion area

}
