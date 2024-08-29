using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsScreen : MonoBehaviour
{  
     public Image[] progressBarImages; // Array of 26 images representing the progress bar
    public TextMeshProUGUI dataPackCountText; // Reference to TextMeshPro for data pack count display
    public TextMeshProUGUI countdownText; // Reference to TextMeshPro for countdown display
    public LockingDisplay squareScreenController; // Reference to SquareScreenController

    private int dataPackCount = 0;
    private bool isTransferringData = false;
    private bool isAntennaBroken = false; // New flag to check if antenna is broken
    private Coroutine dataTransferCoroutine = null; // Reference to data transfer coroutine
    private Coroutine countdownCoroutine = null; // Reference to countdown coroutine

    private void Start()
    {
        ResetProgressBar(); // Initialize the progress bar to empty
        countdownText.gameObject.SetActive(false); // Hide countdown text initially
    }

    public void StartDataTransfer()
    {
        if (isTransferringData) return; // Prevent multiple data transfers
        isTransferringData = true;
        isAntennaBroken = false;
        dataTransferCoroutine = StartCoroutine(DataTransferProgress());
    }

    private IEnumerator DataTransferProgress()
    {
        float progress = 0f;
        float transferDuration = 60f; // Assume data transfer takes 5 seconds
        int numberOfImages = progressBarImages.Length;

        while (progress < 1f)
        {
            if (isAntennaBroken)
            {
                yield break; // Exit if antenna is broken
            }

            progress += Time.deltaTime / transferDuration;
            int currentIndex = Mathf.FloorToInt(progress * numberOfImages);
            UpdateProgressBar(currentIndex);
            yield return null;
        }

        CompleteDataTransfer();
    }

    public void BreakAntenna()
    {
        if (isTransferringData && !isAntennaBroken)
        {
            isAntennaBroken = true;
            StopCoroutine(dataTransferCoroutine); // Stop data transfer

            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine); // Ensure no existing countdown is running

            countdownCoroutine = StartCoroutine(CountdownToReset());
        }
    }

    private IEnumerator CountdownToReset()
    {
        countdownText.gameObject.SetActive(true);
        int countdown = 10;
        countdownText.text = $"Antenna broken! Press 'P' to fix: {countdown}";

        while (countdown > 0)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                countdownText.gameObject.SetActive(false);
                isAntennaBroken = false;
                dataTransferCoroutine = StartCoroutine(DataTransferProgress()); // Resume data transfer
                yield break;
            }

            yield return new WaitForSeconds(1f);
            countdown--;
            countdownText.text = $"Antenna broken! Press 'P' to fix: {countdown}";
        }

        // Countdown expired, reset transfer and respawn target frequency
        ResetProgressAndSearch();
    }

    private void UpdateProgressBar(int index)
    {
        index = Mathf.Clamp(index, 0, progressBarImages.Length - 1);

        // Activate all images up to the current index to create a filling effect
        for (int i = 0; i <= index; i++)
        {
            progressBarImages[i].gameObject.SetActive(true);
        }
    }

    private void CompleteDataTransfer()
    {
        isTransferringData = false;
        dataPackCount++;
        dataPackCountText.text = $"Data Packs Transferred: {dataPackCount}";
        ResetProgressBar();
        squareScreenController.StartSearching();
    }

    private void ResetProgressAndSearch()
    {
        isTransferringData = false;
        ResetProgressBar();
        countdownText.gameObject.SetActive(false); // Hide countdown
        squareScreenController.StartSearching(); // Reset and start searching again
    }

    public void ResetProgressBar()
    {
        foreach (Image img in progressBarImages)
        {
            img.gameObject.SetActive(false);
        }
    }
}
