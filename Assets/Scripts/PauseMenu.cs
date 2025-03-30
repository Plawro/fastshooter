using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    public bool isPaused = false;
    [SerializeField] AudioSource soundSource;
    [SerializeField] AudioClip pause;
    int mainMenuSelected;
    int entitySelected;
    [SerializeField] Volume postProcessingVolume;

    [SerializeField] AudioMixer audioMixer;

    [SerializeField] static string endMessage = "You died.";
    [SerializeField] TextMeshProUGUI endText;
    [SerializeField] GameObject skullImage;
    [SerializeField] bool canBePaused;
    [SerializeField] GameObject loadingMenu;
    [SerializeField] TextMeshProUGUI loadingText;
    Coroutine blinkImage;
    bool alreadyBlinking;
    [SerializeField] GameObject followerText;
    [SerializeField] TextMeshProUGUI followerTitle;
    [SerializeField] GameObject driftText;
    [SerializeField] TextMeshProUGUI driftTitle;
    [SerializeField] GameObject sentinelText;
    [SerializeField] TextMeshProUGUI sentinelTitle;

    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject options;
    [SerializeField] GameObject entities;
    [SerializeField] GameObject warning;
    [SerializeField] GameObject firstTimePlaying;
    [SerializeField] Slider[] volumeSliders;
    [SerializeField] TMP_Dropdown graphics;
    [SerializeField] Toggle fullscreen;
    [SerializeField] GameObject introTexts;
    public bool introTextsOn;
    [SerializeField] TextMeshProUGUI[] mainMenuButtons;
    [SerializeField] AmbientController ambientCont;
    void Start()
    {
        if(PlayerPrefs.HasKey("Graphics")){
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Graphics"));
            if(PlayerPrefs.GetInt("Graphics") == 2){
                postProcessingVolume.enabled = false;
            }else if(PlayerPrefs.GetInt("Graphics") > 2){
                PlayerPrefs.SetInt("Graphics", 2); // Older builds might have set other graphic levels
            }else{
                postProcessingVolume.enabled = true;
            }
            if(SceneManager.GetActiveScene().name == "MainMenu"){
                graphics.value = PlayerPrefs.GetInt("Graphics");
            }
        }

        if(PlayerPrefs.HasKey("Fullscreen")){
            Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("Fullscreen"));
            if(SceneManager.GetActiveScene().name == "MainMenu"){
                fullscreen.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("Fullscreen"));
            }
        }


        if(PlayerPrefs.HasKey("Volume_Master") && PlayerPrefs.HasKey("Volume_Ambient") && PlayerPrefs.HasKey("Volume_SFX") && PlayerPrefs.HasKey("Volume_Siren")){
                if(SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Game1 1"){
                    volumeSliders[0].value = PlayerPrefs.GetFloat("Volume_Master");
                }

                if(SceneManager.GetActiveScene().name == "Game1 1"){
                    volumeSliders[3].value = PlayerPrefs.GetFloat("Volume_Ambient");
                    volumeSliders[1].value = PlayerPrefs.GetFloat("Volume_SFX");
                    volumeSliders[2].value = PlayerPrefs.GetFloat("Volume_Siren");
                }
        }else if(PlayerPrefs.HasKey("Volume_Master")){
                volumeSliders[0].value = PlayerPrefs.GetFloat("Volume_Master");
        }else{
                volumeSliders[0].value = 16;
                if(SceneManager.GetActiveScene().name != "MainMenu"){
                    volumeSliders[3].value = 16;
                    volumeSliders[1].value = 16;
                    volumeSliders[2].value = 16;
                }
        }

        if(SceneManager.GetActiveScene().name != "Game1 1"){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Destroy(GameObject.Find("MASTER gameobject"));
            Time.timeScale = 1;
            if(SceneManager.GetActiveScene().name == "TheEnd"){
                
            endText = GameObject.Find("EndMessage").GetComponent<TextMeshProUGUI>();
            endText.text = endMessage;
            skullImage = GameObject.Find("DeathImage");

            }
        }

        if(SceneManager.GetActiveScene().name == "MainMenu"){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mainMenuSelected = 1;
            entitySelected = 1;
            MainMenuChangeColor(mainMenuSelected);
            MainMenuChangeEntity(entitySelected);
            if(PlayerPrefs.GetString("Username") != null){
                ChangeUserName();
            }

            if(PlayerPrefs.HasKey("Username")){
                firstTimePlaying.SetActive(false);
                introTexts.SetActive(false);
                introTextsOn = false;
            }
        }else if(SceneManager.GetActiveScene().name == "Game1 1"){

        }

        loadingMenu.SetActive(false);
        pauseMenu.SetActive(isPaused);
        if(SceneManager.GetActiveScene().name == "TheEnd" || SceneManager.GetActiveScene().name == "Victory"){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if(SceneManager.GetActiveScene().name == "TheEnd"){
                endText.text = endMessage;
            }
            if(PlayerPrefs.HasKey("Volume_Master") && PlayerPrefs.HasKey("Volume_Ambient") && PlayerPrefs.HasKey("Volume_SFX") && PlayerPrefs.HasKey("Volume_Siren")){
                SetMasterVolume(PlayerPrefs.GetFloat("Volume_Master"));
                SetAmbientVolume(PlayerPrefs.GetFloat("Volume_Ambient"));
                SetSFXVolume(PlayerPrefs.GetFloat("Volume_SFX"));
                SetSirenVolume(PlayerPrefs.GetFloat("Volume_Siren"));
            }else{
                SetMasterVolume(16);
                SetAmbientVolume(16);
                SetSFXVolume(16);
                SetSirenVolume(16);
            }
        }else if(SceneManager.GetActiveScene().name == "MainMenu"){
            if(PlayerPrefs.HasKey("Volume_Master")){
                SetMasterVolume(PlayerPrefs.GetFloat("Volume_Master"));
            }else{
                SetMasterVolume(16);
            }
        }else{
            if(PlayerPrefs.HasKey("Volume_Master") && PlayerPrefs.HasKey("Volume_Ambient") && PlayerPrefs.HasKey("Volume_SFX") && PlayerPrefs.HasKey("Volume_Siren")){
                SetMasterVolume(PlayerPrefs.GetFloat("Volume_Master"));
                SetAmbientVolume(PlayerPrefs.GetFloat("Volume_Ambient"));
                SetSFXVolume(PlayerPrefs.GetFloat("Volume_SFX"));
                SetSirenVolume(PlayerPrefs.GetFloat("Volume_Siren"));
            }else{
                SetMasterVolume(16);
                SetAmbientVolume(16);
                SetSFXVolume(16);
                SetSirenVolume(16);
            }
        }
    }
    float heldSpaceTime = 0;
    [SerializeField] Slider acceptSlider;
    [SerializeField] Slider startSlide1;
    [SerializeField] Slider startSlide2;
    [SerializeField] TMP_InputField input;
    [SerializeField] TextMeshProUGUI welcomeText;
    [SerializeField] TextMeshProUGUI resetText;
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu"){
            if(options.activeSelf){
                if(Input.GetKey(KeyCode.LeftShift)){
                    if(Input.GetKeyDown(KeyCode.R)){
                        PlayerPrefs.DeleteAll();
                        resetText.color = Color.red;
                        resetText.text = "Gone. All of it.";
                    }
                }
            }

            acceptSlider.value = heldSpaceTime;
            startSlide1.value = heldSpaceTime;
            startSlide2.value= heldSpaceTime;

            if(Input.GetKeyDown(KeyCode.S) && !warning.activeSelf && !firstTimePlaying.activeSelf){
                mainMenuSelected++;
                if(mainMenuSelected <= 4){
                    if(mainMenuSelected != 3){
                        options.SetActive(false);
                    }

                    if(mainMenuSelected != 2){
                        entities.SetActive(false);
                    }
                    MainMenuChangeColor(mainMenuSelected);
                }else{
                    mainMenuSelected = 1;
                    MainMenuChangeColor(mainMenuSelected);
                }
            }else if(Input.GetKeyDown(KeyCode.W) && !warning.activeSelf && !firstTimePlaying.activeSelf){
                mainMenuSelected--;
                if(mainMenuSelected >= 1){
                    if(mainMenuSelected != 3){
                        options.SetActive(false);
                    }

                    if(mainMenuSelected != 2){
                        entities.SetActive(false);
                    }
                    MainMenuChangeColor(mainMenuSelected);
                }else{
                    mainMenuSelected = 4;
                    MainMenuChangeColor(mainMenuSelected);
                }
            }else if(Input.GetKeyDown(KeyCode.A) && !warning.activeSelf && !firstTimePlaying.activeSelf && entities.activeSelf){
                entitySelected--;
                if(entitySelected >= 1){
                    MainMenuChangeEntity(entitySelected);
                }else{
                    entitySelected = 3;
                    MainMenuChangeEntity(entitySelected);
                }
            }else if(Input.GetKeyDown(KeyCode.D) && !warning.activeSelf && !firstTimePlaying.activeSelf && entities.activeSelf){
                entitySelected++;
                if(entitySelected <= 3){
                    MainMenuChangeEntity(entitySelected);
                }else{
                    entitySelected = 1;
                    MainMenuChangeEntity(entitySelected);
                }
            }else if(Input.GetKey(KeyCode.Space)){
                if(warning.activeSelf){
                    warning.SetActive(false);
                }else if(firstTimePlaying.activeSelf){
                    heldSpaceTime += Time.deltaTime/2;
                    if(firstTimePlaying.activeSelf && heldSpaceTime >= 1){
                        firstTimePlaying.SetActive(false);
                        PlayerPrefs.SetString("Username", input.text);
                        introTextsOn = true;
                        introTexts.SetActive(true);
                        ChangeUserName();
                        heldSpaceTime = 0;
                    }
                }else{
                    if(mainMenuSelected == 1){
                        heldSpaceTime += Time.deltaTime;
                        if(heldSpaceTime >= 1){
                            MainMenuPick(mainMenuSelected);
                            heldSpaceTime = 0;
                        }
                    }else{
                        if(Input.GetKeyDown(KeyCode.Space)){
                            MainMenuPick(mainMenuSelected);
                        }
                    }
                }
            }else if(Input.GetKeyUp(KeyCode.Space)){
                    heldSpaceTime = 0;
            }
            MainMenuChangeColor(mainMenuSelected);
        }

        if(SceneManager.GetActiveScene().name == "Game1 1"){
        if((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && SceneManager.GetActiveScene().name == "Game1 1" && GameController.Instance.fadeCanvasGroup.alpha < 0.5){
            ChangePauseMode();
        }
        }

        if(endMessage != null){
            if(blinkImage == null && !alreadyBlinking){
                alreadyBlinking = true;
                blinkImage = StartCoroutine(BlinkImage());
            }
        }
    }

    void ChangeUserName()
    {
        string playerName = PlayerPrefs.HasKey("Username") ? PlayerPrefs.GetString("Username") : "Worker";
        welcomeText.text = playerName + ", welcome to";
    }


    public void MainMenuChangeColor(int selected){
        switch(selected){
            case 1:
                mainMenuButtons[0].color = Color.white; // For could be used, but its unnecessary if we know there are always 3 buttons and only 2 of them set to basic color
                mainMenuButtons[1].color = Color.gray;
                mainMenuButtons[2].color = Color.gray;
                mainMenuButtons[3].color = Color.gray;
                break;
            case 2:
                mainMenuButtons[0].color = Color.gray;
                mainMenuButtons[1].color = Color.white;
                mainMenuButtons[2].color = Color.gray;
                mainMenuButtons[3].color = Color.gray;
                break;
            case 3:
                mainMenuButtons[0].color = Color.gray;
                mainMenuButtons[1].color = Color.gray;
                mainMenuButtons[2].color = Color.white;
                mainMenuButtons[3].color = Color.gray;
                break;
            case 4:
                mainMenuButtons[0].color = Color.gray;
                mainMenuButtons[1].color = Color.gray;
                mainMenuButtons[2].color = Color.gray;
                mainMenuButtons[3].color = Color.white;
                break;
        }
    }

    public void MainMenuChangeEntity(int selected){
        switch(selected){
            case 1:
                followerText.SetActive(true);
                followerTitle.color = Color.white;
                driftText.SetActive(false);
                driftTitle.color = Color.gray;
                sentinelText.SetActive(false);
                sentinelTitle.color = Color.gray;
            break;
            case 2:
                followerText.SetActive(false);
                followerTitle.color = Color.gray;
                driftText.SetActive(true);
                driftTitle.color = Color.white;
                sentinelText.SetActive(false);
                sentinelTitle.color = Color.gray;
            break;
            case 3:
                followerText.SetActive(false);
                followerTitle.color = Color.gray;
                driftText.SetActive(false);
                driftTitle.color = Color.gray;
                sentinelText.SetActive(true);
                sentinelTitle.color = Color.white;
            break;
        }
    }

    public void MainMenuPick(int selected){
        switch(selected){
            case 1:
                StartGame();
                break;
            case 2:
                Entities();
                break;
            case 3:
                Options();
                break;
            case 4:
                Quit();
                break;
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
        }
    }
    
    public void TheEnd(string endMessageGot){
        endMessage = endMessageGot;
        endText.text = endMessage;
        SceneManager.LoadScene (sceneName:"TheEnd");
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
            StartCoroutine(ambientCont.FadeOutBoth());
            AudioListener.pause = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
    }

    void ResumeGame(){
        StartCoroutine(ambientCont.FadeBackIn());
        AudioListener.pause = false;
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
        entities.SetActive(false);
        options.SetActive(!options.activeSelf);
    }

    public void Entities(){
        options.SetActive(false);
        entities.SetActive(!entities.activeSelf);
    }

    public void SetQuality(int qualityIndex){
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Graphics", qualityIndex);
    }

    public void SetFullscreen(bool fullscreen){
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("Fullscreen", System.Convert.ToInt32(fullscreen));
    }

    public void SetMasterVolume(float level){
        //audioMixer.SetFloat("masterVolume", level*2);  // -40 to 0 range
        audioMixer.SetFloat("masterVolume", Mathf.Lerp(-20f, 0f, level / 20f)); // Better "0 to 100" difference (above method isn't "linear" - ig that's the name)
        PlayerPrefs.SetFloat("Volume_Master", level);
    }

    public void SetSFXVolume(float level){
        //audioMixer.SetFloat("soundsVolume", level*2);
        audioMixer.SetFloat("soundsVolume", Mathf.Lerp(-20f, 0f, level / 20f));
        PlayerPrefs.SetFloat("Volume_SFX", level);
    }

    public void SetSirenVolume(float level){
        //audioMixer.SetFloat("sirensVolume", level*2);
        audioMixer.SetFloat("sirensVolume", Mathf.Lerp(-20f, 0f, level / 20f));
        PlayerPrefs.SetFloat("Volume_Siren", level);
    }

    public void SetAmbientVolume(float level){
        //audioMixer.SetFloat("ambientVolume", level*2);
        audioMixer.SetFloat("ambientVolume", Mathf.Lerp(-20f, 0f, level / 20f));
        PlayerPrefs.SetFloat("Volume_Ambient", level);
    }
}
