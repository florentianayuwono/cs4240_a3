using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections.Generic; // Added to fix List<> error

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject placementIndicator;
    public GameObject objectToPlace;
    private Pose PlacementPose;
    private ARRaycastManager aRRaycastManager;
    private bool placementPoseIsValid = false;

    private PlayerInput playerInput;
    private InputAction touchAction;

    void Start()
    {
        // Updated obsolete method
        aRRaycastManager = FindFirstObjectByType<ARRaycastManager>();
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
        touchAction.started += PlaceObject;
    }

    void OnDisable()
    {
        touchAction.started -= PlaceObject;
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0; // Fixed invalid comparison
        if (placementPoseIsValid)
        {
            PlacementPose = hits[0].pose;
            //Debug.Log("Placement Pose Valid: " + PlacementPose.position); // Log position
        }
        else
        {
            //Debug.Log("Placement Pose Invalid");
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(PlacementPose.position, PlacementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void PlaceObject(InputAction.CallbackContext context)
    {
        Debug.Log("Input action triggered!");

        if (placementPoseIsValid)
        {
            Quaternion correctedRotation = PlacementPose.rotation * Quaternion.Euler(-90f, 0f, 0f);

            Debug.Log("Placing object at position: " + PlacementPose.position);
            Instantiate(objectToPlace, PlacementPose.position, correctedRotation);
        }
        else
        {
            Debug.Log("Invalid placement pose");
        }
    }

}
