using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    private bool isFollowing = true;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isFollowing && player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
    }

    public void SetFollowing(bool follow)
    {
        isFollowing = follow;
    }
}