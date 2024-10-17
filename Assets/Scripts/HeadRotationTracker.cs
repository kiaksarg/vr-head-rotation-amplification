using System.Collections.Generic;
using UnityEngine;

public class HeadRotationTracker : MonoBehaviour
{
    public SharedDebugData sharedDebugData;
    public ScaleHeadRotation scaleHeadRotationScript;

    public Transform headsetTransform;
    public float rotationThreshold = 10f; // Threshold for detecting significant movement
    public float settleDuration = 1.2f; // Duration to check if the user has settled

    private List<float> recentAngles = new List<float>();
    private List<float> timestamps = new List<float>(); // Timestamps corresponding to the recorded angles
    public bool isSettling = false; // Public flag to indicate if the user is settling

    void Update()
    {
        TrackHeadRotation();
        CheckIfUserHasSettled();
    }

    private void TrackHeadRotation()
    {
        float currentAngle = headsetTransform.eulerAngles.y;
        AdjustAngle(ref currentAngle);

        recentAngles.Add(currentAngle);
        timestamps.Add(Time.time);

        // Maintain only relevant recent angles within the settle duration timeframe
        while (timestamps.Count > 0 && Time.time - timestamps[0] > settleDuration)
        {
            timestamps.RemoveAt(0);
            recentAngles.RemoveAt(0);
        }
    }

    private void CheckIfUserHasSettled()
    {
        if (recentAngles.Count < 1)
            return;

        float averageAngle = CalculateAverage(recentAngles);

        isSettling = true;

        foreach (float angle in recentAngles)
        {
            if (Mathf.Abs(angle - averageAngle) > rotationThreshold)
            {
                isSettling = false;
                break;
            }
        }

        sharedDebugData.isSettling = isSettling;
        scaleHeadRotationScript.isSettling = isSettling;

        // Debug.Log(isSettling ? "User has settled" : "User is not settled");
    }

    private void AdjustAngle(ref float angle)
    {
        if (angle > 180f)
            angle -= 360f;
    }

    private float CalculateAverage(List<float> values)
    {
        float total = 0f;

        foreach (float value in values)
            total += value;

        return total / values.Count;
    }
}
