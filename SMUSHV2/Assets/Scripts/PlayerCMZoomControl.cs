using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCMZoomControl : MonoBehaviour {
    // Start is called before the first frame update
    //[SerializeField]private InputActionAsset inputProvider;
    [SerializeField] private CinemachineFreeLook cmFreeLook;
    [SerializeField] private float zoomSpeed = 10f; //how fast we can zoom in and out out of our target
    [SerializeField] private float zoomAccerleration = 2.5f; //smoothing
    [SerializeField] private float zoomInnerRange = 9f; //how close we can get to target
    [SerializeField] private float zoomOuterRange = 50f; //how far we can get away
    private PlayerInput playerInput;

    private float currentMiddleRigRadius = 10f; //where we are
    private float newMiddleRigRadius = 10f; //where we need to go

    [SerializeField] private float zoomYAxis = 0f; //capture from input system

    public float ZoomYAxis {
        get { return zoomYAxis; }
        set {
            //detects if we are zooming in and out
            if (zoomYAxis == value) return; //if no movement, return

            //movement detected
            zoomYAxis = value;

            AdjustCameraZoomIndex(ZoomYAxis);
        }
    }
    private void Awake(){
        playerInput = new PlayerInput();
        playerInput.CharacterControls.Zoom.performed
            += cntxt => ZoomYAxis = cntxt.ReadValue<float>(); // -120 to 120 returns

        playerInput.CharacterControls.Zoom.canceled += cntxt => ZoomYAxis = 0f; // if the mouse isn't moving, set to 0
    }
    public void AdjustCameraZoomIndex(float zoomYAxis){
        if (zoomYAxis == 0) return;

        if (zoomYAxis < 0) {
            newMiddleRigRadius = currentMiddleRigRadius + zoomSpeed;
        }

        if (zoomYAxis > 0) {
            newMiddleRigRadius = currentMiddleRigRadius - zoomSpeed;
        }
    }
    void LateUpdate(){
        UpdateZoomLevel();
    }
    public void UpdateZoomLevel(){
        //no change occurred
        if (currentMiddleRigRadius == newMiddleRigRadius) return;
        
        //
        currentMiddleRigRadius
            = Mathf.Lerp(currentMiddleRigRadius, newMiddleRigRadius, zoomAccerleration * Time.deltaTime);

        // zoom distance restrictions  [restricts distance from target & prevents being too close to target]
        currentMiddleRigRadius = Mathf.Clamp(currentMiddleRigRadius, zoomInnerRange, zoomOuterRange); 

        // keeps our zoom in a nice sphere around target.
        cmFreeLook.m_Orbits[1].m_Radius = currentMiddleRigRadius;
        cmFreeLook.m_Orbits[0].m_Height = cmFreeLook.m_Orbits[1].m_Radius;
        cmFreeLook.m_Orbits[2].m_Radius = cmFreeLook.m_Orbits[1].m_Radius;
    }
    void Start(){ }

    // Update is called once per frame
    void Update(){ }
    void OnEnable(){ playerInput.CharacterControls.Enable(); }

    //disable the character controls action map
    void OnDisable(){ playerInput.CharacterControls.Disable(); }
}