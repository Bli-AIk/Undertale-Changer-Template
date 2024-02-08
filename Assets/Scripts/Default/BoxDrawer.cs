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
/// <summary>
/// 战斗框绘制
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(LineRenderer))]

public class BoxDrawer : MonoBehaviour
{
    public Vector3 localPosition;
    [Header("别用Transform的旋转")]
    public Quaternion rotation; // 获取当前物体的旋转
    [Header("线宽")]
    public float width = 0.15f;
    public List<Vector2> vertexPoints;

    [Header("是否启用贝塞尔插值")]
    public bool isBessel;
    public List<Vector2> besselPoints;
    public int besselPointsNum = 16;
    [Header("真正组框所用的点")]
    public List<Vector2> realPoints;//真正的曲线插值，插入点数由besselPointsNum决定
    public int besselInsertNum = 2;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public LineRenderer lineRenderer;


    [Header("当该Box为父级时，以此存储子级的相关计算后数据")]
    public BoxController.BoxType boxType;

    [Header("子级realPoints之和")]
    public List<Vector2> pointsSonSum;

    [Header("交点")]
    public List<Vector2> pointsCross;
    [Header("非重合点")]
    public List<Vector2> pointsOutCross;
    [Header("重合点")]
    public List<Vector2> pointsInCross;//交点/非重合点/重合点

    public List<BoxDrawer> sonBoxDrawer;//存储孩子们

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
        //BoxController.instance.boxes.Add(this);


        lineRenderer = transform.GetComponent<LineRenderer>();

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = Resources.Load<Material>("Materials/BoxLine");
        lineRenderer.loop = true;

        meshFilter = transform.GetComponent<MeshFilter>();

        meshRenderer = transform.GetComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/BoxBack");
    }


    public void Update()
    {
        if (sonBoxDrawer.Count == 0)//作为子级
        {
            if (isBessel)
                realPoints = GenerateBezierCurve(besselPoints, besselInsertNum, besselPointsNum);
            else
                realPoints = vertexPoints;

        }
        else if (sonBoxDrawer.Count == 2)//作为父级
        {
            pointsSonSum.Clear();

            List<Vector2> realPointsBack0 = BoxController.instance.GetRealPoints(sonBoxDrawer[0].realPoints, sonBoxDrawer[0].rotation, sonBoxDrawer[0].transform);
            List<Vector2> realPointsBack1 = BoxController.instance.GetRealPoints(sonBoxDrawer[1].realPoints, sonBoxDrawer[1].rotation, sonBoxDrawer[1].transform);

            pointsSonSum = BoxController.instance.AddLists(realPointsBack0, realPointsBack1);


            //计算三大List

            pointsCross = BoxController.instance.FindIntersections(realPointsBack0, realPointsBack1);

            pointsOutCross = BoxController.instance.ProcessPolygons(realPointsBack0, realPointsBack1, pointsCross);

            pointsInCross = BoxController.instance.AddAndSubLists(realPointsBack0, realPointsBack1, pointsCross, pointsOutCross);


            //重合时合并
            if (!(pointsCross.Count == 0 && pointsInCross.Count == 0))
            {
                List<Vector2> points;

                points = BoxController.instance.AddLists(pointsCross, pointsSonSum);
                points = BoxController.instance.SubLists(points, pointsInCross);
                List<Vector2> pointsFinal = BoxController.instance.SortPoints(BoxController.instance.CalculatePolygonCenter(BoxController.instance.AddLists(pointsCross, pointsInCross)), points);

                realPoints = pointsFinal;
            }
            else//不重合就解散
            {
                ExitParent();
                return;
            }



        }
        else ExitParent();



        transform.localPosition = localPosition;
        if (transform.parent == BoxController.instance.transform)//只有父物体为BoxController时生成框
            SummonBox();
        else transform.localPosition = localPosition + transform.parent.localPosition * -1;
    }

    void ExitParent()
    {
        ClearComponentsData();
        BoxController.instance.ReturnPool(gameObject);
        BoxController.instance.boxes.Remove(this);



        pointsCross.Clear();
        pointsInCross.Clear();
        pointsOutCross.Clear();


        sonBoxDrawer[0].transform.SetParent(BoxController.instance.transform);
        sonBoxDrawer[1].transform.SetParent(BoxController.instance.transform);
        sonBoxDrawer[0].IsOpenComponentsData(true);
        sonBoxDrawer[1].IsOpenComponentsData(true);

        BoxController.instance.boxes.Add(sonBoxDrawer[0]);
        BoxController.instance.boxes.Add(sonBoxDrawer[1]);

        sonBoxDrawer[0].SummonBox();
        sonBoxDrawer[1].SummonBox();

        sonBoxDrawer.Clear();
        transform.SetParent(BoxController.instance.transform);
    }

    /// <summary>
    /// 通过BoxController生成框
    /// </summary>
    public List<Vector2> SummonBox()
    {
        return BoxController.instance.SummonBox(realPoints, rotation, transform, width, lineRenderer, meshFilter);

    }
    public List<Vector2> GetRealPoints() 
    {
        return BoxController.instance.GetRealPoints(realPoints, rotation, transform);
    }
    
    /// <summary>
    /// 开关组件
    /// </summary>
    public void IsOpenComponentsData(bool isOpen = false)
    {
        //meshFilter.mesh = null;
        //lineRenderer.positionCount = 0;
        meshRenderer.enabled = isOpen;
        lineRenderer.enabled = isOpen;

    }
    public void ClearComponentsData(bool onlyMesh = false)
    {
        meshFilter.mesh = null;
        lineRenderer.positionCount = 0;

    }

    /// <summary>
    /// 获取组件
    /// </summary>
    /// 
    public void GetComponents(bool forceBesselFlash = false)
    {
        if (!forceBesselFlash)
        {
            if (!isBessel)
                besselPoints.Clear();
            else if (besselPoints.Count == 0 || besselPoints.Count != vertexPoints.Count * (besselInsertNum + 1))
                besselPoints = InterpolatePoints(vertexPoints, besselInsertNum);
        }
        else
        {
            besselPoints.Clear();
            if (isBessel)
                besselPoints = InterpolatePoints(vertexPoints, besselInsertNum);
        }

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

    /// <summary>
    /// 生成贝塞尔曲线上的点
    /// </summary>
    public static List<Vector2> GenerateBezierCurve(List<Vector2> points, int besselInsertNum, int numPoints)
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

        // 遍历控制点列表，每次取出besselInsertNum + 1个点生成贝塞尔曲线段
        List<Vector2> pointList = new List<Vector2>();
        for (int i = 0; i < controlPoints.Count - besselInsertNum; i += besselInsertNum + 1)
        {
            for (int k = 0; k < besselInsertNum + 2; k++)
            {
                pointList.Add(controlPoints[i + k]);
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

    /// <summary>
    /// 计算组合数 C(n, k)
    /// </summary>
    private static float BinomialCoefficient(int n, int k)
    {
        float result = 1;

        for (int i = 1; i <= k; i++)
        {
            result *= (n - i + 1) / (float)i;
        }

        return result;
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
        if (vertexPoints == null)
            return;
        /*
        if (meshFilter != null && showMesh && showGizmosPoint != ShowGizmosPoint.Nope)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(meshFilter.sharedMesh, 0, transform.position);
        }
        */
        if (showGizmosPoint == ShowGizmosPoint.All && isBessel)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in realPoints)
            {
                Gizmos.DrawSphere(transform.TransformPoint(rotation * (new Vector3(point.x, point.y, 0))), 0.1f / 2);
            }
        }

        if (isBessel)
        {

            for (int i = 0; i < besselPoints.Count; i++)
            {
                var point = besselPoints[i];
                if (i % (besselInsertNum + 1) != 0)
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

                Gizmos.DrawSphere(transform.TransformPoint(rotation * new Vector3(point.x, point.y, 0)), 0.1f);
            }
            return;
        }

        if (showGizmosPoint == ShowGizmosPoint.Nope)
            return;

        Gizmos.color = Color.white;
        foreach (var point in vertexPoints)
        {
            Gizmos.DrawSphere(transform.TransformPoint(rotation * new Vector3(point.x, point.y, 0)), 0.1f);
        }



    }
#endif

}

