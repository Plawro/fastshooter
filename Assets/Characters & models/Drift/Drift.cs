using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drift : MonoBehaviour
{
    int enemyPosition = 2; // Starts in the middle position (2)
    bool isVisibleWarning;
    public SurveillanceScreen surveillanceScreen;
    [SerializeField] GameObject playerObject;
    Coroutine currentCoroutine;
    [SerializeField] AudioClip jumpscareSound;
    [SerializeField] AudioSource musicAudioSource;

    [SerializeField] GameObject leftDrift;
    [SerializeField] GameObject rightDrift;
    Coroutine enemyCoroutine;
    bool canChange = true;

    void Start()
    {
        StartCoroutine(CheckAndStartEnemyMovement());
    }

IEnumerator CheckAndStartEnemyMovement()
{
    while (true)
    {
        if (canChange && GameController.Instance.gameStarted && enemyCoroutine == null)
        {
            enemyCoroutine = StartCoroutine(EnemyMovement());
        }
        yield return null;
    }
}

IEnumerator EnemyMovement()
{
    while (canChange && GameController.Instance.gameStarted)
    {
        
        // Wait until isActive12 is true
        while (playerObject.activeSelf == true)
        {
            yield return null; // Wait for the next frame and recheck
        }

        yield return new WaitForSeconds(Random.Range(3f, 6f)); // Delay between movements

        // Randomly move enemy (+1 or -1)
        enemyPosition += Random.Range(0, 2) == 0 ? -1 : 1;

        // Clamp position between 0 and 4
        enemyPosition = Mathf.Clamp(enemyPosition, 0, 4);

        // Check if enemy is in a visible position
        isVisibleWarning = (enemyPosition == 0 || enemyPosition == 1 || enemyPosition == 2 || enemyPosition == 3 || enemyPosition == 4);
        if(enemyPosition == 0){
            leftDrift.SetActive(true);
            rightDrift.SetActive(false);
        }else if(enemyPosition == 4){
            rightDrift.SetActive(true);
            leftDrift.SetActive(false);
        }else{
            leftDrift.SetActive(false);
            rightDrift.SetActive(false);
        }
        
        if (isVisibleWarning)
        {
            surveillanceScreen.TriggerWarning(enemyPosition);
        }
    }
}


void Update()
{
    if (playerObject.activeSelf)
    {
        if(enemyPosition == 0 || enemyPosition == 4){
            if(currentCoroutine == null){
                currentCoroutine = StartCoroutine(PlayerDetected());
            }
            
        }
    }
}

public void Flash(int rotation){
    if(currentCoroutine != null) StopCoroutine(currentCoroutine);
    currentCoroutine = null;

    if(rotation == -180){
        leftDrift.SetActive(false);
        enemyPosition = 1;
        StartCoroutine(Flashed());
    }else{
        rightDrift.SetActive(false);
        enemyPosition = 3;
        StartCoroutine(Flashed());
    }
}

IEnumerator Flashed(){
    canChange = false;
    yield return new WaitForSeconds(10);
    canChange = true;
    StartCoroutine(EnemyMovement());
    yield return null;
}

float timer;
IEnumerator PlayerDetected(){
    while(playerObject.activeSelf){
        timer += 1;
        if(timer >= 10){
            if(!musicAudioSource.isPlaying){
                musicAudioSource.PlayOneShot(jumpscareSound);
            }
            if(enemyPosition == 0){
                GameController.Instance.Jumpscare("Drift1");
            }else{
                GameController.Instance.Jumpscare("Drift2");
            }
        }
        yield return new WaitForSeconds(0.5f);
    }
    currentCoroutine = null;
    yield return null;
}

}
