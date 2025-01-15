using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UCT.EventSystem;
using UnityEngine;

namespace UCT.Overworld.FiniteStateMachine
{
    /// <summary>
    ///     为对象添加视野。
    /// </summary>
    public class SightComponent : MonoBehaviour
    {
        private const float VisibilityThreshold = 0.125f;
        private const float MaxDistance = 5;
        public int segments = 30;

        [ReadOnly] public FiniteStateMachine fsm;

        public List<Vector2> directionList = new();
        public List<Vector2> offsetList = new();
        public Vector2 rayAngleRange = new(-15, 15);

        [Title("Gizmos")] public float gizmosDistance = 5f;

        private readonly HashSet<Collider2D> _currentColliders = new();
        private readonly HashSet<Collider2D> _processedColliders = new();

        private readonly HashSet<Collider2D> _visibleColliders = new();

        private Ray _basicRay;

        private Vector3 _rayOffset;

        private void Start()
        {
            fsm = transform.GetComponent<FiniteStateMachine>();
        }

        private void Update()
        {
            if (!fsm) return;

            var direction = fsm.data.directionPlayer;
            if (fsm && directionList.Count == offsetList.Count)
            {
                var closestAngle = float.MaxValue;
                var closestIndex = -1;

                for (var i = 0; i < directionList.Count; i++)
                {
                    var angle = Vector2.Angle(direction, directionList[i]);
                    if (!(angle < closestAngle)) continue;
                    closestAngle = angle;
                    closestIndex = i;
                }

                if (closestIndex >= 0) _rayOffset = offsetList[closestIndex];
            }

            _basicRay = new Ray(_rayOffset + transform.position, direction);

            DetectCollidersInSight();
        }

        private void OnDrawGizmos()
        {
            if (fsm == null || fsm.data == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_basicRay.origin, _basicRay.direction * gizmosDistance);

            Gizmos.color = Color.green;
            for (var i = 0; i <= 1; i++)
            {
                var currentAngle = Mathf.Lerp(rayAngleRange.x, rayAngleRange.y, (float)i / 1);
                Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * _basicRay.direction;
                Gizmos.DrawRay(_basicRay.origin, dir * gizmosDistance);
            }
        }

        /// <summary>
        ///     检测当前视野范围内的碰撞体，并根据状态触发进入或退出事件。
        /// </summary>
        private void DetectCollidersInSight()
        {
            _currentColliders.Clear();

            var angles = CalculateRayAngles();

            foreach (var currentAngle in angles) ProcessRayAtAngle(currentAngle);

            HandleExitEvents();
        }

        /// <summary>
        ///     计算射线的角度列表，从中心角度开始依次向上和向下偏移。
        /// </summary>
        /// <returns>射线角度的列表。</returns>
        private List<float> CalculateRayAngles()
        {
            var centerAngle = (rayAngleRange.x + rayAngleRange.y) / 2;
            var angleStep = (rayAngleRange.y - rayAngleRange.x) / (segments * 2);

            var angles = new List<float> { centerAngle };
            for (var i = 1; i <= segments; i++)
            {
                var upwardAngle = centerAngle + i * angleStep;
                var downwardAngle = centerAngle - i * angleStep;

                if (upwardAngle <= rayAngleRange.y) angles.Add(upwardAngle);
                if (downwardAngle >= rayAngleRange.x) angles.Add(downwardAngle);
            }

            return angles;
        }

