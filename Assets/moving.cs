using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class moving : MonoBehaviour
{

    public float horizontalSpeed = 0.08f;
    public float verticalSpeed = 2.0f;
    public float height = 1.0f;

    public Vector3 tmpPosition;
    // Start is called before the first frame update
    void Start()
    {
        tmpPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        tmpPosition.z -= horizontalSpeed;
        tmpPosition.y += MathF.Sin(Time.realtimeSinceStartup * verticalSpeed) * height;
        transform.position = tmpPosition;

    }


}
