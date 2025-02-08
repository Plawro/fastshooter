using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SentinelController : MonoBehaviour
{
    float timeTillBoot;
    float timeTillFootstep;
    Vector3 lookAtPlayerHeadRot = new Vector3(326.608459f,55.8295555f,347.688599f);
    Vector3 normalLookHeadRot = new Vector3(3.03034806f,5.1711669f,0.717486799f);
    [SerializeField] GameObject head;
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
    [SerializeField] PowerPlantController powerPlantController;
    int footstepAmmount = 0;
    float charge = 80;
    bool isOnline = false;
    Coroutine restartCoroutine;
    void Start()
    {
        timeTillBoot = Random.Range(5,30);
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
        head.transform.eulerAngles = normalLookHeadRot;
        preBoot = true;
        Boot();
    }


    IEnumerator LightFlicker(){
        StartCoroutine(BootSentinel());
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        lightToFlicker.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.02f);
        lightToFlicker.SetActive(true);
        yield break;
    }

    IEnumerator BootSentinel(){
        head.transform.eulerAngles = lookAtPlayerHeadRot;
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
        lightToFlicker.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.02f);
        lightToFlicker.SetActive(true);
        yield return new WaitForSeconds(0.02f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        sentinelHallway.SetActive(false);
        sentinelRoom.SetActive(false);
        sentinelOff.SetActive(true);
        charge = 6;
        StartCoroutine(doorPowerPlant.OpenCloseDoor());
        yield return new WaitForSeconds(0.5f);
        lightToFlicker.SetActive(true);
        isOnline = false;
            while (charge < 85 && isOnline == false)
            {
                charge += Time.deltaTime * 0.85f; // Increase charge over time
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
        lightToFlicker.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.02f);
        lightToFlicker.SetActive(true);
        yield return new WaitForSeconds(0.02f);
        lightToFlicker.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        sentinelOff.SetActive(false);
        StartCoroutine(doorPowerPlant.OpenCloseDoor());
        yield return new WaitForSeconds(0.5f);
        lightToFlicker.SetActive(true);
        isOnline = true;
        timeTillFootstep = Random.Range(7,30);
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
        GameController.Instance.Jumpscare("Sentinel");
    }

    void ResetFootstep(){
        timeTillFootstep = Random.Range(7,30);
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

            if (countdown <= 0f){
                if (GameController.Instance.playerTower.gameObject.activeSelf){
                    StartCoroutine(doorTower.OpenCloseDoor());
                    sentinelRoom.transform.localPosition = new Vector3(22.538f,0,-4.2f);
                    sentinelRoom.transform.eulerAngles = new Vector3(0,180,0);
                    sentinelRoom.SetActive(true);
                    sentinelHallway.SetActive(false);
                    if(playerCheck != null){
                        StopCoroutine(playerCheck);
                    }
                    playerCheck = StartCoroutine(CheckPlayer());

                    yield break;
                } else if (GameController.Instance.playerPowerPlant.gameObject.activeSelf){
                    StartCoroutine(doorPowerPlant.OpenCloseDoor());
                    sentinelRoom.transform.localPosition = new Vector3(22.538f,0,4.2f);
                    sentinelRoom.transform.eulerAngles = new Vector3(0,0,0);
                    sentinelRoom.SetActive(true);
                    sentinelHallway.SetActive(false);
                    if(playerCheck != null){
                        StopCoroutine(playerCheck);
                    }
                    yield return playerCheck = StartCoroutine(CheckPlayer());
                    powerPlantController.RestartGenerator();
                    yield break;
                }
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator CheckPlayer()
    {
        yield return new WaitForSeconds(2f); // Wait 2 seconds before checking
        float timeLookingAtSentinel = 0f;
        float requiredTime = 6f;
        audioSource.PlayOneShot(checking);
        while (timeLookingAtSentinel < requiredTime)
        {

            if (!isOnline)
            {
                yield break; // Stop checking if Sentinel is charging
            }
            // Get direction from player to Sentinel
            Vector3 directionToSentinel = (this.transform.position - playerCamera.transform.position).normalized;

            // Get the player's forward direction
            Vector3 playerForward = playerCamera.transform.forward;

            // Check dot product to determine if player is looking at Sentinel
            float dot = Vector3.Dot(playerForward, directionToSentinel);
            bool isLookingAtSentinel = dot > 0.15f && dot < 0.4f;
            print(dot + " " + timeLookingAtSentinel);
            if (isLookingAtSentinel)
            {
                timeLookingAtSentinel += Time.deltaTime; // Increase timer if looking
            }
            else
            {
                print("jumpscare");
                Jumpscare(); // Player looked away, trigger jumpscare
                yield break; // Stop coroutine
            }

            yield return null; // Wait for next frame
        }
        timeLookingAtSentinel = 0;
        StartCoroutine(doorPowerPlant.OpenCloseDoor());
        ResetFootstep();
        footstepAmmount = 0;
        playerCheck = null;
        sentinelRoom.SetActive(false);
        yield break;
    }

}
