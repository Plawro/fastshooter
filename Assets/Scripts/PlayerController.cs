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
    public AudioSource footstepAudioSource;
    public AudioClip footstepSound;


    [Header("User settings")]
    // PRE-SET, DOESNT CHANGE INGAME
    // Movement
    float defWalkSpeed = 4f;
    float defRunSpeed = 8f;
    float maxSpeed = 12f;
    float acceleration = 80f;
    float deceleration = 80f;
    float jumpPower = 6f;
    public float gravity = 30f;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 initialCameraPosition;

    // Camera & body
    float lookXLimit = 80f;
    private float rotationX = 0;
    float defaultHeight = 2f;
    float crouchHeight = 1f;
    float crouchSpeed = 2f;

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
    private bool playAtLowPoint = true;
    private float lastFootstepTime = 0f;

    private float timer = Mathf.PI / 2;

    float rotationZ;
    bool canStartBobbing;

    // SETTINGS
    public float lookSpeed = 2f;
    float bobAmount = 0.1f;

    void Start() // Prepares character for the game
    {
        defaultCameraY = 0.7f;
        playerCamera.transform.localPosition = new Vector3(0, 0.7f, 0);
        walkSpeed = defWalkSpeed;
        runSpeed = defRunSpeed;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        initialCameraPosition = playerCamera.transform.localPosition;
    }
    
    void Update()
    { 
        //##2 Movement
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

        if (canMove)
        {
            // Calculate the movement direction
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            // Normalize the movement vector to avoid diagonal speed boosting (yeah I added it here, it was too OP)
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
                moveDirection *= isRunning ? runSpeed : walkSpeed; // Apply the correct speed after normalization
            }
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
        }else
        {
            characterController.height = defaultHeight; // Reduce to default
            walkSpeed = defWalkSpeed;
            runSpeed = defRunSpeed;
        }

        // Makes the character actually move
        if (canMove)
        {
            characterController.Move(moveDirection * Time.deltaTime);
        }
        else
        {
            characterController.Move(Vector3.zero);
        }



        //##3 Camera
        // Camera rotation (and body rotation)
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        if(canMove && !GameController.Instance.IsGamePaused()){
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, rotationZ);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
            
        
        /*
        if(Input.GetKey(KeyCode.U) && RenderSettings.fogDensity >= 0.01){
            RenderSettings.fogDensity -= Time.deltaTime /20;
        }
        */
        



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
                    Mathf.Clamp(newPosition.y + rb.velocity.y / 10, 0.6f, 1.2f),
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
        if (canMove){
            // Calculate currentSpeed based on character velocity
            float currentSpeed = characterController.velocity.magnitude;

            // Scale bobSpeed and footstep delay based on the currentSpeed
            float scaledBobSpeed = bobSpeed * (currentSpeed / 6); // Bobbing faster with higher speed
            float scaledFootstepDelay = Mathf.Clamp(0.5f / (currentSpeed / 4), 0.2f, 0.4f); // Adjust delay, with minimum and maximum thresholds

            float targetRotation = Input.GetAxis("Horizontal") * -70 * Time.deltaTime * lookSpeed; // Adjust horizontal rotation
            accumulatedRotation = Mathf.Lerp(accumulatedRotation, targetRotation, 0.08f); // Smoothness
            rotationZ = Mathf.Clamp(accumulatedRotation, -6, 6); // Maximum rotation

            // If the player is moving and grounded
            if ((Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) && characterController.isGrounded)
            {
                if (!canStartBobbing)
                {
                    initialCameraPosition = playerCamera.transform.localPosition;
                    canStartBobbing = true;
                }

                // Adjust bobbing speed based on current speed
                timer += scaledBobSpeed * Time.deltaTime;
                newPosition = new Vector3(
                    Mathf.Cos(timer) * bobAmount,
                    initialCameraPosition.y + Mathf.Abs(Mathf.Sin(timer) * bobAmount),
                    playerCamera.transform.localPosition.z
                );

                // Smooth camera bobbing transition
                playerCamera.transform.localPosition = Vector3.Lerp(
                    playerCamera.transform.localPosition,
                    newPosition,
                    smoothSpeed * Time.deltaTime
                );

                // Play footstep sound based on timer value and bobbing position
                if (Time.time > lastFootstepTime + scaledFootstepDelay /4)
                {
                    if (timer < 0.2f)
                    {
                        if (playAtLowPoint) // Low point of the bobbing cycle
                        {
                            footstepAudioSource.pitch = Random.Range(0.6f, 1.4f);
                            footstepAudioSource.PlayOneShot(footstepSound);
                            playAtLowPoint = false; // Switch to high point for next sound
                            lastFootstepTime = Time.time; // Reset footstep timer
                        }
                    }else if(timer > 3f){
                
                        if (!playAtLowPoint) // High point of the bobbing cycle
                        {
                            footstepAudioSource.pitch = Random.Range(0.6f, 1.4f);
                            footstepAudioSource.PlayOneShot(footstepSound);
                            playAtLowPoint = true; // Switch back to low point for next sound
                            lastFootstepTime = Time.time; // Reset footstep timer
                        }
                    }
                }
            }else{
                // Reset bobbing when not moving
                canStartBobbing = false;
                playerCamera.transform.localPosition = Vector3.Lerp(
                    playerCamera.transform.localPosition,
                    new Vector3(
                        0,//playerCamera.transform.localPosition.x,
                        0.7531425f,//newPosition.y,
                        0//playerCamera.transform.localPosition.z
                    ),
                    smoothSpeed * 2 * Time.deltaTime
                );
            }

            // Reset the bobbing timer to avoid overflow
            if (timer > Mathf.PI * 2)
            {
                timer = 0;
            }
        }

    }

    

}