using UnityEngine;
using LibTessDotNet;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Log;
using Color = UnityEngine.Color;
using UnityEditor.U2D.Path;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class MeshGenerator : MonoBehaviour
{
    [Header("线宽")]
    public float width;
    public List<Vector2> polygonVertices;

    [Header("是否启用贝塞尔插值")]
    public bool isBesselInterpolation;
    public List<Vector2> besselVertices;
    public int besselVerticesPointNum = 16;
    public List<Vector2> realPoints;//真正的曲线插值，插入点数由besselVerticesPointNum决定
    public int besselNum = 2;

    MeshFilter meshFilter;
    LineRenderer lineRenderer;


#if UNITY_EDITOR
    [Header("给Editor用的")]
    public int regularEdge;
    public float regularAngle;
    [Header("是否展示Mesh（红线）")]
    public bool showMesh;
#endif

    void Start()
    {
        GetComponents();

    }
    /// <summary>
    /// 获取meshFilter
    /// </summary>
    /// 
    public void GetComponents(bool forceBesselFlash = false)
    {
        if (!forceBesselFlash)
        {
            if (!isBesselInterpolation)
                besselVertices.Clear();
            else if (besselVertices.Count == 0 || besselVertices.Count != polygonVertices.Count * (besselNum + 1))
                besselVertices = InterpolatePoints(polygonVertices, besselNum);
        }
        else
        {
            besselVertices.Clear();
            if (isBesselInterpolation)
                besselVertices = InterpolatePoints(polygonVertices, besselNum);
        }

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }
    /// <summary>
    /// 插值函数
    /// </summary>
    /// <param name="points">原始List</param>
    /// <param name="interpolation">平分点数</param>
    /// <returns></returns>
    List<Vector2> InterpolatePoints(List<Vector2> points, int interpolation)
    {
        List<Vector2> interpolatedPoints = new List<Vector2>();

        if (points.Count < 2)
        {
            Debug.LogWarning("插值需要至少两个点。");
            return interpolatedPoints;
        }

        for (int i = 0; i < points.Count; i++)
        {
            interpolatedPoints.Add(points[i]);
            if (i == points.Count - 1)
                break;
            for (int j = 1; j <= interpolation; j++)
            {
                float t = (float)j / (float)(interpolation + 1);
                Vector2 interpolatedPoint = Vector2.Lerp(points[i], points[i + 1], t);
                interpolatedPoints.Add(interpolatedPoint);
            }
        }

        // 插入首尾之间的插值
        for (int j = 1; j <= interpolation; j++)
        {
            float t = (float)j / (float)(interpolation + 1);
            Vector2 interpolatedPoint = Vector2.Lerp(points[points.Count - 1], points[0], t);
            interpolatedPoints.Add(interpolatedPoint);
        }
        return interpolatedPoints;
    }

    // 生成贝塞尔曲线上的点
    public static List<Vector2> GenerateBezierCurve(List<Vector2> points, int besselNum, int numPoints)
    {
        List<Vector2> controlPoints = new List<Vector2>(points);

        controlPoints.Add(controlPoints[0]);
        List<Vector2> bezierPoints = new List<Vector2>(); // 创建一个Vector2列表用于存储生成的贝塞尔曲线上的点

        // 检查控制点的数量，至少需要4个控制点才能形成一个立方贝塞尔曲线
        if (controlPoints.Count < 4)
        {
            DebugLogger.Log("至少需要4个控制点才能形成立方贝塞尔曲线。", DebugLogger.Type.err); // 在控制台显示错误消息
            return bezierPoints; // 返回空的贝塞尔点列表
        }

        // 遍历控制点列表，每次取出besselNum + 1个点生成贝塞尔曲线段
        List<Vector2> pointList = new List<Vector2>();
        for (int i = 0; i < controlPoints.Count - besselNum; i += besselNum + 1)
        {
            for (int k = 0; k < besselNum + 2; k++)
            {
                pointList.Add(controlPoints[i + k]);
                Debug.Log(i + k);
            }
            // 根据所需点的数量在当前曲线段上生成点
            for (int j = 0; j <= numPoints; j++)
            {
                float t = j / (float)numPoints; // 计算参数t的值，用于插值
                Vector2 point = CalculateNthDegreeBezierPoint(pointList, t); // 调用计算贝塞尔点的函数
                bezierPoints.Add(point); // 将计算得到的点添加到贝塞尔点列表中
            }
            pointList.Clear();
        }

        return bezierPoints; // 返回生成的贝塞尔点列表
    }

    private static Vector2 CalculateNthDegreeBezierPoint(List<Vector2> controlPoints, float t)
    {
        int n = controlPoints.Count - 1;
        float u = 1 - t;
        Vector2 p = Vector2.zero;

        for (int i = 0; i <= n; i++)
        {
            float coefficient = BinomialCoefficient(n, i) * Mathf.Pow(u, n - i) * Mathf.Pow(t, i);
            p += coefficient * controlPoints[i];
        }

        return p;
    }

    // 计算组合数 C(n, k)
    private static float BinomialCoefficient(int n, int k)
    {
        float result = 1;

        for (int i = 1; i <= k; i++)
        {
            result *= (n - i + 1) / (float)i;
        }

        return result;
    }



    public void Update()
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;


        if (isBesselInterpolation)
            realPoints = GenerateBezierCurve(besselVertices, besselNum, besselVerticesPointNum);
        else
            realPoints = polygonVertices;

        SummonBox(realPoints);

        if (Input.GetKeyDown(KeyCode.F5))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
    public void SummonBox(List<Vector2> polygon)
    {
        GenerateMesh(polygon.ToArray());//最核心代码：构建Mesh！！

        lineRenderer.positionCount = polygon.Count;

        for (int i = 0; i < polygon.Count; i++)
        {
            lineRenderer.SetPosition(i, polygon[i] + (Vector2)transform.position);
        }


    }
    /// <summary>
    /// 构造Mesh
    /// </summary>
    public void GenerateMesh(Vector2[] polygonVertices)
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
        meshFilter.mesh = mesh;
    }


