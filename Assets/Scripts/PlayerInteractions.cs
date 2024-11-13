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

    void Start(){
        crosshair.gameObject.SetActive(true);
        crosshair.text = crosshairSymbol;
    }

    void Update()
{
    Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, lookDistance, ~ignoreLayer))
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
                        SwitchToMainCamera();
                    }
                    else
                    {
                        if (hit.transform.name == "Screen1")
                        {
                            powerPlantController = hit.transform.parent.GetComponent<PowerPlantController>();
                        }
                        SwitchToVirtualCamera(foundCamera);
                    }
                }
                

            }else if(hit.transform.name == "powerswitch"){
                if(Input.GetKeyDown(KeyCode.E)){
                    crosshairText = "Use";
                    crosshair.text = crosshairText;

                    hit.transform.parent.transform.GetComponent<TowerController>().moveSpeed = hit.transform.parent.transform.GetComponent<TowerController>().fixSpeed;
                    hit.transform.parent.transform.GetComponent<TowerController>().MoveAntennaToZero();
                }
                
            }else if(hit.transform.name == "DC uploader"){
                if(Input.GetKeyDown(KeyCode.E)){
                    Debug.Log("Activated");
                }

                if(leftHand.childCount > 0 || rightHand.childCount > 0){//    !!!!    Later add what can be placed in    !!!!
                    crosshairText = "Put " + (leftHand.childCount > 0 ? leftHand.GetChild(0).name : "") + (rightHand.childCount > 0 && leftHand.childCount > 0 ? " or " : "") + (rightHand.childCount > 0 ? rightHand.GetChild(0).name : "") + " in Uploader";
                    crosshair.text = crosshairText;
                }
                
                if(Input.GetKeyDown(KeyCode.Mouse0) && leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>()){
                    leftHand.GetChild(0).transform.parent = hit.transform;
                    hit.transform.GetChild(2).transform.localPosition = hit.transform.GetComponent<DCUploaderController>().capsulePos; // First 2 childs are model parts
                }

                if(Input.GetKeyDown(KeyCode.Mouse1) && rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>()){
                    rightHand.GetChild(0).transform.parent = hit.transform;
                    hit.transform.GetChild(2).transform.localPosition = hit.transform.GetComponent<DCUploaderController>().capsulePos;
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
                if(Input.GetKeyDown(KeyCode.Mouse0)){
                    hit.transform.position = leftHand.position;
                    hit.transform.parent = leftHand;
                }

                if(Input.GetKeyDown(KeyCode.Mouse1)){
                    hit.transform.position = rightHand.position;
                    hit.transform.parent = rightHand;
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

    if (isUsingVirtualCamera && powerPlantController != null)
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
}


    void SwitchToVirtualCamera(CinemachineVirtualCamera vCam)
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

    void SwitchToMainCamera()
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