        /// <summary>
        ///     根据指定的角度发射射线并处理检测到的碰撞体。
        /// </summary>
        /// <param name="currentAngle">当前射线的角度。</param>
        private void ProcessRayAtAngle(float currentAngle)
        {
            var rayDirection = Quaternion.Euler(0, 0, currentAngle) * _basicRay.direction;

            var layerMask = ~((1 << LayerMask.NameToLayer("UI")) |
                              (1 << LayerMask.NameToLayer("CanvasUI")) |
                              (1 << LayerMask.NameToLayer("Ignore Raycast")));

            var currentLayer = gameObject.layer;

            if (currentLayer == LayerMask.NameToLayer("Player"))
                layerMask &= ~(1 << LayerMask.NameToLayer("Player"));

            if (currentLayer == LayerMask.NameToLayer("NPC"))
                layerMask &= ~(1 << LayerMask.NameToLayer("NPC"));

            var hit = Physics2D.Raycast(_basicRay.origin, rayDirection, Mathf.Infinity, layerMask);
            if (!hit.collider) return;
            if (!hit.collider.CompareTag("owObjTrigger")) return;
            if (!hit.collider.TryGetComponent<OverworldEventTrigger>(out var trigger)) return;
            if (!IsObjectVisible(trigger)) return;

            _currentColliders.Add(hit.collider);
            _visibleColliders.Add(hit.collider);

            if (_processedColliders.Contains(hit.collider) ||
                !trigger.IsEventTriggerModeActive(EventTriggerMode.LineOfSightEnter)) return;

            trigger.TriggerEvent();
            _processedColliders.Add(hit.collider);
        }

        /// <summary>
        ///     检测当前帧不可见的物体并触发对应的退出事件。
        /// </summary>
        private void HandleExitEvents()
        {
            foreach (var item in _visibleColliders.ToList())
            {
                if (_currentColliders.Contains(item) ||
                    !item.TryGetComponent<OverworldEventTrigger>(out var trigger)) continue;


                if (trigger.IsEventTriggerModeActive(EventTriggerMode.LineOfSightExit))
                {
                    trigger.TriggerEvent();
                }

                _visibleColliders.Remove(item);
                _processedColliders.Remove(item);
            }
        }

        /// <summary>
        ///     判断物体是否可见
        /// </summary>
        /// <param name="clarity">物体可视度值（0到1之间）</param>
        /// <param name="distance">物体与玩家的实际距离</param>
        /// <param name="itemAngle">物体与玩家的角度</param>
        /// <returns>物体是否可见</returns>
        private bool IsObjectVisible(float clarity, float distance, float itemAngle)
        {
            if (distance >= MaxDistance) return false;

            var distanceFactor = Mathf.Clamp01(1 - distance / MaxDistance);

            var centerAngle = Mathf.Lerp(rayAngleRange.x, rayAngleRange.y, 0.5f);
            var range = Mathf.Abs(rayAngleRange.y - rayAngleRange.x) * 0.5f;

            var angleDifference = Mathf.Abs(Mathf.DeltaAngle(itemAngle, centerAngle));

            if (angleDifference > range) return false;

            var angleFactor = 1 - angleDifference / range;

            var visibility = clarity * distanceFactor * angleFactor;

            //UnityEngine.Debug.Log($"Visibility: {visibility}, Clarity: {clarity}, DistanceFactor: {distanceFactor}, AngleFactor: {angleFactor}");

            return visibility >= VisibilityThreshold;
        }


        /// <summary>
        ///     判断物体是否可见
        /// </summary>
        private bool IsObjectVisible(OverworldEventTrigger trigger)
        {
            return IsObjectVisible(trigger.clarity, CalculateDistance(trigger.transform),
                CalculateAngle(trigger.transform, fsm.data.directionPlayer));
        }

        /// <summary>
        ///     计算当前物体与目标物体之间的距离。
        /// </summary>
        /// <param name="target">目标物体的Transform。</param>
        /// <returns>返回两物体之间的距离。</returns>
        private float CalculateDistance(Transform target)
        {
            if (target)
                return Vector3.Distance(transform.position, target.position);

            Other.Debug.LogError("目标物体为空。");
            return 0f;
        }


        /// <summary>
        ///     计算当前物体与目标物体之间的角度（基于自身朝向）。
        /// </summary>
        /// <param name="target">目标物体的Transform。</param>
        /// <param name="selfDirection">当前物体的朝向（Vector2）。</param>
        /// <returns>返回两物体之间的角度（取绝对值，以度为单位）。</returns>
        private float CalculateAngle(Transform target, Vector2 selfDirection)
        {
            if (target)
            {
                var directionToTarget = new Vector2(target.position.x - transform.position.x,
                    target.position.y - transform.position.y);
                var angle = Vector2.SignedAngle(selfDirection, directionToTarget);
                return Mathf.Abs(angle);
            }

            Other.Debug.LogError("目标物体为空。");
            return 0f;
        }
    }
}