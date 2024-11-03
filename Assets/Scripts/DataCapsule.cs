using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataCapsule : MonoBehaviour
{
    int mode = 0; // 0 - red, 1 - blinking orange, 2 - green
    public GameObject indicator;

    void Update(){
        if(Input.GetKeyDown(KeyCode.Q)){
            if(mode == 2){
                mode = 0;
            }else{
                mode++;
            }
            ChangeMode(mode);
        }
    }

    public void ChangeMode(int whatMode){
        mode = whatMode;
        if(mode == 0){
            indicator.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
        }else if(mode == 1){
            StartCoroutine(Blink());
        }else{
            indicator.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
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
