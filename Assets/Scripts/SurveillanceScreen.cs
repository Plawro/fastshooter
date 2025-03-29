using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SurveillanceScreen : MonoBehaviour
{  
    public GameObject playerObject;
    public Image leftWarning;
    public Image centerWarning;
    public Image centerMiddleWarning;
    public Image center2Warning;
    public Image rightWarning;
    [SerializeField] AudioSource audioSource;

    public void TriggerWarning(int position)
{
    if (position == 0)
    {
        StartCoroutine(FlashWarning(leftWarning));
    }
    else if (position == 1)
    {
        StartCoroutine(FlashWarning(centerWarning));
    }
    else if (position == 1)
    {
        StartCoroutine(FlashWarning(centerMiddleWarning));
    }
    else if (position == 3)
    {
        StartCoroutine(FlashWarning(center2Warning));
    }
    else if (position == 4)
    {
        StartCoroutine(FlashWarning(rightWarning));
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