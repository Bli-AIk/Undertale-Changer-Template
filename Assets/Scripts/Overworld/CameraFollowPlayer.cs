using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Overworld Camera Follow Player
/// </summary>
public class CameraFollowPlayer : MonoBehaviour
{
    public bool limit = true;
    public Vector2 limitX;//Limit the maximum XY range of the camera, if it is 0, it will not move
    public Vector2 limitY;//Limit the maximum XY range of the camera, if it is 0, it will not move
    public GameObject player;
    public bool isFollow;
    public Vector3 followPosition;
    void Start()
    {
        player = GameObject.Find("Player");
    }
    void Update()
    {
        if (!isFollow) 
        {
            return;
        }
        followPosition = transform.position;
        //Follow player
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

            //Limits
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
