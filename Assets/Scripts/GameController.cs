using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject[] lights;
    
    public void SwitchAllLights(bool turnMode){
        foreach (var light in lights){
            light.gameObject.SetActive(turnMode);
        }
    }
}