#if UNITY_EDITOR
    public enum ShowGizmosPoint
    {
        Nope,
        JustVertex,
        JustVertexBessel,
        All
    };

    [Header("展示哪些点的坐标")]
    public ShowGizmosPoint showGizmosPoint;

    public void OnDrawGizmos()
    {
        if (polygonVertices == null)
            return;
        if (meshFilter != null && showMesh && showGizmosPoint != ShowGizmosPoint.Nope)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(meshFilter.sharedMesh, 0, transform.position);
        }
        if (showGizmosPoint == ShowGizmosPoint.All)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in realPoints)
            {
                Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.1f);
            }
        }

        if (isBesselInterpolation)
        {

            for (int i = 0; i < besselVertices.Count; i++)
            {
                var point = besselVertices[i];
                if (i % (besselNum + 1) != 0)
                {
                    if (showGizmosPoint == ShowGizmosPoint.JustVertexBessel || showGizmosPoint == ShowGizmosPoint.All)
                        Gizmos.color = Color.cyan;
                    else
                        continue;

                }
                else
                {

                    if (showGizmosPoint != ShowGizmosPoint.Nope)
                        Gizmos.color = Color.white;
                    else
                        continue;

                }

                Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.1f);
            }
            return;
        }

        if (showGizmosPoint == ShowGizmosPoint.Nope)
            return;

        Gizmos.color = Color.white;
        foreach (var point in polygonVertices)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(point.x, point.y, 0)), 0.1f);
        }



    }
#endif

}

#if UNITY_EDITOR


[CustomEditor(typeof(MeshGenerator))]
public class SceneExtEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshGenerator example = (MeshGenerator)target;

        base.OnInspectorGUI(); //绘制一次GUI。
        if (GUILayout.Button("切分(不强制刷新)"))
        {
            example.GetComponents(false);
            example.Update();
        }
        if (GUILayout.Button("切分(强制刷新)"))
        {
            example.GetComponents(true);
            example.Update();
        }

        if (GUILayout.Button("生成标准战斗框"))
        {
            example.polygonVertices = new List<Vector2>
            {
                new Vector2(5.93f,1.4f),
                new Vector2(5.93f,-1.4f),
                new Vector2(-5.93f,-1.4f),
                new Vector2(-5.93f,1.4f),
            };
            example.GetComponents(true);
            example.Update();
        }

        if (GUILayout.Button("生成正方战斗框"))
        {
            example.polygonVertices = new List<Vector2>
            {
                new Vector2(1.4f,1.4f),
                new Vector2(1.4f,-1.4f),
                new Vector2(-1.4f,-1.4f),
                new Vector2(-1.4f,1.4f),
            };
            example.GetComponents(true);
            example.Update();
        }
        if (GUILayout.Button("生成正多边形"))
        {
            example.polygonVertices.Clear();
            int sides = 3;
            if (example.regularEdge >= 3)
                sides = example.regularEdge;
            else DebugLogger.Log("regularEdge should > 3", DebugLogger.Type.err, "#FF0000");
            float radius = 3;
            for (int i = sides - 1; i >= 0; i--)
            {
                float angle = (2 * Mathf.PI * i) / sides - example.regularAngle * Mathf.PI / 180;
                float x = radius * Mathf.Cos(angle);
                float y = radius * Mathf.Sin(angle);
                example.polygonVertices.Add(new Vector2(x, y));
            }
            example.GetComponents(true);
            example.Update();
        }

        /*
        if (GUILayout.Button("R A N D O M"))
        {
            example.polygonVertices.Clear();
            for (int i = 0; i < Random.Range(3,100); i++)
            {
                example.polygonVertices.Add(new Vector2(Random.Range(-5, 5f), Random.Range(-5, 5f)));
            }
            example.Update();
        }
        */
    }


    bool isUndoRedoPerformed = false;
    private void OnSceneGUI()
    {
        MeshGenerator example = (MeshGenerator)target;

        List<Vector2> vertices;
        if (example.isBesselInterpolation && example.besselVertices.Count > 0)
            vertices = example.besselVertices;
        else
            vertices = example.polygonVertices;


        for (int i = 0; i < vertices.Count; i++)
        {
            EditorGUI.BeginChangeCheck();



            Vector3 newPolygonVertices = Handles.PositionHandle((Vector2)example.transform.position + vertices[i], Quaternion.identity) - example.transform.position;

            if (EditorGUI.EndChangeCheck())
            {
                example.GetComponents();
                Undo.RecordObject(example, "Changed point " + i);
                vertices[i] = newPolygonVertices;

                if (i % (example.besselNum + 1) == 0)
                {
                    example.polygonVertices[i / (example.besselNum + 1)] = newPolygonVertices;
                }
                example.Update();
                if (!isUndoRedoPerformed)
                {
                    Undo.undoRedoPerformed += example.Update;
                    isUndoRedoPerformed = true;
                }
            }
        }


    }
}
#endif
