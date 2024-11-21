using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPlantController : MonoBehaviour
{
    public GameObject playerObject;
    public float minPower;
    public float maxPower;
    // -60 & 60

    public float addedPower;
    public float decreasedPower;
    public float power;
    public GameObject arrow;

    public AudioClip sound1;
    public AudioClip sound2;

    public AudioSource audioSource;

    public Transform skullImage;
    private Coroutine blinkCoroutine;
    private bool isBlinking = true;

    bool isInWarningZone = false;
    bool isInDeadZone = false;

    private Coroutine rotateCoroutine;

    
    void Start()
    {
        skullImage.gameObject.SetActive(false);
    }


    /*public void AddPower(float amount)
    {
        if(power < 170){
           power += amount;
        }
    }*/

    void Update()
    {
         if(!isInDeadZone){
            power -= 0.003f;
            arrow.transform.eulerAngles = new Vector3(
                arrow.transform.eulerAngles.x,
                arrow.transform.eulerAngles.y,
                power
            );
         }
        

        if (!isInDeadZone)
        {
            if (power < minPower + 6 || power > maxPower - 6)
            {
                if (!isInWarningZone)
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.clip = sound1;
                        audioSource.Play();
                    }
                    isInWarningZone = true;
                }
            }
            else
            {
                isInWarningZone = false;
                if (audioSource.clip == sound1) audioSource.Stop();
            }
        }

        if (power >= 59.8f) //overclocked
        {
            if (!isInDeadZone)
            {
                power = 180;
                if (!audioSource.isPlaying || audioSource.clip != sound2)
                {
                    audioSource.clip = sound2;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                if (blinkCoroutine == null)
                {
                    blinkCoroutine = StartCoroutine(BlinkImage());
                }
                isInDeadZone = true;

               rotateCoroutine = StartCoroutine(RotateArrowToTarget(180));

                isInWarningZone = false;
            }
        }else if(power <= -59.8f){ //nuclear reactor fell asleep - yeah, you can't restart it (such a skill issue)
            power = 0;
            isInDeadZone = true;
            rotateCoroutine = StartCoroutine(RotateArrowToTarget(180));
            isInWarningZone = false;
            GameObject.Find("MASTER gameobject").GetComponent<GameController>().SwitchAllLights(false); //Also turn all electricity off
            if (audioSource.isPlaying && audioSource.clip == sound1)
            {
                audioSource.Stop();
            }
        }
        
    }

    public void AddPower(float ammount){
        if(!isInDeadZone){
        if(ammount == 1){
            power += 0.03f + Mathf.Clamp((power + 60) * 0.001f, 0, 1f);
            power = Mathf.Clamp(power, minPower, maxPower);
        }else if(ammount == -1){
            power -= 0.05f + Mathf.Clamp((power + 60) * 0.002f, 0, 1f);
            power = Mathf.Clamp(power, minPower, maxPower);
        }
        }
    }


    private IEnumerator RotateArrowToTarget(float targetZ)
    {
        Quaternion startRotation = arrow.transform.rotation; //here we are
        Quaternion targetRotation = Quaternion.Euler(arrow.transform.eulerAngles.x, arrow.transform.eulerAngles.y, targetZ);

        float duration = 2.0f; //make it longer, so it looks more "dramatic"
        float elapsed = 0f;

        while (elapsed < duration)
        {
            arrow.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        arrow.transform.rotation = targetRotation;
    }

    private IEnumerator BlinkImage()
    {
        while (isBlinking)
        {
            skullImage.gameObject.SetActive(!skullImage.gameObject.activeSelf);
            yield return new WaitForSeconds(0.7f);
        }
    }
}
