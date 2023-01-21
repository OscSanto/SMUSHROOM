using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float turnSpeed = 90f;

    // declared reference variables
    CharacterController characterController;
    private Animator animator;
    PlayerInput playerInput;

    // variables for player data
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 appliedMovement;
    bool isMovementPressed;
    bool isRunPressed;

    // constants
    float runMultiplier = 3.0f;
    float rotationFactorPerFrame = 15.0f;

    // gravity varaibles
    float gravity = -9.8f;
    float groundedGravity = -.05f;

    // jumping variables
    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 3.0f;
    float maxJumpTime = 0.75f;
    bool isJumping = false;


    // Start is called before the first frame update
    void Start()
    {
    }

    //Awake is called even earlier than start.
    void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // set the player input callbacks. The charactercontroller triggers the functions on key events
        // cancel phase is releaseing the button
        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput; //This is for joysticks. Due to inbetween values between 0 and 1
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        setupJumpVaraibles();

    }


    // Update is called once per frame
    void Update()
    {

        handleRotation();
        handleAnimation();

        if (isRunPressed)
        {
            appliedMovement.x = currentRunMovement.x;
            appliedMovement.z = currentRunMovement.z;
        }
        else
        {
            appliedMovement.x = currentMovement.x;
            appliedMovement.z = currentMovement.z;
        }
        characterController.Move(appliedMovement * Time.deltaTime);

        /*
                //Checks to see if player has moved the camera sinse last horizontal movement.
                //If so, moved is set to true, and player may rotate by camera movement
                if (currentMovementInput.y > Mathf.Abs(.1f) || currentMovementInput.x > Mathf.Abs(.1f))
                {
                    //camera is moving.
                    moved = true;

                }
              *//*  if (Input.GetAxis("Mouse Y") > Mathf.Abs(.1f) || Input.GetAxis("Mouse X") > Mathf.Abs(.1f)) {
                    //camera is moving.
                    moved = true;

                }*//*
                //Keeps the player Locked into rotation of camera. No freedom
                //var CharacterRotation = Camera.main.transform.rotation;
                //CharacterRotation.x = 0;
                //CharacterRotation.z = 0;
                // transform.rotation = CharacterRotation;

                //If player if holding w, and no other horizontal is being pressed, rotate character to camera forward.
                if (currentMovementInput.y >0 && currentMovementInput.x <=0 && moved)
                {

                    //gently rotate the player towards the camera forward
                    //get camera rotation.y 
                    Quaternion newrotation = Camera.main.transform.rotation;
                    newrotation.x = transform.rotation.x;
                    newrotation.z = transform.rotation.z; 
                    transform.rotation = Quaternion.Slerp(transform.rotation, newrotation, .013f);

                }
                else
               {

                    //Normal Horizontal rotation (DOn't delte)
                    transform.Rotate(Vector3.up, currentMovementInput.x * Time.deltaTime * turnSpeed);
                    //Camera has not been moved
                    moved = false;
                }
                */

        //forward movement speed.
        //var velocity = Vector3.forward * currentMovementInput.x * speed;
        //transform.Translate(velocity * Time.deltaTime);

        //we want to have negative values for Speed parameter (to go backwards when the S key is held).
        //So we change it from velocity.magnitude (which is always positive).
        // animator.SetFloat("Speed", velocity.z);

        //handle gravity applies gravity to currentmovement and currentrunmovement
        handleGravity();
        handleJump();
    }
    void setupJumpVaraibles()
    {
        //  timeToApex, how long it takes to reach the max height. (assuming its a symetrical parabola, we divide maxjumptime in half to get the top)
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    void handleJump()
    {
        // if the character is touching the ground but there is no gravity (downward velocity) 
        // the charactercontroller.isgrounded will return false.
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {

            // set animator here
            animator.SetBool("isJumping", true);
            isJumping = true;
            currentMovement.y = initialJumpVelocity;
            appliedMovement.y = initialJumpVelocity;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            isJumping = false;
        }
    }
    void handleGravity()
    {

        // !isJumpPressed allows player to release the jump and jump down sooner.
        // isfalling is determined by character.y and affected by gravity.
        // in mid jump, characterMovement.y > 0 
        bool isfalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;
        // apply gravity depending if the character is grounded or not

        if (characterController.isGrounded)
        {
            // set animator here
            animator.SetBool("isJumping", false);
            currentMovement.y = groundedGravity;
            appliedMovement.y = groundedGravity;

        }
        else if (isfalling)
        {
            Debug.Log("Is falling = " + isfalling);

            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
            //clamp, so you don't fall at very fast speeds (a cliff fall would be like 200mph drop)
            appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * .5f, -20.0f);

        }
        else
        {
            //due to frame rates, different jumps can occur. This will average 60 and 30 fps differences.
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (gravity * Time.deltaTime);
            appliedMovement.y = (previousYVelocity + currentMovement.y) * .5f;

            // old velocity + acceleration = new velocity
            // old position + new velocity = new position 'this applies inside Update function (isRunPressed) conditions'


        }
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        //currentMovementInput now stores the WASD/Joystick input values
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x * speed;
        currentMovement.z = currentMovementInput.y * speed;
        currentRunMovement.x = currentMovementInput.x * runMultiplier * speed;
        currentRunMovement.z = currentMovementInput.y * runMultiplier * speed;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
    void handleRotation()
    {

        Vector3 positionToLookAt; //where we are moving next
        // the change in position our character should point to
        positionToLookAt.x = currentMovementInput.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovementInput.y;
        // the current rotation of our character
        Quaternion currentRotation = transform.rotation;
        if (isMovementPressed)
        {
            // create a new rotation based on where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }

    }
    void handleAnimation()
    {
        //get parameter values from animator
        bool isWalking = animator.GetBool("isWalking");
        bool isRunning = animator.GetBool("isRunning");
        //start walking if movment pressed is true and not already walking
        if (isMovementPressed && !isWalking!)
        {
            animator.SetBool("isWalking", true);
        }
        //stop walking if ismomvement is false and not already walking
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool("isWalking", false);
        }
        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool("isRunning", true);
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool("isRunning", false);
        }

    }
    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }
    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    void OnEnable()
    {

        //enable the character controls action map
        playerInput.CharacterControls.Enable();
    }
    void OnDisable()
    {
        //disable the character controls action map
        playerInput.CharacterControls.Disable();
    }
}
