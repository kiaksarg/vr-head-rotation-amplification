using UnityEngine;

public class DegreeMarkerPlacement : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float distance = 10f; // Distance from the player to place markers

    public GameObject frontMarker; // 0° marker
    public GameObject rightMarker; // 90° marker
    public GameObject backMarker; // 180° marker
    public GameObject leftMarker; // -90° (270°) marker

    void Start()
    {
        PositionMarkers();
    }

    void Update()
    {
        // PositionMarkers();
    }

    void PositionMarkers()
    {
        // Calculate positions based on the player's position and desired distance
        Vector3 frontPosition = player.position + player.forward * distance; // 0° in front
        Vector3 rightPosition = player.position + player.right * distance; // 90° to the right
        Vector3 backPosition = player.position - player.forward * distance; // 180° behind
        Vector3 leftPosition = player.position - player.right * distance; // -90° (270°) to the left

        // Set marker positions
        if (frontMarker != null)
            frontMarker.transform.position = frontPosition;
        if (rightMarker != null)
            rightMarker.transform.position = rightPosition;
        if (backMarker != null)
            backMarker.transform.position = backPosition;
        if (leftMarker != null)
            leftMarker.transform.position = leftPosition;
    }
}
