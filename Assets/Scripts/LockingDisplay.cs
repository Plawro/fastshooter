using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LockingDisplay : MonoBehaviour
{
     public RectTransform currentFrequency;
    public RectTransform targetFrequency;
    public RectTransform squareScreenArea;
    public TextMeshProUGUI statusText;
    public StatsScreen rectangularScreenController;


    public Transform gameUI;
    public Transform preGameUI;
    public Transform activateText;
    public Transform activateTextTC;

    private bool isLocked = false;
    private bool isSearching = true;
    private Coroutine lockingCoroutine = null;
    private Coroutine searchBlinkCoroutine = null;
    private float antennaBreakChance = 0.1f; // 10% chance of antenna breaking during data transfer (Will be replaced with automatical system later)

    public AudioSource audioSource;
    private Coroutine blinkCoroutine;
    private bool isBlinking = true;

    private void Start()
    {
        StartSearching(); // Start in searching mode
        preGameUI.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
        activateText.gameObject.SetActive(true);
        activateTextTC.gameObject.SetActive(true);
        blinkCoroutine = StartCoroutine(BlinkText());
    }

    private void Update()
    {
        if(!gameUI.gameObject.activeSelf){
            if(Input.GetKeyDown(KeyCode.F)){
                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    activateText.gameObject.SetActive(false);
                    activateTextTC.gameObject.SetActive(false);
                }
                gameUI.gameObject.SetActive(true);
                preGameUI.gameObject.SetActive(false);
                activateText.gameObject.SetActive(false);
            }
        }

        if (isSearching && gameUI.gameObject.activeSelf)
        {
            HandleInput();
        }

        ClampCurrentFrequencyPosition(); // Ensure current frequency stays within bounds
    }

    private void HandleInput()
    {
        float moveSpeed = 200f * Time.deltaTime;
        Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * moveSpeed;
        currentFrequency.anchoredPosition += move;

        if (isLocked) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(targetFrequency, currentFrequency.position, null))
        {
            if (lockingCoroutine == null)
                lockingCoroutine = StartCoroutine(LockingProcess());
        }
        else
        {
            if (lockingCoroutine != null)
            {
                StopCoroutine(lockingCoroutine);
                lockingCoroutine = null;
                statusText.text = "Searching";
            }
        }
    }

    private void ClampCurrentFrequencyPosition()
    {
        // Clamp the current frequency's position within the screen bounds
        Vector2 clampedPosition = currentFrequency.anchoredPosition;
        float halfWidth = currentFrequency.rect.width / 2;
        float halfHeight = currentFrequency.rect.height / 2;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -squareScreenArea.rect.width / 2 + halfWidth, squareScreenArea.rect.width / 2 - halfWidth);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -squareScreenArea.rect.height / 2 + halfHeight, squareScreenArea.rect.height / 2 - halfHeight);

        currentFrequency.anchoredPosition = clampedPosition;
    }

    private IEnumerator LockingProcess()
    {
        float lockTime = 3f;
        float elapsed = 0f;

        while (elapsed < lockTime)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(targetFrequency, currentFrequency.position, null))
            {
                statusText.text = "Searching";
                lockingCoroutine = null;
                yield break;
            }
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Lock is successful
        isLocked = true;
        isSearching = false;
        statusText.text = "Locked";
        StopSearchingAnimation(); // Stop the blinking animation

        // Random chance to break antenna (will be replaced soon)
        if (Random.value < antennaBreakChance)
        {
            rectangularScreenController.BreakAntenna();
        }
        else
        {
            rectangularScreenController.StartDataTransfer();
        }
    }

    public void StartSearching()
    {
        isSearching = true;
        isLocked = false;
        statusText.text = "Searching";

        // Start the blinking animation for "searching" mode
        if (searchBlinkCoroutine == null)
        {
            searchBlinkCoroutine = StartCoroutine(SearchBlinkAnimation());
        }

        // Reset the target frequency to a new random position
        StartCoroutine(SpawnNewTargetFrequency());
    }


    private IEnumerator SearchBlinkAnimation()
    {
        int currentIndex = 0;
        int totalImages = rectangularScreenController.progressBarImages.Length;

        while (isSearching)
        {
            for (int i = 0; i < totalImages; i++)
            {
                rectangularScreenController.progressBarImages[i].gameObject.SetActive(i == currentIndex);
            }

            currentIndex = (currentIndex + 1) % totalImages;
            yield return new WaitForSeconds(0.1f); // Adjust for blinking speed
        }

        // Reset all images when not searching
        rectangularScreenController.ResetProgressBar();
    }

    public void StopSearchingAnimation()
    {
        if (searchBlinkCoroutine != null)
        {
            StopCoroutine(searchBlinkCoroutine);
            searchBlinkCoroutine = null;
        }
        rectangularScreenController.ResetProgressBar(); // Ensure the progress bar is reset
    }

    private IEnumerator SpawnNewTargetFrequency()
    {
        targetFrequency.gameObject.SetActive(false); // Hide target frequency initially

        yield return new WaitForSeconds(Random.Range(1f, 10f)); // Random delay between 1 and 10 seconds

        if (isSearching)
        {
            // Ensure it spawns within the screen bounds
            float halfSquareWidth = 125f / 2;
            float halfSquareHeight = 125f / 2;

            Vector2 randomPosition = new Vector2(
                Random.Range(-squareScreenArea.rect.width / 2 + halfSquareWidth, squareScreenArea.rect.width / 2 - halfSquareWidth),
                Random.Range(-squareScreenArea.rect.height / 2 + halfSquareHeight, squareScreenArea.rect.height / 2 - halfSquareHeight)
            );

            targetFrequency.anchoredPosition = randomPosition;
            targetFrequency.gameObject.SetActive(true); // Show target frequency
        }
    }

    private IEnumerator BlinkText()
    {
        while (isBlinking)
        {
            activateText.gameObject.SetActive(!activateText.gameObject.activeSelf);
            activateTextTC.gameObject.SetActive(!activateTextTC.gameObject.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
