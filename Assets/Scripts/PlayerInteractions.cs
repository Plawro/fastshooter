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
    public Image crosshair;

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        int combinedMask = objectLayer.value | ~ignorePlayerLayer.value;

        if (Physics.Raycast(ray, out hit, lookDistance, combinedMask))
        {

            CinemachineVirtualCamera foundCamera = hit.transform.GetComponentInChildren<CinemachineVirtualCamera>(true);

            if (foundCamera != null)
            {

                if (Input.GetKeyDown(KeyCode.E))
                {
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
            }
        }
        else
        {
            // No objects detected
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
