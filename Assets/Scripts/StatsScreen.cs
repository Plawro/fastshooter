using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsScreen : MonoBehaviour
{  
    public GameObject playerObject;
     public Image[] progressBarImages; // Array of 26 images representing the progress bar
    public TextMeshProUGUI dataPackCountText;
    public TextMeshProUGUI countdownText; // Antenna warning text
    public LockingDisplay squareScreenController;
    public TowerController towerController;

    private int dataPackCount = 0;
    private bool isTransferringData = false;
    private Coroutine dataTransferCoroutine = null;
    private Coroutine countdownCoroutine = null;
    public AudioSource audioSource;
    float progress = 0;

    private void Start()
    {
        ResetProgressBar();
        countdownText.gameObject.SetActive(false);
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
        int numberOfImages = progressBarImages.Length;

        while (progress < 1f)
        {
            if (towerController.isAntennaBroken)
            {
                yield break; // Exit if antenna is broken
            }

            progress += Time.deltaTime / transferDuration;
            int currentIndex = Mathf.FloorToInt(progress * numberOfImages);
            if(GameController.Instance.DCuploader.transform.childCount != 3){
                isTransferringData = false;
                StopCoroutine(dataTransferCoroutine);
                ResetProgressBar();
                progress = 0;
                squareScreenController.StartSearching();
                //Set color to red
            }
            UpdateProgressBar(currentIndex);
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
        countdownText.text = $"Antenna broken! Time till signal lose: {countdown}";
        countdownText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            /*if (Input.GetKeyDown(KeyCode.P))
            {
                countdownText.gameObject.SetActive(false);
                towerController.isAntennaBroken = false;
                dataTransferCoroutine = StartCoroutine(DataTransferProgress()); // Resume data transfer
                yield break;
            }*/

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
