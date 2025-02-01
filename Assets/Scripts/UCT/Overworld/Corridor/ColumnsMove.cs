using System;
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
        
        private void Awake()
        {
            _parentCamera = CameraFollowPlayer.Instance;
        }

        private void Start()
        {
            if (!_parentCamera)
            {
                Other.Debug.LogError("未找到主摄像机");
                return;
            }

            transform.position = _parentCamera.transform.position + _parentCamera.transform.position * speed;
        }

        private void Update()
        {
            if (_parentCamera.transform.position.x >= _parentCamera.limitX.x ||
                _parentCamera.transform.position.x <= _parentCamera.limitY.y)
                transform.position = _parentCamera.transform.position * speed;
        }
    }
}