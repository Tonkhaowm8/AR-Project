using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class scc : MonoBehaviour
{
    public ARGameManager ARGameManager;
    public bool activate = false;
    public ARRaycastHit ARRaycastHit;
    public ARRaycastManager arRaycastMng;
    public bool canPlace = false;
    private Pose placementPose;

    public GameObject f16Aircraft; // Reference to the F16 aircraft GameObject
    public GameObject ufoAircraft; // Reference to the UFO aircraft GameObject

    public GameObject objectToSpawn;
    public GameObject playableArea;

    public Camera xrCamera;
    public GameObject targetIndicator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (activate)
        {
            UpdatePlacementPose();
            UpdateTargetIndicator();

            if (canPlace && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                InstantiateObject();
                targetIndicator.SetActive(false);
                activate = false;
                ARGameManager.currentStage = ARGameManager.GameStage.Gameplay;
            }

            if (canPlace && Input.GetKeyDown(KeyCode.Space))
            {
                InstantiateObject();
                targetIndicator.SetActive(false);
                activate = false;
                ARGameManager.currentStage = ARGameManager.GameStage.Gameplay;
            }
        }
    }

    // Method to change the aircraft type
    public void ChangeAircraftType(string aircraftType)
    {
        // Deactivate both aircraft to ensure only one is active at a time

        // Check the provided aircraft type and activate the corresponding GameObject
        if (aircraftType == "F16")
        {
            objectToSpawn = f16Aircraft;
        }
        else if (aircraftType == "UFO")
        {
            objectToSpawn = ufoAircraft;
        }
        else
        {
            Debug.LogWarning("Unknown aircraft type: " + aircraftType);
        }
    }

    void InstantiateObject()
    {
        // Define the offset
        float offsetDistance = 1f; // Adjust this value according to your needs
        float planeHeight = 1f;

        // Calculate the position in front of the detected plane
        Vector3 offset = placementPose.position + (placementPose.rotation * Vector3.forward * offsetDistance);
        Vector3 offset1 = placementPose.position + (placementPose.rotation * Vector3.up * planeHeight);

        // Instantiate the object to spawn (e.g., enemy)
        Instantiate(objectToSpawn, offset1, placementPose.rotation);
        objectToSpawn.SetActive(true);

        // Instantiate the playable area with the calculated offset
        Instantiate(playableArea, offset, placementPose.rotation);
    }


    void UpdatePlacementPose()
    {
        Vector3 screenCenter = xrCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        arRaycastMng.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        canPlace = hits.Count > 0;

        if (canPlace)
        {
            placementPose = hits[0].pose;

            var cameraForward = xrCamera.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    void UpdateTargetIndicator()
    {
        if (canPlace)
        {
            targetIndicator.SetActive(true);
            targetIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        } else
        {
            targetIndicator.SetActive(false);
        }
    }
}
