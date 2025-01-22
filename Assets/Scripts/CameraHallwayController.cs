using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class CameraHallwayController : MonoBehaviour
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

    void OnEnable(){ // Just to make sure we are rotated correctly after getting back (not last-know-rotation) :>
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
        
        if(targetOffset == -90){
        if(nowInteractingWith == ""){

        
        if(Input.GetKeyDown(KeyCode.W) && GameController.Instance.canMove){
            GameController.Instance.SwitchModeOutside();
        }

        if(Input.GetKeyDown(KeyCode.A) && GameController.Instance.canMove){
            GameController.Instance.SwitchModeTower();
        }

        if(Input.GetKeyDown(KeyCode.D) && GameController.Instance.canMove){
            GameController.Instance.SwitchModePowerPlant();
        }

        if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove){
                targetOffset = 90;
            }

        }else{
            if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove){
                SwitchToMainCamera();
                nowInteractingWith = "";
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                crosshair.enabled = true;
            }
        }}else{
            if(Input.GetKey(KeyCode.W) && GameController.Instance.canMove){
                GameController.Instance.SwitchModeControlPanel();
            }

            if(Input.GetKeyDown(KeyCode.A) && GameController.Instance.canMove){
            GameController.Instance.SwitchModePowerPlant();
            }

            if(Input.GetKeyDown(KeyCode.D) && GameController.Instance.canMove){
                GameController.Instance.SwitchModeTower();
            }

            if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove){
                targetOffset = -90;
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
