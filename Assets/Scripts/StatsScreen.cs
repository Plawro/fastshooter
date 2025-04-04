using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;

public class StatsScreen : MonoBehaviour
{  
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject cameraStatsScreen;
    [SerializeField] Image[] progressBarImages; // Array of 26 images representing the progress bar
    [SerializeField] TextMeshProUGUI dataPackCountText;
    [SerializeField] TextMeshProUGUI countdownText; // Antenna warning text
    [SerializeField] LockingDisplay squareScreenController;
    [SerializeField] TowerController towerController;

    private int dataPackCount = 0;
    private bool isTransferringData = false;
    private Coroutine dataTransferCoroutine = null;
    private Coroutine countdownCoroutine = null;
    [SerializeField] AudioSource audioSource;
    float progress = 0;
    [SerializeField] AudioClip broken;
    [SerializeField] AudioClip minigameSuccess;
    [SerializeField] Transform arrow;
    int moveDir;
    Coroutine finishedArrow;
    [SerializeField] Transform boostImage;
    string textValue;
    int size;
    int boostPower;
    int numberOfImages;

    private void Start()
    {
        ResetProgressBar();
        countdownText.gameObject.SetActive(false);
        moveDir = 1;
        boostImage.parent.transform.gameObject.SetActive(false);
        numberOfImages = progressBarImages.Length;
    }

    void Update()
    {
        if(isTransferringData){
            boostImage.parent.transform.gameObject.SetActive(true);
        }else{
            boostImage.parent.transform.gameObject.SetActive(false);
        }

        if (Input.GetKey(KeyCode.Space) && isTransferringData && !towerController.isAntennaBroken && cameraStatsScreen.GetComponent<CameraTowerController>().nowInteractingWith == "StatsScreen")
        {
            if(finishedArrow == null){
                MoveArrow();
            }
        }else if (Input.GetKeyUp(KeyCode.Space) && isTransferringData && !towerController.isAntennaBroken && cameraStatsScreen.GetComponent<CameraTowerController>().nowInteractingWith == "StatsScreen"){
            finishedArrow = StartCoroutine(StopArrow());
        }
    }


    void MoveArrow()
    {
        if(arrow.transform.localPosition.x <= -280){
            moveDir = 10;
        }else if(arrow.transform.localPosition.x >= 280){
            moveDir = -10;
        }

        arrow.transform.localPosition = new Vector2(arrow.transform.localPosition.x+moveDir*Time.deltaTime*100, 141f);
    }

    IEnumerator StopArrow(){
        CheckCollision();
        yield return new WaitForSeconds(0.2f);
        Reposition();
        arrow.transform.localPosition = new Vector2(0, 141f);
        finishedArrow = null;
        yield break;
    }
    
    void CheckCollision()
    {
        if(arrow.transform.localPosition.x - boostImage.transform.localPosition.x > -30 && arrow.transform.localPosition.x - boostImage.transform.localPosition.x < 30){
            boostPower = size+1;
            audioSource.PlayOneShot(minigameSuccess);
            int currentIndex = Mathf.FloorToInt(progress * numberOfImages);
            UpdateProgressBar(currentIndex);
        }
    }
    
