using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class CMFreeLookZoom : MonoBehaviour
{
    public CinemachineFreeLook cmFreeLook;
    private CinemachineFreeLook.Orbit[] OriginalOrbits;
    private PlayerInput playerInput;

    private bool canZoom = true; //is camera allowed to zoom
    public bool CanZoom { get { return canZoom; } set { canZoom = value; } }
    [Range(-1F, 1F)]
    public float zoom;
    [Range(-1F, 1F)]
    public float zoomFactor;

    // Start is called before the first frame update
    void Start(){
        

    }
    void awake(){
        //cmFreeLook = Camera.main.GetComponent<CinemachineFreeLook>();
        OriginalOrbits = new CinemachineFreeLook.Orbit[cmFreeLook.m_Orbits.Length];
        
        playerInput = new PlayerInput();
        playerInput.CharacterControls.Zoom.started += onZoom;
        playerInput.CharacterControls.Zoom.canceled += onZoom;
        Debug.Log("what "+OriginalOrbits);

        for (int i = 0; i < cmFreeLook.m_Orbits.Length; i++) {
            OriginalOrbits[i].m_Height = cmFreeLook.m_Orbits[i].m_Height;
            OriginalOrbits[i].m_Radius = cmFreeLook.m_Orbits[i].m_Radius;
        }
    }

  
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cmFreeLook.m_Orbits.Length; i++) {
            cmFreeLook.m_Orbits[i].m_Height = OriginalOrbits[i].m_Height * (zoom/Math.Abs(zoom) * zoomFactor);
            cmFreeLook.m_Orbits[i].m_Radius = OriginalOrbits[i].m_Radius * (zoom/Math.Abs(zoom) * zoomFactor);
        }
    }
    private void CameraZoom() {
        //input scroll y ranges from (-240, 240)
        //my mouse only ranges from (-120, 120)
        // multiply it with zoomfactor, scale it to (-0.5,0.5)
        // zoom is not mouse wheel's axis position, but the amount of mouse scroll up or down.
        
        Debug.Log("before" + zoom); //-120
        zoom = zoom / 120f * zoomFactor;
        Debug.Log("after" + zoom); //-0.5

        //zoom = Mathf.Clamp(zoom, -0.5f, zoomMaxDistance);
        //Debug.Log("clamped" + zoom); 

        //use smooth damp to calcululate current camera distance to get a smooth transition
        //cameraDistance = Mathf.SmoothDamp(cameraDistance, zoom, ref zoomvelocity, Time.unscaledTime * zoomspeed);
        //Debug.Log("cameradistane" + zoom);


        //this.cinemachineVirtualCamera.GetComponents<CinemachineFreeLook>();
        //CinemachineCore.GetInputAxis
        //        this.cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = this.cameraDistance;
    }
    void onZoom(InputAction.CallbackContext context) { zoom = context.ReadValue<float>(); }


}
