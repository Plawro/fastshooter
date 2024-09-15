using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RetroButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image whiteLine;
    public Image buttonBackground;
    public TextMeshProUGUI buttonText;
     
    public Color hoverTextColor = Color.white;
    public Color clickTextColor = Color.black;
    public Color originalTextColor;

    public AudioSource soundSource;
    public AudioClip hover;
    public AudioClip click;


    private void Start()
    {
        whiteLine.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        whiteLine.enabled = true;
        buttonText.color = hoverTextColor;
        soundSource.PlayOneShot(hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        whiteLine.enabled = false;
        buttonText.color = originalTextColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        buttonText.color = clickTextColor;
        soundSource.PlayOneShot(click);
    }
}
