using UnityEngine;

[CreateAssetMenu(fileName = "SharedDebugData", menuName = "ScriptableObjects/SharedDebugData")]
public class SharedDebugData : ScriptableObject
{
    public float headsetAngle;
    public float gain;
    public float yaw;
    public float velocity;
    public bool isSettling;

    public Quaternion headsetRotationDelta;
    public Quaternion scaledRotationDifference;
    // public Quaternion cumulativeRotation;
    public Quaternion combinedRotation; // initialHeadsetRotation * cumulativeRotation
    public Quaternion relativeHeadsetRotation;
    public string Technique;
    public string ApplyTechnique;
    public string GainApplyAngleTechnique;
}
