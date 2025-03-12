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
    public float lookDistance = 10f;

    private bool isUsingVirtualCamera = false;

    public LayerMask ignoreLayer;
    public TextMeshProUGUI crosshair;
    string crosshairText = "[E] Interact";
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
                }
                

            }else if (hit.transform.name == "powerswitch") {
                if (hit.transform.parent.transform.GetComponent<TowerController>().isAntennaBroken) {
                    crosshairText = "Hold [E] to turn tower on";
                    crosshair.text = crosshairText;
                    if (Input.GetKey(KeyCode.E)) {
                        var towerController = hit.transform.parent.transform.GetComponent<TowerController>();
                        towerController.RepairAntenna();
                    }else{
                        var towerController = hit.transform.parent.transform.GetComponent<TowerController>();
                        towerController.StopRepairingAntenna();
                    }
                } else {
                    crosshairText = "[E] Turn tower off";
                    crosshair.text = crosshairText;
                    if (Input.GetKeyDown(KeyCode.E)) {
                        statsScreen.BreakAntenna();
                    }
                }
            }else if (hit.transform.name == "Van") {
                crosshairText = "[E] Interact";
                crosshair.text = crosshairText;
                if (Input.GetKey(KeyCode.E)) {
                    if(!GameController.Instance.vanHereAgain){
                        GameController.Instance.RemoveTutorial();
                    }else{
                        GameController.Instance.touchedVan = true;
                    }
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
            if (crosshair.text != crosshairText) 
            crosshair.text = crosshairText;
        }
    }
    else
    {
        //crosshair.color = Color.white;
        crosshairText = crosshairSymbol;
        if (crosshair.text != crosshairText) 
        crosshair.text = crosshairText;
    }

}


    private void OnTriggerEnter(Collider other){
        if(other == GameController.Instance.walkInsideCollider){
            GameController.Instance.SwitchModeHallway(true);
        }
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

        
            playerController.canMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            crosshair.enabled = false;
        
    }

    public Vector3 lastKnownCamPos;
    public void SaveLastKnownCameraPos(){
        lastKnownCamPos = GameController.Instance.activeVirtualCamera.transform.position;
    }

    //Shaking camera function (jumpscare)
    public IEnumerator ShakeCamera(float ammount){
        while(GameController.Instance.activeVirtualCamera != null){
            GameController.Instance.activeVirtualCamera.transform.position = lastKnownCamPos + Random.insideUnitSphere * ammount;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SwitchToMainCamera()
    {
        if (GameController.Instance.activeVirtualCamera != null)
        {
            GameController.Instance.activeVirtualCamera.gameObject.SetActive(false);
        }
        GameController.Instance.activeVirtualCamera = null;
        isUsingVirtualCamera = false;
       // mainCamera.transform.position = camPosSave;

        
            playerController.canMove = true;
             Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            crosshair.enabled = true;
        
        powerPlantController = null;
    }
}
