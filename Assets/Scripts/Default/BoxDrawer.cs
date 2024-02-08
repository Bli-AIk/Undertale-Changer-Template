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
    public Vector3 localPosition;
    [Header("����Transform����ת")]
    public Quaternion rotation; // ��ȡ��ǰ�������ת
    [Header("�߿�")]
    public float width = 0.15f;
    public List<Vector2> vertexPoints;

    [Header("�Ƿ����ñ�������ֵ")]
    public bool isBessel;
    public List<Vector2> besselPoints;
    public int besselPointsNum = 16;
    [Header("����������õĵ�")]
    public List<Vector2> realPoints;//���������߲�ֵ�����������besselPointsNum����
    public int besselInsertNum = 2;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public LineRenderer lineRenderer;


    [Header("����BoxΪ����ʱ���Դ˴洢�Ӽ�����ؼ��������")]
    public BoxController.BoxType boxType;

    [Header("�Ӽ�realPoints֮��")]
    public List<Vector2> pointsSonSum;

    [Header("����")]
    public List<Vector2> pointsCross;
    [Header("���غϵ�")]
    public List<Vector2> pointsOutCross;
    [Header("�غϵ�")]
    public List<Vector2> pointsInCross;//����/���غϵ�/�غϵ�

    public List<BoxDrawer> sonBoxDrawer;//�洢������

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
        if (sonBoxDrawer.Count == 0)//��Ϊ�Ӽ�
        {
            if (isBessel)
                realPoints = GenerateBezierCurve(besselPoints, besselInsertNum, besselPointsNum);
            else
                realPoints = vertexPoints;

        }
        else if (sonBoxDrawer.Count == 2)//��Ϊ����
        {
            pointsSonSum.Clear();

            List<Vector2> realPointsBack0 = BoxController.instance.GetRealPoints(sonBoxDrawer[0].realPoints, sonBoxDrawer[0].rotation, sonBoxDrawer[0].transform);
            List<Vector2> realPointsBack1 = BoxController.instance.GetRealPoints(sonBoxDrawer[1].realPoints, sonBoxDrawer[1].rotation, sonBoxDrawer[1].transform);

            pointsSonSum = BoxController.instance.AddLists(realPointsBack0, realPointsBack1);


            //��������List

            pointsCross = BoxController.instance.FindIntersections(realPointsBack0, realPointsBack1);

            pointsOutCross = BoxController.instance.ProcessPolygons(realPointsBack0, realPointsBack1, pointsCross);

            pointsInCross = BoxController.instance.AddAndSubLists(realPointsBack0, realPointsBack1, pointsCross, pointsOutCross);


            //�غ�ʱ�ϲ�
            if (!(pointsCross.Count == 0 && pointsInCross.Count == 0))
            {
                List<Vector2> points;

                points = BoxController.instance.AddLists(pointsCross, pointsSonSum);
                points = BoxController.instance.SubLists(points, pointsInCross);
                List<Vector2> pointsFinal = BoxController.instance.SortPoints(BoxController.instance.CalculatePolygonCenter(BoxController.instance.AddLists(pointsCross, pointsInCross)), points);

                realPoints = pointsFinal;
            }
            else//���غϾͽ�ɢ
            {
                ExitParent();
                return;
            }



        }
        else ExitParent();



        transform.localPosition = localPosition;
        if (transform.parent == BoxController.instance.transform)//ֻ�и�����ΪBoxControllerʱ���ɿ�
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
    /// ͨ��BoxController���ɿ�
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
    /// �������
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
    /// ��ȡ���
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
    public static List<Vector2> GenerateBezierCurve(List<Vector2> points, int besselInsertNum, int numPoints)
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

        // �������Ƶ��б�ÿ��ȡ��besselInsertNum + 1�������ɱ��������߶�
        List<Vector2> pointList = new List<Vector2>();
        for (int i = 0; i < controlPoints.Count - besselInsertNum; i += besselInsertNum + 1)
        {
            for (int k = 0; k < besselInsertNum + 2; k++)
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

        if (GUILayout.Button("��������ս����"))
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
        if (GUILayout.Button("�����������"))
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
