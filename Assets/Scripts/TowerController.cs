using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    [SerializeField] AudioClip sound1;
    [SerializeField] AudioClip sound2;
    [SerializeField] AudioClip sound3;

    [SerializeField] AudioSource audioSource;

    public bool isAntennaBroken = false;
    [SerializeField] GameObject handle;
    [SerializeField] float moveSpeed;
    [SerializeField] Renderer light;
    [SerializeField] StatsScreen statsScreen;
    [SerializeField] bool isRepairing = false;
    float timer= 0;

    void Update()
    {
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
    }

    public void StopRepairingAntenna(){
        isRepairing = false;  
    }

    public void BreakAntenna()
    {
        isRepairing = false;
        isAntennaBroken = true;
    }
}
