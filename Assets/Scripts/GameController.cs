using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set;}
    
    

    [Header("Generator stuff")]
    [SerializeField] private GameObject[] lights;
    [SerializeField] public bool isGeneratorDead = false;


    [Header("References")]
    [SerializeField] public DCUploaderController DCuploader;
    [SerializeField] public PauseMenu pauseMenu;
    [SerializeField] private PlayerInteractions playerInteractions;

    [Header("Jumpscare related")]
    public CinemachineVirtualCamera jumpscareCameraFollower;
    public CinemachineVirtualCamera jumpscareCameraDrift;
    public CinemachineBrain cameraBrain;
    bool wasJumpscared = false;
    bool isSceneLoading = false;

    [Header("Camera related")]
    public CinemachineVirtualCamera activeVirtualCamera;
    [SerializeField] Transform playerHallway;
    private Vector3 hallwayRot = new Vector3(0,0,0);
    [SerializeField] Transform playerTower;
    private Vector3 towerRot = new Vector3(0,0,0);
    [SerializeField] Transform playerPowerPlant;
    private Vector3 powerPlantRot = new Vector3(0,0,0);
    [SerializeField] Transform playerControlPanel;
    private Vector3 controlPanelRot = new Vector3(0,0,0);
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] Transform playerObject; // Enable or disable outside body
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

    void Start(){
        canMove = true;
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


    public void KillGenerator(){
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

    public void SwitchModeHallway(bool fadeOut)
    {
        StartCoroutine(SwitchModeHallwayCoroutine(fadeOut));
    }


    private IEnumerator SwitchModeHallwayCoroutine(bool fadeOut)
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
                yield return new WaitForSeconds(1);
                ambient.SetInside();
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


    public CanvasGroup fadeCanvasGroup; // Fading image CanvasGroup
    float fadeDuration = 0.2f;   // Duration of fade in/out for out/in

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
        if(enemyName == "Follower" && !wasJumpscared){
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            playerInteractions.SwitchToVirtualCamera(jumpscareCameraFollower);
            playerInteractions.SaveLastKnownCameraPos();
            StartCoroutine(playerInteractions.ShakeCamera(0.02f));
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cameraBrain.m_DefaultBlend.m_Time = 0.4f;
            StartCoroutine(EndJumpscare());
        }else if(enemyName == "Drift" && !wasJumpscared){
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            playerInteractions.SwitchToVirtualCamera(jumpscareCameraDrift);
            playerInteractions.SaveLastKnownCameraPos();
            StartCoroutine(playerInteractions.ShakeCamera(0.02f));
            cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cameraBrain.m_DefaultBlend.m_Time = 0.4f;
            StartCoroutine(EndJumpscare());
        }
    }

    public IEnumerator EndJumpscare(){
        if (isSceneLoading) yield break;
        isSceneLoading = true;

        yield return new WaitForSeconds(2);
        wasJumpscared = true;
        pauseMenu.TheEnd("DEAD");
        yield return null; 
    }
}
