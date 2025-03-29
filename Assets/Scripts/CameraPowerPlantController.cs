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


    [SerializeField] PowerPlantController powerPlantController;
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
    [SerializeField] CinemachineVirtualCamera surveillanceCam;

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
        crosshair.transform.position = new Vector2(Input.mousePosition.x + 200,Input.mousePosition.y - 50);
        // Check for "look back" key (S)
        //if (Input.GetKeyDown(KeyCode.S))
        //{
            //targetOffset = targetOffset == 0 ? 180 : 0; // Toggle between 0 and 180 degrees
        //}

        // Get cursor position as a percentage of the screen
        Vector2 cursorPosition = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        // Invert cursor mapping to make movement intuitive
        float invertedX = cursorPosition.x;
        float invertedY = 1f - cursorPosition.y;

        // Map cursor position to rotation range, adding targetOffset for additional rotation
        targetRotation.x = Mathf.Lerp(horizontalClamp.x, horizontalClamp.y, invertedX) + targetOffset;
        targetRotation.y = Mathf.Lerp(verticalClamp.x, verticalClamp.y, invertedY);

        // Smoothly interpolate to the target rotation
        currentRotation = Vector2.Lerp(currentRotation, targetRotation, Time.deltaTime * smoothing);

        // Apply rotation to the camera
        virtualCamera.transform.localRotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        // KEYBOARD MODE
        if(targetOffset == 0){
        if(nowInteractingWith == ""){
        if(Input.GetKeyDown(KeyCode.W) && GameController.Instance.canMove){
            nowInteractingWith = "PowerPlantDisplay";
            GameController.Instance.exitScreenButton.SetActive(true);
            SwitchToVirtualCamera(powerPlantCam);
        }


        if(Input.GetKeyDown(KeyCode.D) && GameController.Instance.canMove){
            nowInteractingWith = "SurveillanceDisplay";
            GameController.Instance.exitScreenButton.SetActive(true);
            SwitchToVirtualCamera(surveillanceCam);
        }

        if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove){
                targetOffset = -180;
            }

        }
        }else{
            if(Input.GetKeyDown(KeyCode.W) && GameController.Instance.canMove){
                GameController.Instance.SwitchModeHallway(true,true);
            }

            if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove){
                targetOffset = 0;
            }
        }

        // MOUSE MODE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        ResetCrosshair();
        

        if (isUsingVirtualCamera && nowInteractingWith == "PowerPlantDisplay")
        {
            HandlePowerPlantControl();
        }
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
