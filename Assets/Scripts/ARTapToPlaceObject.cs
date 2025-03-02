using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections.Generic; // Added to fix List<> error

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject placementIndicator;
    private Pose PlacementPose;
    private ARRaycastManager aRRaycastManager;
    private bool placementPoseIsValid = false;

    void Start()
    {
        // Updated obsolete method
        aRRaycastManager = FindFirstObjectByType<ARRaycastManager>();
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
}
