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


    [SerializeField] PowerPlantController powerPlantController;
    [SerializeField] StatsScreen statsScreen;
    [SerializeField] Camera mainCamera;
    float lookDistance = 10f;

    private bool isUsingVirtualCamera = false;

    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] TextMeshProUGUI crosshair;
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
        crosshair.transform.position = new Vector2(Input.mousePosition.x + 200,Input.mousePosition.y - 30);
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


        if(targetOffset == -180){
        if(nowInteractingWith == ""){

        
        if(Input.GetKeyDown(KeyCode.W) && GameController.Instance.canMove){
            nowInteractingWith = "LockingDisplay";
            GameController.Instance.exitScreenButton.SetActive(true);
            SwitchToVirtualCamera(lockingCam);
        }

        if(Input.GetKeyDown(KeyCode.A) && GameController.Instance.canMove){
            nowInteractingWith = "StatsScreen";
            GameController.Instance.exitScreenButton.SetActive(true);
            SwitchToVirtualCamera(statsCam);
        }

        if(Input.GetKeyDown(KeyCode.D) && GameController.Instance.canMove){

        }
        if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove){
                targetOffset = 0;
            }

        }
        
        }else{
            if(Input.GetKeyDown(KeyCode.W) && GameController.Instance.canMove){
                GameController.Instance.SwitchModeHallway(true, true);
            }
            
            if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove){
                targetOffset = -180;
            }
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lookDistance, ~ignoreLayer) && !GameController.Instance.IsGamePaused())
        {
            CinemachineVirtualCamera foundCamera = hit.transform.GetComponentInChildren<CinemachineVirtualCamera>(true);
            if(hit.transform.name == "ControlPanelCapsuleHolder"){
                    if((leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()) || (rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>())){
                        crosshairText = "Put " + (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " back";
                        crosshair.text = crosshairText;
                    }
                    
                    if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()){
                        leftHand.GetChild(0).gameObject.layer = 0;
                        foreach (Transform child in leftHand.GetChild(0).transform)
                        {
                            child.gameObject.layer = 0;
                        }
                        leftHand.GetChild(0).transform.parent = hit.transform;
                        hit.transform.GetChild(0).transform.localPosition = new Vector3(0,0,0);
                        hit.transform.GetChild(0).transform.localRotation = Quaternion.Euler(90, 0, -90);
                    }

                    if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>()){
                        rightHand.GetChild(0).gameObject.layer = 0;
                        foreach (Transform child in rightHand.GetChild(0).transform)
                        {
                            child.gameObject.layer = 0;
                        }
                        rightHand.GetChild(0).transform.parent = hit.transform;
                        hit.transform.GetChild(0).transform.localPosition = new Vector3(0,0,0);
                        hit.transform.GetChild(0).transform.localRotation = Quaternion.Euler(90, 0, -90);
                    }
            }else if(hit.transform.name == "ControlPanelFlashHolder"){
                    if((leftHand.childCount > 0 && leftHand.GetChild(0).transform.name == "Flash") || (rightHand.childCount > 0 && rightHand.GetChild(0).transform.name == "Flash")){
                        crosshairText = "Put " + (leftHand.childCount > 0 && leftHand.transform.name == "Flash" ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 && rightHand.transform.name == "Flash" ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " back";
                        crosshair.text = crosshairText;
                    }
                    
                    if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).transform.name == "Flash"){
                        leftHand.GetChild(0).gameObject.layer = 0;
                        leftHand.GetChild(0).transform.parent = hit.transform;
                        hit.transform.GetChild(0).transform.localPosition = new Vector3(0,0,0);
                        hit.transform.GetChild(0).transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    }

                    if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).transform.name == "Flash"){
                        rightHand.GetChild(0).gameObject.layer = 0;
                        rightHand.GetChild(0).transform.parent = hit.transform;
                        hit.transform.GetChild(0).transform.localPosition = new Vector3(0,0,0);
                        hit.transform.GetChild(0).transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    }
            }else if (hit.transform.name == "DC uploader")
            {
                HandleDCUploader(hit);
            }
            else if (hit.transform.name == "DataCapsuleBasket")
            {
                HandleDataCapsuleBasket(hit);
            }
            else if(hit.transform.CompareTag("Interactable") && (hit.transform.name == "Datacapsule" || hit.transform.name == "Flash"))
            {
                HandlePickupable(hit);
            }else{
                ResetCrosshair();
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

    void HandleDCUploader(RaycastHit hit)
    {
        if((leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty") || (rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty")){
                    crosshairText = "Put " + (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " in Uploader";
                    crosshair.text = crosshairText;
                }
                
                if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty"){
                    leftHand.GetChild(0).gameObject.layer = 0;
                    foreach (Transform child in leftHand.GetChild(0).transform)
                        {
                            child.gameObject.layer = 0;
                        }
                    leftHand.GetChild(0).transform.parent = hit.transform;
                    hit.transform.GetChild(2).transform.localPosition = hit.transform.GetComponent<DCUploaderController>().capsulePos; // First 2 children are model parts
                    hit.transform.GetChild(2).transform.localRotation = new Quaternion(0,0,0,0);
                    hit.transform.GetComponent<DCUploaderController>().CheckCapsule();
                }

                if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == "Empty"){
                    rightHand.GetChild(0).gameObject.layer = 0;
                    foreach (Transform child in rightHand.GetChild(0).transform)
                        {
                            child.gameObject.layer = 0;
                        }
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
        obj.gameObject.layer = 17;
        foreach (Transform child in obj.transform)
        {
            child.gameObject.layer = 17;
        }
        obj.position = hand.position;
        obj.parent = hand;
        if(obj.transform.name == "Flash"){
            obj.localRotation = Quaternion.Euler(-60, 0, 0);
        }else{
            obj.localRotation = Quaternion.Euler(0, 180, 0);
        }

        statsScreen.ResetAll();
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
        powerPlantController = null;
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
