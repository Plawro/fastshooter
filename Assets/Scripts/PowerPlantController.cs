using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPlantController : MonoBehaviour
{
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

    
    void Start()
    {
        skullImage.gameObject.SetActive(false);
    }


    public void AddPower(float amount)
    {
       power += amount;
    }

    void Update()
    {
        power -= 0.002f;
        arrow.transform.eulerAngles = new Vector3(
        arrow.transform.eulerAngles.x,
        arrow.transform.eulerAngles.y,
        power
        );

        

        if(power < minPower + 2){
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sound1;
                audioSource.Play();
            }
        }

        if(power > maxPower - 2){
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sound2;
                audioSource.Play();
            }

            if(power >= 59.8f){
                power = 180;
                if (blinkCoroutine == null)
                {
                    blinkCoroutine = StartCoroutine(BlinkImage());
                }
            }
            if(power >= 179){
                power = 180;
                if (blinkCoroutine == null)
                {
                    blinkCoroutine = StartCoroutine(BlinkImage());
                }
            }
        }
    }

    public void addPower(float ammount){
        if(ammount == 1){
            power += 0.03f + Mathf.Clamp((power + 60) * 0.001f, 0, 0.2f);
            power = Mathf.Clamp(power, minPower, maxPower);
        }else if(ammount == -1){
            power -= 0.05f + Mathf.Clamp((power + 60) * 0.002f, 0, 1f);
            power = Mathf.Clamp(power, minPower, maxPower);
        }
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
