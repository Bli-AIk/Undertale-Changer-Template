using UnityEngine;

namespace UCT.Overworld.Corridor
{
    /// <summary>
    /// 长廊柱子移动
    /// </summary>
    public class ColumnsMove : MonoBehaviour
    {
        private CameraFollowPlayer parentCamera;
        public float speed;//包括方向(正负)

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
}