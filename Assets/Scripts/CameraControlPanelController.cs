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
        crosshair.transform.position = new Vector2(Input.mousePosition.x + 100,Input.mousePosition.y - 50);
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

        // MOUSE MODE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lookDistance, ~ignoreLayer) && !GameController.Instance.IsGamePaused())
        {
            print(hit.transform.name);
            if(hit.transform.name == "ControlPanelCapsuleHolder"){
                    if((leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()) || (rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>())){
                        crosshairText = "Put " + (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " into control panel";
                        crosshair.text = crosshairText;
                    }
                    
                    if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()){
                        leftHand.GetChild(0).transform.parent = hit.transform;
                        hit.transform.GetChild(0).transform.localPosition = new Vector3(0,0,0);
                        hit.transform.GetChild(0).transform.localRotation = Quaternion.Euler(90, 0, 90);
                    }

                    if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>()){
                        rightHand.GetChild(0).transform.parent = hit.transform;
                        hit.transform.GetChild(0).transform.localPosition = new Vector3(0,0,0);
                        hit.transform.GetChild(0).transform.localRotation = Quaternion.Euler(90, 0, 90);
                    }
                }else if(hit.transform.name == "Datacapsule")
            {
                HandlePickupable(hit);
            }

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


    void HandlePickupable(RaycastHit hit)
    {
        crosshairText = "Pick up " + hit.transform.name;
        crosshair.text = crosshairText;

        if (Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount < 1)
        {
            PickupObject(hit.transform, leftHand);
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount < 1)
        {
            PickupObject(hit.transform, rightHand);
        }
    }

    void PickupObject(Transform obj, Transform hand)
    {
        obj.position = hand.position;
        obj.parent = hand;
        obj.localRotation = Quaternion.Euler(0, 180, 0);
        /*inventoryText.text = 
        "Inventory:\n"+
        (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "Empty") + 
        " | " + 
        (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "Empty");
        */
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
