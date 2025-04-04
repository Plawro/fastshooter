using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.ProBuilder.MeshOperations;

public class LockingDisplay : MonoBehaviour
{
    [SerializeField] GameObject playerObject;
    [SerializeField] RectTransform currentFrequency;
    public RectTransform targetFrequency;
    [SerializeField] RectTransform squareScreenArea;
    public TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI coordsText;
    [SerializeField] StatsScreen rectangularScreenController;
    
    [SerializeField] private AudioClip bootSound;


    [SerializeField] Transform gameUI;
    [SerializeField] Transform preGameUI;
    [SerializeField] Transform activateText;
    [SerializeField] Transform minigameUI;
    [SerializeField] Transform activateTextTC;

    [SerializeField] bool isLocked = false;
    public bool isSearching = true;
    private Coroutine lockingCoroutine = null;
    private Coroutine searchBlinkCoroutine = null;
    [SerializeField] AudioSource audioSource;
    private Coroutine blinkCoroutine;
    private bool isBlinking = true;
    [SerializeField] TowerController towerController;
    private Coroutine checkAntennaRoutine;
    [SerializeField] TMPro.TMP_FontAsset easvhsFont;
    bool soundPlayed;
    float soundTimer = 0;

    public Coroutine newTarget;

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
            if(Input.GetKeyDown(KeyCode.F) && GameController.Instance.vanLeft && playerObject.GetComponent<CameraTowerController>().nowInteractingWith == "LockingDisplay"){
                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    audioSource.PlayOneShot(bootSound);
                    activateText.gameObject.SetActive(false);
                    activateTextTC.gameObject.SetActive(false);
                    GameController.Instance.gameStarted = true;
                }
                StartSearching(); // Start in searching mode
                gameUI.gameObject.SetActive(true);
                preGameUI.gameObject.SetActive(false);
                activateText.gameObject.SetActive(false);
                minigameUI.gameObject.SetActive(true);
            }
        }

        if (isSearching && gameUI.gameObject.activeSelf && playerObject.GetComponent<CameraTowerController>().nowInteractingWith == "LockingDisplay")
        {
            HandleInput();
        }

        if(preGameUI.gameObject.activeSelf == false){
            coordsText.text = "X:" + (currentFrequency.transform.localPosition.x / 3.4 + 150).ToString("F0") + " " + "Y:" + (currentFrequency.transform.localPosition.y / 3.4 + 150).ToString("F0");
        }

        ClampCurrentFrequencyPosition(); // Ensure current frequency stays within bounds
    }

    private void HandleInput()
    {
        float moveSpeed = 120f * Time.deltaTime;
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
            if (lockingCoroutine == null){
                statusText.text = "SYNCING";
                lockingCoroutine = StartCoroutine(LockingProcess());
            }
        }
        else
        {
            if (lockingCoroutine != null)
            {
                StopCoroutine(lockingCoroutine);
                lockingCoroutine = null;
                statusText.text = "STANDBY";
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

    void LateUpdate()
    {
        if(soundPlayed){
            soundTimer += Time.deltaTime;
        }else{
            soundTimer = 0;
        }
        if(soundTimer > 6){
            soundTimer = 0;
            soundPlayed = false;
        }
    }


    private IEnumerator LockingProcess()
    {
        float lockTime = 3f;
        float elapsed = 0f;
        statusText.text = "LOCKING";
        while (elapsed < lockTime)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(targetFrequency, currentFrequency.position, null)){
                if(!audioSource.isPlaying && !soundPlayed){
                    audioSource.Play();
                    soundPlayed = true;
                }
            }else{
                statusText.text = "STANDBY";
                lockingCoroutine = null;
                if(statusText.text != "LOCKED"){
                    audioSource.Stop();
                    soundPlayed = false;
                }
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        // Lock is successful
        isLocked = true;
        isSearching = false;
        statusText.text = "LOCKED";
        StopSearchingAnimation(); // Stop the blinking animation
        // Random chance to break antenna
        if (Random.Range(1,10) == 2)
        {
            print("2");
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
        statusText.text = "STANDBY";

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
        foreach (Transform child in targetFrequency.parent)
{
        if (child.name.StartsWith("FakeFreq_"))
        {
            Destroy(child.gameObject);
        }
    }
        Debug.Log(towerController.isAntennaBroken + GameController.Instance.DCuploader.CheckCapsule() + GameController.Instance.DCuploader.CheckCapsuleMode());
        while (towerController.isAntennaBroken || GameController.Instance.DCuploader.CheckCapsule() == "Empty" || GameController.Instance.DCuploader.CheckCapsuleMode() == 1 || GameController.Instance.DCuploader.CheckCapsuleMode() == 2 || GameController.Instance.DCuploader.CheckCapsuleMode() == 3)
        {
            yield return new WaitForSeconds(2f);
        }
        // Once the antenna is not broken, start the other coroutine
        yield return newTarget = StartCoroutine(SpawnNewTargetFrequency());
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
        foreach (Transform child in targetFrequency.parent)
        {
            if (child.name.StartsWith("FakeFreq_"))
            {
                Destroy(child.gameObject);
            }
        }
        int cloneCount = Random.Range(2, 5); // Random between 2 and 4 clones
        for (int i = 0; i < cloneCount; i++)
        {
            StartCoroutine(SpawnFakeFrequency());
        }
        yield return new WaitForSeconds(Random.Range(1f, 6f)); // Random delay between 1 and 6 seconds

        if (isSearching && newTarget != null)
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
        newTarget = null;
        yield break;
    }


    IEnumerator SpawnFakeFrequency()
    {
        yield return new WaitForSeconds(Random.Range(2,6));
        // Create a new empty GameObject for the fake frequency
        GameObject fakeFreq = new GameObject("FakeFreq_" + Random.Range(1000, 9999), typeof(RectTransform), typeof(Image));
        fakeFreq.transform.SetParent(targetFrequency.parent, false); // Parent it to the same UI canvas

        RectTransform fakeRect = fakeFreq.GetComponent<RectTransform>();
        fakeRect.sizeDelta = targetFrequency.sizeDelta; // Match the size of the original

        // Set random position within screen bounds
        float halfWidth = 125f / 2;
        float halfHeight = 125f / 2;
        fakeRect.anchoredPosition = new Vector2(
            Random.Range(-squareScreenArea.rect.width / 2 + halfWidth, squareScreenArea.rect.width / 2 - halfWidth),
            Random.Range(-squareScreenArea.rect.height / 2 + halfHeight, squareScreenArea.rect.height / 2 - halfHeight)
        );

        // Set the Image component to gray
        Image img = fakeFreq.GetComponent<Image>();
        img.sprite = targetFrequency.GetComponent<Image>().sprite; // Use the same sprite
        img.color = Color.gray; // Change color to gray

        GameObject textObj = new GameObject("FakeFreq_Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        textObj.transform.SetParent(fakeFreq.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(70f, 20f); // Set a reasonable size for the text
        textRect.anchorMin = new Vector2(0f, 0f); // Bottom-left corner
        textRect.anchorMax = new Vector2(0f, 0f);
        textRect.pivot = new Vector2(0f, 0f);
        textRect.anchoredPosition = new Vector2(5f, 5f); // Offset from bottom-left corner

        TMPro.TextMeshProUGUI textComponent = textObj.GetComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = "UNKNOWN";
        textComponent.fontSize = 10f;
        textComponent.alignment = TMPro.TextAlignmentOptions.BottomLeft;
        if (easvhsFont != null)
        {
            textComponent.font = easvhsFont;
        }
        else
        {
            Debug.LogWarning("Font not assigned in Inspector! Assign easvhsFont manually.");
        }

        // Ensure it's visible
        fakeFreq.SetActive(true);
        yield return null;
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
