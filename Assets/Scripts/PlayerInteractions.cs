using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerInteractions : MonoBehaviour
{
    public PlayerController playerController;
    public PowerPlantController powerPlantController;
    public Camera mainCamera;
    public float lookDistance = 5f;

    private CinemachineVirtualCamera activeVirtualCamera;
    private bool isUsingVirtualCamera = false;

    public LayerMask objectLayer;
    public LayerMask ignorePlayerLayer;
    public LayerMask ignorePreInteractableLayer;
    public Image crosshair;
    public Image crosshairDot;

    void Start(){
        crosshairDot.gameObject.SetActive(false);
    }

    void Update()
{
    Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
    RaycastHit hit;

    int combinedMask = objectLayer.value | ~ignorePlayerLayer.value | ~ignorePreInteractableLayer.value;

    if (Physics.Raycast(ray, out hit, lookDistance, combinedMask))
    {
        if (hit.transform.CompareTag("Interactable"))
        {
            //crosshair.color = Color.blue; 
            crosshairDot.gameObject.SetActive(true);
            
            CinemachineVirtualCamera foundCamera = hit.transform.GetComponentInChildren<CinemachineVirtualCamera>(true);
            Debug.DrawRay(ray.origin, ray.direction * lookDistance, Color.red);
            if (foundCamera != null)
            {
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
                    hit.transform.parent.transform.GetComponent<TowerController>().moveSpeed = hit.transform.parent.transform.GetComponent<TowerController>().fixSpeed;
                    hit.transform.parent.transform.GetComponent<TowerController>().MoveAntennaToZero();
                }
            }else{ // Not screen or powerswitch, but door
                if(Input.GetKeyDown(KeyCode.F)){
                    hit.transform.parent.transform.GetComponent<DoorController>().StartCoroutine(hit.transform.parent.transform.GetComponent<DoorController>().OpenPartialDoor());;
                }

                if(Input.GetKeyDown(KeyCode.E)){
                    hit.transform.parent.transform.GetComponent<DoorController>().ChangeDoorMode();
                } 
            }
        }
        else
        {
            //crosshair.color = Color.white;
            crosshairDot.gameObject.SetActive(false);
        }
    }
    else
    {
        //crosshair.color = Color.white;
        crosshairDot.gameObject.SetActive(false);
    }

    if (isUsingVirtualCamera && powerPlantController != null)
    {
        if (Input.GetKey(KeyCode.A))
        {
            powerPlantController.addPower(-1f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            powerPlantController.addPower(1f);
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
