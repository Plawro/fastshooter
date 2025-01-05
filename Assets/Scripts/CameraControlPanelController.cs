using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class CameraControlPanelController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float smoothing = 8.0f;
    [SerializeField] private Vector2 horizontalClamp = new Vector2(-60, 60);
    [SerializeField] private Vector2 verticalClamp = new Vector2(-80, 80);

    private Vector2 targetRotation;
    private Vector2 currentRotation;
    private float rotationOffset = 0; // Offset for "looking back"
    private float targetOffset = 0;  // Target offset for smooth transition


    public Camera mainCamera;
    public float lookDistance = 5f;

    private bool isUsingVirtualCamera = false;

    public LayerMask ignoreLayer;
    public TextMeshProUGUI crosshair;
    //public TextMeshProUGUI inventoryText;
    string crosshairText = " ";

    [SerializeField] Transform inventory;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    public string nowInteractingWith = "";


    [SerializeField] CinemachineVirtualCamera controlPanelCam;

    void OnEnable(){
        targetOffset = -90;
    }

    void Start()
    {
        targetOffset = -90;
        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera is not assigned!");
            enabled = false;
            return;
        }
        currentRotation = Vector2.zero;
        targetRotation = Vector2.zero;

        crosshair.gameObject.SetActive(true);
        crosshair.text = GameController.Instance.crosshairSymbol;

        nowInteractingWith = "";
    }

    void Update()
    {
        // Check for "look back" key (S)
        //if (Input.GetKeyDown(KeyCode.S))
        //{
            //targetOffset = targetOffset == 0 ? 180 : 0; // Toggle between 0 and 180 degrees
        //}

        // Smoothly transition rotation offset
        rotationOffset = Mathf.Lerp(rotationOffset, targetOffset, Time.deltaTime * smoothing);

        // Get mouse input
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * sensitivity;

        // Update target rotation based on mouse input
        targetRotation.x = Mathf.Clamp(targetRotation.x + mouseInput.x, horizontalClamp.x, horizontalClamp.y);
        targetRotation.y = Mathf.Clamp(targetRotation.y - mouseInput.y, verticalClamp.x, verticalClamp.y);

        // Smoothly interpolate to the target rotation
        currentRotation = Vector2.Lerp(currentRotation, targetRotation, Time.deltaTime * smoothing);

        // Apply rotation to the camera with the offset
        virtualCamera.transform.localRotation = Quaternion.Euler(currentRotation.y, currentRotation.x + rotationOffset, 0);


        // KEYBOARD MODE
        
        if(targetOffset == -90 ||targetOffset == 0 || targetOffset == -180){
        if(nowInteractingWith == ""){

            if(targetOffset == 0 || targetOffset == -180){
                if(Input.GetKeyDown(KeyCode.W)){
                    targetOffset = -90;
                }
            }else{
                if(Input.GetKeyDown(KeyCode.W)){
                    nowInteractingWith = "ControlPanel";
                    SwitchToVirtualCamera(controlPanelCam);
                }
            }


        if(Input.GetKeyDown(KeyCode.D)){
            targetOffset = 0;
        }

        if(Input.GetKeyDown(KeyCode.A)){
            targetOffset = -180;
        }

        if(Input.GetKeyDown(KeyCode.S)){
                targetOffset = 90;
            }

        }else{
            if(Input.GetKeyDown(KeyCode.S)){
                SwitchToMainCamera();
                nowInteractingWith = "";
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                crosshair.enabled = true;
            }
        }}else{
            if(Input.GetKey(KeyCode.W)){
                GameController.Instance.SwitchModeHallway(true);
            }

            if(Input.GetKeyDown(KeyCode.S)){
                targetOffset = -90;
            }
        }

        /* MOUSE MODE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lookDistance, ~ignoreLayer) && !GameController.Instance.IsGamePaused())
        {
            CinemachineVirtualCamera foundCamera = hit.transform.GetComponentInChildren<CinemachineVirtualCamera>(true);
            if (foundCamera != null)
            {
                string crosshairText = "Use";
                crosshair.text = crosshairText;
                if(Input.GetKeyDown(KeyCode.E) | Input.GetKeyDown(KeyCode.Mouse0)){
                    
                    if (nowInteractingWith != "")
                    {
                        nowInteractingWith = "";
                        SwitchToMainCamera();
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        crosshair.enabled = true;
                    }
                    else
                    {
                        if(hit.transform.name == "ControlPanel"){
                            nowInteractingWith = "ControlPanel";
                        }
                        SwitchToVirtualCamera(foundCamera);
                    }
                }
            }else if (hit.transform.parent != null && hit.transform.parent.GetComponent<DoorController>() != null)
            {
                HandleDoorInteraction(hit);
            }
        }
        else
        {
            ResetCrosshair();
        }
        */
    }



    void HandleDoorInteraction(RaycastHit hit)
    {
        crosshairText = "Go";
        crosshair.text = crosshairText;
    }

    void ResetCrosshair()
    {
        crosshairText = ""; // No interaction text
        crosshair.text = crosshairText;
    }


    public void SwitchToMainCamera()
    {
        if (GameController.Instance.activeVirtualCamera != null)
        {
            GameController.Instance.activeVirtualCamera.gameObject.SetActive(false);
        }
        GameController.Instance.activeVirtualCamera = virtualCamera;
        GameController.Instance.activeVirtualCamera.gameObject.SetActive(true);
        isUsingVirtualCamera = false;
       // mainCamera.transform.position = camPosSave;
    }

    public void SwitchToVirtualCamera(CinemachineVirtualCamera vCam)
    {
        //camPosSave = mainCamera.transform.position;
        if (GameController.Instance.activeVirtualCamera != null)
        {
            GameController.Instance.activeVirtualCamera.gameObject.SetActive(false);
        }

        GameController.Instance.activeVirtualCamera = vCam;
        GameController.Instance.activeVirtualCamera.gameObject.SetActive(true);
        isUsingVirtualCamera = true;
    }
}
