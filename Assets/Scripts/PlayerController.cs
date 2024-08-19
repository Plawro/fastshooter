using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]

/*

Documentation for Player (ctrl+f)
##1 Components and variables
##2 Movement
##3 Camera
##4 Bobbing and camera tilting

*/

public class PlayerController : MonoBehaviour
{
    //##1 Components and variables
    [Header("Components")]
    public GameObject playerCamera;
    public Rigidbody rb;
    private CharacterController characterController;


    [Header("User settings")]
    // PRE-SET, DOESNT CHANGE INGAME
    // Movement
    float defWalkSpeed = 5f;
    float defRunSpeed = 12f;
    float maxSpeed = 12f;
    float acceleration = 80f;
    float deceleration = 80f;
    float jumpPower = 8f;
    public float gravity = 25f;
    private Vector3 moveDirection = Vector3.zero;

    // Camera & body
    float lookXLimit = 80f;
    private float rotationX = 0;
    float defaultHeight = 2f;
    float crouchHeight = 1f;
    float crouchSpeed = 3f;

    // TEMP VARS
    float walkSpeed;
    float runSpeed;
    private float accumulatedRotation;
    private float curSpeedX;
    private float curSpeedY;
    public bool canMove = true;
    Vector3 restPosition;
    float bobSpeed = 10f;
    float defaultCameraY;
    Vector3 newPosition = new Vector3(0,0.7f,0);

    private float timer = Mathf.PI / 2;

    float rotationZ;
    bool canStartBobbing;
    float initialCameraY;

    // SETTINGS
    public float lookSpeed = 2f;
    float bobAmount = 0.15f;

    void Start() // Prepares character for the game
    {
        defaultCameraY = 0.7f;
        playerCamera.transform.localPosition = new Vector3(0, 0.7f, 0);
        walkSpeed = defWalkSpeed;
        runSpeed = defRunSpeed;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    { //##2 Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Running
        bool isRunning = Input.GetKey(KeyCode.LeftShift); // Next 2 lines slowly match running speed
        float targetSpeedX = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical");
        float targetSpeedY = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal");
        
        // Main movement control
        curSpeedX = Mathf.MoveTowards(curSpeedX, targetSpeedX, (isRunning ? acceleration : deceleration) * Time.deltaTime);
        curSpeedY = Mathf.MoveTowards(curSpeedY, targetSpeedY, (isRunning ? acceleration : deceleration) * Time.deltaTime);
        curSpeedX = Mathf.Clamp(curSpeedX, -maxSpeed, maxSpeed);
        curSpeedY = Mathf.Clamp(curSpeedY, -maxSpeed, maxSpeed);
        float movementDirectionY = moveDirection.y;
        if(canMove){
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        }

        // Jump controller
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // "Synthetic" gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Crouch control
        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed; // Sets new walk & run speeds for crouch mode
            runSpeed = crouchSpeed; // How we deal with running in crouch mode :D, DONT REDUCE STAMINA (if there is any soon)
        }
        else
        {
            characterController.height = defaultHeight; // Reduce to default
            walkSpeed = defWalkSpeed;
            runSpeed = defRunSpeed;
        }

        // Makes the character actually move
        if(canMove){
            characterController.Move(moveDirection * Time.deltaTime);
        }else{
            characterController.Move(new Vector3(0, 0, 0));
        }




        //##3 Camera
        // Camera rotation (and body rotation)
                    rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            if(canMove){
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, rotationZ);
            }

        if(canMove){
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
            






        // ##4 Bobbing and camera tilting (that's all)



        // IGNORE
        /* ROTATION BY SPEED
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, rb.velocity.magnitude);
*/
/* ROTATION BY PRESSED A / D
float targetZRotation = Input.GetAxis("Horizontal") < 0 ? -maxTiltAngle : maxTiltAngle;
        float smoothedZRotation = Mathf.Lerp(playerCamera.transform.localRotation.eulerAngles.z, targetZRotation, tiltSmoothing);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, Input.GetAxis("Horizontal") < 0 ? -maxTiltAngle : maxTiltAngle);
*/




// Camera movement by jump / mid air velocity
float smoothSpeed = 8.0f;
if (!characterController.isGrounded) {
    playerCamera.transform.localPosition = Vector3.Lerp(
        playerCamera.transform.localPosition,
        new Vector3(
            playerCamera.transform.localPosition.x,
            Mathf.Clamp(newPosition.y + rb.velocity.y / 10, 0.2f, 1.2f),
            playerCamera.transform.localPosition.z
        ),
        smoothSpeed * Time.deltaTime * 3
    );
} else { // Move the camera back when on ground
    newPosition.y = 0.7f + Mathf.Abs((Mathf.Sin(timer) * bobAmount));
playerCamera.transform.localPosition = Vector3.Lerp(
    playerCamera.transform.localPosition,
    new Vector3(
        playerCamera.transform.localPosition.x,
        newPosition.y,
        playerCamera.transform.localPosition.z
    ),
    smoothSpeed * Time.deltaTime
);
}




        // Main camera bobbing controller (movement based)
        if (canMove)
        {
            float targetRotation = Input.GetAxis("Horizontal") * -100 * Time.deltaTime * lookSpeed; // -<number> changes horizontal value multiplication
            accumulatedRotation = Mathf.Lerp(accumulatedRotation, targetRotation, 0.08f); // Smoothness
            rotationZ = Mathf.Clamp(accumulatedRotation, -6, 6); // Maximum rotation
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 && characterController.isGrounded)
        {
            if (!canStartBobbing)
            {
                initialCameraY = playerCamera.transform.localPosition.y;
                canStartBobbing = true;
            }
            timer += bobSpeed * Time.deltaTime;
            newPosition = new Vector3(
                Mathf.Cos(timer) * bobAmount,
                initialCameraY + Mathf.Abs((Mathf.Sin(timer) * bobAmount)),
                restPosition.z
            );
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                newPosition,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            canStartBobbing = false;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                new Vector3(
                    playerCamera.transform.localPosition.x,
                    newPosition.y ,
                    playerCamera.transform.localPosition.z
                ),
                smoothSpeed * Time.deltaTime
            );
        }
             
            if (timer > Mathf.PI * 2) // Resets the "animation timer" for bobbing
            {
                timer = 0;
            }
            }
            
        }


}