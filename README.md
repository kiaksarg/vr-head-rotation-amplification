# Head Rotation Amplification in Virtual Reality (VR)

This Unity project implements various techniques to manipulate and amplify head rotation in VR. By applying an amplification factor—such as a gain of 2—a 30° physical head turn results in a 60° virtual rotation, allowing users to explore more of the virtual environment with less movement.

![VR Head Rotation Amplification](</Assets/Screenshots/OculusScreenshot1729202272.jpeg>)

---

## Table of Contents

- [Head Rotation Amplification in Virtual Reality (VR)](#head-rotation-amplification-in-virtual-reality-vr)
  - [Table of Contents](#table-of-contents)
  - [Gain Techniques](#gain-techniques)
    - [Constant Gain](#constant-gain)
    - [Dynamic Gain (linear and non-linear)](#dynamic-gain-linear-and-non-linear)
    - [Velocity-Guided Gain](#velocity-guided-gain)
    - [Controller-Based Gain](#controller-based-gain)
  - [Implementation Techniques](#implementation-techniques)
    - [Constant](#constant)
    - [Direct Delta](#direct-delta)
    - [Pre-Calculated](#pre-calculated)
    - [Cubic Spline Interpolation](#cubic-spline-interpolation)
  - [References](#references)


---

## Gain Techniques

Gain techniques define how the gain value is calculated based on parameters such as the physical angle, velocity and user input. The following gain techniques have been implemented:

```csharp
public enum GainTechniquesEnum
{
    constant,
    dynamicLinear,
    dynamicNonLinear,
    velocityGuided,
    controller,
}
```
### Constant Gain

With Constant Gain, the virtual angle is calculated by multiplying the physical input angle by a fixed gain value (e.g. 30° × 2 = 60°).

### Dynamic Gain (linear and non-linear)
In contrast to the static gain, a dynamic gain is not constant and changes during head rotation. It is implemented based on "Turn Your Head Half Round: VR Rotation Techniques for Situations With Physically Limited Turning Angle" paper by Langbehn et al., 2019.

In both the dynamic and linear techniques, at the beginning of the rotation, the gain is 1 and at the end of the rotation, it becomes 1 again. It is for, as the user approaches the target, the lower gain results in a reduced rotation speed, which can improve accuracy.

In the linear dynamic gain technique, as the user begins rotating their head toward the target, the gain gradually increases, reaching its maximum at half the target angle. For example, if the target is 180 degrees, the gain peaks at 90 degrees and then gradually decreases, returning to 1 upon reaching the target.

The dynamic linear gain is calculated as:

$\[
\text{gain} = \left( \text{minGain} \times \left| \frac{\text{virtualRotationAngle} - \text{halfRotation}}{\text{halfRotation}} \right| \right) + \left( \text{maxGain} \times \left( 1 - \left| \frac{\text{virtualRotationAngle} - \text{halfRotation}}{\text{halfRotation}} \right| \right) \right)
\]$


The dynamic non-linear gain technique follows a similar pattern but increases and decreases the gain non-linearly. The gain grows at first, reaching a maximum value of 2.5 at half the target angle. Afterward, it decreases non-linearly until it returns to 1 at the target. This non-linear adjustment results in a smoother experience for users.

The dynamic non-linear using a parabola equation and calculated as:

$\[
\text{gain} = -\frac{6}{\text{targetRotation}^2} \times \text{headRotation}^2 + \frac{6}{\text{targetRotation}} \times \text{headRotation} + 1
\]$
![Dynamic Rotation Gains](</Assets/Screenshots/Screenshot 2024-10-17 210726.png>)
Applied rotation gains during a 90◦ virtual rotation. Adapted from Langbehn et al. (2019).

### Velocity-Guided Gain
Velocity-Guided Gain calculates the gain based on the rotational velocity of the user's head movement. It is based on the "Velocity Guided Amplification of View Rotation for Seated VR Scene Exploration" paper by Zhang et al., 2021. As the user increases the head rotation speed, it decrease the gain. In their test, even when the gain is decreased, (at higher head rotation speeds) still the final rotation view speed in VR will increase.
```csharp
        public static float velocityGuidedGain(float velocity)
        {
            if (velocity <= 26.7)
                return 2.95f;

            if (26.7 < velocity && velocity <= 41.1)
            {
                return 2.55f + (41.1f - velocity) / (41.1f - 26.7f) * (2.95f - 2.55f);
            }

            if (41.1 < velocity && velocity < 62.2)
            {
                return 2.22f + (62.2f - velocity) / (62.2f - 41.1f) * (2.55f - 2.22f);
            }

            if (velocity >= 62.2)
                return 2.22f;

            return 0;
        }
```

### Controller-Based Gain
Controller-Based Gain derives the gain value from an external controller input. This allows for real-time adjustments based on user interactions or other input devices.

---

## Implementation Techniques
To implement and apply gain to the VR environment, this project utilizes various techniques. These techniques determine how the calculated gain is applied to the camera in Unity.

```csharp
public enum GainApplyTechnique
{
    constant,
    directDelta,
    cubicSpline,
    preCalculated,
}
```

### Constant
With the Constant Technique,  the virtual angle is calculated by multiplying the gain by the input angle (e.g., 30° × 2 = 60° virtual angle).This method is suitable when using a fixed gain technique.

### Direct Delta
The technique measures the yaw change (headsetRotationDelta) from the user's input (e.g., a 0.22-degree rotation). This yaw delta is then multiplied by a predefined gain value to amplify the effect. For example, with a gain of 2: deltaYaw = 0.22 * 2 = 0.44. The calculated yaw (0.44) is then added to the camera rotation.

```csharp
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
```

### Pre-Calculated
With the Pre-Calculated method, we determine the output angle for every possible input angle.

### Cubic Spline Interpolation
The Cubic Spline Interpolation technique allows us to calculate fewer predefined input-output pairs and then use cubic spline interpolation to estimate the values between them. For instance, if we have precomputed that 20-degree maps to 30 degrees and 30-degree maps to 50 degrees, cubic spline interpolation can estimate the output for an input of 25 degrees, even though it wasn’t explicitly precomputed.


![Cubic Spline Interpolation Accuracy](</Assets/Screenshots/CubicSplineInterpolation.png>)
Cubic Spline Interpolation Accuracy

---

## References

- Langbehn, E., Bruder, G., Steinicke, F. (2019). [Turn Your Head Half Round: VR Rotation Techniques for Situations With Physically Limited Turning Angle](https://dl.acm.org/doi/10.1145/3340764.3340778). *Proceedings of the 25th ACM Symposium on Virtual Reality Software and Technology (VRST)*.

- Wang, Y., Su, S., Hao, Q., Lee, J. (2022). [On Rotation Gains Within and Beyond Perceptual Limitations for Seated VR](http://arxiv.org/abs/2203.02750). *arXiv preprint arXiv:2203.02750*.

- Zhang, H., Yu, S., Babu, S., Lee, J. (2021). [Velocity Guided Amplification of View Rotation for Seated VR Scene Exploration](https://ieeexplore.ieee.org/document/9419095). *IEEE Transactions on Visualization and Computer Graphics*.
