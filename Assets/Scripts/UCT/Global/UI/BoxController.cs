using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using LibTessDotNet;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.UI
{
    /// <summary>
    /// 战斗框总控
    /// </summary>
    public class BoxController : ObjectPool
    {
        public static BoxController Instance;
        [Header("线宽")]
        public float width = 0.15f;

        [Header("起始时生成框，名字为空不生成")]
        public string startSummonName;
        public Vector3 startSummonPos;


        public List<BoxDrawer> boxes = new List<BoxDrawer>();

        public List<Vector2> pointsCrossSave, pointsOutCrossSave, pointsInCrossSave;//交点/非重合点/重合点

        public enum BoxType
        {
            None,
            Add,
            Sub
        }

        private void Awake()
        {
            Instance = this;

            obj = new GameObject
            {
                name = "Box"
            };
            obj.AddComponent<BoxDrawer>();
            obj.SetActive(false);
            FillPool();
        }

        private int _number;

        private BoxDrawer GetFromThePool()
        {
            var points = new List<Vector2>
            {
                new(5.93f,1.4f),
                new(5.93f,-1.4f),
                new(-5.93f,-1.4f),
                new(-5.93f,1.4f),
            };

            var newBoxDrawer = GetFromPool().GetComponent<BoxDrawer>();
            newBoxDrawer.vertexPoints = points;
            boxes.Add(newBoxDrawer);
            _number++;
            newBoxDrawer.name = "Box" + _number;
            newBoxDrawer.width = width;
            newBoxDrawer.tag = "Box";
            return newBoxDrawer;
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(startSummonName)) return;
            var start = GetFromThePool();
            start.name = startSummonName;
            start.localPosition = startSummonPos;
            MainControl.Instance.mainBox = start;
        }

        private void Update()
        {
            for (var i = 0; i < boxes.Count; i++)
            {
                for (var j = 0; j < boxes.Count; j++)
                {
                    if (i >= j)
                        continue;

                    if (boxes[i].transform.parent != transform || boxes[j].transform.parent != transform)
                        continue;



                    var box0 = boxes[i];
                    var box1 = boxes[j];

                    //获取两个Box的realPoints
                    var realPointsBack0 = box0.GetRealPoints();
                    var realPointsBack1 = box1.GetRealPoints();

                    //计算三大List
                    pointsCrossSave = BoxService.FindIntersections(realPointsBack0, realPointsBack1);

                    pointsOutCrossSave = BoxService.ProcessPolygons(realPointsBack0, realPointsBack1, pointsCrossSave);

                    pointsInCrossSave = BoxService.AddAndSubLists(realPointsBack0, realPointsBack1, pointsCrossSave, pointsOutCrossSave);



                    //两个 特殊框 重合时合并，剩下的交给父BoxDrawer。
                    if (pointsCrossSave.Count == 0 && pointsInCrossSave.Count == 0) continue;
                    if (box0.boxType == BoxType.None || box1.boxType == BoxType.None ||
                        box0.boxType == BoxType.Sub && box1.boxType == BoxType.Sub) continue;
                    var boxParent = GetFromThePool();

                    boxParent.localPosition = new Vector3(0, 0, (box0.localPosition.z + box1.localPosition.z) / 2);

                    box0.transform.SetParent(boxParent.transform);
                    box1.transform.SetParent(boxParent.transform);

                    box0.parent = boxParent;
                    box1.parent = boxParent;

                    box0.IsOpenComponentsData();
                    box1.IsOpenComponentsData();

                    boxParent.pointsSonSum = BoxService.AddLists(box0.realPoints, box1.realPoints);
                    boxParent.pointsCross = pointsCrossSave;
                    boxParent.pointsOutCross = pointsOutCrossSave;
                    boxParent.pointsInCross = pointsInCrossSave;

                    boxParent.sonBoxDrawer = new List<BoxDrawer> { box0, box1 };

                    //在父BoxDrawer内会加回来
                    boxes.Remove(box0);
                    boxes.Remove(box1);

                    var pointsFinal = box0.boxType switch
                    {
                        BoxType.Add when box1.boxType == BoxType.Sub => BoxService.GetDifference(realPointsBack0, realPointsBack1),
                        BoxType.Sub when box1.boxType == BoxType.Add => BoxService.GetDifference(realPointsBack1, realPointsBack0),
                        _ => BoxService.GetUnion(realPointsBack0, realPointsBack1)
                    };

                    boxParent.realPoints = pointsFinal;
                    BoxService.SummonBox(pointsFinal, boxParent.rotation, boxParent.transform, 0.15f, boxParent.lineRenderer, boxParent.edgeCollider2D, boxParent.meshFilter);

                    pointsCrossSave.Clear();
                    pointsInCrossSave.Clear();
                    pointsOutCrossSave.Clear();
                }
            }

     





        }

        /*
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (pointsCrossSave == null)
            return;
        Gizmos.color = Color.blue;
        foreach (var point in pointsCrossSave)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.15f);
        }

        if (pointsOutCrossSave == null)
            return;
        Gizmos.color = Color.green;
        foreach (var point in pointsOutCrossSave)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.15f);
        }

        if (pointsInCrossSave == null)
            return;
        Gizmos.color = Color.magenta;
        foreach (var point in pointsInCrossSave)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.15f);
        }

    }
#endif
    */

    }
}
