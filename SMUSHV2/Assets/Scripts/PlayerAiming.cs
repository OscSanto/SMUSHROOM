using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(CinemachineVirutalCamera)] This would require the script to have the component, else it wont run.
// safety measure
public class PlayerAiming : MonoBehaviour {
    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;
    Camera mainCamera;
    public Transform cameraLookAt;
    public float turnspeed = 2.0f;
    public CinemachineInputProvider sdc;

    private CinemachineVirtualCamera cinemachineVirtualCamera;

    // Start is called before the first frame update
    void Start(){
        mainCamera = Camera.main;
        cinemachineVirtualCamera = mainCamera.GetComponent<CinemachineVirtualCamera>();
        GameObject camobj = GameObject.Find("CM vcam1");
        cinemachineVirtualCamera = camobj.GetComponent<CinemachineVirtualCamera>();
        yAxis.SetInputAxisProvider(1, cinemachineVirtualCamera.GetInputAxisProvider());
        xAxis.SetInputAxisProvider(0, cinemachineVirtualCamera.GetInputAxisProvider());
    }

    // Update is called once per frame
    void Update(){ }
    void FixedUpdate(){
        xAxis.Update(Time.fixedDeltaTime);
        yAxis.Update(Time.fixedDeltaTime);

        cameraLookAt.eulerAngles = new Vector3(xAxis.Value, yAxis.Value, 0);

        float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.Euler(0, yawCamera, 0),
            turnspeed * Time.deltaTime);
    }
}