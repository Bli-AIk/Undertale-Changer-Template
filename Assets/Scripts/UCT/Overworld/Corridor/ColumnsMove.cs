using UnityEngine;

namespace UCT.Overworld.Corridor
{
    /// <summary>
    ///     长廊柱子移动
    /// </summary>
    public class ColumnsMove : MonoBehaviour
    {
        public float speed; //包括方向(正负)
        private CameraFollowPlayer _parentCamera;

        private void Start()
        {
            if (Camera.main == null)
            {
                Other.Debug.LogError("未找到主摄像机");
                return;
            }

            _parentCamera = Camera.main.GetComponent<CameraFollowPlayer>();
            transform.position = _parentCamera.transform.position + _parentCamera.transform.position * speed;
        }

        private void Update()
        {
            if (_parentCamera.transform.position.x >= _parentCamera.limitX.x ||
                _parentCamera.transform.position.x <= _parentCamera.limitY.y)
                transform.position = _parentCamera.followPosition * speed;
        }
    }
}