    void Reposition(){
        size = Random.Range(0,3);
        switch(size){
            case 0:
                textValue = "+1";
                boostImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 60);
            break;
            case 1:
                textValue = "+2";
                boostImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(56, 60);
            break;
            case 2:
                textValue = "+3";
                boostImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 60);
            break;
        }
        boostImage.transform.localPosition = new Vector2(Random.Range(-260,260), -2f);
        boostImage.GetChild(0).GetComponent<TextMeshProUGUI>().text = textValue;
    }

    public void StartDataTransfer()
    {
        if (isTransferringData) return; // Prevent multiple data transfers
        isTransferringData = true;
        progress = 0f;
        dataTransferCoroutine = StartCoroutine(DataTransferProgress());
    }
    
    private IEnumerator DataTransferProgress()
    {
        GameController.Instance.DCuploader.CapsuleUploading();
        float transferDuration = 60f; // Assume data transfer takes 60 seconds
        numberOfImages = progressBarImages.Length;

        while (progress < 1f)
        {
            if (towerController.isAntennaBroken)
            {
                yield break; // Exit if antenna is broken
            }

            progress += Time.deltaTime * 28 / transferDuration + boostPower * 0.02f;
            boostPower = 0;
            
            int currentIndex = Mathf.FloorToInt(progress * numberOfImages);
            if(GameController.Instance.DCuploader.transform.childCount != 3){
                isTransferringData = false;
                StopCoroutine(dataTransferCoroutine);
                ResetProgressBar();
                progress = 0;
                squareScreenController.StartSearching();
            }
            UpdateProgressBar(currentIndex);
            yield return new WaitForSeconds(0.5f);
            yield return null;
        }

        CompleteDataTransfer();
    }

    public void BreakAntenna() {
    if (!towerController.isAntennaBroken) {
        towerController.isAntennaBroken = true;
        towerController.BreakAntenna();
        if (dataTransferCoroutine != null) StopCoroutine(dataTransferCoroutine); // Stop data transfer
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine); // Just in case there is already a timer going
        countdownCoroutine = StartCoroutine(CountdownToReset());
    }
}

    private IEnumerator CountdownToReset()
    {
        int countdown = 30;
        audioSource.PlayOneShot(broken);
        countdownText.text = $"Antenna broken! Time till signal lose: {countdown}";
        countdownText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            yield return new WaitForSeconds(1f);
            countdown--;
            countdownText.text = $"Antenna broken! Time till signal lose: {countdown}";
        }

        // Countdown expired, reset transfer and respawn target frequency
        squareScreenController.targetFrequency.gameObject.SetActive(false);
        squareScreenController.statusText.text = "NO SIGNAL";
        isTransferringData = false;
        progress = 0;
        ResetProgress();
    }

    public void FixAntenna() {
        countdownText.gameObject.SetActive(false);
        if (isTransferringData) {
            dataTransferCoroutine = StartCoroutine(DataTransferProgress()); // Resume data transfer
        }
        StopCoroutine(countdownCoroutine);
        countdownText.gameObject.SetActive(false); // Hide countdown
        if (!isTransferringData) {
            squareScreenController.StartSearching(); // Reset and start searching again
        }
    }

    private void UpdateProgressBar(int index)
    {
        index = Mathf.Clamp(index, 0, progressBarImages.Length - 1);
        // Activate all images up to the current index to create a filling effect (should look good)
        for (int i = 0; i <= index; i++)
        {
            progressBarImages[i].gameObject.SetActive(true);
        }
    }

    private void CompleteDataTransfer()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        isTransferringData = false;
        dataPackCount++;
        dataPackCountText.text = $"Data Packs Transferred: {dataPackCount}";
        GameController.Instance.DCuploader.CapsuleFinished();
        ResetProgressBar();
        progress = 0;
        squareScreenController.StartSearching();
    }

    public void ResetAll()
    {
        isTransferringData = false;
        ResetProgressBar();
        progress = 0;
        squareScreenController.StartSearching();
        squareScreenController.newTarget = null;
    }

    private void ResetProgress()
    {
        progress = 0;
        countdownText.text = $"Antenna broken! The download has been lost. Awaiting repair.";
        if(GameController.Instance.DCuploader.transform.childCount == 3){
            GameController.Instance.DCuploader.transform.GetChild(2).GetComponent<DataCapsule>().ChangeMode(3);
        }
        
        countdownText.gameObject.SetActive(true);
        ResetProgressBar();
    }

    public void ResetProgressBar()
    {
        foreach (Image img in progressBarImages)
        {
            img.gameObject.SetActive(false);
        }
    }

    public IEnumerator SearchBlinkAnimation()
    {
        int currentIndex = 0;
        int totalImages = progressBarImages.Length;

        while (squareScreenController.isSearching)
        {
            for (int i = 0; i < totalImages; i++)
            {
                progressBarImages[i].gameObject.SetActive(i == currentIndex);
            }

            currentIndex = (currentIndex + 1) % totalImages;
            yield return new WaitForSeconds(0.1f); // Adjust for blinking speed
        }
        
        // Reset all images when not searching
        ResetProgressBar();
    }
}
