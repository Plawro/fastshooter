using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlPanelController : MonoBehaviour
{
    public Transform[] capsuleHolder;
    public Transform[] capsuleIcons;
    int i = 0;
    public void CapsuleChanged(){
        foreach (var c in capsuleHolder){
            if(c.childCount > 0){
                capsuleIcons[i].transform.gameObject.SetActive(true);
            }else{
                capsuleIcons[i].transform.gameObject.SetActive(false);
            }
            i++;
        }
        i=0;
    }
}
