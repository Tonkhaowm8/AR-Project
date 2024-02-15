using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARCapabilitiesManager : MonoBehaviour
{
    private ARSession arSession;

    private void Awake()
    {
        arSession = FindObjectOfType<ARSession>();
        arSession.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(CheckARSuport());
    }

    private void OnEnable()
    {
        ARSession.stateChanged += ARSessionStateChange;
    }

    private void OnDisable()
    {
        
    }

    private void ARSessionStateChange(ARSessionStateChangedEventArgs obj)
    {
        Logger.Instance.LogInfo($"AR session state changed: {obj.state}");

    }

    private IEnumerator CheckARSuport()
    {
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability) {
            Logger.Instance.LogInfo("Checking if AR is avaialable on this device");
            yield return ARSession.CheckAvailability();
        }

        if(ARSession.state != ARSessionState.Unsupported)
        {
            Logger.Instance.LogInfo("AR is supported on this device");
            arSession.enabled = true;
        }
        else
        {
            Logger.Instance.LogInfo("Disabling AR session as it is not supported");
            arSession.enabled = false;
        }
    }
}