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

            poolObject.AddComponent<PolygonMask>();

            poolObject.gameObject.name = "PlayerLine";
            poolObject.SetActive(false);

            FillPool<LineRenderer>();

            Test();
        }

        private void Test()
        {
            var lineRenderer = GetFromPool<LineRenderer>();
            lineRenderer.SetPositions(new[] { new Vector3(-5, -2), new Vector3(5, -2) });
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