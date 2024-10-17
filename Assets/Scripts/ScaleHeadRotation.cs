using System;
using Techniques;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ScaleHeadRotation : MonoBehaviour
{
    public GainControllerInput gainControllerInput;

    public bool isApplyGain = true;
    public bool isSettling = false;
    public bool disableAmpIfIsSettled = false;
    public GainTechniquesEnum Technique;
    public GainApplyTechnique ApplyTechnique;
    public GainApplyAngleTechnique GainApplyAngle;
    public float ConstantGain = 2f;
    public float velocity = 0f;
    public float previousVelocity = 0f;
    public float acceleration = 0f;
    public float someThresholdValue = 10f;
    public float minGain = 1f;
    public float maxGain = 3f;
    public float halfRotation = 90f;
    public float targetRotation = 180f;
    public float VirtualAngle = 0;
    public float PhysicalAngle = 0;

    public XROrigin xrOrigin; // Reference to the XR Origin
    public Transform headsetTransform; // Reference to the headset transform

    public SharedDebugData sharedDebugData;

    public float minMultiplier = 1.2f; // Minimum multiplier
    public float maxMultiplier = 2.5f; // Maximum multiplier

    private Quaternion initialHeadsetRotation; // Initial rotation of the headset
    private Quaternion previousHeadsetRotation; // Previous frame's rotation of the headset

    public float rayLength = 2.0f; // Length of the debug rays

    public TextMeshPro frontText;
    public TextMeshPro backText;
    public TextMeshPro leftText;
    public TextMeshPro rightText;

    void Start()
    {
        // Save the initial rotations
        initialHeadsetRotation = headsetTransform.localRotation;
        previousHeadsetRotation = initialHeadsetRotation;

        acceleration = (velocity - previousVelocity) / Time.deltaTime;

        RotationTechniques.PreComputeGains();
        RotationTechniques.InitDynamicCubicSplineInterpolation(precision: .1f);

        // Initialize the gain controller input
        gainControllerInput = FindObjectOfType<GainControllerInput>();
    }

    void Update()
    {
        Quaternion currentHeadsetRotation = headsetTransform.localRotation;

        Quaternion headsetRotationDelta =
            Quaternion.Inverse(previousHeadsetRotation) * currentHeadsetRotation;

        Quaternion relativeHeadsetRotation =
            Quaternion.Inverse(initialHeadsetRotation) * currentHeadsetRotation;

        acceleration = velocity - previousVelocity;

        float headsetAngle = Quaternion.Angle(
            onlyYAxisRotation(initialHeadsetRotation),
            onlyYAxisRotation(currentHeadsetRotation)
        );

        float virtualAngle = headsetTransform.eulerAngles.y;

        // Convert to range [-180, 180]
        virtualAngle = NormalizeAngle(virtualAngle);

        VirtualAngle = virtualAngle;

        float initialYaw = initialHeadsetRotation.eulerAngles.y;
        float currentYaw = currentHeadsetRotation.eulerAngles.y;

        float signedHeadsetAngle = Mathf.DeltaAngle(initialYaw, currentYaw);

        PhysicalAngle = signedHeadsetAngle;

        float gain =
            GainApplyAngle == GainApplyAngleTechnique.virtualAngle
                ? GetGain(MathF.Abs(virtualAngle))
                : GetGain(MathF.Abs(headsetAngle));

        ApplyGain(gain, relativeHeadsetRotation, headsetRotationDelta);

        UpdateDebugData(
            headsetAngle,
            gain,
            relativeHeadsetRotation,
            currentHeadsetRotation,
            headsetRotationDelta
        );

        previousHeadsetRotation = currentHeadsetRotation;
        previousVelocity = velocity;

        Debug.DrawRay(headsetTransform.position, headsetTransform.forward * rayLength, Color.green);
    }

    private void ApplyGain(
        float gain,
        Quaternion relativeHeadsetRotation,
        Quaternion headsetRotationDelta
    )
    {
        if (!isApplyGain || (disableAmpIfIsSettled && isSettling))
            return;

        switch (ApplyTechnique)
        {
            case GainApplyTechnique.constant:
                ConstantGainApply(gain, relativeHeadsetRotation);
                break;
            case GainApplyTechnique.directDelta:
                DirectDeltaGainApply(gain, headsetRotationDelta);
                break;
            case GainApplyTechnique.preCalculated:
                PreCalculatedGainApply(relativeHeadsetRotation);
                break;
            case GainApplyTechnique.cubicSpline:
                CubicSplineGainApply(relativeHeadsetRotation);
                break;
            default:
                break;
        }
    }

    private void ConstantGainApply(float gain, Quaternion relativeHeadsetRotation)
    {
        Vector3 relativeEuler = onlyYAxisRotation(relativeHeadsetRotation).eulerAngles;
        float yawAngle = NormalizeAngle(relativeEuler.y);
        float deltaYaw = yawAngle * gain;
        relativeEuler.y = deltaYaw;
        Quaternion scaledRotation = Quaternion.Euler(relativeEuler);
        //To cancel out the headset rotation effect
        Quaternion inverseCurrentRelativeRotation = Quaternion.Inverse(
            onlyYAxisRotation(relativeHeadsetRotation)
        );
        xrOrigin.CameraFloorOffsetObject.transform.localRotation =
            initialHeadsetRotation * scaledRotation * inverseCurrentRelativeRotation;
    }

    private void DirectDeltaGainApply(float gain, Quaternion headsetRotationDelta)
    {
        Vector3 relativeEuler = onlyYAxisRotation(headsetRotationDelta).eulerAngles;
        float yawAngle = NormalizeAngle(relativeEuler.y);
        //To cancel out the headset rotation effect
        Quaternion inverseCurrentRelativeRotation = Quaternion.Inverse(
            Quaternion.Euler(new Vector3(0, yawAngle, 0))
        );
        float deltaYaw = yawAngle * gain;
        relativeEuler.y = deltaYaw;
        Quaternion gainedRotation = Quaternion.Euler(relativeEuler);
        xrOrigin.CameraFloorOffsetObject.transform.localRotation *=
            initialHeadsetRotation * gainedRotation * inverseCurrentRelativeRotation;
    }

    private void PreCalculatedGainApply(Quaternion relativeHeadsetRotation)
    {
        Vector3 relativeEuler = onlyYAxisRotation(relativeHeadsetRotation).eulerAngles;
        float yawAngle = NormalizeAngle(relativeEuler.y);
        float deltaYaw =
            Technique == GainTechniquesEnum.dynamicNonLinear
                ? RotationTechniques.dynamicNonLinearDictionary[$"{Math.Abs(yawAngle):F3}"]
                : RotationTechniques.dynamicDictionary[$"{Math.Abs(yawAngle):F3}"];
        relativeEuler.y = yawAngle < 0 ? -deltaYaw : deltaYaw;
        Quaternion scaledRotation = Quaternion.Euler(relativeEuler);
        Quaternion inverseCurrentRelativeRotation = Quaternion.Inverse(
            onlyYAxisRotation(relativeHeadsetRotation)
        );
        xrOrigin.CameraFloorOffsetObject.transform.localRotation =
            initialHeadsetRotation * scaledRotation * inverseCurrentRelativeRotation;
    }

    private void CubicSplineGainApply(Quaternion relativeHeadsetRotation)
    {
        Vector3 relativeEuler = onlyYAxisRotation(relativeHeadsetRotation).eulerAngles;

        float yawAngle = NormalizeAngle(relativeEuler.y);

        float deltaYaw =
            Technique == GainTechniquesEnum.dynamicNonLinear
                ? (float)
                    RotationTechniques.DynamicNonLinearCubicSplineInterpolation.Interpolate(
                        Mathf.Abs(yawAngle)
                    )
                : (float)
                    RotationTechniques.DynamicLinearCubicSplineInterpolation.Interpolate(
                        Mathf.Abs(yawAngle)
                    );
        relativeEuler.y = yawAngle < 0 ? -deltaYaw : deltaYaw;
        Quaternion scaledRotation = Quaternion.Euler(relativeEuler);
        Quaternion inverseCurrentRelativeRotation = Quaternion.Inverse(
            onlyYAxisRotation(relativeHeadsetRotation)
        );

        xrOrigin.CameraFloorOffsetObject.transform.localRotation =
            initialHeadsetRotation * scaledRotation * inverseCurrentRelativeRotation;
    }

    private Quaternion onlyYAxisRotation(Quaternion q)
    {
        Vector3 eulerRotation = q.eulerAngles;
        eulerRotation.x = 0;
        eulerRotation.z = 0;
        return Quaternion.Euler(eulerRotation);
    }

    private float GetGain(float angle)
    {
        if (disableAmpIfIsSettled && isSettling)
            return 1f;

        IGainStrategy gainStrategy = GainStrategyFactory.Create(Technique, this);

        GainCalculationContext context = new GainCalculationContext
        {
            Angle = angle,
            Velocity = velocity,
            IsSettling = isSettling,
        };

        return gainStrategy.CalculateGain(context);
    }

    private void UpdateDebugData(
        float headsetAngle,
        float gain,
        Quaternion relativeHeadsetRotation,
        Quaternion currentHeadsetRotation,
        Quaternion headsetRotationDelta
    )
    {
        sharedDebugData.headsetAngle = headsetAngle;
        sharedDebugData.gain = gain;
        float virtualAngle = headsetTransform.eulerAngles.y;
        virtualAngle = NormalizeAngle(virtualAngle);

        sharedDebugData.yaw = virtualAngle;
        sharedDebugData.combinedRotation = currentHeadsetRotation;
        sharedDebugData.relativeHeadsetRotation = relativeHeadsetRotation;
        sharedDebugData.headsetRotationDelta = headsetRotationDelta;

        sharedDebugData.Technique = Technique.ToString();
        sharedDebugData.ApplyTechnique = ApplyTechnique.ToString();
        sharedDebugData.GainApplyAngleTechnique = GainApplyAngle.ToString();
    }

    public void SetGain(float gain)
    {
        ConstantGain = gain;
        sharedDebugData.gain = gain;
    }

    public void SetIsApplyGain(bool isOn)
    {
        isApplyGain = isOn;
    }

    public void SetDisableAmpIfIsSettled(bool isOn)
    {
        disableAmpIfIsSettled = isOn;
    }

    public void SetTechnique(int index)
    {
        Technique = (GainTechniquesEnum)index;
        sharedDebugData.Technique = Technique.ToString();
    }

    public void SetApplyTechnique(int index)
    {
        ApplyTechnique = (GainApplyTechnique)index;
        sharedDebugData.ApplyTechnique = ApplyTechnique.ToString();
    }

    public void SetGainApplyAngle(int index)
    {
        GainApplyAngle = (GainApplyAngleTechnique)index;
        sharedDebugData.GainApplyAngleTechnique = GainApplyAngle.ToString();
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            return angle - 360f;
        else if (angle < -180f)
            return angle + 360f;
        else
            return angle;
    }
}
