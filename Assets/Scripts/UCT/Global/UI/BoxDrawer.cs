using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UCT.Global.UI
{
    /// <summary>
    /// 战斗框绘制
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(EdgeCollider2D))]

    public class BoxDrawer : MonoBehaviour
    {
        /*
    public enum Test
    {
        n,a,b,c,d,e,
    }
    public Test test = Test.n;

    */
        public Vector3 localPosition;
        [Header("别用Transform的旋转")]
        public Quaternion rotation; // 获取当前物体的旋转
        [Header("线宽")]
        public float width = 0.15f;
        public List<Vector2> vertexPoints;

        [Header("是否启用贝塞尔插值")]
        public bool isBessel;
        public List<Vector2> besselPoints;
        public int besselPointsNumber = 16;
        [Header("真正组框所用的点")]
        public List<Vector2> realPoints;//真正的曲线插值，插入点数由besselPointsNumber决定
        public int besselInsertNumber = 2;

        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public LineRenderer lineRenderer;
        public EdgeCollider2D edgeCollider2D;


        [Header("设置其是否为特殊框")]
        public BoxController.BoxType boxType;

        [Header("当该Box为父级时，以此存储子级的相关计算后数据")]
        [Header("子级realPoints之和")]
        public List<Vector2> pointsSonSum;

        [Header("交点")]
        public List<Vector2> pointsCross;
        [Header("非重合点")]
        public List<Vector2> pointsOutCross;
        [Header("重合点")]
        public List<Vector2> pointsInCross;//交点/非重合点/重合点

        public BoxDrawer parent;//此框的复合父级
        public List<BoxDrawer> sonBoxDrawer;//此框的子级

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


            lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.material = Resources.Load<Material>("Materials/BoxLine");
            lineRenderer.loop = true;

            edgeCollider2D = GetComponent<EdgeCollider2D>();
            edgeCollider2D.isTrigger = true;

            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = Resources.Load<Material>("Materials/BoxBack");

        }

        //float testTimer;
        public void Update()
        {
            if (parent != null)
                transform.tag = "Untagged";
            else
                transform.tag = "Box";

            if (boxType == BoxController.BoxType.Sub)
                ClearComponentsData();
            /*
        switch (test)
        {
            case Test.a:
                localPosition = new Vector3(Mathf.Cos(Time.time * 6) * 5, Mathf.Sin(Time.time * 6) * 5);
                break;
            case Test.b:
                rotation = Quaternion.Euler(0, 0, Mathf.Cos(Time.time * 10) * 180);
                break;
            case Test.c:
                testTimer += Time.deltaTime;
                if (testTimer > 1)
                {
                    localPosition = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5));
                    testTimer = 0;
                }
                break;
            case Test.d:
                for (int i = 0; i < vertexPoints.Count; i++)
                {
                    vertexPoints[i] = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
                }
                break;
            case Test.e:
                localPosition = new Vector3(0, Mathf.Tan(Time.time));
                break;
            default:
                break;
        }
        foreach (var item in sonBoxDrawer)
        {
            if (!item.gameObject.activeSelf)
            {
                ExitParent();
            }

        }
        */

            if (sonBoxDrawer.Count == 0 && transform.childCount == 0)//作为纯子级
            {

                if (isBessel)
                    realPoints = GenerateBezierCurve(besselPoints, besselInsertNumber, besselPointsNumber);
                else
                    realPoints = vertexPoints;

            }
            else if (sonBoxDrawer.Count == 2 && transform.childCount == 2)//作为父级
            {

                pointsSonSum.Clear();

                //更新一下两个子级的位置坐标
                sonBoxDrawer[0].transform.localPosition = sonBoxDrawer[0].localPosition - localPosition;
                sonBoxDrawer[1].transform.localPosition = sonBoxDrawer[1].localPosition - localPosition;

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
                    /*
                List<Vector2> points;

                points = BoxController.instance.AddLists(pointsCross, pointsSonSum);
                points = BoxController.instance.SubLists(points, pointsInCross);
                List<Vector2> pointsFinal = BoxController.instance.SortPoints(BoxController.instance.CalculatePolygonCenter(BoxController.instance.AddLists(pointsCross, pointsInCross)), points);
                */
                    List<Vector2> pointsFinal;

                    if (sonBoxDrawer[0].boxType == BoxController.BoxType.Add && sonBoxDrawer[1].boxType == BoxController.BoxType.Sub)
                        pointsFinal = BoxController.instance.GetDifference(realPointsBack0, realPointsBack1);
                    else if (sonBoxDrawer[0].boxType == BoxController.BoxType.Sub && sonBoxDrawer[1].boxType == BoxController.BoxType.Add)
                        pointsFinal = BoxController.instance.GetDifference(realPointsBack1, realPointsBack0);
                    else
                        pointsFinal = BoxController.instance.GetUnion(realPointsBack0, realPointsBack1);

                    realPoints = pointsFinal;
                }
                else//不重合就解散
                {
                    ExitParent();
                    return;
                }



            }
            else ExitParent();



            if (transform.parent == BoxController.instance.transform)//只有父物体为BoxController时生成框
            {
                transform.localPosition = localPosition;

                if (boxType != BoxController.BoxType.Sub)//减框不绘制
                    SummonBox();
            }
            else transform.localPosition = localPosition - parent.localPosition;
        }

        void ExitParent()//离开的那个 的爹 会触发这个
        {
            //Debug.Log(transform.childCount);

            ClearComponentsData();

            BoxController.instance.ReturnPool(gameObject);
            BoxController.instance.boxes.Remove(this);
            if (sonBoxDrawer.Count != 0)
            {
                pointsCross.Clear();
                pointsInCross.Clear();
                pointsOutCross.Clear();

                for (int i = 0; i < 2; i++)
                {
                    sonBoxDrawer[i].transform.SetParent(BoxController.instance.transform);
                    sonBoxDrawer[i].parent = null;
                    sonBoxDrawer[i].localPosition = (Vector3)(Vector2)(sonBoxDrawer[i].localPosition + localPosition) + new Vector3(0, 0, sonBoxDrawer[i].localPosition.z);
                    sonBoxDrawer[i].transform.localPosition = sonBoxDrawer[i].localPosition;
                    sonBoxDrawer[i].rotation = AddQuaternions(rotation, sonBoxDrawer[i].rotation);
                    sonBoxDrawer[i].IsOpenComponentsData(true);
                    BoxController.instance.boxes.Add(sonBoxDrawer[i]);
                    sonBoxDrawer[i].SummonBox();

                    if (sonBoxDrawer[i].boxType == BoxController.BoxType.Sub)
                        sonBoxDrawer[i].ClearComponentsData();
                }


                sonBoxDrawer.Clear();


            }

            localPosition = Vector3.zero;
            rotation = Quaternion.identity;

            if (parent != null)
                parent.ExitParent();
            //SubListsWhenExitParent(GetRealPoints());
            if (BoxController.instance.boxes.Find(x => x == this))
                BoxController.instance.boxes.Remove(this);
            transform.SetParent(BoxController.instance.transform);
            parent = null;
        }

        // 函数用于将两个四元数相加
        public Quaternion AddQuaternions(Quaternion quat1, Quaternion quat2)
        {
            // 将两个四元数转换为欧拉角，并相加
            Vector3 euler1 = quat1.eulerAngles;
            Vector3 euler2 = quat2.eulerAngles;
            Vector3 summedEulerAngles = euler1 + euler2;

            // 将相加后的欧拉角转换为四元数
            return Quaternion.Euler(summedEulerAngles);
        }

        /*
    public void SubListsWhenExitParent(List<Vector2> subList)
    {
        parent.realPoints = BoxController.instance.SubLists(parent.realPoints, subList);
        if (parent.parent != null)
            parent.SubListsWhenExitParent(subList);
    }
    */
        /// <summary>
        /// 通过BoxController生成框
        /// </summary>
        public List<Vector2> SummonBox()
        {
            return BoxController.instance.SummonBox(realPoints, rotation, transform, width, lineRenderer, edgeCollider2D, meshFilter);

        }
        public List<Vector2> GetRealPoints(bool isLocal = true) 
        {
            return BoxController.instance.GetRealPoints(realPoints, rotation, transform, isLocal);
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
        public void ClearComponentsData()
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
                else if (besselPoints.Count == 0 || besselPoints.Count != vertexPoints.Count * (besselInsertNumber + 1))
                    besselPoints = InterpolatePoints(vertexPoints, besselInsertNumber);
            }
            else
            {
                besselPoints.Clear();
                if (isBessel)
                    besselPoints = InterpolatePoints(vertexPoints, besselInsertNumber);
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
                Other.Debug.LogWarning("插值需要至少两个点。");
                return interpolatedPoints;
            }

            for (int i = 0; i < points.Count; i++)
            {
                interpolatedPoints.Add(points[i]);
                if (i == points.Count - 1)
                    break;
                for (int j = 1; j <= interpolation; j++)
                {
                    float t = j / (float)(interpolation + 1);
                    Vector2 interpolatedPoint = Vector2.Lerp(points[i], points[i + 1], t);
                    interpolatedPoints.Add(interpolatedPoint);
                }
            }

            // 插入首尾之间的插值
            for (int j = 1; j <= interpolation; j++)
            {
                float t = j / (float)(interpolation + 1);
                Vector2 interpolatedPoint = Vector2.Lerp(points[points.Count - 1], points[0], t);
                interpolatedPoints.Add(interpolatedPoint);
            }
            return interpolatedPoints;
        }

        /// <summary>
        /// 生成贝塞尔曲线上的点
        /// </summary>
        public static List<Vector2> GenerateBezierCurve(List<Vector2> points, int besselInsertNumber, int numberPoints)
        {
            List<Vector2> controlPoints = new List<Vector2>(points);

            controlPoints.Add(controlPoints[0]);
            List<Vector2> bezierPoints = new List<Vector2>(); // 创建一个Vector2列表用于存储生成的贝塞尔曲线上的点

            // 检查控制点的数量，至少需要4个控制点才能形成一个立方贝塞尔曲线
            if (controlPoints.Count < 4)
            {
                Other.Debug.Log("至少需要4个控制点才能形成立方贝塞尔曲线。"); // 在控制台显示错误消息
                return bezierPoints; // 返回空的贝塞尔点列表
            }

            // 遍历控制点列表，每次取出besselInsertNumber + 1个点生成贝塞尔曲线段
            List<Vector2> pointList = new List<Vector2>();
            for (int i = 0; i < controlPoints.Count - besselInsertNumber; i += besselInsertNumber + 1)
            {
                for (int k = 0; k < besselInsertNumber + 2; k++)
                {
                    pointList.Add(controlPoints[i + k]);
                }
                // 根据所需点的数量在当前曲线段上生成点
                for (int j = 0; j <= numberPoints; j++)
                {
                    float t = j / (float)numberPoints; // 计算参数t的值，用于插值
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
        }

        [Header("展示哪些点的坐标")]
        public ShowGizmosPoint showGizmosPoint;

        public void OnDrawGizmos()
        {
            if (vertexPoints == null)
                return;
        
            if (meshFilter != null && showMesh)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireMesh(meshFilter.sharedMesh, 0, transform.position);
            }
        
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
                    if (i % (besselInsertNumber + 1) != 0)
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
                example.GetComponents();
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
                else Other.Debug.Log("regularEdge should > 3", "#FF0000");
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


        bool isUndoRedoPerformed;
        private void OnSceneGUI()
        {
            BoxDrawer example = (BoxDrawer)target;

            List<Vector2> vertices;
            if (example.isBessel && example.besselPoints.Count > 0)
                vertices = example.besselPoints;
            else
                vertices = example.vertexPoints;

            Vector3 localPosition;
            if (example.parent == null)
                localPosition = example.localPosition;
            else localPosition = example.localPosition + example.parent.localPosition;

            Quaternion rotation = example.rotation;
            BoxDrawer parent = example.parent;
            while (parent != null)
            {
                rotation = example.AddQuaternions(rotation, parent.rotation);
                parent = parent.parent;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newVertexPoints = Quaternion.Inverse(rotation) * Handles.PositionHandle(example.transform.parent.localPosition + example.localPosition + rotation * vertices[i], rotation) - example.transform.parent.localPosition;

                if (EditorGUI.EndChangeCheck())
                {
                    example.GetComponents();
                    Undo.RecordObject(example, "Changed point " + i);
                    vertices[i] = newVertexPoints;
                    if (example.isBessel)
                        if (i % (example.besselInsertNumber + 1) == 0)
                            example.vertexPoints[i / (example.besselInsertNumber + 1)] = newVertexPoints;
                    example.Update();
                    if (!isUndoRedoPerformed)
                    {
                        Undo.undoRedoPerformed += example.Update;
                        isUndoRedoPerformed = true;
                    }
                }
            }


            EditorGUI.BeginChangeCheck();
            Vector3 gameObjectPos = Handles.PositionHandle(example.transform.parent.localPosition + localPosition, rotation) - example.transform.parent.localPosition;
            if (EditorGUI.EndChangeCheck())
            {

                if (example.parent == null) 
                    example.localPosition = gameObjectPos;
                else example.localPosition = gameObjectPos - example.parent.localPosition;

           
            }
        }
    }
#endif
}