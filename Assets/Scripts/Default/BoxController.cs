using LibTessDotNet;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;
/// <summary>
/// Battlebox Master Control
/// </summary>
public class BoxController : ObjectPool
{
    public static BoxController instance;
    [Header("Line Width")]
    public float width = 0.15f;

    [Header("Generate box at start, empty name not generated")]
    public string startSummonName;
    public Vector3 startSummonPos;


    public List<BoxDrawer> boxes = new List<BoxDrawer>();

    public List<Vector2> pointsCrossSave, pointsOutCrossSave, pointsInCrossSave;
    //intersection/non-overlapping/overlapping points

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
                //Get the realPoints of the two Boxes.
                realPointsBack0 = box0.GetRealPoints();
                realPointsBack1 = box1.GetRealPoints();

                //Calculate the three major lists

                pointsCrossSave = FindIntersections(realPointsBack0, realPointsBack1);

                pointsOutCrossSave = ProcessPolygons(realPointsBack0, realPointsBack1, pointsCrossSave);

                pointsInCrossSave = AddAndSubLists(realPointsBack0, realPointsBack1, pointsCrossSave, pointsOutCrossSave);



                // When two special boxes overlap, merge them and leave the rest to the parent BoxDrawer.
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

                        //delete it first and add it back in the parent BoxDrawer.
                        boxes.Remove(box0);
                        boxes.Remove(box1);

