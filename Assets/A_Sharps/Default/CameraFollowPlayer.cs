using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Overworld���������
/// </summary>
public class CameraFollowPlayer : MonoBehaviour
{
    public Vector2 limitX;//������������XY��Χ 0�򲻶�
    public Vector2 limitY;//������������XY��Χ 0�򲻶�
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
        //�������
        if (player.transform.position.x >= limitX.x || player.transform.position.x <= limitX.y)
        {
            transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }

        if (player.transform.position.y >= limitY.x || player.transform.position.y <= limitY.y)
        {
            transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
        }

        //���Ʒ�Χ
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
    
}
