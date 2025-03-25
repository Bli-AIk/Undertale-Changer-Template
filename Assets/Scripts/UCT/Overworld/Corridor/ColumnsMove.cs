using UnityEngine;

namespace UCT.Overworld.Corridor
{
    /// <summary>
    ///     长廊柱子移动
    /// </summary>
    public class ColumnsMove : MonoBehaviour
    {
        public float speed = -1.381f; //包括方向(正负)
        private CameraFollowPlayer _parentCamera;

        private void Start()
        {
            if (!_parentCamera)
            {
                _parentCamera = CameraFollowPlayer.Instance;
            }

            transform.position = _parentCamera.transform.position + _parentCamera.transform.position * speed;
        }

        private void Update()
        {
            if (!_parentCamera)
            {
                _parentCamera = CameraFollowPlayer.Instance;
            }

            if (_parentCamera.transform.position.x >= _parentCamera.minX ||
                _parentCamera.transform.position.x <= _parentCamera.maxX)
            {
                transform.position = _parentCamera.transform.position * speed;
            }
        }
    }
}