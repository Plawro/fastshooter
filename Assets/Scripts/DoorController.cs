using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isOpen;
    public bool isDoubleSlidingDoor;
    public bool openAutomatically;
    public float normalDoorOpenAngle = 90f;
    private float currentAngle = 0f;
    private bool isPartiallyOpen = false;

    [SerializeField] GameObject door1;
    [SerializeField] GameObject door2;

    float slideAmmount = 1.76f;
    private Vector3 startPos1;
    private Vector3 startPos2;
    private Vector3 forward;

    private Coroutine currentAnimation = null; //Is there active Coroutine going on?

    private void Awake(){
        forward = transform.right;
        startPos1 = door1.transform.position;
        if(door2 != null){
            startPos2 = door2.transform.position;
        }
    }


    public void ChangeDoorMode(bool? doOpen = null){ // We can tell to open, or to close, or nothing at all, and it will switch automatically
        if (doOpen.HasValue){
            if (doOpen.Value){
                OpenDoor();
            } else {
                CloseDoor();
            }
        } else {
            if (isOpen){
                CloseDoor();
            } else {
                OpenDoor();
            }
        }
    }

    private void OnTriggerEnter(Collider other){
        if (other.TryGetComponent<CharacterController>(out CharacterController controller)){
            
            if(!isOpen && openAutomatically && !GameController.Instance.isGeneratorDead){
                ChangeDoorMode(true); // Or also OpenDoor() directly
            }
        }
    }

    private void OnTriggerExit(Collider other){
        if (other.TryGetComponent<CharacterController>(out CharacterController controller)){
            print(isOpen);
            if(isOpen && openAutomatically && !GameController.Instance.isGeneratorDead){
                ChangeDoorMode(false); // Or also CloseDoor() directly
            }
        }
    }
    
    private void OpenDoor(){
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation); // Stop the ongoing animation
        }

       if (isDoubleSlidingDoor)
        {
            currentAnimation = StartCoroutine(SlideDoorOpen());
        }
        else
        {
            currentAnimation = StartCoroutine(RotateDoorOpen());
        }
        isOpen = true;
        isPartiallyOpen = false;
    }

    private void CloseDoor(){
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation); // Stop the ongoing animation
        }

        if (isDoubleSlidingDoor)
        {
            currentAnimation = StartCoroutine(SlideDoorClose());
        }
        else
        {
            currentAnimation = StartCoroutine(RotateDoorClose());
        }
        isOpen = false;
        isPartiallyOpen = false;
    }






    private IEnumerator SlideDoorOpen()
    {
        Vector3 targetPos1 = startPos1 - forward * slideAmmount;  // Move door1 to the left
        Vector3 targetPos2 = startPos2 + forward * slideAmmount;  // Move door2 to the right

        while (Vector3.Distance(door1.transform.position, targetPos1) > 0.01f)
        {
            door1.transform.position = Vector3.Lerp(door1.transform.position, targetPos1, Time.deltaTime * 5);
            door2.transform.position = Vector3.Lerp(door2.transform.position, targetPos2, Time.deltaTime * 5);
            yield return null;
        }
    }

    private IEnumerator SlideDoorClose()
    {
        while (Vector3.Distance(door1.transform.position, startPos1) > 0.01f)
        {
            door1.transform.position = Vector3.Lerp(door1.transform.position, startPos1, Time.deltaTime * 5);
            door2.transform.position = Vector3.Lerp(door2.transform.position, startPos2, Time.deltaTime * 5);
            yield return null;
        }
    }

    private IEnumerator RotateDoorOpen()
    {
        float dotProduct = Vector3.Dot(transform.forward, (door1.transform.position - Camera.main.transform.position).normalized);

        float targetAngle = (dotProduct > 0) ? normalDoorOpenAngle : -normalDoorOpenAngle; // Rotate away from player

        while (Mathf.Abs(currentAngle) < Mathf.Abs(targetAngle))
        {
            float step = 180f * Time.deltaTime;
            door1.transform.Rotate(Vector3.up, step * Mathf.Sign(targetAngle));
            currentAngle += step * Mathf.Sign(targetAngle);
            yield return null;
        }
        currentAngle = targetAngle;
    }

    private IEnumerator RotateDoorClose()
    {
        float tolerance = 0.1f;
        float targetAngle = 0f;

        while (Mathf.Abs(currentAngle) > tolerance)
        {
            float step = 180f * Time.deltaTime;
            door1.transform.Rotate(Vector3.up, -step * Mathf.Sign(currentAngle));
            currentAngle -= step * Mathf.Sign(currentAngle);
            yield return null;
        }

        door1.transform.localEulerAngles = new Vector3(door1.transform.localEulerAngles.x, targetAngle, door1.transform.localEulerAngles.z);
        currentAngle = targetAngle;
    }





    public IEnumerator OpenPartialDoor() // Opens a bit just to get a slight peek in before fully opening door, rotates only one-way (doesnt depend where player is opening from)
    {
        if(!isOpen){
            if(!isPartiallyOpen){ 
                isPartiallyOpen = true;
                float targetAngle = 15f;
                while (currentAngle < targetAngle)
                {
                    float step = 50f * Time.deltaTime;
                    door1.transform.Rotate(Vector3.up, step);
                    currentAngle += step;
                    yield return null;
                }
            }else{
                CloseDoor();
            }
        }

    }
}
