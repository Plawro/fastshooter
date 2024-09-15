using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public AudioClip sound1;
    public AudioClip sound2;
    public AudioClip sound3;

    public AudioSource audioSource;

    bool isAntennaBroken = false;
    public GameObject handle;
    private Quaternion targetRotation;
    public float moveSpeed;
    bool brokeTheAntennaMovement = false;
    public Renderer light;

    public float fixSpeed = 1f;
    const float returnSpeed = 0.2f;
    const float autoMoveSpeed = 2f;

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

        // Switch antenna state when 'M' is pressed (Just debug, will be done automatically)
        if (Input.GetKeyDown(KeyCode.M))
        {
            brokeTheAntennaMovement = true;
            switchAntenna(true);
        }

        /*if (Input.GetKey(KeyCode.N))  //Used for debugging
        {
            // Move antenna back to 0 if holding N
            moveSpeed = fixSpeed;
            MoveAntennaToZero();
        }*/
        //else 
        if (isAntennaBroken)
        {
            moveSpeed = autoMoveSpeed;
            light.material.SetColor("_EmissionColor", Color.red);
            MoveAntennaToBrokenPosition();
        }
        else
        {
            // If antenna is not broken and not holding N, use return speed
            moveSpeed = returnSpeed;
            // Update target rotation to its current rotation to keep it stationary
            targetRotation = handle.transform.rotation;
        }

        // Apply rotation based on the move speed
        handle.transform.rotation = Quaternion.Lerp(handle.transform.rotation, targetRotation, Time.deltaTime * moveSpeed);

    }

    void switchAntenna(bool isBroken)
    {
        isAntennaBroken = isBroken;

        Vector3 currentRotation = handle.transform.rotation.eulerAngles;
        float targetXRotation = isAntennaBroken ? -90f : 0f;
        targetRotation = Quaternion.Euler(targetXRotation, currentRotation.y, currentRotation.z);
    }

    public void MoveAntennaToZero()
    {
        Vector3 currentRotation = handle.transform.rotation.eulerAngles;
        targetRotation = Quaternion.Euler(0f, currentRotation.y, currentRotation.z);

        // Check if the antenna has reached 0 and mark it as fixed (In the end, it moves reaaaally slow, this is the fix)
        if (Mathf.Abs(handle.transform.rotation.eulerAngles.x - 0f) > 359f)
        {
            isAntennaBroken = false; // Fix the antenna
            brokeTheAntennaMovement = false; // Reset the flag
            light.material.SetColor("_EmissionColor", Color.green); //Switch the light on the panel
        }
    }

    void MoveAntennaToBrokenPosition()
    {
        Vector3 currentRotation = handle.transform.rotation.eulerAngles;
        targetRotation = Quaternion.Euler(-90f, currentRotation.y, currentRotation.z);
    }

    void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
