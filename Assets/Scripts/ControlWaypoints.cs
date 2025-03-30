using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class MarkerData
{
    public Transform marker; // Waypoint transform
    public GameObject uiPrefab; // Specific UI prefab for this waypoint
}

public class ControlWaypoints : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Canvas canvas;
    [SerializeField] List<MarkerData> markers;
    [SerializeField] float parallaxStrength = 5f;
    [SerializeField] float proximityThreshold = 1f;

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
        foreach (MarkerData data in markers)
        {
            if (data.marker == null || data.uiPrefab == null)
                continue;

            GameObject uiElement = Instantiate(data.uiPrefab, canvas.transform);
            uiElements.Add(uiElement);

            // Set the child TextMeshPro text to the marker's name
            TextMeshProUGUI textComponent = uiElement.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = data.marker.name; // Set text to marker's name
            }
        }
    }

    void FixedUpdate()
    {
        // Update each UI element to match its marker
        for (int i = 0; i < markers.Count; i++)
        {
            MarkerData data = markers[i];
            GameObject uiElement = uiElements[i];

            if (data.marker == null || uiElement == null)
                continue;

            // Convert world position to screen position
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(data.marker.position);

            // Check if the marker is in front of the camera
            if (screenPosition.z > 0)
            {
                uiElement.SetActive(true);

                // Convert screen position to canvas position
                RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
                Vector2 viewportPosition = mainCamera.WorldToViewportPoint(data.marker.position);
                Vector2 canvasPosition = new Vector2(
                    (viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                    -200
                );

                // Add parallax effect
                Vector3 cameraOffset = mainCamera.transform.position - initialCameraPosition;
                Vector2 parallaxEffect = new Vector2(cameraOffset.x, cameraOffset.y) * parallaxStrength;

                // Apply position with parallax
                rectTransform.anchoredPosition = canvasPosition + parallaxEffect;

                // Change text based on proximity
                float distanceToMarker = Vector3.Distance(mainCamera.transform.position, data.marker.position);
                TextMeshProUGUI textComponent = uiElement.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    if (distanceToMarker < proximityThreshold)
                    {
                        textComponent.transform.parent.transform.gameObject.SetActive(false); // Close to the marker
                    }
                    else
                    {
                        textComponent.transform.parent.transform.gameObject.SetActive(true); // Far from the marker
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
