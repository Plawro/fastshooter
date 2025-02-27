using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuEffect : MonoBehaviour
{
    public RectTransform uiElement;
    public RectTransform uiElement2;
    public float moveSpeed = 1f; // Adjust speed if needed
    private Vector2 lastMousePos;

    void Start()
    {
        lastMousePos = Input.mousePosition;
    }

    void Update()
    {
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
    }
}
