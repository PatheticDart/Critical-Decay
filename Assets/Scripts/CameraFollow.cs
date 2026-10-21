using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.z = transform.position.z; // Keep the camera's z position unchanged
            transform.position = newPosition;
        }
    }
}
