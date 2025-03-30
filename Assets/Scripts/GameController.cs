using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set;}
    
    public bool gameStarted = false; // STARTS ENEMIES, ETC.


    [Header("Intro")]
    [SerializeField] GameObject[] tutorialTexts;
    [SerializeField] TextMeshProUGUI[] tasks;
    [SerializeField] TextMeshProUGUI taskTextTime;
    [SerializeField] public GameObject van;
    [SerializeField] public AudioSource vanSource;
    public bool vanLeft = false;
    public bool touchedVan = false;
    public bool vanHereAgain = false;
    [SerializeField] bool introTextsOn = false;

    [Header("Generator stuff")]
    [SerializeField] public GameObject[] lights;
    [SerializeField] public bool isGeneratorDead = false;


    [Header("References")]
    [SerializeField] public DCUploaderController DCuploader;
    [SerializeField] public PauseMenu pauseMenu;
    [SerializeField] public PlayerInteractions playerInteractions;
    [SerializeField] public GameObject exitScreenButton;
    [SerializeField] public RectTransform timeMenu;
    Vector2 hiddenPos = new Vector2(-370,-90);
    Vector2 visiblePos = new Vector2(-170,-90);
    bool isOpen;

    [Header("Jumpscare related")]
    [SerializeField] CinemachineVirtualCamera jumpscareCameraFollower;
    public CinemachineVirtualCamera jumpscareCameraDrift1;
    [SerializeField] CinemachineVirtualCamera jumpscareCameraDrift2;
    [SerializeField] CinemachineVirtualCamera jumpscareCameraSentinel;
    [SerializeField] CinemachineVirtualCamera jumpscareCameraCargoMule;
    [SerializeField] CinemachineBrain cameraBrain;
    bool wasJumpscared = false;
    bool isSceneLoading = false;
    [SerializeField] GameObject playerLight;

    [Header("Camera related")]
    public CinemachineVirtualCamera activeVirtualCamera;
    [SerializeField] public Transform playerHallway;
    private Vector3 hallwayRot = new Vector3(0,0,0);
    [SerializeField] public Transform playerTower;
    private Vector3 towerRot = new Vector3(0,0,0);
    [SerializeField] public Transform playerPowerPlant;
    private Vector3 powerPlantRot = new Vector3(0,0,0);
    [SerializeField] public Transform playerControlPanel;
    private Vector3 controlPanelRot = new Vector3(0,0,0);
    [SerializeField] public CinemachineVirtualCamera playerCamera;
    [SerializeField] public Transform playerObject; // Enable or disable outside body
    [SerializeField] public Collider walkInsideCollider;
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] DoorController mainDoorCont;
    private string mode = "tower";
    [SerializeField] TextMeshProUGUI crosshair;
    [SerializeField] public string crosshairSymbol = " ";
    [SerializeField] Transform inventory;
    private Vector3 inventoryOffset = new Vector3(-0.7f,-0.7f,1.25f);
    [SerializeField] AmbientController ambient;
    public bool canMove;
    int gameTime = 20 * 60; // 20:00
    [SerializeField] AudioSource lightAudioSource;
    [SerializeField] AudioClip lightsOff;
    [SerializeField] AudioClip lightsOn;
    [SerializeField] public AudioSource door;
    [SerializeField] public AudioClip doorSound;
    [SerializeField] AudioClip easterEggSound;
    bool isPlayingEasterEgg = false;
    public CanvasGroup fadeCanvasGroup; // Fading image CanvasGroup
    float fadeDuration = 0.2f;   // Duration of fade in/out for out/in
    [SerializeField] AmbientController ambientController;
    Coroutine ambientVictory;

    void Start(){ // Player must survive until 6:00 and also must do all the tasks until that time
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        van.SetActive(true); // On start tutorial, after tutorial, interact with it to make it go away and start the game
        canMove = true;
        timeMenu.anchoredPosition = hiddenPos;
        isOpen = false;
        timeMenu.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "20:00";
        gameTime = 20 * 60;
        Time.timeScale = 1;
        StartCoroutine(ClockCoroutine());
    }

    
    public void RemoveTutorial(){
        vanLeft = true;
        tasks[0].color = Color.green;
        canMove = false;
        vanSource.Play();
        StartCoroutine(FadeOut());
        StartCoroutine(HideVan());
    }

    IEnumerator HideVan(){
        yield return new WaitForSeconds(8);
        van.SetActive(false);
        foreach(GameObject text in tutorialTexts){
            text.SetActive(false);
        }
        StartCoroutine(FadeIn());
        canMove = true;
        yield break;
    }
    

    string LeadingZero (int n){
     return n.ToString().PadLeft(2, '0');
    }

    private IEnumerator ClockCoroutine()
    {
        while (true)
        {
            if (gameStarted)
            {
                string hours = LeadingZero(gameTime / 60);  // Get hours
                string minutes = LeadingZero(gameTime % 60);  // Get minutes

                // Round minutes to the nearest 10
                minutes = ((gameTime % 60 / 10) * 10).ToString();
                if(minutes == "0"){
                    minutes = "00";
                }

                timeMenu.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{hours}:{minutes}";

                if (gameTime / 60 == 6 && gameTime % 60 == 0)
                {
                        taskTextTime.color = Color.green;
                        if(ambientVictory == null){
                            GameController.Instance.van.SetActive(true);
                            GameController.Instance.vanHereAgain = true;
                            ambientVictory = StartCoroutine(ambientController.VanIsHere()); // 6:00 starts
                        }
                    yield break; // Stop the coroutine when the event happens
                }

                yield return new WaitForSeconds(33f); // 33s = +30mins -> 10 minutes until 6:00

                gameTime += 30; // Increase time in 10-minute steps

                // Ensure time wraps around at midnight
                if (gameTime >= 24 * 60)
                    gameTime = 0;
            }
            else
            {
                yield return null; // Wait until gameStarted is true
            }
        }
    }

    void Update(){
        if(Input.GetKey(KeyCode.LeftShift)){
            if(Input.GetKey(KeyCode.LeftControl)){
                if(Input.GetKey(KeyCode.LeftAlt)){
                    if(Input.GetKey(KeyCode.Space) && !isPlayingEasterEgg){
                        isPlayingEasterEgg = true;
                        door.PlayOneShot(easterEggSound);
                    }}}
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            isOpen = true;
        }
        else
        {
            isOpen = false;
        }

        if(timeMenu.anchoredPosition.x <= -179 || !isOpen){
        timeMenu.anchoredPosition = Vector2.Lerp(
            timeMenu.anchoredPosition,
            isOpen ? visiblePos : hiddenPos,
            Time.deltaTime * 8
        );
        }

    }

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
        playerCamera.transform.localRotation = Quaternion.Euler(-7,0,0); // Set camera rotation on start
    }

    public void ExitScreen(){
        if(playerTower.gameObject.activeSelf){
            playerTower.GetComponent<CameraTowerController>().SwitchToMainCamera();
            playerTower.GetComponent<CameraTowerController>().nowInteractingWith = "";
            
        }else if(playerPowerPlant.gameObject.activeSelf){
            playerPowerPlant.GetComponent<CameraPowerPlantController>().SwitchToMainCamera();
            playerPowerPlant.GetComponent<CameraPowerPlantController>().nowInteractingWith = "";
        }else if(playerControlPanel.gameObject.activeSelf){
            playerControlPanel.GetComponent<CameraControlPanelController>().SwitchToMainCamera();
            playerControlPanel.GetComponent<CameraControlPanelController>().nowInteractingWith = "";
        }
        GameController.Instance.exitScreenButton.SetActive(false);
    }

    [SerializeField] GameObject[] turnOffItems;
    public void KillGenerator(){
        isGeneratorDead = true;
        SwitchAllLights(false);
        foreach (var turnOff in turnOffItems){
            turnOff.SetActive(false);
        }
    }

    public void ReviveGenerator(){
        isGeneratorDead = false;
        SwitchAllLights(true);
        foreach (var turnOff in turnOffItems){
            turnOff.SetActive(true);
        }
    }
    bool playedSound = false;
    bool lastPlayed = false;
    Coroutine playLightSound;
    public void SwitchAllLights(bool turnMode){
        if(turnMode){
            if(!isGeneratorDead){
                if(playLightSound == null && (playedSound == false || lastPlayed != turnMode)){
                    playLightSound = StartCoroutine(PlayLightSound(turnMode)); 
                    playedSound = true;
                }
                lastPlayed = turnMode;
                foreach (var light in lights){
                    light.gameObject.SetActive(turnMode);
                    if(ambientController.stormStarted){
                        light.gameObject.transform.parent.GetComponent<Renderer>().material.color = Color.red;
                        light.gameObject.transform.parent.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
                    }else{
                        light.gameObject.transform.parent.GetComponent<Renderer>().material.color = new Color(247, 243, 145);
                        light.gameObject.transform.parent.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(191, 187, 144));
                    }
                }
                playedSound = false;
            }
        }else{
            if(playLightSound == null){
                playLightSound = StartCoroutine(PlayLightSound(turnMode));
            }

            foreach (var light in lights){
                light.gameObject.SetActive(turnMode);
                light.gameObject.transform.parent.GetComponent<Renderer>().material.color = Color.black;
                light.gameObject.transform.parent.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    public void PlayOnLightsSound(){
        StartCoroutine(PlayLightSound(true));
    }

    IEnumerator PlayLightSound(bool turnMode){
        if(turnMode && !playerObject.gameObject.activeSelf){
            lightAudioSource.PlayOneShot(lightsOn);
        }else if(!turnMode && !playerObject.gameObject.activeSelf){
            lightAudioSource.PlayOneShot(lightsOff);
        }
        playLightSound = null;
        yield return new WaitForSeconds(0.1f);
        yield break;
    }

    public void SwitchAllLightsColored(Color turnMode){
        foreach (var light in lights){
            if(turnMode == Color.yellow){
                light.gameObject.transform.parent.GetComponent<Renderer>().material.color = new Color(247, 243, 145);
                light.gameObject.transform.GetComponent<Light>().color = new Color(247f / 255f, 243f / 255f, 145f / 255f); //191, 187, 144
                //Or also Color RGBtoColor(int r,int g,int b)
                light.gameObject.transform.parent.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(247, 243, 145));
            }else{
                light.gameObject.transform.parent.GetComponent<Renderer>().material.color = turnMode;
                light.gameObject.transform.GetComponent<Light>().color = turnMode;
                light.gameObject.transform.parent.GetComponent<Renderer>().material.SetColor("_EmissionColor", turnMode);
            }
        }
    }

    public void SwitchModeControlPanel()
    {
        StartCoroutine(SwitchModeControlPanelCoroutine());
        
    }


    private IEnumerator SwitchModeControlPanelCoroutine()
    {
    yield return StartCoroutine(FadeOut());

    if (activeVirtualCamera != null)
    {
        activeVirtualCamera.transform.parent.gameObject.SetActive(false);
    }
    playerObject.gameObject.SetActive(false);
    activeVirtualCamera = playerControlPanel.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
    activeVirtualCamera.transform.parent.gameObject.SetActive(true);

    // Attach inventory to camera and position/rotate relative to it
    inventory.transform.SetParent(activeVirtualCamera.transform);
    inventory.transform.localPosition = inventoryOffset; // Local offset
    inventory.transform.localRotation = Quaternion.Euler(0, 180, 0); // Reset with specific rotation
    inventory.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

    yield return null;
    yield return new WaitForSeconds(1f);
    yield return StartCoroutine(FadeIn());
    }

    public void SwitchModeHallway(bool fadeOut, bool playDoorSound)
    {
        StartCoroutine(SwitchModeHallwayCoroutine(fadeOut, playDoorSound));
    }

    public void PlaySound(AudioClip clip){
        door.PlayOneShot(clip);
    }


    private IEnumerator SwitchModeHallwayCoroutine(bool fadeOut, bool playDoorSound)
    {
        if(fadeOut){
            yield return StartCoroutine(FadeOut());
        }
    if (activeVirtualCamera != null)
    {
        activeVirtualCamera.transform.parent.gameObject.SetActive(false);
    }
    playerObject.gameObject.SetActive(false);
    activeVirtualCamera = playerHallway.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
    activeVirtualCamera.transform.parent.gameObject.SetActive(true);
    activeVirtualCamera.transform.localRotation = Quaternion.Euler(hallwayRot);
    mainDoorCont.ChangeDoorMode(false);

    // Attach inventory to camera and position/rotate relative to it
    inventory.transform.SetParent(activeVirtualCamera.transform);
    inventory.transform.localPosition = inventoryOffset; // Local offset
    inventory.transform.localRotation = Quaternion.Euler(0, 180, 0); // Reset with specific rotation
    inventory.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);


    Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            crosshair.enabled = false;
            if(fadeOut){
                if(playDoorSound){
                    door.PlayOneShot(doorSound);
                }
                yield return new WaitForSeconds(1);
                if(!ambient.isIndoors){
                    ambient.SetInside();
                }
                yield return StartCoroutine(FadeIn());
            }else{
                yield return null;
            }
    }


    public void SwitchModeTower()
    {
        StartCoroutine(SwitchModeTowerCoroutine());
        
    }

    private IEnumerator SwitchModeTowerCoroutine()
    {
    yield return StartCoroutine(FadeOut());

    if (activeVirtualCamera != null)
        {
            activeVirtualCamera.transform.parent.gameObject.SetActive(false);
        }
        playerObject.gameObject.SetActive(false);
        activeVirtualCamera = playerTower.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        activeVirtualCamera.transform.parent.gameObject.SetActive(true);
        activeVirtualCamera.transform.parent.rotation=Quaternion.Euler(towerRot);

        // Attach inventory to camera and position/rotate relative to it
        inventory.transform.SetParent(activeVirtualCamera.transform);
        inventory.transform.localPosition = inventoryOffset; // Local offset
        inventory.transform.localRotation = Quaternion.Euler(0, 180, 0); // Reset with specific rotation
        inventory.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        crosshair.enabled = false;
        door.PlayOneShot(doorSound);
        
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeIn());
    }

    public void SwitchModePowerPlant()
    {
        StartCoroutine(SwitchModePowerPlantCoroutine());
        
    }


    private IEnumerator SwitchModePowerPlantCoroutine()
    {
    yield return StartCoroutine(FadeOut());

    if (activeVirtualCamera != null)
        {
            activeVirtualCamera.transform.parent.gameObject.SetActive(false);
        }
        playerObject.gameObject.SetActive(false);
        activeVirtualCamera = playerPowerPlant.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        activeVirtualCamera.transform.parent.gameObject.SetActive(true);
        activeVirtualCamera.transform.rotation=Quaternion.Euler(powerPlantRot);

        // Attach inventory to camera and position/rotate relative to it
        inventory.transform.SetParent(activeVirtualCamera.transform);
        inventory.transform.localPosition = inventoryOffset; // Local offset
        inventory.transform.localRotation = Quaternion.Euler(0, 180, 0); // Reset with specific rotation
        inventory.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            crosshair.enabled = false;
            door.PlayOneShot(doorSound);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeIn());
    }


    

    public void SwitchModeOutside()
    {
        StartCoroutine(SwitchModeOutsideCoroutine());
    }


    private IEnumerator SwitchModeOutsideCoroutine()
    {
    yield return StartCoroutine(FadeOut());
    if (activeVirtualCamera != null)
        {
            activeVirtualCamera.transform.parent.gameObject.SetActive(false);
        }
        playerObject.transform.position = playerSpawnPoint.transform.position;
        playerObject.transform.rotation = playerSpawnPoint.transform.rotation;
        playerObject.gameObject.SetActive(true);
        playerCamera.transform.parent.gameObject.SetActive(true);
        activeVirtualCamera = playerCamera;
        activeVirtualCamera.gameObject.SetActive(true);

        // Attach inventory to camera and position/rotate relative to it
        inventory.transform.SetParent(activeVirtualCamera.transform);
        inventory.transform.localPosition = inventoryOffset; // Local offset
        inventory.transform.localRotation = Quaternion.Euler(0, 180, 0); // Reset with specific rotation
        inventory.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
    
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshair.enabled = true;
        ambient.SetOutside();

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOut()
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0;
        canMove = false;
        
        while (timer < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1; // Ensure it's fully opaque
    }

    public IEnumerator FadeIn()
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0;
        

        while (timer < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        canMove = true;
        fadeCanvasGroup.alpha = 0; // Ensure it's fully transparent
    }

    public bool IsGamePaused(){
        return pauseMenu.isPaused;
    }

    public void Jumpscare(string enemyName){
        playerLight.SetActive(true);
        if(enemyName == "Follower" && !wasJumpscared){
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            playerInteractions.SwitchToVirtualCamera(jumpscareCameraFollower);
            playerInteractions.SaveLastKnownCameraPos();
            StartCoroutine(playerInteractions.ShakeCamera(0.02f));
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cameraBrain.m_DefaultBlend.m_Time = 0.4f;
            StartCoroutine(EndJumpscare("Follower"));
        }else if(enemyName == "Drift1" && !wasJumpscared){
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            playerInteractions.SwitchToVirtualCamera(jumpscareCameraDrift1);
            playerInteractions.SaveLastKnownCameraPos();
            StartCoroutine(playerInteractions.ShakeCamera(0.02f));
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cameraBrain.m_DefaultBlend.m_Time = 0.4f;
            StartCoroutine(EndJumpscare("Drift"));
        }else if(enemyName == "Drift2" && !wasJumpscared){
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            playerInteractions.SwitchToVirtualCamera(jumpscareCameraDrift2);
            playerInteractions.SaveLastKnownCameraPos();
            StartCoroutine(playerInteractions.ShakeCamera(0.02f));
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cameraBrain.m_DefaultBlend.m_Time = 0.4f;
            StartCoroutine(EndJumpscare("Drift")); 
        }else if(enemyName == "Sentinel" && !wasJumpscared){
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            playerInteractions.SwitchToVirtualCamera(jumpscareCameraSentinel);
            playerInteractions.SaveLastKnownCameraPos();
            StartCoroutine(playerInteractions.ShakeCamera(0.02f));
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cameraBrain.m_DefaultBlend.m_Time = 0.4f;
            StartCoroutine(EndJumpscare("Sentinel")); 
        }else if(enemyName == "CargoMule" && !wasJumpscared){
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            playerInteractions.SwitchToVirtualCamera(jumpscareCameraCargoMule);
            playerInteractions.SaveLastKnownCameraPos();
            StartCoroutine(playerInteractions.ShakeCamera(0.02f));
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cameraBrain.m_DefaultBlend.m_Time = 0.4f;
            StartCoroutine(EndJumpscare("CargoMule")); 
        }else if(enemyName == "Storm" && !wasJumpscared){
            StartCoroutine(EndJumpscare("Storm")); 
        }else if(enemyName == "Distance" && !wasJumpscared){
            StartCoroutine(EndJumpscare("Distance")); 
        }
    }

    public IEnumerator EndJumpscare(string enemyName){
        if (isSceneLoading) yield break;
        isSceneLoading = true;

        yield return new WaitForSeconds(2);
        wasJumpscared = true;
        if(enemyName == "Follower"){
            pauseMenu.TheEnd("You ran fast. It was pointless.");
        }else if(enemyName == "Sentinel"){
            pauseMenu.TheEnd("He just wanted to repair you.");
        }else if(enemyName == "CargoMule"){
            pauseMenu.TheEnd("Crushed into a gift box.");
        }else if(enemyName == "Storm"){
            pauseMenu.TheEnd("Glowing in the dark like a firefly.");
        }else if(enemyName == "Distance"){
            pauseMenu.TheEnd("Lost in the void... Forever.");
        }else{
            pauseMenu.TheEnd("Another one for Drift's collection.");
        }
        
        yield return null; 
    }

    public IEnumerator EndGameExplosion(){
        if (isSceneLoading) yield break;
        isSceneLoading = true;

        yield return new WaitForSeconds(6);
        wasJumpscared = true;
        pauseMenu.TheEnd("TURNED INTO RADIOACTIVE DUST");
        yield return null; 
    }
}
