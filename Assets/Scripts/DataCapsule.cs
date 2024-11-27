using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class DataCapsule : MonoBehaviour
{
    public int mode = 0; // 0 - off, 1 - blinking orange, 2 - green, 3 - red
    public GameObject indicator;
    public TextMeshProUGUI numberText;
    public int number;
    
    void Start(){
        numberText.text = number.ToString();
    }

    /*void Update(){
        if(Input.GetKeyDown(KeyCode.Q)){
            if(mode == 2){
                mode = 0;
            }else{
                mode++;
            }
            ChangeMode(mode);
        }
    }*/

    public void ChangeMode(int whatMode){
        mode = whatMode;
        if(mode == 3){
            indicator.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
            StopCoroutine(Blink());
        }else if(mode == 1){
            StartCoroutine(Blink());
        }else if(mode == 2){
            indicator.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
            StopCoroutine(Blink());
        }else{
            indicator.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
            StopCoroutine(Blink());
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
