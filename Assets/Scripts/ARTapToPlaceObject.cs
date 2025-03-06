using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject placementIndicator;
    public GameObject[] objectsToPlace;
    public GameObject selectionUI;  // UI for Delete/Relocate options

    private GameObject currentObjectToPlace;
    private Pose PlacementPose;
    private ARRaycastManager aRRaycastManager;
    private bool placementPoseIsValid = false;
    private List<GameObject> placedObjects = new List<GameObject>();
    private GameObject selectedObject = null;  // The tapped object
    private bool isPlacingAfterRelocate = false; // Track if we're relocating an object

    public float minPlacementDistance = 0.5f;  // Prevent objects from being placed too close

    private PlayerInput playerInput;
    private InputAction touchAction;

    void Start()
    {
        aRRaycastManager = FindFirstObjectByType<ARRaycastManager>();
        if (objectsToPlace.Length > 0)
            currentObjectToPlace = objectsToPlace[0];

        placementIndicator.SetActive(false);
        selectionUI.SetActive(false);
    }

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        touchAction = playerInput.actions.FindAction("SingleTouchClick");
        if (touchAction == null)
        {
            Debug.LogError("SingleTouchClick action not found!");
        }
    }

    void OnEnable()
    {
        touchAction.started += HandleTouch;
    }

    void OnDisable()
    {
        touchAction.started -= HandleTouch;
    }

    void Update()
    {
        if (!selectionUI.activeSelf) // Only update placement if no UI is open
        {
            UpdatePlacementPose();
            UpdatePlacementIndicator();
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            PlacementPose = hits[0].pose;
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid && !selectionUI.activeSelf)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(PlacementPose.position, PlacementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void HandleTouch(InputAction.CallbackContext context)
    {
        if (IsPointerOverUiObject()) return; // Ignore UI taps

        if (isPlacingAfterRelocate) 
        {
            PlaceObject(); // If relocating, let the user place the object again
            isPlacingAfterRelocate = false; // Reset relocation mode
            return;
        }

         // If the user taps the selected object again, deselect it
        if (selectedObject != null && Vector3.Distance(selectedObject.transform.position, PlacementPose.position) < minPlacementDistance)
        {
            DeselectObject();
            return; // Exit early to prevent placing a new object
        }

        GameObject nearbyObject = GetNearbyObject(PlacementPose.position);
        if (nearbyObject != null)
        {
            selectedObject = nearbyObject;
            ShowSelectionUI(selectedObject.transform.position);
            return; // Prevent placing a new object
        }

        if (!selectionUI.activeSelf) // Prevent placing while UI is open
        {
            PlaceObject();
        }
    }

    private void DeselectObject()
    {
        selectedObject = null;
        selectionUI.SetActive(false);
        placementIndicator.SetActive(true); // Re-enable placement indicator when deselecting
    }

    private void PlaceObject()
    {
        if (!placementPoseIsValid) return; // Ensure valid placement pose

        if (IsOverlappingWithOtherObjects(PlacementPose.position))
        {
            Debug.Log("Object already exists here! Showing selection UI.");
            return; // Prevent placement if colliding
        }

        Quaternion correctedRotation = PlacementPose.rotation * Quaternion.Euler(-90f, 0f, 0f);
        GameObject placedObject = Instantiate(currentObjectToPlace, PlacementPose.position, correctedRotation);
        placedObjects.Add(placedObject);
    }

    private bool IsOverlappingWithOtherObjects(Vector3 newPosition)
    {
        foreach (GameObject obj in placedObjects)
        {
            if (Vector3.Distance(obj.transform.position, newPosition) < minPlacementDistance)
            {
                return true; // Object is too close
            }
        }
        return false;
    }

    private GameObject GetNearbyObject(Vector3 position)
    {
        foreach (GameObject obj in placedObjects)
        {
            if (Vector3.Distance(obj.transform.position, position) < minPlacementDistance)
            {
                return obj; // Return the existing nearby object
            }
        }
        return null;
    }

    private void ShowSelectionUI(Vector3 position)
    {
        placementIndicator.SetActive(false); // Disable placement indicator when selecting an object
        selectionUI.SetActive(true);
        selectionUI.transform.position = position + Vector3.up * 0.2f; // Position UI above the object
    }

    public void DeleteObject()
    {
        if (selectedObject != null)
        {
            placedObjects.Remove(selectedObject);
            Destroy(selectedObject);
            selectionUI.SetActive(false);
            placementIndicator.SetActive(true); // Re-enable indicator after deletion
        }
    }

    public void RelocateObject()
    {
        if (selectedObject == null) return;

        // 1. Delete the object
        placedObjects.Remove(selectedObject);
        Destroy(selectedObject);
        selectionUI.SetActive(false);

        // 2. Allow user to place a new object
        isPlacingAfterRelocate = true;
        placementIndicator.SetActive(true);
    }

    private bool IsPointerOverUiObject()
    {
        if (Touchscreen.current == null || !Touchscreen.current.primaryTouch.press.isPressed) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void ChangeObject(int index)
    {
        currentObjectToPlace = objectsToPlace[index];
        Debug.Log("Switched to object: " + currentObjectToPlace.name);
    }
}
