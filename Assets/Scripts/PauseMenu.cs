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
    int mainMenuSelected;

    [SerializeField] AudioMixer audioMixer;

    public static string endMessage = "End message";
    public TextMeshProUGUI endText;
    public GameObject skullImage;
    public bool canBePaused;
    public GameObject loadingMenu;
    public TextMeshProUGUI loadingText;
    Coroutine blinkImage;
    bool alreadyBlinking;
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject options;
    void Start()
    {
        if(SceneManager.GetActiveScene().name != "Game1 1"){
            Destroy(GameObject.Find("MASTER gameobject"));
            Time.timeScale = 1;
            if(SceneManager.GetActiveScene().name == "TheEnd"){
                
            endText = GameObject.Find("EndMessage").GetComponent<TextMeshProUGUI>();
            endText.text = endMessage;
            skullImage = GameObject.Find("DeathImage");

            }
        }

        if(SceneManager.GetActiveScene().name == "MainMenu"){
            mainMenuSelected = 1;
            MainMenuChangeColor(mainMenuSelected);
        }
        

        loadingMenu.SetActive(false);
        pauseMenu.SetActive(isPaused);
        if(SceneManager.GetActiveScene().name == "TheEnd")
        endText.text = endMessage;
        SetMasterVolume(16);
        SetAmbientVolume(16);
        SetSFXVolume(16);
        SetSirenVolume(16);
        //Set main menu / game sliders to volume too
    }

    void Update()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu"){
            if(Input.GetKeyDown(KeyCode.S)){
                if(mainMenuSelected < 3){
                    mainMenuSelected++;
                    MainMenuChangeColor(mainMenuSelected);
                }else{
                    mainMenuSelected = 1;
                    MainMenuChangeColor(mainMenuSelected);
                }
            }else if(Input.GetKeyDown(KeyCode.W)){
                if(mainMenuSelected > 1){
                    mainMenuSelected--;
                    MainMenuChangeColor(mainMenuSelected);
                }else{
                    mainMenuSelected = 3;
                    MainMenuChangeColor(mainMenuSelected);
                }
            }else if(Input.GetKeyDown(KeyCode.Space)){
                MainMenuPick(mainMenuSelected);
            }
            MainMenuChangeColor(mainMenuSelected);
        }

        if((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && SceneManager.GetActiveScene().name == "Game1 1"){
            ChangePauseMode();
        }

        if(endMessage != null){
            if(blinkImage == null && !alreadyBlinking){
                alreadyBlinking = true;
                blinkImage = StartCoroutine(BlinkImage());
            }
        }
    }

    public void MainMenuChangeColor(int selected){
        switch(selected){
            case 1:
                mainMenuButtons[0].color = Color.white; // For coule be used, but its unnecessary if we know there are always 3 buttons and only 2 of them set to basic color
                mainMenuButtons[1].color = Color.gray;
                mainMenuButtons[2].color = Color.gray;
                break;
            case 2:
                mainMenuButtons[0].color = Color.gray;
                mainMenuButtons[1].color = Color.white;
                mainMenuButtons[2].color = Color.gray;
                break;
            case 3:
                mainMenuButtons[0].color = Color.gray;
                mainMenuButtons[1].color = Color.gray;
                mainMenuButtons[2].color = Color.white;
                break;
        }
    }

    public void MainMenuPick(int selected){
        switch(selected){
            case 1:
                StartGame();
                break;
            case 2:
                Options();
                break;
            case 3:
                Quit();
                break;
        }
    }
    [SerializeField] TextMeshProUGUI[] mainMenuButtons;
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
        //Settings.Instance.deathMessage = endMessage;
        SceneManager.LoadScene (sceneName:"TheEnd"); //FIX
    }

    public void Victory(){
        SceneManager.LoadScene (sceneName:"Victory");
    }

    private IEnumerator BlinkImage()
    {
        while(true && skullImage != null){
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
        if(playerObject.activeSelf){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }else{
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } 
    }

    public void Restart(){
        SceneManager.LoadScene("Game1 1");
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

    public void Options(){
        options.SetActive(!options.activeSelf);
    }

    public void SetQuality(int qualityIndex){
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool fullscreen){
        Screen.fullScreen = fullscreen;
    }

    public void SetMasterVolume(float level){
        //audioMixer.SetFloat("masterVolume", level*2);  // -40 to 0 range
        audioMixer.SetFloat("masterVolume", Mathf.Lerp(-20f, 0f, level / 20f)); // Better "0 to 100" difference (above method isn't "linear" - ig that's the name)
        print(Mathf.Lerp(-20f, 0f, level / 20f));
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
