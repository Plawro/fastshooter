using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class CameraTowerController : MonoBehaviour
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
    public StatsScreen statsScreen;
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


    [SerializeField] CinemachineVirtualCamera statsCam;
    [SerializeField] CinemachineVirtualCamera lockingCam;

    void OnEnable(){
        targetOffset = -180;
    }

    void Start()
    {
        targetOffset = -180;
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
        
        if(targetOffset == -180){
        if(nowInteractingWith == ""){

        
        if(Input.GetKeyDown(KeyCode.W)){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crosshair.enabled = false;
            nowInteractingWith = "LockingDisplay";
            SwitchToVirtualCamera(lockingCam);
        }

        if(Input.GetKeyDown(KeyCode.A)){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crosshair.enabled = false;
            nowInteractingWith = "StatsScreen";
            SwitchToVirtualCamera(statsCam);
        }

        if(Input.GetKeyDown(KeyCode.D)){

        }
        if(Input.GetKeyDown(KeyCode.S)){
                targetOffset = 0;
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
                targetOffset = -180;
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
                        }else if(hit.transform.name == "LockingDisplay"){
                            Cursor.lockState = CursorLockMode.Locked;
                             Cursor.visible = false;
                            crosshair.enabled = false;
                            nowInteractingWith = "LockingDisplay";
                        }else if(hit.transform.name == "StatsScreen"){
                            Cursor.lockState = CursorLockMode.Locked;
                             Cursor.visible = false;
                            crosshair.enabled = false;
                            nowInteractingWith = "StatsScreen";
                        }else if(hit.transform.name == "ControlPanel"){
                            nowInteractingWith = "ControlPanel";
                        }
                        SwitchToVirtualCamera(foundCamera);
                    }
                }
            }else if (hit.transform.name == "powerswitch")
            {
                HandlePowerSwitch(hit);
            }
            else if (hit.transform.name == "DC uploader")
            {
                HandleDCUploader(hit);
            }
            else if (hit.transform.name == "DataCapsuleBasket")
            {
                HandleDataCapsuleBasket(hit);
            }
            else if (hit.transform.parent != null && hit.transform.parent.GetComponent<DoorController>() != null)
            {
                HandleDoorInteraction(hit);
            }
            else if(hit.transform.CompareTag("Interactable"))
            {
                HandlePickupable(hit);
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

    void HandleInteractable(RaycastHit hit)
    {
        CinemachineVirtualCamera foundCamera = hit.transform.GetComponentInChildren<CinemachineVirtualCamera>(true);
        Debug.DrawRay(hit.point, hit.normal * lookDistance, Color.red);

        if (foundCamera != null)
        {
            crosshairText = "Use";
            crosshair.text = crosshairText;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (isUsingVirtualCamera)
                {
                    nowInteractingWith = "";
                    SwitchToMainCamera();
                }
                else
                {
                    AssignInteraction(hit.transform.name, foundCamera);
                }
            }
        }
    }

    void HandlePowerSwitch(RaycastHit hit)
    {
        var towerController = hit.transform.parent.GetComponent<TowerController>();

        if (towerController.isAntennaBroken)
        {
            crosshairText = "Hold to turn tower on";
            crosshair.text = crosshairText;

            if (Input.GetKey(KeyCode.E))
                towerController.RepairAntenna();
            else
                towerController.StopRepairingAntenna();
        }
        else
        {
            crosshairText = "Turn tower off";
            crosshair.text = crosshairText;

            if (Input.GetKeyDown(KeyCode.E))
                statsScreen.BreakAntenna();
        }
    }

    void HandleDCUploader(RaycastHit hit)
    {
        if((leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty") || (rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty")){
                    crosshairText = "Put " + (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " in Uploader";
                    crosshair.text = crosshairText;
                }
                
                if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty"){
                    leftHand.GetChild(0).transform.parent = hit.transform;
                    hit.transform.GetChild(2).transform.localPosition = hit.transform.GetComponent<DCUploaderController>().capsulePos; // First 2 childs are model parts
                    hit.transform.GetChild(2).transform.localRotation = new Quaternion(0,0,0,0);
                    hit.transform.GetComponent<DCUploaderController>().CheckCapsule();
                }

                if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty"){
                    rightHand.GetChild(0).transform.parent = hit.transform;
                    hit.transform.GetChild(2).transform.localPosition = hit.transform.GetComponent<DCUploaderController>().capsulePos;
                    hit.transform.GetChild(2).transform.localRotation = new Quaternion(0,0,0,0);
                    hit.transform.GetComponent<DCUploaderController>().CheckCapsule();
                }
    }

    void HandleDataCapsuleBasket(RaycastHit hit)
    {
        if((leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()) || (rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>())){
                    crosshairText = "Throw " + (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " into trash";
                    crosshair.text = crosshairText;
                }
                
                if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()){
                    Destroy(leftHand.GetChild(0).gameObject);
                    ResetCrosshair();
                }

                if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>()){
                    Destroy(rightHand.GetChild(0).gameObject);
                    ResetCrosshair();
                }
    }


    void HandleDoorInteraction(RaycastHit hit)
    {
        crosshairText = "Go";
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

        if (obj.GetComponent<DataCapsule>() && obj.GetComponent<DataCapsule>().mode == 1)
        {
            obj.GetComponent<DataCapsule>().ChangeMode(3);
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
