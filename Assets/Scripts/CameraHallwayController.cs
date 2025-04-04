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

    [SerializeField] Camera mainCamera;
    [SerializeField] float lookDistance = 5f;

    private bool isUsingVirtualCamera = false;

    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] TextMeshProUGUI crosshair;
    string crosshairText = " ";

    [SerializeField] Transform inventory;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    [SerializeField] string nowInteractingWith = "";


    [SerializeField] CinemachineVirtualCamera controlPanelCam;

    void OnEnable(){ // Just to make sure we are rotated correctly after getting back (not last-known-rotation) :>
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

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lookDistance, ~ignoreLayer) && !GameController.Instance.IsGamePaused())
        {
            // Doesn't need to interact with nothing
        }
        else
        {
            ResetCrosshair();
        }
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
    }

    public void SwitchToVirtualCamera(CinemachineVirtualCamera vCam)
    {
        if (GameController.Instance.activeVirtualCamera != null)
        {
            GameController.Instance.activeVirtualCamera.gameObject.SetActive(false);
        }

        GameController.Instance.activeVirtualCamera = vCam;
        GameController.Instance.activeVirtualCamera.gameObject.SetActive(true);
        isUsingVirtualCamera = true;
    }
}
