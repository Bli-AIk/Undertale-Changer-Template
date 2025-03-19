using System.Collections.Generic;
using Alchemy.Inspector;
using UCT.Core;
using UnityEngine;

namespace UCT.Battle
{
    public class PlayerLineController : ObjectPool
    {
        [ReadOnly]
        public List<LineRenderer> lines;
        private void Start()
        {
            const float width = 0.05f;
            var color = MainControl.Instance.BattleControl.playerColorList[6];

            poolObject = new GameObject();

            var lineRenderer = poolObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.sortingOrder = 40;
            lineRenderer.useWorldSpace = false;
            poolObject.AddComponent<PolygonMask>();

            poolObject.gameObject.name = "PlayerLine";
            poolObject.SetActive(false);

            FillPool<LineRenderer>();

            Test();
        }

        private void Test()
        {
            var curveRenderer = GetFromPool<LineRenderer>();
            curveRenderer.positionCount = 10;
            var curvePoints = new Vector3[10];
            for (var i = 0; i < 10; i++)
            {
                var x = Mathf.Lerp(-5, 5, i / 9f);
                var y = Mathf.Sin(x) * 1.5f - 2;
                curvePoints[i] = new Vector3(x, y, 0);
            }
            curveRenderer.SetPositions(curvePoints);
    
            var polylineRenderer = GetFromPool<LineRenderer>();
            var polylinePoints = new[]
            {
                new Vector3(-5, -1, 0),
                new Vector3(-2, -0.5f, 0),
                new Vector3(1, -1.5f, 0),
                new Vector3(4, -1, 0),
                new Vector3(5, -1.2f, 0)
            };
            polylineRenderer.positionCount = polylinePoints.Length;
            polylineRenderer.SetPositions(polylinePoints);
        }

        public override T GetFromPool<T>()
        {
            var result = base.GetFromPool<T>();
            if (result is LineRenderer lineRenderer)
            {
                lines.Add(lineRenderer);
            }
            else
            {
                Debug.LogError("PlayerLineController应当返回LineRenderer!");
            }

            return result;
        }

        public override void ReturnPool<T>(GameObject inputGameObject, T script)
        {
            base.ReturnPool(inputGameObject, script);
            if (script is LineRenderer lineRenderer)
            {
                lines.Remove(lineRenderer);
            }
            else
            {
                Debug.LogError("PlayerLineController应当返回LineRenderer!");
            }
        }
    }
}