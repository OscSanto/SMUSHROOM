using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour {
    // declared reference variables
    CharacterController characterController;
    private Animator animator;
    PlayerInput playerInput;

    // variables for player data
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 cameraRelativeMovement;
    Vector3 appliedMovement;
    bool isMovementPressed;
    bool isRunPressed;

    // constants
    float runMultiplier = 3.0f;
    float rotationFactorPerFrame = 15.0f;

    // gravity varaibles
    float gravity = -9.8f;

    // jumping variables
    bool isJumpPressed = false;
    float initialJumpVelocity;
    public float maxJumpHeight = 3.0f;
    public float maxJumpTime = 0.75f;
    bool isJumping = false;
    bool requireNewJumpPress = false;

    // state varaibles
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    private float zoom = 1.0f;
    public CinemachineFreeLook cinemachineVirtualCamera;
    public float zoomMinDistance = 1.5f;
    public float zoomMaxDistance = 6f;
    public float zoomspeed = 0.02f;
    public float zoomFactor = 0.5f;
    private float targetzoom = 1.0f; //target camera distance
    private float cameraDistance = 2f; //current camera distance
    private float zoomvelocity = 0f; //smooth velocity

    [SerializeField] private float speed = 12f;

    // getters and setters
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public CharacterController CharacterController { get { return characterController; } }
    public Animator Animator { get { return animator; } set { animator = value; } }

    public float CurrentMovementY { get { return currentMovement.y; } set { currentMovement.y = value; } }
    public float AppliedMovementY { get { return appliedMovement.y; } set { appliedMovement.y = value; } }
    public float AppliedMovementX { get { return appliedMovement.x; } set { appliedMovement.x = value; } }
    public float AppliedMovementZ { get { return appliedMovement.z; } set { appliedMovement.z = value; } }

    public Vector2 CurrentMovementInput { get { return currentMovementInput; } }
    public float RunMultiplier { get { return runMultiplier; } set { runMultiplier = value; } }

    public float Gravity { get { return gravity; } }

    public bool RequireNewJumpPress { get { return requireNewJumpPress; } set { requireNewJumpPress = value; } }
    public bool IsJumping { get { return isJumping; } set { isJumping = value; } }
    public bool IsJumpPressed { get { return isJumpPressed; } set { isJumpPressed = value; } }
    public float InitialJumpVelocity { get { return initialJumpVelocity; } }

    public bool IsRunPressed { get { return isRunPressed; } }
    public bool IsMovementPressed { get { return isMovementPressed; } }

    public float Speed { get { return speed; } }

    public float Zoom { get { return zoom; } set { zoom = value; } }


    // Start is called before the first frame update
    void Start(){ characterController.Move(appliedMovement * Time.deltaTime); }

    // Update is called once per frame
    void Update(){
        handleRotation();
        _currentState.UpdateStates();

        cameraRelativeMovement = ConvertToCameraSpace(appliedMovement);
        characterController.Move(cameraRelativeMovement * Time.deltaTime);
        CameraZoom();

    }
   
    void Awake(){
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // setup state
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        // set the player input callbacks. The charactercontroller triggers the functions on key events
        // cancel phase is releaseing the button
        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed
            += onMovementInput; //This is for joysticks. Due to inbetween values between 0 and 1

        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        playerInput.CharacterControls.Zoom.started += onZoom;
        playerInput.CharacterControls.Zoom.canceled += onZoom;

        SetupJumpVaraibles();
    }
    //* TODO: create a condition, where if the range of mouse is -240 to 240, divide by 240. 
    private void CameraZoom(){
        //input scroll y ranges from (-240, 240)
        //my mouse only ranges from (-120, 120)
        // multiply it with zoomfactor, scale it to (-0.5,0.5)
        // zoom is not mouse wheel's axis position, but the amount of mouse scroll up or down.
        this.Zoom -= this.Zoom / 120f * this.zoomFactor;
        this.Zoom = Mathf.Clamp(this.zoom, 0.0f, this.zoomMaxDistance);

        //use smooth damp to calcululate current camera distance to get a smooth transition
        this.cameraDistance = Mathf.SmoothDamp(this.cameraDistance, this.zoom, ref this.zoomvelocity, Time.unscaledTime * this.zoomspeed);
        Debug.Log("cam distance" + cameraDistance);
        Debug.Log("zoom" + zoom);
        this.cinemachineVirtualCamera.GetComponents<CinemachineFreeLook>();
        cinemachineVirtualCamera.m_Lens.FieldOfView += cameraDistance;

//        this.cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = this.cameraDistance;
    }


    Vector3 ConvertToCameraSpace(Vector3 vectorToRotate){
        // try to change this to player forward and right? to enable convert to player space rotation. * And NOT based on camera*
        float currentYValue = vectorToRotate.y;

        //get forward and right of camera
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // remove the Y values to ignore upward/downard camera angles
        cameraForward.y = 0;
        cameraRight.y = 0;

        // re-normalize both vectors
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        // rotate the x and z vectorTortate values to camera space
        Vector3 cameraForwardZproduct = vectorToRotate.z * cameraForward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        // the sum of both products is the Vector3 in camera space and set Y value
        Vector3 vectorRotatedToCameraSpace = cameraForwardZproduct + cameraRightXProduct;
        vectorRotatedToCameraSpace.y = currentYValue;

        return vectorRotatedToCameraSpace;
    }
    void handleRotation(){
        //where we are moving next
        Vector3 positionToLookAt; 

        // the change in position our character should point to
        positionToLookAt.x = cameraRelativeMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = cameraRelativeMovement.z;

        // the current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed) {
            // create a new rotation based on where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation
                = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }
    void SetupJumpVaraibles(){
        //  timeToApex, how long it takes to reach the max height. (assuming its a symetrical parabola, we divide maxjumptime in half to get the top)
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }
    void onMovementInput(InputAction.CallbackContext context){
        //currentMovementInput now stores the WASD/Joystick input values
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
    void onRun(InputAction.CallbackContext context){ isRunPressed = context.ReadValueAsButton(); }
    void onJump(InputAction.CallbackContext context){
        isJumpPressed = context.ReadValueAsButton();
        requireNewJumpPress = false;
    }
    void onZoom(InputAction.CallbackContext context){ zoom = context.ReadValue<float>(); }

    //enable the character controls action map
    void OnEnable(){ playerInput.CharacterControls.Enable(); }

    //disable the character controls action map
    void OnDisable(){ playerInput.CharacterControls.Disable(); }
}