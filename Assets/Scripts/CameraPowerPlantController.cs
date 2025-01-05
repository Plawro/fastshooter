using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class CameraPowerPlantController : MonoBehaviour
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


    public PowerPlantController powerPlantController;
    public Camera mainCamera;
    public float lookDistance = 5f;

    private bool isUsingVirtualCamera = false;

    public LayerMask ignoreLayer;
    public TextMeshProUGUI crosshair;
    //public TextMeshProUGUI inventoryText;
    string crosshairText = " ";

    [SerializeField] Transform inventory;

    public string nowInteractingWith = "";


    [SerializeField] CinemachineVirtualCamera powerPlantCam;

    void OnEnable(){
        targetOffset = 0;
    }

    void Start()
    {
        targetOffset = 0;
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
        
        if(targetOffset == 0){
        if(nowInteractingWith == ""){
        if(Input.GetKeyDown(KeyCode.W)){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crosshair.enabled = false;
            nowInteractingWith = "PowerPlantDisplay";
            SwitchToVirtualCamera(powerPlantCam);
        }

        if(Input.GetKeyDown(KeyCode.S)){
                targetOffset = -180;
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
            if(Input.GetKeyDown(KeyCode.W)){
                GameController.Instance.SwitchModeHallway(true);
            }

            if(Input.GetKeyDown(KeyCode.S)){
                targetOffset = 0;
            }
        }

        // MOUSE MODE
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
                        if (hit.transform.name == "PowerPlantDisplay")
                        {
                            powerPlantController = hit.transform.parent.GetComponent<PowerPlantController>();
                            nowInteractingWith = "PowerPlantDisplay";
                        }else if(hit.transform.name == "ControlPanel"){
                            nowInteractingWith = "ControlPanel";
                        }
                        SwitchToVirtualCamera(foundCamera);
                    }
                }
            }
            else if (hit.transform.parent != null && hit.transform.parent.GetComponent<DoorController>() != null)
            {
                HandleDoorInteraction(hit);
            }
        }
        else
        {
            ResetCrosshair();
        }

        if (isUsingVirtualCamera && powerPlantController != null && nowInteractingWith == "PowerPlantDisplay")
        {
            HandlePowerPlantControl();
        }
    }


    void HandleDoorInteraction(RaycastHit hit)
    {
        crosshairText = "Go";
        crosshair.text = crosshairText;
    }

    void AssignInteraction(string name, CinemachineVirtualCamera camera)
    {
        nowInteractingWith = name;
        SwitchToVirtualCamera(camera);
    }

    void ResetCrosshair()
    {
        crosshairText = ""; // No interaction text
        crosshair.text = crosshairText;
    }

    void HandlePowerPlantControl()
    {
        if (Input.GetKey(KeyCode.A))
        {
            powerPlantController.AddPower(-1f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            powerPlantController.AddPower(1f);
        }
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
        powerPlantController = null;
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
