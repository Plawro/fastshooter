using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuEffect : MonoBehaviour
{
    public RectTransform uiElement;
    public RectTransform uiElement2;
    public float moveSpeed = 1f; // Adjust speed if needed
    private Vector2 lastMousePos;


    bool isIntroActive = true;
    public TextMeshProUGUI textDisplay; // Assign the TextMeshProUGUI
    public CanvasGroup canvasGroup; // Assign the CanvasGroup for fading
    public string[] introTexts; // Array of texts to display
    public float textSpeed = 0.05f; // Speed of text writing
    public float fadeDuration = 0.7f; // Time it takes to fade in
    private int currentTextIndex = 0;
    private bool isWriting = false;
    private bool textFullyDisplayed = false;
    private Coroutine typingCoroutine;
    [SerializeField] PauseMenu pauseMenu;

    void Start()
    {
        lastMousePos = Input.mousePosition;
    }

    void Update()
    {
        if(!pauseMenu.introTextsOn){
        Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMousePos;

        // Convert mouse movement to percentage of screen
        float percentX = (mouseDelta.x / Screen.width) * 42f;
        float percentY = (mouseDelta.y / Screen.height / 3) * 36f;

        // Apply movement to UI element
        uiElement.anchoredPosition += new Vector2(percentX * moveSpeed, percentY * moveSpeed);



        // Convert mouse movement to percentage of screen
        float percentX2 = (mouseDelta.x / Screen.width) * 32f;
        float percentY2 = (mouseDelta.y / Screen.height / 3) * 26f;

        // Apply movement to UI element
        uiElement2.anchoredPosition += new Vector2(percentX2 * moveSpeed, percentY2 * moveSpeed);

        lastMousePos = Input.mousePosition;
        }else{
            if (Input.GetKeyDown(KeyCode.W))
        {
            if (isWriting) // If text is still being written, skip to full text
            {
                SkipTyping();
            }
            else if (textFullyDisplayed) // If text is fully displayed, go to next
            {
                NextText();
            }
            else // If no text is shown yet, start first text
            {
                StartCoroutine(DisplayText(introTexts[currentTextIndex]));
            }
        }
        }
    }

    IEnumerator DisplayText(string text)
    {
        isWriting = true;
        textFullyDisplayed = false;
        textDisplay.text = ""; // Clear text
        canvasGroup.alpha = 0; // Ensure text is invisible

        text = InsertPlayerName(text);

        // Fade in effect
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        if (textFullyDisplayed) 
        {
            textDisplay.text = text; // Instantly show text
            isWriting = false;
            yield break; // Stop coroutine to prevent retyping
        }

        // Start writing text
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string text)
    {
        foreach (char letter in text)
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
        isWriting = false;
        textFullyDisplayed = true;
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        textDisplay.text = InsertPlayerName(introTexts[currentTextIndex]); // Instantly show full text
        isWriting = false;
        textFullyDisplayed = true;
    }

    void NextText()
    {
        currentTextIndex++;

        if (currentTextIndex >= introTexts.Length) // If no more texts, remove UI
        {
            StartCoroutine(FadeOutAndDisable());
            pauseMenu.introTextsOn = false;
            return;
        }

        // Stop any previous typing coroutine before starting a new one
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        StopAllCoroutines(); // Ensure no overlapping coroutines
        StartCoroutine(DisplayText(introTexts[currentTextIndex]));
    }


    IEnumerator FadeOutAndDisable()
    {
        yield return StartCoroutine(FadeCanvasGroup(1, 0)); // Fade out UI
        transform.Find("IntroTexts").gameObject.SetActive(false); // Disable UI after fade-out
    }

    IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            transform.Find("IntroTexts").GetComponent<CanvasGroup>().alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.Find("IntroTexts").GetComponent<CanvasGroup>().alpha = endAlpha;
    }

    string InsertPlayerName(string text)
    {
        string playerName = PlayerPrefs.HasKey("Username") ? PlayerPrefs.GetString("Username") : "Worker";
        return text.Replace("{USERNAME}", playerName);
    }
}
