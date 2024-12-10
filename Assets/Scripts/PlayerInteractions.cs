using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using UnityEditor;

public class PlayerInteractions : MonoBehaviour
{
    public PlayerController playerController;
    public PowerPlantController powerPlantController;
    public StatsScreen statsScreen;
    public Camera mainCamera;
    public float lookDistance = 5f;

    private CinemachineVirtualCamera activeVirtualCamera;
    private bool isUsingVirtualCamera = false;

    public LayerMask ignoreLayer;
    public TextMeshProUGUI crosshair;
    string crosshairText = "Interact";
    public string crosshairSymbol = "+";

    public Transform leftHand;
    public Transform rightHand;

    public string nowInteractingWith = "";

    void Start(){
        crosshair.gameObject.SetActive(true);
        crosshair.text = crosshairSymbol;
    }

    void Update()
    {
    Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, lookDistance, ~ignoreLayer) && GameController.Instance.IsGamePaused() == false)
    {
        if (hit.transform.CompareTag("Interactable") && hit.transform.name != "DoubleSlideDoor") // Doors may be interactable, but automatical doors may cause problems :( and don't need interaction
        {
            CinemachineVirtualCamera foundCamera = hit.transform.GetComponentInChildren<CinemachineVirtualCamera>(true);
            Debug.DrawRay(ray.origin, ray.direction * lookDistance, Color.red);
            if (foundCamera != null)
            {
                
                string crosshairText = "Use";
                crosshair.text = crosshairText;
                //crosshair.color = Color.blue; 

                if(Input.GetKeyDown(KeyCode.E)){
                    if (isUsingVirtualCamera)
                    {
                        nowInteractingWith = "";
                        SwitchToMainCamera();
                    }
                    else
                    {
                        if (hit.transform.name == "PowerPlantDisplay")
                        {
                            powerPlantController = hit.transform.parent.GetComponent<PowerPlantController>();
                            nowInteractingWith = "PowerPlantDisplay";
                        }else if(hit.transform.name == "LockingDisplay"){
                            nowInteractingWith = "LockingDisplay";
                        }else if(hit.transform.name == "StatsScreen"){
                            nowInteractingWith = "StatsScreen";
                        }else if(hit.transform.name == "ControlPanel"){
                            nowInteractingWith = "ControlPanel";
                        }
                        SwitchToVirtualCamera(foundCamera);
                    }
                }
                

            }else if (hit.transform.name == "powerswitch") {
                if (hit.transform.parent.transform.GetComponent<TowerController>().isAntennaBroken) {
                    crosshairText = "Hold to turn tower on";
                    crosshair.text = crosshairText;
                    if (Input.GetKey(KeyCode.E)) {
                        var towerController = hit.transform.parent.transform.GetComponent<TowerController>();
                        towerController.RepairAntenna();
                    }else{
                        var towerController = hit.transform.parent.transform.GetComponent<TowerController>();
                        towerController.StopRepairingAntenna();
                    }
                } else {
                    crosshairText = "Turn tower off";
                    crosshair.text = crosshairText;
                    if (Input.GetKeyDown(KeyCode.E)) {
                        statsScreen.BreakAntenna();
                    }
                }
            }else if(hit.transform.name == "DC uploader"){
                if(Input.GetKeyDown(KeyCode.E)){
                    Debug.Log("Activated");
                }

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
                
            }else if(hit.transform.name == "DataCapsuleBasket"){
                if((leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()) || (rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>())){
                    crosshairText = "Throw " + (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " into trash";
                    crosshair.text = crosshairText;
                }
                
                if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()){
                    Destroy(leftHand.GetChild(0).gameObject);
                }

                if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>()){
                    Destroy(rightHand.GetChild(0).gameObject);
                }
                
            }else if(hit.transform.name == "ControlPanelCapsuleHolder"){
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
                
            }else if (hit.transform.parent != null && hit.transform.parent.GetComponent<DoorController>() != null){ // Any interactable door, to fix "curtain bug" - add option for doorcontroller and istrigger collider on main object - when opened
                                                                            //for example, player doesnt need to look at the object (side of curtains where the collider is) but in the centre where will always be one collider
                crosshairText = hit.transform.parent.GetComponent<DoorController>().isOpen ? "Close" : "Open";
                crosshair.text = crosshairText;
                    
                if(Input.GetKeyDown(KeyCode.F)){
                    hit.transform.parent.transform.GetComponent<DoorController>().StartCoroutine(hit.transform.parent.transform.GetComponent<DoorController>().OpenPartialDoor());;
                }

                if(Input.GetKeyDown(KeyCode.E)){
                    hit.transform.parent.transform.GetComponent<DoorController>().ChangeDoorMode();
                } 
            }else{ // Pick up-able items
                crosshairText = "Pick up " + hit.transform.name;
                crosshair.text = crosshairText;
                if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount < 1){
                    hit.transform.position = leftHand.position;
                    hit.transform.parent = leftHand;
                    leftHand.transform.GetChild(0).localRotation = new Quaternion(0,180,0,0);
                    if(leftHand.GetChild(0).transform.GetComponent<DataCapsule>().mode == 1){ //Capsule is in mode 1 only when uploading/downloading,.. whenever we touch it when in mode 1, it goes red
                        leftHand.GetChild(0).transform.GetComponent<DataCapsule>().ChangeMode(3);
                    }

                    if(leftHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == hit.transform.name){ // This allows to tell uploader we took the capsule (we don't need to use Update to keep checking :OO)
                        hit.transform.GetComponent<DCUploaderController>().CheckCapsule();
                    }
                }

                if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount < 1){
                    hit.transform.position = rightHand.position;
                    hit.transform.parent = rightHand;
                    rightHand.transform.GetChild(0).localRotation = new Quaternion(0,180,0,0);
                    if(rightHand.GetChild(0).transform.GetComponent<DataCapsule>().mode == 1){ //Capsule is in mode 1 only when uploading/downloading,.. whenever we touch it when in mode 1, it goes red
                        rightHand.GetChild(0).transform.GetComponent<DataCapsule>().ChangeMode(3);
                    }
                    
                    if(rightHand.GetChild(0).GetComponent<DataCapsule>() && GameController.Instance.DCuploader.CheckCapsule() == hit.transform.name){
                        hit.transform.GetComponent<DCUploaderController>().CheckCapsule();
                    }
                }
            }
        }
        else
        {
            //crosshair.color = Color.white;
            crosshairText = crosshairSymbol;
            crosshair.text = crosshairText;
        }
    }
    else
    {
        //crosshair.color = Color.white;
        crosshairText = crosshairSymbol;
        crosshair.text = crosshairText;
    }

    if (isUsingVirtualCamera && powerPlantController != null && nowInteractingWith == "PowerPlantDisplay")
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

    if (isUsingVirtualCamera && nowInteractingWith == "ControlPanel")
    {
        
    }
}


    public void SwitchToVirtualCamera(CinemachineVirtualCamera vCam)
    {
        //camPosSave = mainCamera.transform.position;
        if (activeVirtualCamera != null)
        {
            activeVirtualCamera.gameObject.SetActive(false);
        }

        activeVirtualCamera = vCam;
        activeVirtualCamera.gameObject.SetActive(true);
        isUsingVirtualCamera = true;

        
            playerController.canMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            crosshair.enabled = false;
        
    }

    Vector3 lastKnownCamPos;
    public void SaveLastKnownCameraPos(){
        lastKnownCamPos = activeVirtualCamera.transform.position;
    }

    //Shaking camera function (jumpscare)
    public IEnumerator ShakeCamera(float ammount){
        while(activeVirtualCamera != null){
            activeVirtualCamera.transform.position = lastKnownCamPos + Random.insideUnitSphere * ammount;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SwitchToMainCamera()
    {
        if (activeVirtualCamera != null)
        {
            activeVirtualCamera.gameObject.SetActive(false);
        }
        activeVirtualCamera = null;
        isUsingVirtualCamera = false;
       // mainCamera.transform.position = camPosSave;

        
            playerController.canMove = true;
             Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crosshair.enabled = true;
        
        powerPlantController = null;
    }
}
