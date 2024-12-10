using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlPanelController : MonoBehaviour
{
    public Transform[] capsuleHolder;
    public Transform[] capsuleIcons;
    int e = 0;
    int i = 0;
    int correct = 0;


    void Start()
    {
        InvokeRepeating(nameof(CapsuleChanged), 0f, 3f); // Every 3 seconds, or else we can use corountines
    }

    public void CapsuleChanged(){
        foreach (var c in capsuleHolder){
            if(c.childCount > 0){
                capsuleIcons[e].transform.gameObject.SetActive(true);
                if(c.GetChild(0).transform.GetComponent<DataCapsule>().number == e+1 && c.GetChild(0).transform.GetComponent<DataCapsule>().mode == 2){
                    Debug.Log("Found: "+ (e+1));
                }else{
                capsuleIcons[e].transform.gameObject.SetActive(false);
            }
            }
            e++;
        }
        e=0;
    }

    public void Finish(){
        correct = 0;
        foreach (var c in capsuleHolder){
            if(c.childCount > 0){
                if(c.GetChild(0).transform.GetComponent<DataCapsule>().number == i+1 && c.GetChild(0).transform.GetComponent<DataCapsule>().mode == 2){
                    correct++;
                }else{
            }
            }
            i++;
        }
        i=0;
        if (correct == 6){
            GameController.Instance.pauseMenu.TheEnd("You did it!");
        }
    }


    //CancelInvoke(nameof(CapsuleChanged));


}
