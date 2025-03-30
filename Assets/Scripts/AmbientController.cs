using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AmbientController : MonoBehaviour
{
    [SerializeField] AudioSource inAmbience;
    [SerializeField] AudioSource outAmbience;
    [SerializeField] AudioSource outWindAmbience;
    [SerializeField] AudioSource sirenSource;
    [SerializeField] float fadeDuration = 2.0f; // Transition in seconds

    [SerializeField] public bool isIndoors = false;
    [SerializeField] bool isOtherPlaying = false; // Stop ambient when chasing, ... music is playing
    private Coroutine currentFadeRoutine;
    private bool previousIsOtherPlaying = false;
    public Material skyboxMaterial;
    float timeTillStorm;
    public bool stormStarted = false;

    [SerializeField] GameObject exitSign;
    float timerVan;
    bool countVanTimer;
    float scarySoundTimer = 0;
    [Header("Danger text")]
    [SerializeField] TextMeshProUGUI dangerText;
    Coroutine dangerTextBlinking;
    [Header("Scary sounds")]
    [SerializeField] AudioSource scarySoundSource;
    [SerializeField] AudioClip[] scarySounds;
    [Header("Update")]
    [SerializeField] Volume volume;
    [SerializeField] GameObject centerObject;
    float normalizedDistance;
    float playerStormHP = 0;
    float distance;

    void Start()
    {
        scarySoundTimer = Random.Range(40, 100);
        inAmbience.volume = 0;
        outAmbience.volume = 1;
        outWindAmbience.volume = 1;
        inAmbience.Play();
        outAmbience.Play();
        outWindAmbience.Play();
        timeTillStorm = Random.Range(40, 210);
        dangerText.enabled = false;
        if(SceneManager.GetActiveScene().name == "TheEnd" || SceneManager.GetActiveScene().name == "Victory"){
            dangerText.alpha = 0;
        }else{
            dangerText.alpha = 255;
        }
    }
    
    public IEnumerator VanIsHere(){
        countVanTimer = true;
        Debug.Log("GG2");
        dangerText.enabled = true;
        dangerText.alpha = 255;
        
        while(timerVan <= 30){
            if(dangerTextBlinking == null){
                GameController.Instance.vanSource.Play();
                dangerTextBlinking = StartCoroutine(DangerTextBlinking());
            }

            if(GameController.Instance.touchedVan){
                GameController.Instance.FadeOut();
                yield return new WaitForSeconds(1);
                GameController.Instance.pauseMenu.Victory();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        GameController.Instance.RemoveTutorial(); // He left
        yield break;
    }

    public void SetOutside()
    {
        isIndoors = false;
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeToOutdoor());
    }

    IEnumerator BlinkSiren(){
        GameController.Instance.SwitchAllLightsColored(Color.red);
        while(stormStarted){
            exitSign.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            exitSign.GetComponent<Renderer>().material.color = Color.red;
            exitSign.transform.GetChild(0).GetComponent<Light>().color = Color.red;
            exitSign.transform.GetChild(1).GetComponent<Light>().color = Color.red;
            exitSign.transform.GetChild(2).transform.GetChild(0).GetComponent<Image>().color = Color.red;
            yield return new WaitForSeconds(1);
            exitSign.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            exitSign.GetComponent<Renderer>().material.color = Color.black;
            exitSign.transform.GetChild(0).GetComponent<Light>().color = Color.black;
            exitSign.transform.GetChild(1).GetComponent<Light>().color = Color.black;
            exitSign.transform.GetChild(2).transform.GetChild(0).GetComponent<Image>().color = Color.black;
            yield return new WaitForSeconds(1);
        }
        foreach (var light in GameController.Instance.lights){
            light.gameObject.transform.parent.GetComponent<Renderer>().material.color = new Color(247, 243, 145);
            light.gameObject.transform.parent.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(191, 187, 144));
        }
        exitSign.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
        exitSign.GetComponent<Renderer>().material.color = Color.green;
        exitSign.transform.GetChild(0).GetComponent<Light>().color = Color.green;
        exitSign.transform.GetChild(1).GetComponent<Light>().color = Color.green;
        exitSign.transform.GetChild(2).transform.GetChild(0).GetComponent<Image>().color = Color.white;
        stormLife = null;
        yield break; 
    }

    IEnumerator StormLife(){
        yield return new WaitForSeconds(25);
        StopStorm();
        yield return null;
    }

    Coroutine stormLife;
    public void Storm(){
        stormStarted = true;
        sirenSource.Play();
        StartCoroutine(BlinkSiren());
        stormLife = StartCoroutine(StormLife());
    }
    Color color;
    public void StopStorm(){
        GameController.Instance.SwitchAllLightsColored(Color.yellow);
        StopCoroutine(BlinkSiren());
        sirenSource.Stop();
        stormStarted = false;
        timeTillStorm = Random.Range(40,210);
    }


    public void SetInside(){
        isIndoors = true;
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = StartCoroutine(FadeToIndoor());
    }

    
    IEnumerator DangerTextBlinking(){
        CinemachineVirtualCamera mainCamera = GameController.Instance.playerCamera;
        float originalFOV = mainCamera.m_Lens.FieldOfView;

        while (dangerText.enabled)
        {
                mainCamera.m_Lens.FieldOfView = originalFOV + Random.Range(-2f, 2f);
                dangerText.alpha = 0;
                yield return new WaitForSeconds(0.1f);

                dangerText.alpha = 255;
                mainCamera.m_Lens.FieldOfView = originalFOV + Random.Range(-2f, 2f);
                if (stormLife != null && normalizedDistance >= 0.75f)
                {
                    dangerText.text = "I AM DEAD";
                }
                else if (stormLife != null)
                {
                    dangerText.text = "I SHOULD GET BACK INSIDE QUICKLY";
                }
                else if (normalizedDistance >= 0.75f)
                {
                    dangerText.text = "I SHOULD RETURN BACK TO SAFETY";
                }else if(timerVan <= 30){
                    dangerText.text = "Time is up. Get to the van fast.";
                }

                yield return new WaitForSeconds(0.1f);
            
        }
    }
    
    void PlayScarySound(){
        if (!scarySoundSource.isPlaying){
            scarySoundSource.PlayOneShot(scarySounds[Random.Range(0, scarySounds.Length)]);
        }
    }

    public void Update()
    {   
        if(countVanTimer){
            timerVan += Time.deltaTime;
        }
        volume.weight = playerStormHP;
        if(GameController.Instance.gameStarted){
            if(timeTillStorm <= 0){
                if(stormStarted == false){
                    Storm();
                }
            }else{
                if(stormStarted == false){
                    timeTillStorm -= Time.deltaTime;
                }
            }
        }

        if (isIndoors)
        {
            if(scarySoundTimer >= 0){
                scarySoundTimer -= Time.deltaTime;
            }else{
                PlayScarySound();
                scarySoundTimer = Random.Range(40, 100);
            }
            playerStormHP = 0;
            if (RenderSettings.fogDensity > 0.055f)
            {
                RenderSettings.fogDensity -= Time.deltaTime;

                float transitionProgress = Mathf.InverseLerp(0.07f, 0.055f, RenderSettings.fogDensity);
                RenderSettings.fogColor = Color.Lerp(new Color(95 / 255f, 95 / 255f, 95 / 255f), Color.black, transitionProgress);

                //skyboxMaterial.SetColor("_Tint", Color.Lerp(new Color(95 / 255f, 95 / 255f, 95 / 255f), new Color(20 / 255f, 20 / 255f, 20 / 255f), transitionProgress));
            }
            if(!countVanTimer){
            dangerText.enabled = false;
            }
        }
        else
        {
            if(centerObject != null && GameController.Instance.playerCamera != null){
                distance = Vector3.Distance(centerObject.transform.position, GameController.Instance.playerCamera.transform.position);
            }

            normalizedDistance =  Mathf.Clamp01(distance / 200f);
            if(normalizedDistance >= 1){
                StartCoroutine(GameController.Instance.FadeOut());
                StartCoroutine(KilledByStorm(2));
            }else if (normalizedDistance >= 0.75f){
                dangerText.text = "I SHOULD RETURN BACK TO SAFETY";
                dangerText.enabled = true;
                if(dangerTextBlinking == null){ // Or remove this for random blinking effect
                    dangerTextBlinking = StartCoroutine(DangerTextBlinking());
                }
            }else{
                if(stormLife == null && !countVanTimer){
                    dangerText.enabled = false;
                    dangerTextBlinking = null;
                }
            }

            if(stormLife != null){
                if(playerStormHP <= 1){
                   playerStormHP += Time.deltaTime/20;
                }else{
                    StartCoroutine(GameController.Instance.FadeOut());
                    StartCoroutine(KilledByStorm(1));
                }

                dangerText.text = "I SHOULD GET BACK INSIDE QUICKLY";
                dangerText.enabled = true;
                if(dangerTextBlinking == null){ // Or remove this for random blinking effect
                    dangerTextBlinking = StartCoroutine(DangerTextBlinking());
                }
            }else{
                if (normalizedDistance <= 0.75f && !countVanTimer){
                    dangerText.enabled = false;
                    dangerTextBlinking = null;
                }
                playerStormHP = 0;
            }


            if (RenderSettings.fogDensity < 0.07f)
            {
                RenderSettings.fogDensity += Time.deltaTime;

                float transitionProgress = Mathf.InverseLerp(0.055f, 0.07f, RenderSettings.fogDensity);
                RenderSettings.fogColor = Color.Lerp(Color.black, new Color(95 / 255f, 95 / 255f, 95 / 255f), transitionProgress);

                //skyboxMaterial.SetColor("_Tint", Color.Lerp(new Color(20 / 255f, 20 / 255f, 20 / 255f), new Color(95 / 255f, 95 / 255f, 95 / 255f), transitionProgress));
            }
        }

        IEnumerator KilledByStorm(int i){
            if(i==1){
                GameController.Instance.Jumpscare("Storm");
            }else{
                GameController.Instance.Jumpscare("Distance");
            }
            yield break;
        }
    }

        private IEnumerator FadeToIndoor()
    {
        float timer = 0f;
        isInsidePlaying = true;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            outAmbience.volume = Mathf.Lerp(1, 0, timer / fadeDuration); // Fade outdoor sound out
            outWindAmbience.volume = Mathf.Lerp(0.2f, 0, timer / fadeDuration); // Fade outdoor sound out
            inAmbience.volume = Mathf.Lerp(0, 1, timer / fadeDuration); // Fade indoor sound in
            yield return null;
        }
    }
    bool isInsidePlaying = false;

    private IEnumerator FadeToOutdoor()
    {
        float timer = 0f;
        isInsidePlaying = false;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            inAmbience.volume = Mathf.Lerp(1, 0, timer / fadeDuration); // Fade indoor sound out
            outAmbience.volume = Mathf.Lerp(0, 1, timer / fadeDuration); // Fade outdoor sound in
            outWindAmbience.volume = Mathf.Lerp(0, 0.2f, timer / fadeDuration); // Fade outdoor sound in
            yield return null;
        }
    }

    public IEnumerator FadeOutBoth()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            inAmbience.volume = Mathf.Lerp(inAmbience.volume, 0, timer / fadeDuration); // Fade indoor to silence
            outAmbience.volume = Mathf.Lerp(outAmbience.volume, 0, timer / fadeDuration); // Fade outdoor to silence
            outWindAmbience.volume = Mathf.Lerp(outAmbience.volume, 0, timer / fadeDuration); // Fade outdoor to silence
            yield return null;
        }
    }

    public IEnumerator FadeBackIn()
    {
        if(isInsidePlaying){
            StartCoroutine(FadeToIndoor());
        }else{
            StartCoroutine(FadeToOutdoor());
        }
        yield break;
    }
}
