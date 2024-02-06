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
/// ս�����ܿ�
/// </summary>
public class BoxController : MonoBehaviour
{
    public static BoxController instance;

    public List<BoxDrawer> boxes = new List<BoxDrawer>();

    public List<Vector2> pointsCross, pointsOutCross, pointsInCross;//����/���غϵ�/�غϵ�

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
    /// ���ɿ�
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
        // ��ÿ��������ת��Ȼ���ټ��������λ��
        for (int i = 0; i < polygon.Count; i++)
        {
            polygon[i] = rotation * polygon[i];
        }

        polygon = RemoveDuplicates(polygon);

        meshFilter.mesh = GenerateMesh(polygon.ToArray(), meshFilter); // ����Ĵ��룺����Mesh����

        lineRenderer.positionCount = polygon.Count;


        for (int i = 0; i < polygon.Count; i++)
        {
            polygon[i] += (Vector2)transform.position;
            lineRenderer.SetPosition(i, polygon[i]);
        }

        return polygon;
    }
    /// <summary>
    /// ���ɿ򣨽��������꣩
    /// </summary>
    public List<Vector2> SummonBox(List<Vector2> list, Quaternion rotation, Transform transform)
    {
        List<Vector2> polygon = new List<Vector2>(list);
        // ��ÿ��������ת��Ȼ���ټ��������λ��
        for (int i = 0; i < polygon.Count; i++)
        {
            polygon[i] = rotation * polygon[i] + transform.position; 
        }

        polygon = RemoveDuplicates(polygon);

        return polygon;
    }
    /// <summary>
    /// ����Mesh
    /// </summary>
    public Mesh GenerateMesh(Vector2[] polygonVertices, MeshFilter meshFilter)
    {

        // ��Vector����ת��ΪLibTessDotNet�����ContourVertex����
        ContourVertex[] contourVertices = new ContourVertex[polygonVertices.Length];
        for (int i = 0; i < polygonVertices.Length; i++)
        {
            contourVertices[i].Position = new Vec3 { X = polygonVertices[i].x, Y = polygonVertices[i].y, Z = 0 };
        }

        // ����Tess�����������
        Tess tess = new Tess();
        tess.AddContour(contourVertices, ContourOrientation.Original);

        // ���������ʷ�
        tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

        // ����Mesh����
        Mesh mesh = new Mesh();

        // ��Tess���ת��ΪUnity Mesh��ʽ
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

        // Ӧ�ö���������ε�mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Ϊmesh����UV����
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            // ������һ���򵥵�ӳ�䣬����������ӳ�䵽UV�ռ�
            // ͨ��������Ҫ���ݾ�������������ⲿ�ִ���


            uvs[i] = new Vector2(vertices[i].x, vertices[i].y);


        }
        mesh.uv = uvs;

        // Ϊ�˸��õ���ȾЧ�������Լ��㷨�ߺͱ߽�
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // ��meshӦ�õ�GameObject
        return mesh;
    }
    /// <summary>
    /// �޳��ظ���
    /// </summary>
    public List<Vector2> RemoveDuplicates(List<Vector2> originalList)
    {
        // ʹ��HashSet<Vector2>���洢�Ѿ�������Vector2Ԫ�أ���ΪHashSet�Զ�ȥ��
        HashSet<Vector2> seen = new HashSet<Vector2>();
        // �����洢ȥ�غ���б�
        List<Vector2> resultList = new List<Vector2>();

        foreach (var item in originalList)
        {
            // ���HashSet����ӳɹ�����֮ǰδ���������Ԫ�أ���������ӵ�����б���
            if (seen.Add(item))
            {
                resultList.Add(item);
            }
        }

        return resultList;
    }




    /// <summary>
    /// �����������������߶ε����н���
    /// </summary>
    public static List<Vector2> FindIntersections(List<Vector2> poly1, List<Vector2> poly2)
    {
        List<Vector2> intersections = new List<Vector2>();

        for (int i = 0; i < poly1.Count; i++)
        {
            Vector2 a = poly1[i];
            Vector2 b = poly1[(i + 1) % poly1.Count]; // ѭ���б�

            for (int j = 0; j < poly2.Count; j++)
            {
                Vector2 c = poly2[j];
                Vector2 d = poly2[(j + 1) % poly2.Count]; // ѭ���б�

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
    ///  �����������
    /// </summary>
    private static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    /// <summary>
    /// ����C�Ƿ���AB�߶���
    /// </summary>
    private static bool IsPointOnLineSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return Cross(a, b, c) == 0 && (c.x - a.x) * (c.x - b.x) <= 0 && (c.y - a.y) * (c.y - b.y) <= 0;
    }

    /// <summary>
    /// ����߶�AB��CD�Ƿ��ཻ
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
    /// �������߶�AB��CD�Ľ���
    /// </summary>
    public static Vector2? CalculateIntersectionPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (!DoLineSegmentsIntersect(a, b, c, d))
            return null;

        // �������Է��̵Ĳ���
        float denominator = (b.x - a.x) * (d.y - c.y) - (b.y - a.y) * (d.x - c.x);
        if (denominator == 0)
            return null; // �߶�ƽ�л���

        float u = ((c.x - a.x) * (d.y - c.y) - (c.y - a.y) * (d.x - c.x)) / denominator;
        return new Vector2(a.x + u * (b.x - a.x), a.y + u * (b.y - a.y));
    }
    /// <summary>
    /// ������غϵ�
    /// </summary>
    public static List<Vector2> ProcessPolygons(List<Vector2> box1, List<Vector2> box2, List<Vector2> intersection)
    {
        List<Vector2> filteredBox1 = RemovePointsInsideOtherPolygon(box1, box2);
        List<Vector2> filteredBox2 = RemovePointsInsideOtherPolygon(box2, box1);

        // �ϲ��޳�����б�
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
    /// ���������е�
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
    /// �����б����
    /// </summary>
    public List<Vector2> SortPoints(Vector2 initialPoint, List<Vector2> points)
    {
        return points.OrderBy(p => Mathf.Atan2(initialPoint.y - p.y, initialPoint.x - p.x))
                      .ThenBy(p => (p - initialPoint).sqrMagnitude)
                      .ToList();
    }
    /// <summary>
    /// ǰ��������ӣ���ȥ��������
    /// </summary>
    public static List<Vector2> AddAndSubLists(List<Vector2> list1, List<Vector2> list2, List<Vector2> list3, List<Vector2> list4)
    {
        List<Vector2> concatenatedList = AddLists(list1, list2);
        List<Vector2> subtractedResult = SubLists(concatenatedList, list3);
        subtractedResult = SubLists(subtractedResult, list4);

        return subtractedResult;
    }
    /// <summary>
    /// ��List���
    /// </summary>
    public static List<T> AddLists<T>(List<T> list1, List<T> list2)
    {
        List<T> concatenatedList = new List<T>(list1);
        concatenatedList.AddRange(list2);
        return concatenatedList;
    }
    /// <summary>
    /// ��List���
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
