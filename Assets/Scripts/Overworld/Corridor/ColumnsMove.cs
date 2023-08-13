using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 长廊柱子移动
/// </summary>
public class ColumnsMove : MonoBehaviour
{
    CameraFollowPlayer parentCamera;
    public float speed;//包括方向(正负)
    
    void Start()
    {
        parentCamera = transform.parent.GetComponent<CameraFollowPlayer>();
        transform.localPosition = parentCamera.transform.position * speed;
    }

    
    void Update()
    {
        if (parentCamera.transform.position.x >= parentCamera.limitX.x || parentCamera.transform.position.x <= parentCamera.limitY.y)
        {
            transform.position = parentCamera.followPosition * speed;
        }
    }
}
