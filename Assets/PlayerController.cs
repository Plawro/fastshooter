
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Camera playerCamera;
    public Rigidbody rb;
    private CharacterController characterController;

    [Header("User interface")]
    public float lookSpeed = 2f;

    float defWalkSpeed = 14f;
    float defRunSpeed = 7f;
    float jumpPower = 8f;
    public float gravity = 25f;

    float lookXLimit = 80f;
    float defaultHeight = 2f;
    float crouchHeight = 1f;
    float crouchSpeed = 3f;
    float walkSpeed;
    float runSpeed;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float accumulatedRotation;
    private float curSpeedX;
    private float curSpeedY;

    public float acceleration = 10f; // Adjust this for acceleration
public float deceleration = 10f; // Adjust this for deceleration

float maxSpeed = 20f;

    public bool canMove = true;
    Vector3 restPosition;
    float bobSpeed = 10f;
    float bobAmount = 0.08f;
    float defaultCameraY;

    private float timer = Mathf.PI / 2;

    void Start()
    {
        defaultCameraY = 0.7f;
        walkSpeed = defWalkSpeed;
        runSpeed = defRunSpeed;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeedX = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical");
        float targetSpeedY = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal");

        // Smoothly interpolate the current speed towards the target speed
        curSpeedX = Mathf.MoveTowards(curSpeedX, targetSpeedX, (isRunning ? acceleration : deceleration) * Time.deltaTime);
        curSpeedY = Mathf.MoveTowards(curSpeedY, targetSpeedY, (isRunning ? acceleration : deceleration) * Time.deltaTime);

        // Limit the speed to the maximum speed
        curSpeedX = Mathf.Clamp(curSpeedX, -maxSpeed, maxSpeed);
        curSpeedY = Mathf.Clamp(curSpeedY, -maxSpeed, maxSpeed);

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = defWalkSpeed;
            runSpeed = defRunSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);

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


        if (canMove)
        {
            float targetRotation = Input.GetAxis("Horizontal") * -100 * Time.deltaTime * lookSpeed; // -<number> changes horizontal value multiplication
            accumulatedRotation = Mathf.Lerp(accumulatedRotation, targetRotation, 0.08f); // Smoothness
            float rotationZ = Mathf.Clamp(accumulatedRotation, -6, 6); // Maximum rotation

            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, rotationZ);

            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 && characterController.isGrounded)
            {
                timer += bobSpeed * Time.deltaTime;

                Vector3 newPosition = new Vector3(Mathf.Cos(timer) * bobAmount,
                    restPosition.y + Mathf.Abs((Mathf.Sin(timer) * bobAmount)) + defaultCameraY, restPosition.z);
                playerCamera.transform.localPosition = newPosition;
            }

             /* SETS CAMERA POSITION (when pressing move a few times in short time, makes a lagging effect)
        else
        {
            timer = Mathf.PI / 2;
        }
        */

            if (timer > Mathf.PI * 2)
            {
                timer = 0;
            }

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}
