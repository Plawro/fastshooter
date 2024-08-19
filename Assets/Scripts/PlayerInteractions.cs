using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerInteractions : MonoBehaviour
{
    public PlayerController playerController;
    public Camera mainCamera;
    public float lookDistance = 5f;

    private CinemachineVirtualCamera activeVirtualCamera;
    private bool isUsingVirtualCamera = false;

    public LayerMask objectLayer;
    public LayerMask ignorePlayerLayer;

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
                        SwitchToVirtualCamera(foundCamera);
                    }
                }
            }
        }
        else
        {
            Debug.Log("No object detected in raycast");
        }
    }

private Vector3 camPosSave;
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
        
    }
}
