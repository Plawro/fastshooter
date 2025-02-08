using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public AudioClip sound3;

    public AudioSource audioSource;

    public Transform skullImage;
    private Coroutine blinkCoroutine;
    private bool isBlinking = true;

    bool isInWarningZone = false;
    bool isInDeadZone = false;

    private Coroutine rotateCoroutine;
    public float heat;
    float maxHeat = 100;
    public FollowerController enemyCont;
    public RawImage[] progressBarImages;
    float heatBarAmmount;
    bool enabledBar = false;

    private IEnumerator BlinkBar()
    {
        while (true) // Blink on and off, refreshing the progress bar
        {
            heatBarAmmount = Mathf.Clamp(heat/20-1+0.25f, 0, progressBarImages.Length - 1);

            if(enabledBar){
                foreach (RawImage img in progressBarImages)
                {
                    img.gameObject.SetActive(false);
                }

                enabledBar = false;
            }else{
                // Activate all images up to the current index to create a filling effect (should look good)
                for (int i = 0; i <= heatBarAmmount; i++)
                {
                    progressBarImages[i].gameObject.SetActive(true);
                }

                enabledBar = true;
            }

            yield return new WaitForSeconds(1.4f);
        }
    }

    
    void Start()
    {
        skullImage.gameObject.SetActive(false);
        StartCoroutine(BlinkBar());
    }


    /*public void AddPower(float amount)
    {
        if(power < 170){
           power += amount;
        }
    }*/

    void Update()
    {   
        if(!GameController.Instance.IsGamePaused() && GameController.Instance.gameStarted){
        enemyCont.Charge((power+60)/2400);
        if (!isInDeadZone){
            if(power > 0){
                heat += (power+60)/30000;
            }else if (heat >= 0){
                heat -= 0.001f;
            }
        }

         if(!isInDeadZone && !GameController.Instance.IsGamePaused() && GameController.Instance.gameStarted){
            power -= 0.003f;
            arrow.transform.eulerAngles = new Vector3(
                arrow.transform.eulerAngles.x,
                arrow.transform.eulerAngles.y,
                power
            );
         
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
        }
        if (power >= 60.5f || heat > maxHeat) //overclocked
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
                StartCoroutine(GameController.Instance.EndGameExplosion());
            }
        }else if(power <= -60.5f){ //nuclear reactor fell asleep - yeah, you can't restart it (such a skill issue)
            power = 0;
            audioSource.clip = sound3;
            audioSource.Play();
            isInDeadZone = true;
            rotateCoroutine = StartCoroutine(RotateArrowToTarget(180));
            isInWarningZone = false;
            GameController.Instance.KillGenerator();
            GameController.Instance.SwitchAllLights(false); //Also turn all electricity off
            if (audioSource.isPlaying && audioSource.clip == sound1)
            {
                audioSource.Stop();
            }
        }
        
    }

    public void RestartGenerator(){
            power = -30;
            isInDeadZone = false;
            isInWarningZone = false;
            GameController.Instance.ReviveGenerator();
            GameController.Instance.SwitchAllLights(true); //Also turn all electricity off
            if (audioSource.isPlaying && audioSource.clip == sound3)
            {
                audioSource.Stop();
            }
    }

    public void AddPower(float ammount){
        if(!isInDeadZone && !GameController.Instance.IsGamePaused()){
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
