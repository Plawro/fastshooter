using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamosvalController : MonoBehaviour
{

    [SerializeField] GameObject cargoMuleObject;
    [SerializeField] AudioClip jumpscareSound;
    [SerializeField] AudioSource audioSource;



    //Play sounds, after while activate in hallway, looking at player
    //While active, buzzing sounds, with more and more glitchy sounds
    //If detects player, make sure he leaves before cargomule finishes charging
    //If doesnt detect player, glitch the system and either go to control panel (looking at player), or disappear

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void Jumpscare(){
        if(audioSource.isPlaying){
            audioSource.Stop();
        }
        if(!audioSource.isPlaying){
            audioSource.PlayOneShot(jumpscareSound);
        }
        GameController.Instance.Jumpscare("Sentinel");
    }
}
