using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCUploaderController : MonoBehaviour
{
    public Vector3 capsulePos;
    Transform currentCapsule;

    public string CheckCapsule(){
        if(this.transform.childCount > 2){
            currentCapsule = this.transform.GetChild(2);
            return(currentCapsule.name);
        }else{
            return("Empty");
        }
    }

    public int CheckCapsuleMode(){
        if(this.transform.childCount > 2){
            return currentCapsule.transform.GetComponent<DataCapsule>().mode;
        }else{
            return(10);
        }
    }

    public void CapsuleUploading(){
        currentCapsule.transform.GetComponent<DataCapsule>().ChangeMode(1);
    }

    public void CapsuleFinished(){
        currentCapsule.transform.GetComponent<DataCapsule>().ChangeMode(2);
    }
}
