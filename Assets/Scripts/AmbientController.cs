using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class AmbientController : MonoBehaviour
{
    public AudioSource inAmbience;
    public AudioSource outAmbience;
    public AudioSource outWindAmbience;
    public AudioSource sirenSource;
    public float fadeDuration = 2.0f; // Transition in seconds

    public bool isIndoors = false;
    public bool isOtherPlaying = false; // Stop ambient when chasing, ... music is playing
    private Coroutine currentFadeRoutine;
    private bool previousIsOtherPlaying = false;
    public Material skyboxMaterial;
    float timeTillStorm;
    bool stormStarted = false;

    [SerializeField] GameObject exitSign;

    void Start()
    {
        inAmbience.volume = 1;  // Ensure indoor starts muted when outside
        outAmbience.volume = 0;
        inAmbience.Play();
        outAmbience.Play();
        timeTillStorm = Random.Range(40, 210);
        dangerText.enabled = false;
    }

    private int indoorsCollidersTouched = 0;  // Track how many indoor colliders are touched

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
        stormLife = null;
        yield break; 
    }

    IEnumerator StormLife(){
        print("STORM");
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
        //Start sirens, glowing red on tower
        //Darken fog
        //Player must
    }
    Color color;
    public void StopStorm(){
        GameController.Instance.SwitchAllLightsColored(Color.yellow);
        StopCoroutine(BlinkSiren());
        exitSign.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
        exitSign.GetComponent<Renderer>().material.color = Color.green;
        exitSign.transform.GetChild(0).GetComponent<Light>().color = Color.green;
        exitSign.transform.GetChild(1).GetComponent<Light>().color = Color.green;
        exitSign.transform.GetChild(2).transform.GetChild(0).GetComponent<Image>().color = Color.white;
        sirenSource.Stop();
        stormStarted = false;
        timeTillStorm = Random.Range(40,210);
    }


    public void SetInside(){
        isIndoors = true;
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = StartCoroutine(FadeToIndoor());
    }

/*
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player") && !isOtherPlaying)
    {
        indoorsCollidersTouched++;
        if (indoorsCollidersTouched == 1) // Only fade when entering the first indoor collider
        {
            isIndoors = true;
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = StartCoroutine(FadeToIndoor());
        }
    }

}

private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player") && !isOtherPlaying)
    {
        indoorsCollidersTouched--;
        if (indoorsCollidersTouched == 0) // Only fade when leaving the last indoor collider
        {
            isIndoors = false;
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = StartCoroutine(FadeToOutdoor());
        }
    }

}
*/
    [SerializeField] TextMeshProUGUI dangerText;
    Coroutine dangerTextBlinking;
    bool useText1 = true;
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
            if (stormLife != null && normalizedDistance >= 0.6f)
            {
                // Alternate between both messages
                dangerText.text = "I AM DEAD";
            }
            else if (stormLife != null)
            {
                dangerText.text = "I SHOULD GET BACK INSIDE QUICKLY";
            }
            else if (normalizedDistance >= 0.6f)
            {
                dangerText.text = "I SHOULD RETURN BACK TO SAFETY";
            }

            yield return new WaitForSeconds(0.1f);
        
    }
    }
    [SerializeField] Volume volume;
    [SerializeField] GameObject centerObject;
    float normalizedDistance;
    float playerStormHP = 0;
    float playerDistanceHP = 0; // not now
    public void Update()
    {
        volume.weight = playerStormHP;
        print(playerStormHP);
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
            playerStormHP = 0;
            if (RenderSettings.fogDensity > 0.055f)
            {
                RenderSettings.fogDensity -= Time.deltaTime;

                float transitionProgress = Mathf.InverseLerp(0.07f, 0.055f, RenderSettings.fogDensity);
                RenderSettings.fogColor = Color.Lerp(new Color(95 / 255f, 95 / 255f, 95 / 255f), Color.black, transitionProgress);

                skyboxMaterial.SetColor("_Tint", Color.Lerp(new Color(95 / 255f, 95 / 255f, 95 / 255f), new Color(20 / 255f, 20 / 255f, 20 / 255f), transitionProgress));
            }
            dangerText.enabled = false;
            //Remove darkening screen
        }
        else
        {
            float distance = Vector3.Distance(centerObject.transform.position, GameController.Instance.playerCamera.transform.position);
            normalizedDistance =  Mathf.Clamp01(distance / 200f);
            if(normalizedDistance >= 1){
                StartCoroutine(GameController.Instance.FadeOut());
                StartCoroutine(KilledByStorm(2));
            }else if (normalizedDistance >= 0.6f){
                dangerText.text = "I SHOULD RETURN BACK TO SAFETY";
                dangerText.enabled = true;
                if(dangerTextBlinking == null){ // Or remove this for random blinking effect
                    dangerTextBlinking = StartCoroutine(DangerTextBlinking());
                }
            }else{
                if(stormLife == null){
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
                //Coroutine blinking text start, start darkening the screen until player dies
            }else{
                if (normalizedDistance <= 0.6f){
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

                skyboxMaterial.SetColor("_Tint", Color.Lerp(new Color(20 / 255f, 20 / 255f, 20 / 255f), new Color(95 / 255f, 95 / 255f, 95 / 255f), transitionProgress));
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
        
/*
    if (isOtherPlaying != previousIsOtherPlaying)
    {
        previousIsOtherPlaying = isOtherPlaying;

        if (isOtherPlaying)
        {
            if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = StartCoroutine(FadeOutBoth());
        }
        else
        {
            if (isIndoors)
            {
                print("indoor");
                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                currentFadeRoutine = StartCoroutine(FadeToIndoor());
            }
            else
            {
                print("outdoor");
                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                currentFadeRoutine = StartCoroutine(FadeToOutdoor());
            }
        }
    }*/
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

public IEnumerator FadeBackIn() // Could receive fixes, but works
{
    if(isInsidePlaying){
        StartCoroutine(FadeToIndoor());
    }else{
        StartCoroutine(FadeToOutdoor());
    }
    yield break;
}

}
