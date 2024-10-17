using System.Collections.Generic;
using Clipper2Lib;
using LibTessDotNet;
using UCT.Global.Core;
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

            obj = new GameObject();
            obj.name = "Box";
            obj.AddComponent<BoxDrawer>();
            obj.SetActive(false);
            FillPool();
        }
        int _number;
        public BoxDrawer GetFromThePool()
        {
            List<Vector2> points = new List<Vector2>
            {
                new Vector2(5.93f,1.4f),
                new Vector2(5.93f,-1.4f),
                new Vector2(-5.93f,-1.4f),
                new Vector2(-5.93f,1.4f),
            };

            BoxDrawer newBoxDrawer = GetFromPool().GetComponent<BoxDrawer>();
            newBoxDrawer.vertexPoints = points;
            boxes.Add(newBoxDrawer);
            _number++;
            newBoxDrawer.name = "Box" + _number;
            newBoxDrawer.width = width;
            newBoxDrawer.tag = "Box";
            return newBoxDrawer;
        }

        void Start()
        {
            if (startSummonName != null && startSummonName != "")
            {
                BoxDrawer start = GetFromThePool();
                start.name = startSummonName;
                start.localPosition = startSummonPos;
                MainControl.Instance.mainBox = start;
            }

            /*测试使用
        GetFromThePool();
        BoxDrawer a = GetFromThePool();
        a.localPosition += Vector3.right + Vector3.up;
        a = GetFromThePool();
        a.localPosition -= Vector3.right + Vector3.up;
        */
        }

        void Update()
        {
            /*
        if (Input.GetKeyDown(KeyCode.Y))
        {
            BoxDrawer a = GetFromThePool();
            a.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            a.boxType = BoxType.Add;
        }
        */


            for (int i = 0; i < boxes.Count; i++)
            {
                for (int j = 0; j < boxes.Count; j++)
                {
                    if (i >= j)
                        continue;

                    if (boxes[i].transform.parent != transform || boxes[j].transform.parent != transform)
                        continue;



                    BoxDrawer box0 = boxes[i];
                    BoxDrawer box1 = boxes[j];

                    List<Vector2> realPointsBack0, realPointsBack1;
                    //获取两个Box的realPoints
                    realPointsBack0 = box0.GetRealPoints();
                    realPointsBack1 = box1.GetRealPoints();

                    //计算三大List

                    pointsCrossSave = FindIntersections(realPointsBack0, realPointsBack1);

                    pointsOutCrossSave = ProcessPolygons(realPointsBack0, realPointsBack1, pointsCrossSave);

                    pointsInCrossSave = AddAndSubLists(realPointsBack0, realPointsBack1, pointsCrossSave, pointsOutCrossSave);



                    //两个 特殊框 重合时合并，剩下的交给父BoxDrawer。
                    if (!(pointsCrossSave.Count == 0 && pointsInCrossSave.Count == 0))
                    {
                        if (!(box0.boxType == BoxType.None || box1.boxType == BoxType.None) && !(box0.boxType == BoxType.Sub && box1.boxType == BoxType.Sub))
                        {
                            BoxDrawer boxParent = GetFromThePool();

                            boxParent.localPosition = new Vector3(0, 0, (box0.localPosition.z + box1.localPosition.z) / 2);

                            box0.transform.SetParent(boxParent.transform);
                            box1.transform.SetParent(boxParent.transform);

                            box0.parent = boxParent;
                            box1.parent = boxParent;

                            box0.IsOpenComponentsData();
                            box1.IsOpenComponentsData();

                            boxParent.pointsSonSum = AddLists(box0.realPoints, box1.realPoints);
                            boxParent.pointsCross = pointsCrossSave;
                            boxParent.pointsOutCross = pointsOutCrossSave;
                            boxParent.pointsInCross = pointsInCrossSave;

                            boxParent.sonBoxDrawer = new List<BoxDrawer> { box0, box1 };

                            //先删了，在父BoxDrawer内加回来
                            boxes.Remove(box0);
                            boxes.Remove(box1);

                            /*
                        //先生成一下
                        List<Vector2> points;
                        points = AddLists(realPointsBack0, realPointsBack1);
                        points = AddLists(points, pointsCrossSave);
                        points = SubLists(points, pointsInCrossSave);

                        //List<Vector2> pointsFinal = SortPoints(CalculatePolygonCenter(AddLists(pointsCrossSave, pointsInCrossSave)), points);
                        */
                            List<Vector2> pointsFinal;
                            if (box0.boxType == BoxType.Add && box1.boxType == BoxType.Sub)
                                pointsFinal = GetDifference(realPointsBack0, realPointsBack1);
                            else if (box0.boxType == BoxType.Sub && box1.boxType == BoxType.Add)
                                pointsFinal = GetDifference(realPointsBack1, realPointsBack0);
                            else
                                pointsFinal = GetUnion(realPointsBack0, realPointsBack1);


                            boxParent.realPoints = pointsFinal;
                            SummonBox(pointsFinal, boxParent.rotation, boxParent.transform, 0.15f, boxParent.lineRenderer, boxParent.edgeCollider2D, boxParent.meshFilter);


                            pointsCrossSave.Clear();
                            pointsInCrossSave.Clear();
                            pointsOutCrossSave.Clear();
                        }
                    }




                    /*
                continue;
                if (pointsCrossSave.Count == 0 && pointsInCrossSave.Count == 0)
                {
                    ResetBox();
                    boxes[i].realPoints = boxes[i].SummonBox();
                    boxes[j].realPoints = boxes[j].SummonBox();

                    boxes[i].transform.SetParent(transform);
                    boxes[j].transform.SetParent(transform);
                }
                else
                {
                    Debug.Log(i + " / " + j);

                    boxes[i].realPoints = realPointsBack0;
                    boxes[j].realPoints = realPointsBack1;
                    boxes[i].ClearComponentsData();
                    boxes[j].ClearComponentsData();

                    List<Vector2> points;

                    points = AddLists(boxes[i].realPoints, boxes[j].realPoints);
                    points = AddLists(pointsCrossSave, points);
                    points = SubLists(points, pointsInCrossSave);
                    List<Vector2> pointsFinal = SortPoints(CalculatePolygonCenter(AddLists(pointsCrossSave, pointsInCrossSave)), points);
                    SummonBox(pointsFinal, transform.rotation, transform, 0.15f);
                }
                */
                }
            }

     





        }

        /// <summary>
        /// 生成框
        /// </summary>
        public List<Vector2> SummonBox(List<Vector2> list, Quaternion rotation, Transform transform, float width = 0.15f, LineRenderer lineRenderer = null, EdgeCollider2D edgeCollider2D = null, MeshFilter meshFilter = null)
        {
            if (lineRenderer == null)
            {
                lineRenderer = transform.GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = transform.gameObject.AddComponent<LineRenderer>();
                    lineRenderer.startWidth = width;
                    lineRenderer.endWidth = width;
                    lineRenderer.material = Resources.Load<Material>("Materials/BoxLine");
                    lineRenderer.loop = true;
                }
            }

            if (edgeCollider2D == null)
            {
                edgeCollider2D = transform.GetComponent<EdgeCollider2D>();
                if (edgeCollider2D == null)
                {
                    edgeCollider2D = transform.gameObject.AddComponent<EdgeCollider2D>();
                }
            }

            if (meshFilter == null)
            {
                meshFilter = transform.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = transform.gameObject.AddComponent<MeshFilter>();

                    MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();
                    if (meshRenderer == null)
                    {
                        meshRenderer = transform.gameObject.AddComponent<MeshRenderer>();
                        meshRenderer.material = Resources.Load<Material>("Materials/BoxBack");
                    }

                }
            }

            List<Vector2> polygon = new List<Vector2>(list);
            // 将每个点先旋转，然后再加上物体的位置
            for (int i = 0; i < polygon.Count; i++)
            {
                polygon[i] = rotation * polygon[i];
            }

            polygon = RemoveDuplicates(polygon);

            lineRenderer.positionCount = polygon.Count;

            for (int i = 0; i < polygon.Count; i++)
            {
                lineRenderer.SetPosition(i, (Vector3)polygon[i] + transform.position);
            }

            meshFilter.mesh = GenerateMesh(polygon.ToArray()); // 最核心代码：构建Mesh！！

            edgeCollider2D.SetPoints(AddLists(polygon, new List<Vector2> { polygon[0] }));
            edgeCollider2D.edgeRadius = width / 2;

            return polygon;
        }
        /// <summary>
        /// 计算坐标获取RealPoints
        /// </summary>
        public List<Vector2> GetRealPoints(List<Vector2> list, Quaternion rotation, Transform transform, bool isLocal = true)
        {
            Vector3 local = isLocal ? transform.localPosition : transform.position;

            List<Vector2> polygon = new List<Vector2>(list);
            // 将每个点先旋转，然后再加上物体的位置
            for (int i = 0; i < polygon.Count; i++)
            {
                polygon[i] = rotation * polygon[i] + local;
            }

            polygon = RemoveDuplicates(polygon);

            return polygon;
        }
        /*
    /// <summary>
    /// 重置框
    /// </summary>
    public void ResetBox(LineRenderer lineRenderer = null, MeshFilter meshFilter = null)
    {
        if (lineRenderer == null)
        {
            lineRenderer = transform.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = transform.gameObject.AddComponent<LineRenderer>();
            }
        }
        if (meshFilter == null)
        {

            meshFilter = transform.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = transform.gameObject.AddComponent<MeshFilter>();
            }
        }

        meshFilter.mesh = null;
        lineRenderer.positionCount = 0;
    }
    */
        /// <summary>
        /// 构造Mesh
        /// </summary>
        public Mesh GenerateMesh(Vector2[] vertexPoints)
        {

            // 将Vector数组转换为LibTessDotNet所需的ContourVertex数组
            ContourVertex[] contourVertices = new ContourVertex[vertexPoints.Length];
            for (int i = 0; i < vertexPoints.Length; i++)
            {
                contourVertices[i].Position = new Vec3 { X = vertexPoints[i].x, Y = vertexPoints[i].y, Z = 0 };
            }

            // 创建Tess对象并添加轮廓
            Tess tess = new Tess();
            tess.AddContour(contourVertices);

            // 进行三角剖分
            tess.Tessellate(WindingRule.NonZero);

            // 创建Mesh对象
            Mesh mesh = new Mesh();

            // 将Tess结果转换为Unity Mesh格式
            Vector3[] vertices = new Vector3[tess.Vertices.Length];
            for (int i = 0; i < tess.Vertices.Length; i++)
            {
                vertices[i] = new Vector3(tess.Vertices[i].Position.X, tess.Vertices[i].Position.Y, 0);
            }

            int[] triangles = new int[tess.Elements.Length];
            for (int i = 0; i < tess.Elements.Length; i++)
            {
                triangles[i] = tess.Elements[i];
            }

            // 应用顶点和三角形到mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // 为mesh设置UV坐标
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                // 这里是一个简单的映射，将顶点坐标映射到UV空间
                // 通常，你需要根据具体情况来调整这部分代码


                uvs[i] = new Vector2(vertices[i].x, vertices[i].y);


            }
            mesh.uv = uvs;

            // 为了更好的渲染效果，可以计算法线和边界
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // 将mesh应用到GameObject
            return mesh;
        }
        /// <summary>
        /// 剔除重复项
        /// </summary>
        public List<Vector2> RemoveDuplicates(List<Vector2> originalList)
        {
            // 使用HashSet<Vector2>来存储已经遇到的Vector2元素，因为HashSet自动去重
            HashSet<Vector2> seen = new HashSet<Vector2>();
            // 用来存储去重后的列表
            List<Vector2> resultList = new List<Vector2>();

            foreach (var item in originalList)
            {
                // 如果HashSet中添加成功（即之前未遇到过这个元素），则将其添加到结果列表中
                if (seen.Add(item))
                {
                    resultList.Add(item);
                }
            }

            return resultList;
        }




        /// <summary>
        /// 主函数，计算两组线段的所有交点
        /// </summary>
        public List<Vector2> FindIntersections(List<Vector2> poly1, List<Vector2> poly2)
        {
            List<Vector2> intersections = new List<Vector2>();

            for (int i = 0; i < poly1.Count; i++)
            {
                Vector2 a = poly1[i];
                Vector2 b = poly1[(i + 1) % poly1.Count]; // 循环列表

                for (int j = 0; j < poly2.Count; j++)
                {
                    Vector2 c = poly2[j];
                    Vector2 d = poly2[(j + 1) % poly2.Count]; // 循环列表

                    if (DoLineSegmentsIntersect(a, b, c, d))
                    {
                        Vector2? intersection = CalculateIntersectionPoint(a, b, c, d);
                        if (intersection != null)
                        {
                            intersections.Add(intersection.Value);
                        }
                    }
                }
            }

            return intersections;
        }
        /// <summary>
        ///  计算向量叉乘
        /// </summary>
        private static float CrossSave(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        /// <summary>
        /// 检查点C是否在AB线段上
        /// </summary>
        private static bool IsPointOnLineSegment(Vector2 a, Vector2 b, Vector2 c)
        {
            return CrossSave(a, b, c) == 0 && (c.x - a.x) * (c.x - b.x) <= 0 && (c.y - a.y) * (c.y - b.y) <= 0;
        }

        /// <summary>
        /// 检查线段AB和CD是否相交
        /// </summary>
        public static bool DoLineSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            if (IsPointOnLineSegment(a, b, c) || IsPointOnLineSegment(a, b, d) ||
                IsPointOnLineSegment(c, d, a) || IsPointOnLineSegment(c, d, b))
            {
                return true;
            }

            return CrossSave(a, b, c) * CrossSave(a, b, d) < 0 && CrossSave(c, d, a) * CrossSave(c, d, b) < 0;
        }

        /// <summary>
        /// 计算两线段AB和CD的交点
        /// </summary>
        public static Vector2? CalculateIntersectionPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            if (!DoLineSegmentsIntersect(a, b, c, d))
                return null;

            // 计算线性方程的参数
            float denominator = (b.x - a.x) * (d.y - c.y) - (b.y - a.y) * (d.x - c.x);
            if (denominator == 0)
                return null; // 线段平行或共线

            float u = ((c.x - a.x) * (d.y - c.y) - (c.y - a.y) * (d.x - c.x)) / denominator;
            return new Vector2(a.x + u * (b.x - a.x), a.y + u * (b.y - a.y));
        }
        /// <summary>
        /// 计算非重合点
        /// </summary>
        public List<Vector2> ProcessPolygons(List<Vector2> box1, List<Vector2> box2, List<Vector2> intersection)
        {
            List<Vector2> filteredBox1 = RemovePointsInsideOtherPolygon(box1, box2);
            List<Vector2> filteredBox2 = RemovePointsInsideOtherPolygon(box2, box1);

            // 合并剔除后的列表
            List<Vector2> result = new List<Vector2>();
            result.AddRange(filteredBox1);
            result.AddRange(filteredBox2);

            return result;
        }


        private static List<Vector2> RemovePointsInsideOtherPolygon(List<Vector2> subjectPolygon, List<Vector2> clippingPolygon)
        {
            List<Vector2> result = new List<Vector2>();

            foreach (var point in subjectPolygon)
            {
                if (!IsPointInsidePolygon(point, clippingPolygon))
                {
                    result.Add(point);
                }
            }

            return result;
        }

        private static bool IsPointInsidePolygon(Vector2 point, List<Vector2> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        /*
/// <summary>
/// 以initialPoint为圆心，若干长度为半径，顺时针旋转，排序列表各点。
/// </summary>
public List<Vector2> SortPoints(Vector2 initialPoint, List<Vector2> points)
{

    return points.OrderBy(p => Mathf.Atan2(initialPoint.y - p.y, initialPoint.x - p.x))
                  .ThenBy(p => (p - initialPoint).sqrMagnitude)
                  .ToList();
}
*/
        /// <summary>
        /// 前面两个相加，减去后面两个
        /// </summary>
        public List<Vector2> AddAndSubLists(List<Vector2> list1, List<Vector2> list2, List<Vector2> list3, List<Vector2> list4)
        {
            List<Vector2> concatenatedList = AddLists(list1, list2);
            List<Vector2> subtractedResult = SubLists(concatenatedList, list3);
            subtractedResult = SubLists(subtractedResult, list4);

            return subtractedResult;
        }
        /// <summary>
        /// 把List相加
        /// </summary>
        public List<T> AddLists<T>(List<T> list1, List<T> list2)
        {
            List<T> concatenatedList = new List<T>(list1);
            concatenatedList.AddRange(list2);
            return concatenatedList;
        }
        /// <summary>
        /// 把List相减
        /// </summary>
        public List<T> SubLists<T>(List<T> sourceList, List<T> subtractedList)
        {
            List<T> result = new List<T>(sourceList);

            foreach (T point in subtractedList)
            {
                result.Remove(point);
            }

            return result;
        }

        //Clipper2 API 相关
        public PathsD ConvertVectorToPath(List<Vector2> vector)
        {
            List<double> doubles = new List<double>();
            int j = 0;
            for (int i = 0; i < vector.Count * 2; i++)
            {
                if (i % 2 == 0)//X
                {
                    doubles.Add(vector[j].x);
                }
                else//Y
                {
                    doubles.Add(vector[j].y);
                    j++;
                }
            }

            return new PathsD { Clipper.MakePath(doubles.ToArray()) };
        }

        public List<Vector2> ConvertPathToVector(PathsD path)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < path[0].Count; i++)
            {
                list.Add(new Vector2((float)path[0][i].x, (float)path[0][i].y));
            }
            return list;
        }


        /// <summary>
        /// 取交集
        /// </summary>
        public List<Vector2> GetUnion(List<Vector2> a, List<Vector2> b)
        {
            PathsD subj = ConvertVectorToPath(a);
            PathsD clip = ConvertVectorToPath(b);
            PathsD solution = Clipper.Union(subj, clip, FillRule.NonZero);

            return ConvertPathToVector(solution);
        }
        public List<Vector2> GetDifference(List<Vector2> origin, List<Vector2> sub)
        {
            PathsD subj = ConvertVectorToPath(origin);
            PathsD clip = ConvertVectorToPath(sub);
            PathsD solution = Clipper.Difference(subj, clip, FillRule.NonZero);
            return ConvertPathToVector(solution);
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