#if UNITY_EDITOR


[CustomEditor(typeof(BoxDrawer))]
public class SceneExtEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BoxDrawer example = (BoxDrawer)target;

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
            example.vertexPoints = new List<Vector2>
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
            example.vertexPoints = new List<Vector2>
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
            example.vertexPoints.Clear();
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
                example.vertexPoints.Add(new Vector2(x, y));
            }
            example.GetComponents(true);
            example.Update();
        }

        /*
        if (GUILayout.Button("R A N D O M"))
        {
            example.vertexPoints.Clear();
            for (int i = 0; i < Random.Range(3,100); i++)
            {
                example.vertexPoints.Add(new Vector2(Random.Range(-5, 5f), Random.Range(-5, 5f)));
            }
            example.Update();
        }
        */
    }


    bool isUndoRedoPerformed = false;
    private void OnSceneGUI()
    {
        BoxDrawer example = (BoxDrawer)target;

        List<Vector2> vertices;
        if (example.isBessel && example.besselPoints.Count > 0)
            vertices = example.besselPoints;
        else
            vertices = example.vertexPoints;


        for (int i = 0; i < vertices.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newvertexPoints = Quaternion.Inverse(example.rotation) * (Handles.PositionHandle(example.transform.position + example.rotation * vertices[i], example.rotation) - example.transform.position);

            if (EditorGUI.EndChangeCheck())
            {
                example.GetComponents();
                Undo.RecordObject(example, "Changed point " + i);
                vertices[i] = newvertexPoints;
                if (example.isBessel)
                    if (i % (example.besselInsertNum + 1) == 0)
                        example.vertexPoints[i / (example.besselInsertNum + 1)] = newvertexPoints;
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
