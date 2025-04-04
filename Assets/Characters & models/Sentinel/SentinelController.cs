using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SentinelController : MonoBehaviour
{
    float timeTillBoot;
    float timeTillFootstep;
    Vector3 lookAtPlayerHeadRot = new Vector3(272.69986f,201.150116f,173.773285f);
    Vector3 normalLookHeadRot = new Vector3(305.309998f,270f,90f);
    [SerializeField] GameObject head;
    [SerializeField] GameObject headPlayer;
    [SerializeField] GameObject lightToFlicker;
    Coroutine flickerLight;
    bool preBoot = true;
    [SerializeField] DoorController doorTower;
    [SerializeField] DoorController doorPowerPlant;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] GameObject sentinelOff;
    [SerializeField] GameObject sentinelHallway;
    [SerializeField] GameObject sentinelRoom; // Other room is the same, but flipped
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip footstep;
    [SerializeField] AudioClip footstep2;
    [SerializeField] AudioClip jumpscareSound;
    [SerializeField] AudioClip checking;
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform roomCamera;
    [SerializeField] PowerPlantController powerPlantController;
    int footstepAmmount = 0;
    float charge = 80;
    bool isOnline = false;
    Coroutine restartCoroutine;
    void Start()
    {
        timeTillBoot = Random.Range(5,20);
    }

    void Update()
    {
        if(timeTillBoot > 0 && GameController.Instance.gameStarted){
            timeTillBoot -= Time.deltaTime;
        }else if(preBoot && GameController.Instance.gameStarted){
            Boot();
        }else if(charge <= 5){
            if(restartCoroutine == null){
                restartCoroutine = StartCoroutine(ReStartSentinel());
            }
        }else if(isOnline && GameController.Instance.gameStarted && !preBoot){
            charge -= Time.deltaTime/2;
            statusText.text = $"{charge:F0}%";
            timeTillFootstep -= Time.deltaTime/2;

            if(timeTillFootstep < 0 && footstepAmmount < 4){
                footstepAmmount++;
                if(footstepAmmount != 3){
                    PlayFootstep();
                    ResetFootstep();
                }else{
                    AppearInHallway();
                }
            }
        }
    }

    void Boot(){
        preBoot = false;
        flickerLight = StartCoroutine(LightFlicker());
    }

    void TurnOff(){
        head.gameObject.SetActive(true);
        headPlayer.gameObject.SetActive(false);
        preBoot = true;
        Boot();
    }


    IEnumerator LightFlicker(){
        StartCoroutine(BootSentinel());
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        yield return new WaitForSeconds(0.05f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.02f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        
        yield break;
    }

    IEnumerator BootSentinel(){
        head.gameObject.SetActive(false);
        headPlayer.gameObject.SetActive(true);
        isOnline = false;
        for (int i = 0; i < 3; i++)
        {
            statusText.text = "BOOTING";
            yield return new WaitForSeconds(1f);
            statusText.text = "BOOTING.";
            yield return new WaitForSeconds(1f);
            statusText.text = "BOOTING..";
            yield return new WaitForSeconds(1f);
            statusText.text = "BOOTING...";
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(0.2f);
        statusText.text = "CONNECTING";
         yield return new WaitForSeconds(0.5f);
         isOnline = true;
        StartCoroutine(StartSentinel());
        yield break;
    }

    IEnumerator ReStartSentinel(){
        yield return new WaitForSeconds(3f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        yield return new WaitForSeconds(0.05f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.02f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        yield return new WaitForSeconds(0.02f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        sentinelHallway.SetActive(false);
        sentinelRoom.SetActive(false);
        sentinelOff.SetActive(true);
        charge = 6;
        StartCoroutine(doorPowerPlant.OpenCloseDoor());
        yield return new WaitForSeconds(0.5f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        isOnline = false;
            while (charge < 85 && isOnline == false)
            {
                charge += Time.deltaTime * 1.65f; // Increase charge over time
                statusText.text = $"{charge:F0}%";
                yield return null;
            }
            isOnline = true;
            TurnOff();
        restartCoroutine = null;
        yield break;
    }

    IEnumerator StartSentinel(){
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        yield return new WaitForSeconds(0.05f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.02f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        yield return new WaitForSeconds(0.02f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        sentinelOff.SetActive(false);
        StartCoroutine(doorPowerPlant.OpenCloseDoor());
        yield return new WaitForSeconds(0.5f);
        if(!GameController.Instance.isGeneratorDead){
        lightToFlicker.SetActive(true);
        }
        isOnline = true;
        timeTillFootstep = Random.Range(7,20);
        footstepAmmount = 0;
        yield break;
    }

    void Jumpscare(){
        if(audioSource.isPlaying){
            audioSource.Stop();
        }
        if(!audioSource.isPlaying){
            audioSource.PlayOneShot(jumpscareSound);
        }
        sentinelRoom.transform.Find("VCam").gameObject.SetActive(true);
        GameController.Instance.Jumpscare("Sentinel");
    }

    void ResetFootstep(){
        timeTillFootstep = Random.Range(7,20);
    }

    void PlayFootstep(){
        if (Random.Range(0, 2) == 0){
            audioSource.PlayOneShot(footstep);
        }
        else{
            audioSource.PlayOneShot(footstep2);
        }
    }

    bool isMovingToRoom = false;
    void AppearInHallway(){
        if(!isMovingToRoom){
            isMovingToRoom = true;
            sentinelHallway.SetActive(true);
            StartCoroutine(GetInRoom());
        }
    }
    Coroutine playerCheck;

    void GetOffRoom(string where){
        sentinelHallway.SetActive(false);
        sentinelRoom.SetActive(false);
        sentinelOff.SetActive(false);
        if(where == "tower"){
            StartCoroutine(doorTower.OpenCloseDoor());
        }else{
            StartCoroutine(doorPowerPlant.OpenCloseDoor());
        }
        timeTillFootstep = Random.Range(7,20);
        footstepAmmount = 0;
    }

    IEnumerator GetInRoom(){
        float countdown = 10f;
        float timeInHallway = 0f;
        isMovingToRoom = false;
        
        while (countdown > 0f){
            countdown -= Time.deltaTime;

            if (GameController.Instance.playerHallway.gameObject.activeSelf){
                timeInHallway += Time.deltaTime;
                if (timeInHallway >= 5f){
                    footstepAmmount = 0;
                    sentinelHallway.SetActive(false);
                    yield break;
                }
            } else {
                timeInHallway = 0f;
            }

            if(countdown <= 3 && countdown >0){
                GameController.Instance.SwitchAllLights(false);
            }

            if (countdown <= 0f){
                    GameController.Instance.SwitchAllLights(true);
                    GameController.Instance.PlayOnLightsSound();
                if (GameController.Instance.playerTower.gameObject.activeSelf){
                    StartCoroutine(doorTower.OpenCloseDoor());
                    sentinelRoom.transform.localPosition = new Vector3(22.2630005f,-1.20899963f,-3.44000244f);
                    sentinelRoom.transform.eulerAngles = new Vector3(0,-90,0);
                    sentinelRoom.SetActive(true);
                    sentinelHallway.SetActive(false);
                    if(playerCheck != null){
                        StopCoroutine(playerCheck);
                    }
                    yield return playerCheck = StartCoroutine(CheckPlayer());
                    yield return new WaitForSeconds(5);
                    GetOffRoom("tower");
                    yield break;
                } else if (GameController.Instance.playerPowerPlant.gameObject.activeSelf){
                    StartCoroutine(doorPowerPlant.OpenCloseDoor());
                    sentinelRoom.transform.localPosition = new Vector3(22.2630005f,-1.20899963f,3.44000244f);
                    sentinelRoom.transform.eulerAngles = new Vector3(0,90,0);
                    sentinelRoom.SetActive(true);
                    sentinelHallway.SetActive(false);
                    if(playerCheck != null){
                        StopCoroutine(playerCheck);
                    }
                    yield return playerCheck = StartCoroutine(CheckPlayer());
                    yield return new WaitForSeconds(5);
                    powerPlantController.RestartGenerator();
                    GetOffRoom("powerplant");
                    yield break;
                }else{
                    int goRand = Random.Range(0,1);
                    if(goRand == 0){
                            StartCoroutine(doorTower.OpenCloseDoor());
                        sentinelRoom.transform.localPosition = new Vector3(22.538f,0,-4.2f);
                        sentinelRoom.transform.eulerAngles = new Vector3(0,180,0);
                        sentinelRoom.SetActive(true);
                        sentinelHallway.SetActive(false);
                        if(playerCheck != null){
                            StopCoroutine(playerCheck);
                        }
                        print("GONE1");
                        yield return playerCheck = StartCoroutine(CheckPlayer());
                        yield return new WaitForSeconds(5);
                        GetOffRoom("tower");
                        yield break;
                    }else{
                        StartCoroutine(doorPowerPlant.OpenCloseDoor());
                        sentinelRoom.transform.localPosition = new Vector3(22.538f,0,4.2f);
                        sentinelRoom.transform.eulerAngles = new Vector3(0,0,0);
                        sentinelRoom.SetActive(true);
                        sentinelHallway.SetActive(false);
                        if(playerCheck != null){
                            StopCoroutine(playerCheck);
                        }
                        print("GONE2");
                        yield return playerCheck = StartCoroutine(CheckPlayer());
                        yield return new WaitForSeconds(5);
                        powerPlantController.RestartGenerator();
                        GetOffRoom("powerplant");
                        yield break;
                    }
                    
                }
            }

            yield return null;
        }
    }

    IEnumerator CheckPlayer(){
        yield return new WaitForSeconds(2f); // Small delay before checking (time for player to react)

        float timeLookingAtSentinel = 0f;
        float requiredTime = 6f;
        audioSource.PlayOneShot(checking);

        bool playerWasInTower = GameController.Instance.playerTower.gameObject.activeSelf;
        bool playerWasInPowerPlant = GameController.Instance.playerPowerPlant.gameObject.activeSelf;

        while (timeLookingAtSentinel < requiredTime){
            if (!isOnline){
                yield break;
            }

            // Get player's current position
            bool playerInTower = GameController.Instance.playerTower.gameObject.activeSelf;
            bool playerInPowerPlant = GameController.Instance.playerPowerPlant.gameObject.activeSelf;
            bool playerInHallway = GameController.Instance.playerHallway.gameObject.activeSelf; // Consider hallway safe
            bool playerInCP = GameController.Instance.playerControlPanel.gameObject.activeSelf;

            // Check if player LEFT the room completely
            if ((playerWasInTower && !playerInTower && !playerInHallway && !playerInCP) || 
                (playerWasInPowerPlant && !playerInPowerPlant && !playerInHallway && !playerInCP)){
                    Jumpscare();
                    yield break;
            }

            // Looking direction check
            Vector3 directionToSentinel = (this.transform.position - playerCamera.transform.position).normalized;
            Vector3 playerForward = playerCamera.transform.forward;
            float dot = Vector3.Dot(playerForward, directionToSentinel);
            bool isLookingAtSentinel = dot > -0.2f && dot < 0.4f;
            print(dot);
            if (isLookingAtSentinel){

                timeLookingAtSentinel += 0.2f; // Increase timer slower to smooth out flickers
            }
            else{
                
                timeLookingAtSentinel -= 1f; // Give the player a grace period before jumpscare
                if (timeLookingAtSentinel <= 0)
                {
                    Jumpscare();
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.2f); // Optimized to run 5 times per second instead of every frame
        }

        // Player survived, Sentinel leaves
        StartCoroutine(doorPowerPlant.OpenCloseDoor());
        ResetFootstep();
        footstepAmmount = 0;
        playerCheck = null;
        sentinelRoom.SetActive(false);
    }
}
