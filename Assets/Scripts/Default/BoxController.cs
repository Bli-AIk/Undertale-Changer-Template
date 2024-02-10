using LibTessDotNet;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;
/// <summary>
/// ս�����ܿ�
/// </summary>
public class BoxController : ObjectPool
{
    public static BoxController instance;

    public List<BoxDrawer> boxes = new List<BoxDrawer>();

    public List<Vector2> pointsCrossSave, pointsOutCrossSave, pointsInCrossSave;//����/���غϵ�/�غϵ�

    public enum BoxType
    {
        None,
        Add,
        Sub
    }

    private void Awake()
    {
        instance = this;

        obj = new GameObject();
        obj.name = "Box";
        obj.AddComponent<BoxDrawer>();
        obj.SetActive(false);
        FillPool();
    }
    int num;
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
        num++;
        newBoxDrawer.name = "Box" + num;
        return newBoxDrawer;
    }

    void Start()
    {
        GetFromThePool();
        BoxDrawer a = GetFromThePool();
        a.localPosition += Vector3.right + Vector3.up;
        a = GetFromThePool();
        a.localPosition -= Vector3.right + Vector3.up;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
            GetFromThePool();
        //return;


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
                //��ȡ����Box��realPoints
                realPointsBack0 = box0.GetRealPoints();
                realPointsBack1 = box1.GetRealPoints();

                //��������List

                pointsCrossSave = FindIntersections(realPointsBack0, realPointsBack1);

                pointsOutCrossSave = ProcessPolygons(realPointsBack0, realPointsBack1, pointsCrossSave);

                pointsInCrossSave = AddAndSubLists(realPointsBack0, realPointsBack1, pointsCrossSave, pointsOutCrossSave);



                //�������غ�ʱ�ϲ���ʣ�µĽ�����BoxDrawer
                if (!(pointsCrossSave.Count == 0 && pointsInCrossSave.Count == 0))
                {
                    BoxDrawer boxParent = GetFromThePool();

                    Debug.Log(boxParent);

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

                    //��ɾ�ˣ��ڸ�BoxDrawer�ڼӻ���
                    boxes.Remove(box0);
                    boxes.Remove(box1);

                    //������һ��
                    List<Vector2> points;
                    points = AddLists(realPointsBack0, realPointsBack1);
                    points = AddLists(points, pointsCrossSave);
                    points = SubLists(points, pointsInCrossSave);

                    //List<Vector2> pointsFinal = SortPoints(CalculatePolygonCenter(AddLists(pointsCrossSave, pointsInCrossSave)), points);
                    List<Vector2> pointsFinal = GetUnion(realPointsBack0, realPointsBack1);
                    boxParent.realPoints = pointsFinal;
                    SummonBox(pointsFinal, boxParent.rotation, boxParent.transform, 0.15f, boxParent.lineRenderer, boxParent.meshFilter);


                    pointsCrossSave.Clear();
                    pointsInCrossSave.Clear();
                    pointsOutCrossSave.Clear();
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

        lineRenderer.positionCount = polygon.Count;


        for (int i = 0; i < polygon.Count; i++)
        {
            lineRenderer.SetPosition(i, polygon[i] + (Vector2)transform.position);
        }

        meshFilter.mesh = GenerateMesh(polygon.ToArray(), meshFilter); // ����Ĵ��룺����Mesh����

        

        return polygon;
    }
    /// <summary>
    /// ���������ȡRealPoints
    /// </summary>
    public List<Vector2> GetRealPoints(List<Vector2> list, Quaternion rotation, Transform transform)
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
    /*
    /// <summary>
    /// ���ÿ�
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
    /// ����Mesh
    /// </summary>
    public Mesh GenerateMesh(Vector2[] vertexPoints, MeshFilter meshFilter)
    {

        // ��Vector����ת��ΪLibTessDotNet�����ContourVertex����
        ContourVertex[] contourVertices = new ContourVertex[vertexPoints.Length];
        for (int i = 0; i < vertexPoints.Length; i++)
        {
            contourVertices[i].Position = new Vec3 { X = vertexPoints[i].x, Y = vertexPoints[i].y, Z = 0 };
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
    public List<Vector2> FindIntersections(List<Vector2> poly1, List<Vector2> poly2)
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
    private static float CrossSave(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    /// <summary>
    /// ����C�Ƿ���AB�߶���
    /// </summary>
    private static bool IsPointOnLineSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return CrossSave(a, b, c) == 0 && (c.x - a.x) * (c.x - b.x) <= 0 && (c.y - a.y) * (c.y - b.y) <= 0;
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

        return CrossSave(a, b, c) * CrossSave(a, b, d) < 0 && CrossSave(c, d, a) * CrossSave(c, d, b) < 0;
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
    public List<Vector2> ProcessPolygons(List<Vector2> box1, List<Vector2> box2, List<Vector2> intersection)
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
    /*
    /// <summary>
    /// ���������е�
    /// </summary>
    public Vector2 CalculatePolygonCenter(List<Vector2> vertexPoints)
    {
        Vector2 center = Vector2.zero;

        if (vertexPoints == null || vertexPoints.Count == 0)
        {
            return center;
        }

        foreach (Vector2 vertex in vertexPoints)
        {
            center += vertex;
        }

        center /= vertexPoints.Count;

        return center;
    }
    /// <summary>
    /// ��initialPointΪԲ�ģ����ɳ���Ϊ�뾶��˳ʱ����ת�������б���㡣
    /// </summary>
    public List<Vector2> SortPoints(Vector2 initialPoint, List<Vector2> points)
    {

        return points.OrderBy(p => Mathf.Atan2(initialPoint.y - p.y, initialPoint.x - p.x))
                      .ThenBy(p => (p - initialPoint).sqrMagnitude)
                      .ToList();
    }
    */
    /// <summary>
    /// ǰ��������ӣ���ȥ��������
    /// </summary>
    public List<Vector2> AddAndSubLists(List<Vector2> list1, List<Vector2> list2, List<Vector2> list3, List<Vector2> list4)
    {
        List<Vector2> concatenatedList = AddLists(list1, list2);
        List<Vector2> subtractedResult = SubLists(concatenatedList, list3);
        subtractedResult = SubLists(subtractedResult, list4);

        return subtractedResult;
    }
    /// <summary>
    /// ��List���
    /// </summary>
    public List<T> AddLists<T>(List<T> list1, List<T> list2)
    {
        List<T> concatenatedList = new List<T>(list1);
        concatenatedList.AddRange(list2);
        return concatenatedList;
    }
    /// <summary>
    /// ��List���
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

    //Clipper2 API ���
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

        return new PathsD() { Clipper.MakePath(doubles.ToArray()) };
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
    /// ȡ����
    /// </summary>
    public List<Vector2> GetUnion(List<Vector2> a, List<Vector2> b)
    {
        PathsD subj = ConvertVectorToPath(a);
        PathsD clip = ConvertVectorToPath(b);
        PathsD solution = Clipper.Union(subj, clip, FillRule.NonZero, 2);

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
