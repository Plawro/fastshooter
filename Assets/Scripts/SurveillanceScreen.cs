using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SurveillanceScreen : MonoBehaviour
{  
    [SerializeField] GameObject playerObject;
    [SerializeField] Image leftWarning;
    [SerializeField] Image centerWarning;
    [SerializeField] Image centerMiddleWarning;
    [SerializeField] Image center2Warning;
    [SerializeField] Image rightWarning;
    [SerializeField] AudioSource audioSource;

    public void TriggerWarning(int position)
    {
        switch(position){
            case 0:
                StartCoroutine(FlashWarning(leftWarning));
            break;
            case 1:
                StartCoroutine(FlashWarning(centerWarning));
            break;
            case 2:
                StartCoroutine(FlashWarning(centerMiddleWarning));
            break;
            case 3:
                StartCoroutine(FlashWarning(center2Warning));
            break;
            case 4:
                StartCoroutine(FlashWarning(rightWarning));
            break;
        }
    }

    void Start(){
        leftWarning.enabled = false;
        rightWarning.enabled = false;
        centerWarning.enabled = false;
        centerMiddleWarning.enabled = false;
        center2Warning.enabled = false;
        audioSource.Play();
    }

    IEnumerator FlashWarning(Image image)
    {
        // Simulate flashing logic
        for (int i = 0; i < 2; i++)
        {
            image.enabled = true;
            yield return new WaitForSeconds(0.05f); // Flash on
            image.enabled = false;
            yield return new WaitForSeconds(0.05f); // Flash off
            image.enabled = true;
            yield return new WaitForSeconds(0.05f); // Flash on
            image.enabled = false;
        }
    }
}