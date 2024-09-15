using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused = false;
    public AudioSource soundSource;
    public AudioClip pause;

    [SerializeField] AudioMixer audioMixer;
    void Start()
    {
        pauseMenu.SetActive(false);
        SetMasterVolume(18);
        SetAmbientVolume(18);
        SetSFXVolume(18);
        SetSirenVolume(18);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            ChangePauseMode();
        }
    }

    public void ChangePauseMode(){
        soundSource.PlayOneShot(pause);
        if(isPaused){
                ResumeGame();
            }else{
                PauseGame();
        }
    }
    void PauseGame(){
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
    }

    void ResumeGame(){
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    public void MainMenu(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit(){
        Application.Quit();
    }

    public void SetMasterVolume(float level){
        //audioMixer.SetFloat("masterVolume", level*2);  // -40 to 0 range
        audioMixer.SetFloat("masterVolume", Mathf.Lerp(-80f, 0f, level / 20f)); // Better "0 to 100" difference (above method isn't "linear" - ig that's the name)
    }

    public void SetSFXVolume(float level){
        //audioMixer.SetFloat("soundsVolume", level*2);
        audioMixer.SetFloat("soundsVolume", Mathf.Lerp(-80f, 0f, level / 20f));
    }

    public void SetSirenVolume(float level){
        //audioMixer.SetFloat("sirensVolume", level*2);
        audioMixer.SetFloat("sirensVolume", Mathf.Lerp(-80f, 0f, level / 20f));
    }

    public void SetAmbientVolume(float level){
        //audioMixer.SetFloat("ambientVolume", level*2);
        audioMixer.SetFloat("ambientVolume", Mathf.Lerp(-80f, 0f, level / 20f));
    }
}
