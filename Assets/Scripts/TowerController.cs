using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public AudioClip sound1;
    public AudioClip sound2;
    public AudioClip sound3;

    public AudioSource audioSource;

    public bool isAntennaBroken = false;
    public GameObject handle;
    private Quaternion targetRotation;
    public float moveSpeed;
    public Renderer light;
    public StatsScreen statsScreen;
    public bool isRepairing = false;
    float timer= 0;

    // Another way to do this: have is broken bool, also add is moving bool

    void Update()
    {
        // Play sounds based on key input (NOT AUTOMATICAL YET)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlaySound(sound1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlaySound(sound2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlaySound(sound3);
        }


        if(isAntennaBroken){
            light.material.SetColor("_EmissionColor", Color.red);
            Vector3 currentRotation = handle.transform.rotation.eulerAngles;
            float currentX = currentRotation.x;
            
            
            if (isRepairing == false)
            {
                currentX = Mathf.MoveTowardsAngle(currentX, -90, 100 * Time.deltaTime);
                handle.transform.rotation = Quaternion.Euler(currentX, currentRotation.y, currentRotation.z);
            }
        }else{
            handle.transform.rotation = Quaternion.Euler(0, handle.transform.rotation.eulerAngles.y, handle.transform.rotation.eulerAngles.z);
            light.material.SetColor("_EmissionColor", Color.green);
        }
    }

    public void RepairAntenna(){
        isRepairing = true;
        Vector3 currentRotation = handle.transform.rotation.eulerAngles;
        float newX = Mathf.MoveTowardsAngle(currentRotation.x, 0, 100 * Time.deltaTime);
        handle.transform.rotation = Quaternion.Euler(newX, currentRotation.y, currentRotation.z); 

        if(handle.transform.rotation.eulerAngles.x <= 10 && isAntennaBroken && isRepairing){
            timer += Time.deltaTime;
            if(timer > 0.2f){
                handle.transform.rotation = Quaternion.Euler(0, handle.transform.rotation.eulerAngles.y, handle.transform.rotation.eulerAngles.z);
                isAntennaBroken = false;
                isRepairing = false;
                statsScreen.FixAntenna();
                timer = 0;
            }
        } 

       /* Vector3 currentRotation = handle.transform.rotation.eulerAngles;
        float newX = Mathf.MoveTowardsAngle(currentRotation.x, 1, 100 * Time.deltaTime);
        handle.transform.rotation = Quaternion.Euler(newX, currentRotation.y, currentRotation.z);  

        if (Mathf.Abs(handle.transform.rotation.eulerAngles.x) > 359 && isAntennaBroken) {
            isAntennaBroken = false; // Fix the antenna
            light.material.SetColor("_EmissionColor", Color.green); //Switch the light on the panel
            statsScreen.FixAntenna(); // Call FixAntenna on StatsScreen to resume data transfer
        } */
    }

    public void StopRepairingAntenna(){
        isRepairing = false;  
    }


    public void BreakAntenna()
    {
        isRepairing = false;
        isAntennaBroken = true;
        /*
        StartCoroutine(RotateHandleToTarget(-90f));
        */
    }

    /*private IEnumerator RotateHandleToTarget(float targetAngle)
    {
        
        Vector3 currentRotation = handle.transform.rotation.eulerAngles;
        float currentX = currentRotation.x;

        if (currentX > 180f) currentX -= 360f; // Just fixing stuff
        light.material.SetColor("_EmissionColor", Color.red);

        while (Mathf.Abs(targetAngle - currentX) > 0.1f)
        {
            currentX = Mathf.MoveTowardsAngle(currentX, targetAngle, 100 * Time.deltaTime);
            handle.transform.rotation = Quaternion.Euler(currentX, currentRotation.y, currentRotation.z);
            yield return null;
        }
        
        isAntennaBroken = true;
        handle.transform.rotation = Quaternion.Euler(targetAngle, currentRotation.y, currentRotation.z);
        
    }*/




    void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
