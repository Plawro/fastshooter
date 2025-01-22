using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isPaused = false;
    public AudioSource soundSource;
    public AudioClip pause;

    [SerializeField] AudioMixer audioMixer;

    public static string endMessage = "End message";
    public TextMeshProUGUI endText;
    public bool canBePaused;
    public GameObject loadingMenu;
    public TextMeshProUGUI loadingText;
    Coroutine blinkImage;
    bool alreadyBlinking;
    void Start()
    {
        loadingMenu.SetActive(false);
        pauseMenu.SetActive(isPaused);
        endText.text = endMessage;
        SetMasterVolume(16);
        SetAmbientVolume(16);
        SetSFXVolume(16);
        SetSirenVolume(16);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)){
            ChangePauseMode();
        }

        if(endMessage == "DEAD"){
            if(blinkImage == null && !alreadyBlinking){
                alreadyBlinking = true;
                blinkImage = StartCoroutine(BlinkImage());
            }
        }
    }

    public void ChangePauseMode(){
        if(canBePaused){
            soundSource.PlayOneShot(pause);
            if(isPaused){
                ResumeGame();
            }else{
                PauseGame();
            }
        }
    }

    public void StartGame(){
        StartCoroutine(LoadScene("Game1 1"));
    }


    public IEnumerator LoadScene(string name){
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName:name);
        operation.allowSceneActivation = false;
        loadingMenu.SetActive(true);
        while(!operation.isDone){
             if (operation.progress < 0.9f)
            {
                loadingText.text = "LOADING... " + (operation.progress * 100f).ToString("F0") + "%";
            }
            else
            {
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true;
                }
                loadingText.text = "Press any key to continue";
            }
            yield return null;



            /*
            float progress = Mathf.Clamp01(operation.progress * 100);

            loadingText.text = progress.ToString() + " %";

            yield return null;*/
        }
    }
    
    public void TheEnd(string endMessageGot){
        endMessage = endMessageGot;
        endText.text = endMessage;
        SceneManager.LoadScene (sceneName:"TheEnd");
    }

    public GameObject skullImage;

    private IEnumerator BlinkImage()
    {
        while(true){
            skullImage.SetActive(!skullImage.activeSelf);
            yield return new WaitForSeconds(1.4f);
        }
    }

    void PauseGame(){
        if(canBePaused){
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
    }

    void ResumeGame(){
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    public void Restart(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu(){
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit(){
        Application.Quit();
    }

    public void SetMasterVolume(float level){
        //audioMixer.SetFloat("masterVolume", level*2);  // -40 to 0 range
        audioMixer.SetFloat("masterVolume", Mathf.Lerp(-20f, 0f, level / 20f)); // Better "0 to 100" difference (above method isn't "linear" - ig that's the name)
    }

    public void SetSFXVolume(float level){
        //audioMixer.SetFloat("soundsVolume", level*2);
        audioMixer.SetFloat("soundsVolume", Mathf.Lerp(-20f, 0f, level / 20f));
    }

    public void SetSirenVolume(float level){
        //audioMixer.SetFloat("sirensVolume", level*2);
        audioMixer.SetFloat("sirensVolume", Mathf.Lerp(-20f, 0f, level / 20f));
    }

    public void SetAmbientVolume(float level){
        //audioMixer.SetFloat("ambientVolume", level*2);
        audioMixer.SetFloat("ambientVolume", Mathf.Lerp(-20f, 0f, level / 20f));
    }
}
