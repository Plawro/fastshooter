using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

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

    [Header("Drifter spawning")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] public int amountOfSpawns;
    [SerializeField] GameObject enemyPrefab;

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
        SpawnDrifts();
    }

    void SpawnDrifts(){
        if (spawnPoints.Length == 0 || enemyPrefab == null || amountOfSpawns <= 0)
        {
            Debug.LogWarning("Spawner is not set up properly!");
            return;
        }

        int spawnsToCreate = Mathf.Min(amountOfSpawns, spawnPoints.Length);

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < spawnsToCreate; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];
            Instantiate(enemyPrefab, selectedPoint.position, selectedPoint.rotation);
            availablePoints.RemoveAt(randomIndex);
        }
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

    void Update(){

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
