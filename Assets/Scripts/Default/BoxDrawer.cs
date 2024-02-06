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
/// ս�������
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(LineRenderer))]

public class BoxDrawer : MonoBehaviour
{
    [Header("����Transform����ת")]
    public Quaternion rotation; // ��ȡ��ǰ�������ת
    [Header("�߿�")]
    public float width = 0.15f;
    public List<Vector2> polygonVertices;

    [Header("�Ƿ����ñ�������ֵ")]
    public bool isBesselInterpolation;
    public List<Vector2> besselVertices;
    public int besselVerticesPointNum = 16;
    public List<Vector2> realPoints;//���������߲�ֵ�����������besselVerticesPointNum����
    public int besselNum = 2;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    LineRenderer lineRenderer;

#if UNITY_EDITOR
   [Header("��Editor�õ�")]
    public int regularEdge;
    public float regularAngle;
    [Header("�Ƿ�չʾMesh�����ߣ�")]
    public bool showMesh;
#endif

    void Start()
    {
        GetComponents();
        BoxController.instance.boxes.Add(this);


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


        if (isBesselInterpolation)
            realPoints = GenerateBezierCurve(besselVertices, besselNum, besselVerticesPointNum);
        else
            realPoints = polygonVertices;



    }
    /// <summary>
    /// ͨ��BoxController���ɿ�
    /// </summary>
    public void SummonBox(bool justVector = false)
    {
        if (!justVector)
            realPoints = BoxController.instance.SummonBox(realPoints, rotation, transform, width, lineRenderer, meshFilter);
        else
            realPoints = BoxController.instance.SummonBox(realPoints, rotation, transform);
    }

    /// <summary>
    /// ��ȡ���
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

    }
    /// <summary>
    /// ��ֵ����
    /// </summary>
    /// <param name="points">ԭʼList</param>
    /// <param name="interpolation">ƽ�ֵ���</param>
    /// <returns></returns>
    List<Vector2> InterpolatePoints(List<Vector2> points, int interpolation)
    {
        List<Vector2> interpolatedPoints = new List<Vector2>();

        if (points.Count < 2)
        {
            Debug.LogWarning("��ֵ��Ҫ���������㡣");
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

        // ������β֮��Ĳ�ֵ
        for (int j = 1; j <= interpolation; j++)
        {
            float t = (float)j / (float)(interpolation + 1);
            Vector2 interpolatedPoint = Vector2.Lerp(points[points.Count - 1], points[0], t);
            interpolatedPoints.Add(interpolatedPoint);
        }
        return interpolatedPoints;
    }

    /// <summary>
    /// ���ɱ����������ϵĵ�
    /// </summary>
    public static List<Vector2> GenerateBezierCurve(List<Vector2> points, int besselNum, int numPoints)
    {
        List<Vector2> controlPoints = new List<Vector2>(points);

        controlPoints.Add(controlPoints[0]);
        List<Vector2> bezierPoints = new List<Vector2>(); // ����һ��Vector2�б����ڴ洢���ɵı����������ϵĵ�

        // �����Ƶ��������������Ҫ4�����Ƶ�����γ�һ����������������
        if (controlPoints.Count < 4)
        {
            DebugLogger.Log("������Ҫ4�����Ƶ�����γ��������������ߡ�", DebugLogger.Type.err); // �ڿ���̨��ʾ������Ϣ
            return bezierPoints; // ���ؿյı��������б�
        }

        // �������Ƶ��б�ÿ��ȡ��besselNum + 1�������ɱ��������߶�
        List<Vector2> pointList = new List<Vector2>();
        for (int i = 0; i < controlPoints.Count - besselNum; i += besselNum + 1)
        {
            for (int k = 0; k < besselNum + 2; k++)
            {
                pointList.Add(controlPoints[i + k]);
            }
            // ���������������ڵ�ǰ���߶������ɵ�
            for (int j = 0; j <= numPoints; j++)
            {
                float t = j / (float)numPoints; // �������t��ֵ�����ڲ�ֵ
                Vector2 point = CalculateNthDegreeBezierPoint(pointList, t); // ���ü��㱴������ĺ���
                bezierPoints.Add(point); // ������õ��ĵ���ӵ����������б���
            }
            pointList.Clear();
        }

        return bezierPoints; // �������ɵı��������б�
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
    /// ��������� C(n, k)
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

    [Header("չʾ��Щ�������")]
    public ShowGizmosPoint showGizmosPoint;

    public void OnDrawGizmos()
    {
        if (polygonVertices == null)
            return;
        /*
        if (meshFilter != null && showMesh && showGizmosPoint != ShowGizmosPoint.Nope)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(meshFilter.sharedMesh, 0, transform.position);
        }
        */
        if (showGizmosPoint == ShowGizmosPoint.All && isBesselInterpolation)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in realPoints)
            {
                Gizmos.DrawSphere(transform.TransformPoint(rotation * (new Vector3(point.x, point.y, 0) - transform.position)), 0.1f / 2);
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

                Gizmos.DrawSphere(transform.TransformPoint(rotation * new Vector3(point.x, point.y, 0)), 0.1f);
            }
            return;
        }

        if (showGizmosPoint == ShowGizmosPoint.Nope)
            return;

        Gizmos.color = Color.white;
        foreach (var point in polygonVertices)
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

        base.OnInspectorGUI(); //����һ��GUI��
        if (GUILayout.Button("�з�(��ǿ��ˢ��)"))
        {
            example.GetComponents(false);
            example.Update();
        }
        if (GUILayout.Button("�з�(ǿ��ˢ��)"))
        {
            example.GetComponents(true);
            example.Update();
        }

        if (GUILayout.Button("���ɱ�׼ս����"))
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

        if (GUILayout.Button("��������ս����"))
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
        if (GUILayout.Button("�����������"))
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
        BoxDrawer example = (BoxDrawer)target;

        List<Vector2> vertices;
        if (example.isBesselInterpolation && example.besselVertices.Count > 0)
            vertices = example.besselVertices;
        else
            vertices = example.polygonVertices;


        for (int i = 0; i < vertices.Count; i++)
        {
            EditorGUI.BeginChangeCheck();



            Vector3 newPolygonVertices = Quaternion.Inverse(example.rotation) * (Handles.PositionHandle(example.transform.position + example.rotation * vertices[i], example.rotation) - example.transform.position);

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
