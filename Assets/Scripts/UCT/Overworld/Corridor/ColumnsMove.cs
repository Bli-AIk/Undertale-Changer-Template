using UnityEngine;

namespace UCT.Overworld.Corridor
{
    /// <summary>
    /// 长廊柱子移动
    /// </summary>
    public class ColumnsMove : MonoBehaviour
    {
        private CameraFollowPlayer _parentCamera;
        public float speed;//包括方向(正负)

        private void Start()
        {
            _parentCamera = transform.parent.GetComponent<CameraFollowPlayer>();
            transform.localPosition = _parentCamera.transform.position * speed;
        }

        private void Update()
        {
            if (_parentCamera.transform.position.x >= _parentCamera.limitX.x || _parentCamera.transform.position.x <= _parentCamera.limitY.y)
            {
                transform.position = _parentCamera.followPosition * speed;
            }
        }
    }
}