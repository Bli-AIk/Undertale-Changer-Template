using UnityEngine;

/// <summary>
/// Overworld Camera Following
/// </summary>
public class CameraFollowPlayer : MonoBehaviour
{
    public bool limit = true;
    public Vector2 limitX;
    //limit the maximum XY range of the camera 0 is not moving
    public Vector2 limitY;
    //Limit the maximum XY range of the camera 0 is not moving
    public GameObject player;
    public bool isFollow;
    public Vector3 followPosition;

    private void Start()
    {
        player = GameObject.Find("Player");
    }

    private void Update()
    {
        if (!isFollow)
        {
            return;
        }
        followPosition = transform.position;
        //Follow the player
        if (limit)
        {
            if (player.transform.position.x >= limitX.x || player.transform.position.x <= limitX.y)
            {
                transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
            }

            if (player.transform.position.y >= limitY.x || player.transform.position.y <= limitY.y)
            {
                transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
            }

            // Limit the scope
            if (transform.position.x <= limitX.x)
            {
                transform.position = new Vector3(limitX.x, transform.position.y, transform.position.z);
            }
            else if (transform.position.x >= limitX.y)
            {
                transform.position = new Vector3(limitX.y, transform.position.y, transform.position.z);
            }
            if (transform.position.y <= limitY.x)
            {
                transform.position = new Vector3(transform.position.x, limitY.x, transform.position.z);
            }
            else if (transform.position.y >= limitY.y)
            {
                transform.position = new Vector3(transform.position.x, limitY.y, transform.position.z);
            }
        }
        else
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }
}
