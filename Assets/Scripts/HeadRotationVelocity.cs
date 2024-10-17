using UnityEngine;

public class HeadRotationVelocity : MonoBehaviour
{
    public SharedDebugData sharedDebugData;
    public ScaleHeadRotation scaleHeadRotationScript;
    public Transform headTransform;
    private float rotationSpeed; 

    private Quaternion lastRotation;

    void Update()
    {
        CalculateVelocity();
    }

    void Start()
    {
        lastRotation = transform.rotation;
        if (headTransform == null)
        {
            Debug.LogError("Head Transform is not assigned.");
            enabled = false;
            return;
        }
    }

    void CalculateVelocity()
    {
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);

        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180)
        {
            angle -= 360;
        }

        rotationSpeed = angle / Time.deltaTime;

        lastRotation = transform.rotation;

        sharedDebugData.velocity = rotationSpeed;
        scaleHeadRotationScript.velocity = rotationSpeed;
    }
}
