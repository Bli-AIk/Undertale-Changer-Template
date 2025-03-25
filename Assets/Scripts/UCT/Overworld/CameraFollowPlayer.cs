using System;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace UCT.Overworld
{
    /// <summary>
    ///     Overworld摄像机跟随
    /// </summary>
    public class CameraFollowPlayer : MonoBehaviour
    {
        public bool isLimit = true;
        public bool isFollow;

        public float minX;
        public float minY;
        public float maxX;
        public float maxY;

        public GameObject player;
        public static CameraFollowPlayer Instance { get; private set; }

        private Tween _shakeTween; 
        private Vector3 _shakeOffset; 

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            player = GameObject.Find("Player");
        }

        private void LateUpdate()
        {
            var saveZ = transform.position.z;
            var targetPosition = player.transform.position;

            if (isLimit)
            {
                targetPosition = GetLimitedPosition(targetPosition);
            }

            if (!isFollow)
            {
                return;
            }

            transform.position = targetPosition + _shakeOffset;
            transform.position = new Vector3(transform.position.x,transform.position.y,saveZ);
        }

        public Vector3 GetLimitedPosition(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            return pos;
        }

        /// <summary>
        /// 使摄像机抖动（不会影响跟随）
        /// </summary>
        /// <param name="duration">抖动持续时间</param>
        /// <param name="strength">抖动幅度</param>
        /// <param name="vibrato">振动频率</param>
        public void ShakeCamera(float duration = 0.1f, float strength = 0.5f, int vibrato = 5)
        {
            if (_shakeTween != null && _shakeTween.IsPlaying())
            {
                return;
            }

            _shakeTween = DOTween.To(() => _shakeOffset, x => _shakeOffset = new Vector3(x.x, x.y, 0), 
                Random.insideUnitSphere * strength, duration)
                .SetEase(Ease.OutQuad)
                .SetLoops(vibrato, LoopType.Yoyo)
                .OnComplete(() => _shakeOffset = Vector3.zero); 
        }
    }
}
