using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UCT.Overworld.FiniteStateMachine
{
    public enum StateType
    {
        Idle,
        Walk,
        Run,
        Spin
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class FiniteStateMachine : MonoBehaviour
    {
        [ReadOnly] public FiniteStateMachineData data;
        public string dataRoute;

        protected internal readonly Dictionary<StateType, IState> States = new();

        private List<Vector2> _path;
        private List<Vector2> _traversed;
        protected IState CurrentState;
        
        private int _pathIndex;

        private void Awake()
        {
            InitializeData();
            InitializeStates();
        }

        private void Update()
        {
            CurrentState.OnUpdate();
        }

        private void FixedUpdate()
        {
            CurrentState.OnFixedUpdate();
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (_traversed != null)
            {
                foreach (var traverse in _traversed)
                {
                    Handles.DrawSolidDisc(traverse, Vector3.forward, 0.1f);
                }
            }

            if (_path == null || _path.Count < 2)
            {
                return;
            }

            for (var i = 0; i < _path.Count; i++)
            {
                var t = (float)i / (_path.Count - 1);
                var color = Color.Lerp(Color.cyan, Color.red, t);
                Handles.color = color;

                Handles.DrawSolidDisc(_path[i], Vector3.forward, 0.1f);

                if (i < _path.Count - 1)
                {
                    Handles.DrawLine(_path[i], _path[i + 1]);
                }
            }
#endif
        }

        private void InitializeData()
        {
            if (string.IsNullOrEmpty(dataRoute))
            {
                dataRoute = "Default";
            }

            data = Resources.Load<FiniteStateMachineData>($"FiniteStateMachine/{dataRoute}");

            TryGetComponent(out data.animator);
            data.rigidbody2D = GetComponent<Rigidbody2D>();
        }


        protected virtual void InitializeStates()
        {
            States.Add(StateType.Idle, new IdleState(this, data));
            States.Add(StateType.Walk, new WalkState(this, data));
            TransitionState(States[StateType.Idle]);
        }

        protected void TransitionState(IState newState)
        {
            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }

        protected void UpdateMoveTo(Vector3 targetPosition)
        {
            if (_path == null || _path.Count == 0 || (Vector2)targetPosition != _path[^1] || _pathIndex >= _path.Count)
            {
                var sqrDistance = ((Vector3)data.rigidbody2D.position - targetPosition).sqrMagnitude;


                if (sqrDistance >= 0.01f)
                {
                    _path = GetAStarPath(data.rigidbody2D.position, targetPosition);

                    if (_path.Count == 0)
                    {
                        Other.Debug.LogError($"{transform.name} 尝试移动到 {targetPosition}，但路径过长或无可用路径");
                        return;
                    }
                }
                else
                {
                    _path = new List<Vector2>();
                }

                _pathIndex = 0;
            }

            if (_path == null || _path.Count == 0)
            {
                TransitionState(States[StateType.Idle]);
                data.rigidbody2D.position = targetPosition;
                return;
            }

            // 获取当前目标点
            var currentPosition = data.rigidbody2D.position;
            var targetPoint = _path[_pathIndex];

            // 检查是否到达当前目标点
            if ((currentPosition - targetPoint).sqrMagnitude < 0.01f)
            {
                _pathIndex++;

                if (_pathIndex >= _path.Count) // 路径结束
                {
                    TransitionState(States[StateType.Idle]);
                    return;
                }

                targetPoint = _path[_pathIndex];
            }

            // 计算方向（只取八向）
            var direction = (targetPoint - currentPosition).normalized;
            Vector2[] eightDirections =
            {
                Vector2.right, Vector2.left, Vector2.up, Vector2.down,
                new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
            };

            var bestDot = -1f;
            var bestDirection = Vector2.zero;

            foreach (var dir in eightDirections)
            {
                var dot = Vector2.Dot(direction, dir);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDirection = dir;
                }
            }

            // 设置方向并切换状态
            data.direction = bestDirection;
            TransitionState(States[StateType.Walk]);
        }

        /// <summary>
        ///     计算对角线距离
        /// </summary>
        public static float DiagonalDistance(Vector2 start, Vector2 end)
        {
            var dx = Mathf.Abs(end.x - start.x);
            var dy = Mathf.Abs(end.y - start.y);
            const float d = 1f;
            const float d2 = 1.4f;
            return d * (dx + dy) + (d2 - 2 * d) * Mathf.Min(dx, dy);
        }


        /// <summary>
        ///     通过A*算法获取从起点到终点的路径，包括起点和终点。
        /// </summary>
        /// <returns></returns>
        private List<Vector2> GetAStarPath(Vector2 start, Vector2 end)
        {
            var offset = 0.5f * Vector2.one;

            var candidates = new[]
            {
                RoundVector(start, false, false),
                RoundVector(start, false, true),
                RoundVector(start, true, false),
                RoundVector(start, true, true)
            };

            var startInGrid = candidates.OrderBy(p => Vector2.Distance(p, start)).First() + offset;
            var endInGrid = new Vector2((int)end.x, (int)end.y) + offset;

            var openList = new List<AStarNode>();
            var closedList = new HashSet<Vector2>();

            var startNode = new AStarNode
            {
                Position = startInGrid,
                GCost = 0,
                HCost = DiagonalDistance(startInGrid, endInGrid)
            };
            openList.Add(startNode);

            var directions = new List<Vector2>
            {
                Vector2.right, Vector2.left, Vector2.up, Vector2.down,
                Vector2.right + Vector2.up, Vector2.right + Vector2.down,
                Vector2.left + Vector2.up, Vector2.left + Vector2.down
            };

            Dictionary<Vector2, AStarNode> nodeLookup = new()
            {
                [startInGrid] = startNode
            };

            while (openList.Count is > 0 and < 100)
            {
                openList.Sort((a, b) => (a.GCost + a.HCost).CompareTo(b.GCost + b.HCost));
                var currentNode = openList[0];
                openList.RemoveAt(0);
                closedList.Add(currentNode.Position);

                if (currentNode.Position == endInGrid)
                {
                    List<Vector2> aStarPath = new();
                    var node = currentNode;
                    while (node != null)
                    {
                        aStarPath.Add(node.Position);
                        node = node.Parent;
                    }

                    aStarPath.Reverse();

                    if (aStarPath[0] != start)
                    {
                        aStarPath.Insert(0, start);
                    }

                    if (aStarPath[^1] != end)
                    {
                        aStarPath.Add(end);
                    }

                    if (aStarPath.Count >= 3 &&
                        Vector2.Distance(aStarPath[^1], aStarPath[^3]) < Vector2.Distance(aStarPath[^2], aStarPath[^3]))
                    {
                        aStarPath.RemoveAt(aStarPath.Count - 2);
                    }

                    _traversed = closedList.ToList();
                    return aStarPath;
                }

                foreach (var direction in directions)
                {
                    var newNodePos = currentNode.Position + direction;
                    if (closedList.Contains(newNodePos))
                    {
                        continue;
                    }

                    var layerMask = ~LayerMask.GetMask("Player", "UI", "CanvasUI", "Ignore Raycast");
                    var hit = Physics2D.Raycast(currentNode.Position, direction.normalized, direction.magnitude,
                        layerMask);
                    if (hit.collider && hit.collider.transform != transform && !hit.collider.isTrigger)
                    {
                        continue;
                    }


                    var weight = direction.magnitude;
                    var gCost = currentNode.GCost + weight;
                    var hCost = DiagonalDistance(newNodePos, endInGrid);

                    if (nodeLookup.TryGetValue(newNodePos, out var existingNode))
                    {
                        if (gCost >= existingNode.GCost)
                        {
                            continue;
                        }

                        existingNode.GCost = gCost;
                        existingNode.Parent = currentNode;
                    }
                    else
                    {
                        var newNode = new AStarNode
                        {
                            Position = newNodePos,
                            Parent = currentNode,
                            GCost = gCost,
                            HCost = hCost
                        };
                        openList.Add(newNode);
                        nodeLookup[newNodePos] = newNode;
                    }
                }
            }

            return new List<Vector2>();

            Vector2 RoundVector(Vector2 v, bool roundX, bool roundY)
            {
                return new Vector2(roundX ? Mathf.Round(v.x) : Mathf.Floor(v.x),
                    roundY ? Mathf.Round(v.y) : Mathf.Floor(v.y));
            }
        }
    }

    public class AStarNode
    {
        public float GCost;
        public float HCost;
        public AStarNode Parent;
        public Vector2 Position;
    }
}