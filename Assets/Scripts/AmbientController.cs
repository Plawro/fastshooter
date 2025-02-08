using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientController : MonoBehaviour
{
    public AudioSource inAmbience;
    public AudioSource outAmbience;
    public AudioSource outWindAmbience;
    public float fadeDuration = 2.0f; // Transition in seconds

    public bool isIndoors = false;
    public bool isOtherPlaying = false; // Stop ambient when chasing, ... music is playing
    private Coroutine currentFadeRoutine;
    private bool previousIsOtherPlaying = false;
    public Material skyboxMaterial;

    void Start()
    {
        inAmbience.volume = 1;  // Ensure indoor starts muted when outside
        outAmbience.volume = 0;
        inAmbience.Play();
        outAmbience.Play();
    }

    private int indoorsCollidersTouched = 0;  // Track how many indoor colliders are touched

    public void SetOutside()
    {
        isIndoors = false;
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeToOutdoor());
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

    public void Update()
    {
        if (isIndoors)
        {
            if (RenderSettings.fogDensity > 0.055f)
            {
                RenderSettings.fogDensity -= Time.deltaTime;

                float transitionProgress = Mathf.InverseLerp(0.07f, 0.055f, RenderSettings.fogDensity);
                RenderSettings.fogColor = Color.Lerp(new Color(95 / 255f, 95 / 255f, 95 / 255f), Color.black, transitionProgress);

                skyboxMaterial.SetColor("_Tint", Color.Lerp(new Color(95 / 255f, 95 / 255f, 95 / 255f), new Color(20 / 255f, 20 / 255f, 20 / 255f), transitionProgress));
            }
        }
        else
        {
            if (RenderSettings.fogDensity < 0.07f)
            {
                RenderSettings.fogDensity += Time.deltaTime;

                float transitionProgress = Mathf.InverseLerp(0.055f, 0.07f, RenderSettings.fogDensity);
                RenderSettings.fogColor = Color.Lerp(Color.black, new Color(95 / 255f, 95 / 255f, 95 / 255f), transitionProgress);

                skyboxMaterial.SetColor("_Tint", Color.Lerp(new Color(20 / 255f, 20 / 255f, 20 / 255f), new Color(95 / 255f, 95 / 255f, 95 / 255f), transitionProgress));
            }
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

    while (timer < fadeDuration)
    {
        timer += Time.deltaTime;
        outAmbience.volume = Mathf.Lerp(1, 0, timer / fadeDuration); // Fade outdoor sound out
        outWindAmbience.volume = Mathf.Lerp(0.2f, 0, timer / fadeDuration); // Fade outdoor sound out
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
        outWindAmbience.volume = Mathf.Lerp(0, 0.2f, timer / fadeDuration); // Fade outdoor sound in
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
        outWindAmbience.volume = Mathf.Lerp(outAmbience.volume, 0, timer / fadeDuration); // Fade outdoor to silence
        yield return null;
    }
}

}
