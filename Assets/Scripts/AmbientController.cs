using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientController : MonoBehaviour
{
    public AudioSource inAmbience;
    public AudioSource outAmbience;
    public float fadeDuration = 2.0f; // Transition in seconds

    private bool isIndoors = false;
    public bool isOtherPlaying = false; // Stop ambient when chasing, ... music is playing
    private Coroutine currentFadeRoutine;
    private bool previousIsOtherPlaying = false;

    void Start()
    {
        inAmbience.volume = 1;  // Ensure indoor starts muted when outside
        outAmbience.volume = 0;
        inAmbience.Play();
        outAmbience.Play();
    }

    private int indoorsCollidersTouched = 0;  // Track how many indoor colliders are touched


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


    private void Update()
{
    if(isIndoors){ // When something is waiting outside, spawn it after player has full fog, otherwise player can see it!
        if(RenderSettings.fogDensity >= 0.01){
            RenderSettings.fogDensity -= Time.deltaTime /20;
        }
    }else{
        if(RenderSettings.fogDensity <= 0.06){
            RenderSettings.fogDensity += Time.deltaTime /20;
        }
    }
        

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
                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                currentFadeRoutine = StartCoroutine(FadeToIndoor());
            }
            else
            {
                if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
                currentFadeRoutine = StartCoroutine(FadeToOutdoor());
            }
        }
    }
}

    private IEnumerator FadeToIndoor()
{
    float timer = 0f;

    while (timer < fadeDuration)
    {
        timer += Time.deltaTime;
        outAmbience.volume = Mathf.Lerp(1, 0, timer / fadeDuration); // Fade outdoor sound out
        inAmbience.volume = Mathf.Lerp(0, 1, timer / fadeDuration); // Fade indoor sound in
        yield return null;
    }
}

private IEnumerator FadeToOutdoor()
{
    float timer = 0f;

    while (timer < fadeDuration)
    {
        timer += Time.deltaTime;
        inAmbience.volume = Mathf.Lerp(1, 0, timer / fadeDuration); // Fade indoor sound out
        outAmbience.volume = Mathf.Lerp(0, 1, timer / fadeDuration); // Fade outdoor sound in
        yield return null;
    }
}

private IEnumerator FadeOutBoth()
{
    float timer = 0f;

    while (timer < fadeDuration)
    {
        timer += Time.deltaTime;
        inAmbience.volume = Mathf.Lerp(inAmbience.volume, 0, timer / fadeDuration); // Fade indoor to silence
        outAmbience.volume = Mathf.Lerp(outAmbience.volume, 0, timer / fadeDuration); // Fade outdoor to silence
        yield return null;
    }
}

}
