using UnityEngine;

/// <summary>
/// Promenade Columns Moving
/// </summary>
public class ColumnsMove : MonoBehaviour
{
    private CameraFollowPlayer parentCamera;
    public float speed;
    //including direction (positive and negative)

    private void Start()
    {
        parentCamera = transform.parent.GetComponent<CameraFollowPlayer>();
        transform.localPosition = parentCamera.transform.position * speed;
    }

    private void Update()
    {
        if (parentCamera.transform.position.x >= parentCamera.limitX.x || parentCamera.transform.position.x <= parentCamera.limitY.y)
        {
            transform.position = parentCamera.followPosition * speed;
        }
    }
}