                        /*
                        //Mr. Sung
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
    /// Generate box
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
        // rotate each point first, then add the object's position
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

        meshFilter.mesh = GenerateMesh(polygon.ToArray());
        // Most core code: build the mesh!

        edgeCollider2D.SetPoints(AddLists(polygon, new List<Vector2>() { polygon[0] }));
        edgeCollider2D.edgeRadius = width / 2;

        return polygon;
    }
    /// <summary>
    /// Calculate coordinates to get RealPoints.
    /// </summary>
    public List<Vector2> GetRealPoints(List<Vector2> list, Quaternion rotation, Transform transform, bool isLocal = true)
    {
        Vector3 local = isLocal ? transform.localPosition : transform.position;

        List<Vector2> polygon = new List<Vector2>(list);
        // rotate each point first, then add the object's position
        for (int i = 0; i < polygon.Count; i++)
        {
            polygon[i] = rotation * polygon[i] + local;
        }

        polygon = RemoveDuplicates(polygon);

        return polygon;
    }
    /*
    /// <summary>
    /// Reset box
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
    /// Constructing a Mesh
    /// </summary>
    public Mesh GenerateMesh(Vector2[] vertexPoints)
    {

        // Convert the Vector array to the ContourVertex array needed for LibTessDotNet.
        ContourVertex[] contourVertices = new ContourVertex[vertexPoints.Length];
        for (int i = 0; i < vertexPoints.Length; i++)
        {
            contourVertices[i].Position = new Vec3 { X = vertexPoints[i].x, Y = vertexPoints[i].y, Z = 0 };
        }

        // Create a Tess object and add an outline.
        Tess tess = new Tess();
        tess.AddContour(contourVertices, ContourOrientation.Original);

        // Triangulate
        tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

        // Create Mesh Object
        Mesh mesh = new Mesh();

        // Convert Tess results to Unity Mesh format.
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

        // Apply vertices and triangles to the mesh.
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Set the UV coordinates for the mesh.
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            // Here's a simple mapping that maps vertex coordinates to UV space
            // Often, you need to adapt this part of the code to the specific situation.


            uvs[i] = new Vector2(vertices[i].x, vertices[i].y);


        }
        mesh.uv = uvs;

        // Calculate normals and boundaries for better rendering results
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Apply the mesh to the GameObject.
        return mesh;
    }
    /// <summary>
    /// Eliminate duplicates
    /// </summary>
    public List<Vector2> RemoveDuplicates(List<Vector2> originalList)
    {
        // Use HashSet<Vector2> to store the Vector2 elements that have been encountered, because HashSet automatically de-duplicates them.
        HashSet<Vector2> seen = new HashSet<Vector2>();
        // Used to store the de-duplicated list.
        List<Vector2> resultList = new List<Vector2>();

        foreach (var item in originalList)
        {
            // If the addition to the HashSet was successful (i.e., the element has not been encountered before), add it to the result list
            if (seen.Add(item))
            {
                resultList.Add(item);
            }
        }

        return resultList;
    }




    /// <summary>
    /// Main function, calculates all intersections of two sets of lines.
    /// </summary>
    public List<Vector2> FindIntersections(List<Vector2> poly1, List<Vector2> poly2)
    {
        List<Vector2> intersections = new List<Vector2>();

        for (int i = 0; i < poly1.Count; i++)
        {
            Vector2 a = poly1[i];
            Vector2 b = poly1[(i + 1) % poly1.Count];
            // Loop List

            for (int j = 0; j < poly2.Count; j++)
            {
                Vector2 c = poly2[j];
                Vector2 d = poly2[(j + 1) % poly2.Count];
                // Loop List

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
    /// Compute vector cross product
    /// </summary>
    private static float CrossSave(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    /// <summary>
    /// Check if point C is on line AB.
    /// </summary>
    private static bool IsPointOnLineSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return CrossSave(a, b, c) == 0 && (c.x - a.x) * (c.x - b.x) <= 0 && (c.y - a.y) * (c.y - b.y) <= 0;
    }

    /// <summary>
    /// Check if lines AB and CD intersect.
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
    /// Calculate the intersection of two lines AB and CD.
    /// </summary>
    public static Vector2? CalculateIntersectionPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (!DoLineSegmentsIntersect(a, b, c, d))
            return null;

        // Calculating the parameters of a linear equation
        float denominator = (b.x - a.x) * (d.y - c.y) - (b.y - a.y) * (d.x - c.x);
        if (denominator == 0)
            return null;
            // Line segments parallel or co-linear

        float u = ((c.x - a.x) * (d.y - c.y) - (c.y - a.y) * (d.x - c.x)) / denominator;
        return new Vector2(a.x + u * (b.x - a.x), a.y + u * (b.y - a.y));
    }
    /// <summary>
    /// Calculate non-recombining points
    /// </summary>
    public List<Vector2> ProcessPolygons(List<Vector2> box1, List<Vector2> box2, List<Vector2> intersection)
    {
        List<Vector2> filteredBox1 = RemovePointsInsideOtherPolygon(box1, box2);
        List<Vector2> filteredBox2 = RemovePointsInsideOtherPolygon(box2, box1);

        // Merged and culled list
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
/// Sort the list of points by rotating clockwise with initialPoint as the center and a number of lengths as the radius.
/// </summary>
public List<Vector2> SortPoints(Vector2 initialPoint, List<Vector2> points)
{

    return points.OrderBy(p => Mathf.Atan2(initialPoint.y - p.y, initialPoint.x - p.x))
                  .ThenBy(p => (p - initialPoint).sqrMagnitude)
                  .ToList();
}
*/
    /// <summary>
    //// Add the first two, subtract the last two
    /// </summary>
    public List<Vector2> AddAndSubLists(List<Vector2> list1, List<Vector2> list2, List<Vector2> list3, List<Vector2> list4)
    {
        List<Vector2> concatenatedList = AddLists(list1, list2);
        List<Vector2> subtractedResult = SubLists(concatenatedList, list3);
        subtractedResult = SubLists(subtractedResult, list4);

        return subtractedResult;
    }
    /// <summary>
    /// Add the lists.
    /// </summary>
    public List<T> AddLists<T>(List<T> list1, List<T> list2)
    {
        List<T> concatenatedList = new List<T>(list1);
        concatenatedList.AddRange(list2);
        return concatenatedList;
    }
    /// <summary>
    /// Subtract the List.
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

    //Clipper2 API-related
    public PathsD ConvertVectorToPath(List<Vector2> vector)
    {
        List<double> doubles = new List<double>();
        int j = 0;
        for (int i = 0; i < vector.Count * 2; i++)
        {
            if (i % 2 == 0)
            //X
            {
                doubles.Add(vector[j].x);
            }
            else
            //Y
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
    /// Take the intersection
    /// </summary>
    public List<Vector2> GetUnion(List<Vector2> a, List<Vector2> b)
    {
        PathsD subj = ConvertVectorToPath(a);
        PathsD clip = ConvertVectorToPath(b);
        PathsD solution = Clipper.Union(subj, clip, FillRule.NonZero, 2);

        return ConvertPathToVector(solution);
    }
    public List<Vector2> GetDifference(List<Vector2> origin, List<Vector2> sub)
    {
        PathsD subj = ConvertVectorToPath(origin);
        PathsD clip = ConvertVectorToPath(sub);
        PathsD solution = Clipper.Difference(subj, clip, FillRule.NonZero, 2);
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
