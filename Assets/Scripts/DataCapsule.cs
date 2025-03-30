using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class DataCapsule : MonoBehaviour
{
    public int mode = 0; // 0 - off, 1 - blinking orange, 2 - green, 3 - red
    [SerializeField] GameObject indicator;
    [SerializeField] TextMeshProUGUI numberText;
    public int number;
    Coroutine alreadyBlinking;
    
    void Start(){
        numberText.text = number.ToString();
    }

    public void ChangeMode(int whatMode){
        mode = whatMode;
        if(mode == 3){
            indicator.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
            if(alreadyBlinking != null){
                StopCoroutine(alreadyBlinking);
                alreadyBlinking = null;
            }
        }else if(mode == 1){
            if(alreadyBlinking == null){
                alreadyBlinking = StartCoroutine(Blink());
            }
        }else if(mode == 2){
            indicator.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
            if(alreadyBlinking != null){
                StopCoroutine(alreadyBlinking);
                alreadyBlinking = null;
            }
        }else{
            indicator.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
            if(alreadyBlinking != null){
                StopCoroutine(alreadyBlinking);
                alreadyBlinking = null;
            }
        }
    }

    private IEnumerator Blink()
    {
        while (mode == 1)
        {
            if(indicator.GetComponent<Renderer>().material.color == new Color(255, 215, 0)){
                indicator.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
            }else{
                indicator.GetComponent<Renderer>().material.color = new Color(255, 215, 0);
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
