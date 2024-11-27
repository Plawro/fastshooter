using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set;}
    public GameObject[] lights;
    public bool isGeneratorDead = false;
    public DCUploaderController DCuploader;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep the GameManager across scenes (woah)
    }

    public void killGenerator(){
        isGeneratorDead = true;
        SwitchAllLights(false);
    }
    
    public void SwitchAllLights(bool turnMode){
        foreach (var light in lights){
            light.gameObject.SetActive(turnMode);
            light.gameObject.transform.parent.GetComponent<Renderer>().material.color = Color.black;
            light.gameObject.transform.parent.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
        }
    }
}
