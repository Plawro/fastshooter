using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RetroButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Image whiteLine;
    [SerializeField] Image buttonBackground;
    [SerializeField] TextMeshProUGUI buttonText;
     
    [SerializeField] Color hoverTextColor = Color.white;
    [SerializeField] Color clickTextColor = Color.black;
    [SerializeField] Color originalTextColor;

    [SerializeField] AudioSource soundSource;
    [SerializeField] AudioClip hover;
    [SerializeField] AudioClip click;


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
