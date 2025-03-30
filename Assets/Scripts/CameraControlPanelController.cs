using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;

public class CameraControlPanelController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float smoothing = 8.0f;
    [SerializeField] private Vector2 horizontalClamp = new Vector2(-60, 60);
    [SerializeField] private Vector2 verticalClamp = new Vector2(-80, 80);
    [SerializeField] private GameObject flashLight;

    private Vector2 targetRotation;
    private Vector2 currentRotation;
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
    [SerializeField] AudioClip flashSound;
    Coroutine flashCoroutine;

    public string nowInteractingWith = "";


    [SerializeField] CinemachineVirtualCamera controlPanelCam;
    [SerializeField] Drift driftController;

    private float holdTimer = 0f;
    private bool isHoldingLeft = false;
    private bool isHoldingRight = false;
    private const float holdTimeRequired = 2f; // 2 seconds to reach 100%
    

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
        crosshair.text = "";

        nowInteractingWith = "";
    }

    void Update()
    {
        crosshair.transform.position = new Vector2(Input.mousePosition.x + 200,Input.mousePosition.y - 50);

        // Get cursor position as a percentage of the screen (instead of adding position)
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
        
        if(targetOffset == -90 ||targetOffset == 0 || targetOffset == -180){
        if(nowInteractingWith == ""){

            if(targetOffset == 0 || targetOffset == -180){
                if(Input.GetKeyDown(KeyCode.W) && GameController.Instance.canMove){
                    targetOffset = -90;
                }
            }else{
                if(Input.GetKeyDown(KeyCode.W) && GameController.Instance.canMove){
                    nowInteractingWith = "ControlPanel";
                    GameController.Instance.exitScreenButton.SetActive(true);
                    SwitchToVirtualCamera(controlPanelCam);
                }
            }

        if(Input.GetKeyDown(KeyCode.D) && GameController.Instance.canMove && flashCoroutine == null){
            if((leftHand.childCount > 0 && leftHand.GetChild(0).name == "Flash") | (rightHand.childCount > 0 && rightHand.GetChild(0).name == "Flash")){
                targetOffset = 0;
                flashCoroutine = StartCoroutine(Flash(0));
            }
        }

        if(Input.GetKeyDown(KeyCode.A) && GameController.Instance.canMove && flashCoroutine == null){
            if((leftHand.childCount > 0 && leftHand.GetChild(0).name == "Flash") | (rightHand.childCount > 0 && rightHand.GetChild(0).name == "Flash")){
                targetOffset = -180;
                flashCoroutine = StartCoroutine(Flash(-180));
            }
        }

        if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove && flashCoroutine == null){
                targetOffset = 90;
            }

        }
        
        }else{
            if(Input.GetKey(KeyCode.W) && GameController.Instance.canMove && flashCoroutine == null){
                GameController.Instance.SwitchModeHallway(true, false);
            }

            if(Input.GetKeyDown(KeyCode.S) && GameController.Instance.canMove && flashCoroutine == null){
                targetOffset = -90;
            }
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lookDistance, ~ignoreLayer) && !GameController.Instance.IsGamePaused())
        {
            CinemachineVirtualCamera foundCamera = hit.transform.GetComponentInChildren<CinemachineVirtualCamera>(true);
            if (hit.transform.name == "ControlPanelCapsuleHolder") {
            bool hasLeftCapsule = leftHand.childCount > 0 && leftHand.GetChild(0).GetComponent<DataCapsule>();
            bool hasRightCapsule = rightHand.childCount > 0 && rightHand.GetChild(0).GetComponent<DataCapsule>();

            if (hasLeftCapsule || hasRightCapsule) {
                if (isHoldingLeft || isHoldingRight) {
                    // Show inserting progress
                    float percentage = Mathf.Clamp((holdTimer / holdTimeRequired) * 100, 0, 100);
                    crosshairText = $"Inserting {percentage:F0}%";
                    
                    if (percentage >= 100) {
                        if (isHoldingLeft) {
                            InsertCapsule(leftHand, hit.transform.gameObject);
                        } else if (isHoldingRight) {
                            InsertCapsule(rightHand, hit.transform.gameObject);
                        }
                    }
                } else {
                    crosshairText = "Hold " + (hasLeftCapsule ? "Left" : "") + (hasRightCapsule && hasLeftCapsule ? " or " : "") + (hasRightCapsule ? "Right" : "") + " click to insert " +
                        (hasLeftCapsule ? leftHand.GetChild(0).name : "") +
                        (hasRightCapsule && hasLeftCapsule ? " or " : "") +
                        (hasRightCapsule ? rightHand.GetChild(0).name : "");
                }
                crosshair.text = crosshairText;
            }

            // Check for input
            if (hasLeftCapsule && Input.GetKey(KeyCode.Mouse0)) {
                isHoldingLeft = true;
                holdTimer += Time.deltaTime;
            } else if (hasRightCapsule && Input.GetKey(KeyCode.Mouse1)) {
                isHoldingRight = true;
                holdTimer += Time.deltaTime;
            } else {
                // Reset when key is released
                isHoldingLeft = false;
                isHoldingRight = false;
                holdTimer = 0f;
            }
            }else if(hit.transform.name == "Datacapsule")
            {
                HandlePickupable(hit);
            }else
            {
                ResetCrosshair();
            }
        }
    }

    void InsertCapsule(Transform hand, GameObject hit) {
        Transform capsule = hand.GetChild(0); // Get the capsule
        capsule.gameObject.layer = 0;
        foreach (Transform child in capsule)
        {
            child.gameObject.layer = 0;
        }
        capsule.parent = hit.transform; // Set the new parent
        // Ensure the capsule is properly positioned
        capsule.localPosition = Vector3.zero;
        capsule.localRotation = Quaternion.Euler(90, 0, 90);
        
        // Reset variables after inserting
        holdTimer = 0f;
        isHoldingLeft = false;
        isHoldingRight = false;
    }

    void ResetCrosshair()
    {
        crosshairText = ""; // No interaction text
        crosshair.text = crosshairText;
    }

    IEnumerator Flash(int rotation){
        GameController.Instance.PlaySound(flashSound);
        yield return new WaitForSeconds(0.2f);
        flashLight.SetActive(true);
        driftController.Flash(rotation);
        yield return new WaitForSeconds(0.4f);
        flashLight.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        targetOffset = -90;
        flashCoroutine = null;
        if(leftHand.childCount > 0 && leftHand.GetChild(0).name == "Flash"){
            Destroy(leftHand.GetChild(0).gameObject);
        }else if(rightHand.childCount > 0 && rightHand.GetChild(0).name == "Flash"){
            Destroy(rightHand.GetChild(0).gameObject);
        }
        yield return null;
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
        obj.localRotation = Quaternion.Euler(0, 180, 0);
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
