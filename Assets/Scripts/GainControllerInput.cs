using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GainControllerInput : MonoBehaviour
{
    public float gain = 1.0f; // Initial gain value
    public float gainDeamplified = 1.0f; // Initial deAmplify gain value
    public float minGain = 1.0f; // Minimum gain value
    public float maxGain = 3.0f; // Maximum gain value
    public XRNode controllerNode = XRNode.RightHand; // Use Right Hand Controller; change to XRNode.LeftHand for Left Hand Controller

    private InputDevice controller;

    void Start()
    {
        // Initialize the XR Input Device
        controller = InputDevices.GetDeviceAtXRNode(controllerNode);
    }

    void Update()
    {
        // Check if the device is valid
        if (!controller.isValid)
        {
            controller = InputDevices.GetDeviceAtXRNode(controllerNode);
        }

        // Read the Trigger value
        if (controller.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            // Map triggerValue (0.0 to 1.0) to gain range (1.0 to 3.0)
            gain = Mathf.Lerp(minGain, maxGain, triggerValue);
            gainDeamplified = Mathf.Lerp(maxGain, minGain, triggerValue);
        }
    }

    public float GetCurrentGain(bool isDeamplify = false)
    {
        if (isDeamplify)
            return gainDeamplified;
        return gain;
    }
}
