using LibTessDotNet;
using Log;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
/// <summary>
/// 战斗框总控
/// </summary>
public class BoxController : MonoBehaviour
{
    public static BoxController instance;

    public List<BoxDrawer> boxes = new List<BoxDrawer>();

    public List<Vector2> pointsCross, pointsOutCross, pointsInCross;//交点/非重合点/重合点

    private void Awake()
    {
        instance = this;
        boxes.Clear();
    }

    void Start()
    {

    }

    void Update()
    {
        boxes[0].SummonBox(true);
        boxes[1].SummonBox(true);

        pointsCross = FindIntersections(boxes[0].realPoints, boxes[1].realPoints);
        pointsOutCross = ProcessPolygons(boxes[0].realPoints, boxes[1].realPoints, pointsCross);
        pointsInCross = AddAndSubLists(boxes[0].realPoints, boxes[1].realPoints, pointsCross, pointsOutCross);
        
        List<Vector2> points;
        //if (pointsInCross.Count == 0)
        //    points = AddLists(pointsCross, pointsOutCross);
        //else
        points = AddLists(boxes[0].realPoints, boxes[1].realPoints);
        points = AddLists(pointsCross, points);
        points = SubLists(points, pointsInCross);

        SummonBox(SortPoints(CalculatePolygonCenter(AddLists(pointsCross,pointsInCross)), points), transform.rotation, transform, 0.15f);

        if (Input.GetKeyDown(KeyCode.F5))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    }
    /// <summary>
    /// 生成框
    /// </summary>
    public List<Vector2> SummonBox(List<Vector2> list, Quaternion rotation, Transform transform, float width = 0.15f, LineRenderer lineRenderer = null, MeshFilter meshFilter = null)
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

        meshFilter.mesh = GenerateMesh(polygon.ToArray(), meshFilter); // 最核心代码：构建Mesh！！

        lineRenderer.positionCount = polygon.Count;


        for (int i = 0; i < polygon.Count; i++)
        {
            polygon[i] += (Vector2)transform.position;
            lineRenderer.SetPosition(i, polygon[i]);
        }

        return polygon;
    }
    /// <summary>
    /// 生成框（仅计算坐标）
    /// </summary>
    public List<Vector2> SummonBox(List<Vector2> list, Quaternion rotation, Transform transform)
    {
        List<Vector2> polygon = new List<Vector2>(list);
        // 将每个点先旋转，然后再加上物体的位置
        for (int i = 0; i < polygon.Count; i++)
        {
            polygon[i] = rotation * polygon[i] + transform.position; 
        }

        polygon = RemoveDuplicates(polygon);

        return polygon;
    }
    /// <summary>
    /// 构造Mesh
    /// </summary>
    public Mesh GenerateMesh(Vector2[] polygonVertices, MeshFilter meshFilter)
    {

        // 将Vector数组转换为LibTessDotNet所需的ContourVertex数组
        ContourVertex[] contourVertices = new ContourVertex[polygonVertices.Length];
        for (int i = 0; i < polygonVertices.Length; i++)
        {
            contourVertices[i].Position = new Vec3 { X = polygonVertices[i].x, Y = polygonVertices[i].y, Z = 0 };
        }

        // 创建Tess对象并添加轮廓
        Tess tess = new Tess();
        tess.AddContour(contourVertices, ContourOrientation.Original);

        // 进行三角剖分
        tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

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
    public static List<Vector2> FindIntersections(List<Vector2> poly1, List<Vector2> poly2)
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
    private static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    /// <summary>
    /// 检查点C是否在AB线段上
    /// </summary>
    private static bool IsPointOnLineSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return Cross(a, b, c) == 0 && (c.x - a.x) * (c.x - b.x) <= 0 && (c.y - a.y) * (c.y - b.y) <= 0;
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

        return Cross(a, b, c) * Cross(a, b, d) < 0 && Cross(c, d, a) * Cross(c, d, b) < 0;
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
    public static List<Vector2> ProcessPolygons(List<Vector2> box1, List<Vector2> box2, List<Vector2> intersection)
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
    /// <summary>
    /// 计算多边形中点
    /// </summary>
    public Vector2 CalculatePolygonCenter(List<Vector2> polygonVertices)
    {
        Vector2 center = Vector2.zero;

        if (polygonVertices == null || polygonVertices.Count == 0)
        {
            return center;
        }

        foreach (Vector2 vertex in polygonVertices)
        {
            center += vertex;
        }

        center /= polygonVertices.Count;

        return center;
    }
    /// <summary>
    /// 排序列表各点
    /// </summary>
    public List<Vector2> SortPoints(Vector2 initialPoint, List<Vector2> points)
    {
        return points.OrderBy(p => Mathf.Atan2(initialPoint.y - p.y, initialPoint.x - p.x))
                      .ThenBy(p => (p - initialPoint).sqrMagnitude)
                      .ToList();
    }
    /// <summary>
    /// 前面两个相加，减去后面两个
    /// </summary>
    public static List<Vector2> AddAndSubLists(List<Vector2> list1, List<Vector2> list2, List<Vector2> list3, List<Vector2> list4)
    {
        List<Vector2> concatenatedList = AddLists(list1, list2);
        List<Vector2> subtractedResult = SubLists(concatenatedList, list3);
        subtractedResult = SubLists(subtractedResult, list4);

        return subtractedResult;
    }
    /// <summary>
    /// 把List相加
    /// </summary>
    public static List<T> AddLists<T>(List<T> list1, List<T> list2)
    {
        List<T> concatenatedList = new List<T>(list1);
        concatenatedList.AddRange(list2);
        return concatenatedList;
    }
    /// <summary>
    /// 把List相减
    /// </summary>
    public static List<T> SubLists<T>(List<T> sourceList, List<T> subtractedList)
    {
        List<T> result = new List<T>(sourceList);

        foreach (T point in subtractedList)
        {
            result.Remove(point);
        }

        return result;
    }
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (pointsCross == null)
            return;
        Gizmos.color = Color.blue;
        foreach (var point in pointsCross)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.15f);
        }

        if (pointsOutCross == null)
            return;
        Gizmos.color = Color.green;
        foreach (var point in pointsOutCross)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.15f);
        }

        if (pointsInCross == null)
            return;
        Gizmos.color = Color.magenta;
        foreach (var point in pointsInCross)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.15f);
        }
    }
#endif

}
