using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LockingDisplay : MonoBehaviour
{
    public GameObject playerObject;
     public RectTransform currentFrequency;
    public RectTransform targetFrequency;
    public RectTransform squareScreenArea;
    public TextMeshProUGUI statusText;
    public StatsScreen rectangularScreenController;


    public Transform gameUI;
    public Transform preGameUI;
    public Transform activateText;
    public Transform activateTextTC;

    public bool isLocked = false;
    public bool isSearching = true;
    private Coroutine lockingCoroutine = null;
    private Coroutine searchBlinkCoroutine = null;
    private float antennaBreakChance = 0.1f; // 10% chance of antenna breaking during data transfer (Will be replaced with automatical system later)

    public AudioSource audioSource;
    private Coroutine blinkCoroutine;
    private bool isBlinking = true;
    public TowerController towerController;
    private Coroutine checkAntennaRoutine;

    private void Start()
    {
        targetFrequency.gameObject.SetActive(false);
        preGameUI.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
        activateText.gameObject.SetActive(true);
        activateTextTC.gameObject.SetActive(true);
        blinkCoroutine = StartCoroutine(BlinkText());
    }

    private void Update()
    {
        if(!gameUI.gameObject.activeSelf){
            if(Input.GetKeyDown(KeyCode.F) && playerObject.GetComponent<PlayerInteractions>().nowInteractingWith == "LockingDisplay"){
                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    activateText.gameObject.SetActive(false);
                    activateTextTC.gameObject.SetActive(false);
                }
                StartSearching(); // Start in searching mode
                gameUI.gameObject.SetActive(true);
                preGameUI.gameObject.SetActive(false);
                activateText.gameObject.SetActive(false);
            }
        }

        if (isSearching && gameUI.gameObject.activeSelf && playerObject.GetComponent<PlayerInteractions>().nowInteractingWith == "LockingDisplay")
        {
            HandleInput();
        }

        ClampCurrentFrequencyPosition(); // Ensure current frequency stays within bounds
    }

    private void HandleInput()
    {
        float moveSpeed = 100f * Time.deltaTime;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
        {
            vertical = 0;
        }else{
            horizontal = 0;
        }

        Vector2 move = new Vector2(horizontal, vertical) * moveSpeed;
        currentFrequency.anchoredPosition += move;

        if (isLocked) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(targetFrequency, currentFrequency.position, null) && targetFrequency.gameObject.activeSelf)
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

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -squareScreenArea.rect.width / 2 + halfWidth + 5, squareScreenArea.rect.width / 2 - halfWidth - 5);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -squareScreenArea.rect.height / 2 + halfHeight + 5, squareScreenArea.rect.height / 2 - halfHeight - 5);

        currentFrequency.anchoredPosition = clampedPosition;
    }

    private IEnumerator LockingProcess()
    {
        float lockTime = 3f;
        float elapsed = 0f;
        statusText.text = "Locking";
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
            searchBlinkCoroutine = StartCoroutine(rectangularScreenController.SearchBlinkAnimation());
        }

        checkAntennaRoutine = StartCoroutine(CheckAntennaStatus());
    }

    //Check if antenna is not broken, if not, search for new frequency
    private IEnumerator CheckAntennaStatus()
    {
        targetFrequency.gameObject.SetActive(false);
        Debug.Log(towerController.isAntennaBroken + GameController.Instance.DCuploader.CheckCapsule() + GameController.Instance.DCuploader.CheckCapsuleMode());
        while (towerController.isAntennaBroken || GameController.Instance.DCuploader.CheckCapsule() == "Empty" || GameController.Instance.DCuploader.CheckCapsuleMode() == 1 || GameController.Instance.DCuploader.CheckCapsuleMode() == 2 || GameController.Instance.DCuploader.CheckCapsuleMode() == 3)
        {
            yield return new WaitForSeconds(2f);
        }
        // Once the antenna is not broken, start the other coroutine
        yield return SpawnNewTargetFrequency();
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
