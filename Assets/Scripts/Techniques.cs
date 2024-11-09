using System;
using System.Collections.Generic;
using MathNet.Numerics.Interpolation;
using UnityEngine;

namespace Techniques
{
    public enum GainApplyAngleTechnique
    {
        virtualAngle,
        physicalAngle,
    }

    public enum GainApplyTechnique
    {
        constant,
        directDelta,
        cubicSpline,
        preCalculated,
        none
    }

    public enum GainTechniquesEnum
    {
        constant,
        dynamicLinear,
        dynamicNonLinear,
        velocityGuided,
        controller,
        controllerDeamplify,
        step,
    }

    public static class RotationTechniques
    {
        public static Dictionary<string, float> dynamicDictionary = initDynamicGain();
        public static Dictionary<string, float> dynamicNonLinearDictionary = initDynamicGain(
            nonLinear: true
        );

        public static CubicSpline DynamicNonLinearCubicSplineInterpolation;
        public static CubicSpline DynamicLinearCubicSplineInterpolation;

        public static Dictionary<float, float> precomputedGains = new Dictionary<float, float>();

        public static void InitDynamicCubicSplineInterpolation(float precision = .1f)
        {
            List<double> xValues = new List<double>();
            List<double> yValuesLinear = new List<double>();
            List<double> yValuesNonLinear = new List<double>();
            float vAngleLinear = 0;
            float vAngleNonLinear = 0;

            for (float i = 0; i <= 180f; i += precision)
            {
                float gainLinear = dynamicLinearGain(vAngleLinear, 1, 3, 90);
                float gainNonLinear = dynamicNonLinearGain(vAngleLinear, 180);
                vAngleLinear += gainLinear * precision;
                vAngleNonLinear += gainNonLinear * precision;
                xValues.Add(i);
                yValuesLinear.Add(vAngleLinear);
                yValuesNonLinear.Add(vAngleNonLinear);
            }

            DynamicLinearCubicSplineInterpolation = CubicSpline.InterpolateNatural(
                xValues.ToArray(),
                yValuesLinear.ToArray()
            );

            DynamicNonLinearCubicSplineInterpolation = CubicSpline.InterpolateNatural(
                xValues.ToArray(),
                yValuesNonLinear.ToArray()
            );
        }

        public static void PreComputeGains()
        {
            for (float i = 0; i <= 180f; i += 1f)
            {
                float gain = dynamicLinearGain(i, 1, 3, 90);
                precomputedGains[i] = gain;
            }
        }

        public static float GetDynamicCubicSpline(float inputAngle, float targetRotation = 0)
        {
            // Find the closest precomputed gains
            float lowerKey = Mathf.Floor(inputAngle);
            float upperKey = Mathf.Ceil(inputAngle);

            if (precomputedGains.ContainsKey(lowerKey) && precomputedGains.ContainsKey(upperKey))
            {
                float lowerGain = precomputedGains[lowerKey];
                float upperGain = precomputedGains[upperKey];

                // Interpolate the gain for the input angle
                float interpolatedGain = Mathf.Lerp(lowerGain, upperGain, inputAngle - lowerKey);

                // Calculate the final rotation using the interpolated gain
                float finalRotation = inputAngle * interpolatedGain;

                return finalRotation;
            }

            // Fallback if exact keys are missing (this shouldn't happen if keys are properly precomputed)
            float fallbackGain = dynamicLinearGain(inputAngle, 1, 3, 90);
            return inputAngle * fallbackGain;
        }

        private static readonly float[] defaultValues =
        {
            1.8f, // step1
            2.2f, // step2
            2.5f, // step3
            2.8f, // step4
            2.5f, // step5
            2.0f, // step6
            1.8f // step7
        };

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

        public static float dynamicLinearGain(
            float virtualRotationAngle,
            float minGain,
            float maxGain,
            float halfRotation
        )
        {
            return (
                minGain * Math.Abs((virtualRotationAngle - halfRotation) / halfRotation)
                + maxGain * (1 - Math.Abs((virtualRotationAngle - halfRotation) / halfRotation))
            );
        }

        public static float dynamicNonLinearGain(float currentRotation, float targetRotation)
        {
            float gain =
                -6f / (targetRotation * targetRotation) * (currentRotation * currentRotation)
                + 6f / targetRotation * currentRotation
                + 1f;

            return gain;
        }

        public static float stepGain(float angle)
        {
            // Normalize the angle to the 0-360 range
            angle = Mathf.Repeat(angle, 360);

            if (angle <= 26)
            {
                return Mathf.Lerp(defaultValues[0], defaultValues[1], angle / 26f);
            }
            else if (angle > 26 && angle <= 41)
            {
                return Mathf.Lerp(defaultValues[1], defaultValues[2], (angle - 26) / (41 - 26));
            }
            else if (angle > 41 && angle <= 71)
            {
                return Mathf.Lerp(defaultValues[2], defaultValues[3], (angle - 41) / (71 - 41));
            }
            else if (angle > 71 && angle <= 101)
            {
                return Mathf.Lerp(defaultValues[3], defaultValues[4], (angle - 71) / (101 - 71));
            }
            else if (angle > 101 && angle <= 131)
            {
                return Mathf.Lerp(defaultValues[4], defaultValues[5], (angle - 101) / (131 - 101));
            }
            else if (angle > 131 && angle <= 161)
            {
                return Mathf.Lerp(defaultValues[5], defaultValues[6], (angle - 131) / (161 - 131));
            }
            else
            {
                return defaultValues[6];
            }
        }

        public static Dictionary<string, float> initDynamicGain(bool nonLinear = false)
        {
            float vAngle = 0;
            Dictionary<string, float> dic = new Dictionary<string, float>();
            for (float i = 0.00f; i <= 120; i += 0.001f)
            {
                string formattedKey = i.ToString("F3");
                while (dic.ContainsKey(formattedKey))
                {
                    formattedKey = (i + 0.0001f).ToString("F3"); // Slightly adjust the key
                    i += 0.0001f; // Adjust the loop counter to avoid an infinite loop
                }

                if (nonLinear)
                    vAngle += dynamicNonLinearGain(vAngle, 180) * 0.001f;
                else
                    vAngle += dynamicLinearGain(vAngle, 1, 3, 90) * 0.001f;

                dic.Add(formattedKey, vAngle);
            }

            return dic;
        }
    }
}
