using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlWaypoints : MonoBehaviour
{
    public Camera mainCamera; // Assign the player's camera
    public GameObject uiPrefab; // The UI element prefab
    public List<Transform> markers; // List of world-space markers
    public Canvas canvas; // Your canvas (set in Screen Space - Camera)
    public float parallaxStrength = 5f; // Parallax effect strength
    float proximityThreshold = 1f; // Distance threshold to change text

    private List<GameObject> uiElements = new List<GameObject>();
    private RectTransform canvasRect;
    private Vector3 initialCameraPosition;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        canvasRect = canvas.GetComponent<RectTransform>();
        initialCameraPosition = mainCamera.transform.position;

        // Instantiate UI elements for each marker
        foreach (Transform marker in markers)
        {
            GameObject uiElement = Instantiate(uiPrefab, canvas.transform);
            uiElements.Add(uiElement);

            // Set the child TextMeshPro text to the marker's name
            TextMeshProUGUI textComponent = uiElement.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = marker.name; // Set text to marker's name
            }
        }
    }

    void LateUpdate()
    {
        // Update each UI element to match its marker
        for (int i = 0; i < markers.Count; i++)
        {
            Transform marker = markers[i];
            GameObject uiElement = uiElements[i];

            if (marker == null || uiElement == null)
                continue;

            // Convert world position to screen position
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(marker.position);

            // Check if the marker is in front of the camera
            if (screenPosition.z > 0)
            {
                uiElement.SetActive(true);

                // Convert screen position to canvas position
                RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
                Vector2 viewportPosition = mainCamera.WorldToViewportPoint(marker.position);
                Vector2 canvasPosition = new Vector2(
                    (viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                    (viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
                );

                // Add parallax effect
                Vector3 cameraOffset = mainCamera.transform.position - initialCameraPosition;
                Vector2 parallaxEffect = new Vector2(cameraOffset.x, cameraOffset.y) * parallaxStrength;

                // Apply position with parallax
                rectTransform.anchoredPosition = canvasPosition + parallaxEffect;

                // Change text based on proximity
                float distanceToMarker = Vector3.Distance(mainCamera.transform.position, marker.position);
                TextMeshProUGUI textComponent = uiElement.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    if (distanceToMarker < proximityThreshold)
                    {
                        textComponent.text = "S"; // Close to the marker
                    }
                    else
                    {
                        textComponent.text = marker.name; // Far from the marker
                    }
                }
            }
            else
            {
                uiElement.SetActive(false); // Hide if behind the camera
            }
        }
    }
}
