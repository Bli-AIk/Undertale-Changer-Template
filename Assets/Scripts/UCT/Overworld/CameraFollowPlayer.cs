using UnityEngine;

/// <summary>
/// Overworld摄像机跟随
/// </summary>
public class CameraFollowPlayer : MonoBehaviour
{
    public bool limit = true;
    public Vector2 limitX;//限制摄像机最大XY范围 0则不动
    public Vector2 limitY;//限制摄像机最大XY范围 0则不动
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
        //跟随玩家
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

            //限制范围
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