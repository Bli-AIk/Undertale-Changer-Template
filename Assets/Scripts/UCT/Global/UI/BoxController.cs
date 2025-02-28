using System.Collections.Generic;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.UI
{
    /// <summary>
    ///     战斗框总控
    /// </summary>
    public class BoxController : ObjectPool
    {
        public enum BoxType
        {
            None,
            Add,
            Sub
        }

        [Header("线宽")]
        public float width = 0.15f;

        [Header("起始时生成框，名字为空不生成")]
        public string startSummonName;

        public Vector3 startSummonPos;


        public List<BoxDrawer> boxes = new();

        public List<Vector2> pointsCrossSave, pointsOutCrossSave, pointsInCrossSave; //交点/非重合点/重合点

        private int _number;

        public static BoxController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            poolObject = new GameObject
            {
                name = "Box"
            };
            poolObject.AddComponent<BoxDrawer>();
            poolObject.SetActive(false);
            FillPool<BoxDrawer>();
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(startSummonName))
            {
                return;
            }

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
                    if (PassUpdate(i, j))
                    {
                        continue;
                    }

                    var box0 = boxes[i];
                    var box1 = boxes[j];

                    var realPointsBack0 = box0.GetRealPoints();
                    var realPointsBack1 = box1.GetRealPoints();

                    ComputePoints(realPointsBack0, realPointsBack1);


                    //两个 特殊框 重合时合并，剩下的交给父BoxDrawer。
                    if (IsNotMergeBoxes(box0, box1))
                    {
                        continue;
                    }

                    MergeBoxes(box0, box1, realPointsBack0, realPointsBack1);
                }
            }
        }

        private bool PassUpdate(int i, int j)
        {
            if (i >= j)
            {
                return true;
            }

            return boxes[i].transform.parent != transform || boxes[j].transform.parent != transform;
        }

        private void MergeBoxes(BoxDrawer box0, BoxDrawer box1, List<Vector2> realPointsBack0, List<Vector2> realPointsBack1)
        {
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
                BoxType.Add when box1.boxType == BoxType.Sub => BoxService.GetDifference(realPointsBack0,
                    realPointsBack1),
                BoxType.Sub when box1.boxType == BoxType.Add => BoxService.GetDifference(realPointsBack1,
                    realPointsBack0),
                _ => BoxService.GetUnion(realPointsBack0, realPointsBack1)
            };

            boxParent.realPoints = pointsFinal;
            BoxService.SummonBox(pointsFinal, boxParent.rotation, boxParent.transform, 0.15f,
                boxParent.lineRenderer, boxParent.edgeCollider2D, boxParent.meshFilter);

            pointsCrossSave.Clear();
            pointsInCrossSave.Clear();
            pointsOutCrossSave.Clear();
        }

        private bool IsNotMergeBoxes(BoxDrawer box0, BoxDrawer box1)
        {
            if (pointsCrossSave.Count == 0 && pointsInCrossSave.Count == 0)
            {
                return true;
            }

            return box0.boxType == BoxType.None || box1.boxType == BoxType.None ||
                   (box0.boxType == BoxType.Sub && box1.boxType == BoxType.Sub);
        }

        private void ComputePoints(List<Vector2> realPointsBack0, List<Vector2> realPointsBack1)
        {
            pointsCrossSave = BoxService.FindIntersections(realPointsBack0, realPointsBack1);
            pointsOutCrossSave = BoxService.ProcessPolygons(realPointsBack0, realPointsBack1);
            pointsInCrossSave = BoxService.AddAndSubLists(realPointsBack0, realPointsBack1, pointsCrossSave,
                pointsOutCrossSave);
        }

        private BoxDrawer GetFromThePool()
        {
            var points = new List<Vector2>
            {
                new(5.93f, 1.4f),
                new(5.93f, -1.4f),
                new(-5.93f, -1.4f),
                new(-5.93f, 1.4f)
            };

            var newBoxDrawer = GetFromPool<BoxDrawer>();
            newBoxDrawer.vertexPoints = points;
            boxes.Add(newBoxDrawer);
            _number++;
            newBoxDrawer.name = "Box" + _number;
            newBoxDrawer.width = width;
            newBoxDrawer.tag = "Box";
            return newBoxDrawer;
        }
    }
}