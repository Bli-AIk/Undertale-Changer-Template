using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using LibTessDotNet;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    ///     战斗框生成相关函数
    /// </summary>
    public static class BoxService
    {
        /// <summary>
        ///     生成框
        /// </summary>
        public static void SummonBox(List<Vector2> list,
            Quaternion rotation,
            Transform inputTransform,
            float inputWidth = 0.15f,
            LineRenderer lineRenderer = null,
            EdgeCollider2D edgeCollider2D = null,
            MeshFilter meshFilter = null)
        {
            if (!lineRenderer)
            {
                lineRenderer = inputTransform.GetComponent<LineRenderer>();
                if (!lineRenderer)
                {
                    lineRenderer = inputTransform.gameObject.AddComponent<LineRenderer>();
                    lineRenderer.startWidth = inputWidth;
                    lineRenderer.endWidth = inputWidth;
                    lineRenderer.material = Resources.Load<Material>("Materials/BoxLine");
                    lineRenderer.loop = true;
                }
            }

            if (!edgeCollider2D)
            {
                edgeCollider2D = inputTransform.GetComponent<EdgeCollider2D>();
                if (!edgeCollider2D)
                {
                    edgeCollider2D = inputTransform.gameObject.AddComponent<EdgeCollider2D>();
                }
            }

            if (!meshFilter)
            {
                meshFilter = inputTransform.GetComponent<MeshFilter>();
                if (!meshFilter)
                {
                    meshFilter = inputTransform.gameObject.AddComponent<MeshFilter>();

                    var meshRenderer = inputTransform.GetComponent<MeshRenderer>();
                    if (!meshRenderer)
                    {
                        meshRenderer = inputTransform.gameObject.AddComponent<MeshRenderer>();
                        meshRenderer.material = Resources.Load<Material>("Materials/BoxBack");
                    }
                }
            }

            var polygon = new List<Vector2>(list);
            // 将每个点先旋转，然后再加上物体的位置
            for (var i = 0; i < polygon.Count; i++)
            {
                polygon[i] = rotation * polygon[i];
            }

            polygon = RemoveDuplicates(polygon);

            lineRenderer.positionCount = polygon.Count;

            for (var i = 0; i < polygon.Count; i++)
            {
                lineRenderer.SetPosition(i, (Vector3)polygon[i] + inputTransform.position);
            }

            meshFilter.mesh = GenerateMesh(polygon.ToArray()); // 最核心代码：构建Mesh！！

            edgeCollider2D.SetPoints(AddLists(polygon, new List<Vector2> { polygon[0] }));
            edgeCollider2D.edgeRadius = inputWidth / 2;
        }

        /// <summary>
        ///     计算坐标获取RealPoints
        /// </summary>
        public static List<Vector2> GetRealPoints(List<Vector2> list,
            Quaternion rotation,
            Transform inputTransform,
            bool isLocal = true)
        {
            var local = isLocal ? inputTransform.localPosition : inputTransform.position;

            var polygon = new List<Vector2>(list);
            // 将每个点先旋转，然后再加上物体的位置
            for (var i = 0; i < polygon.Count; i++)
            {
                polygon[i] = rotation * polygon[i] + local;
            }

            polygon = RemoveDuplicates(polygon);

            return polygon;
        }

        /// <summary>
        ///     构造Mesh
        /// </summary>
        private static Mesh GenerateMesh(Vector2[] vertexPoints)
        {
            // 将Vector数组转换为LibTessDotNet所需的ContourVertex数组
            var contourVertices = new ContourVertex[vertexPoints.Length];
            for (var i = 0; i < vertexPoints.Length; i++)
            {
                contourVertices[i].Position = new Vec3 { X = vertexPoints[i].x, Y = vertexPoints[i].y, Z = 0 };
            }

            // 创建Tess对象并添加轮廓
            var tess = new Tess();
            tess.AddContour(contourVertices);

            // 进行三角剖分
            tess.Tessellate(WindingRule.NonZero);

            // 创建Mesh对象
            var mesh = new Mesh();

            // 将Tess结果转换为Unity Mesh格式
            var vertices = new Vector3[tess.Vertices.Length];
            for (var i = 0; i < tess.Vertices.Length; i++)
            {
                vertices[i] = new Vector3(tess.Vertices[i].Position.X, tess.Vertices[i].Position.Y, 0);
            }

            var triangles = new int[tess.Elements.Length];
            for (var i = 0; i < tess.Elements.Length; i++)
            {
                triangles[i] = tess.Elements[i];
            }

            // 应用顶点和三角形到mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // 为mesh设置UV坐标
            var uvs = new Vector2[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
                // 这里是一个简单的映射，将顶点坐标映射到UV空间
                // 通常，你需要根据具体情况来调整这部分代码
            {
                uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
            }

            mesh.uv = uvs;

            // 为了更好地渲染效果，可以计算法线和边界
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // 将mesh应用到GameObject
            return mesh;
        }

        /// <summary>
        ///     剔除重复项
        /// </summary>
        private static List<Vector2> RemoveDuplicates(List<Vector2> originalList)
        {
            // 使用HashSet<Vector2>来存储已经遇到的Vector2元素，因为HashSet自动去重
            // 用来存储去重后的列表
            return originalList.Distinct().ToList();
        }

        /// <summary>
        ///     主函数，计算两组线段的所有交点
        /// </summary>
        public static List<Vector2> FindIntersections(List<Vector2> poly1, List<Vector2> poly2)
        {
            var intersections = new List<Vector2>();

            for (var i = 0; i < poly1.Count; i++)
            {
                var a = poly1[i];
                var b = poly1[(i + 1) % poly1.Count]; // 循环列表

                for (var j = 0; j < poly2.Count; j++)
                {
                    var c = poly2[j];
                    var d = poly2[(j + 1) % poly2.Count]; // 循环列表

                    if (!DoLineSegmentsIntersect(a, b, c, d))
                    {
                        continue;
                    }

                    var intersection = CalculateIntersectionPoint(a, b, c, d);
                    if (intersection != null)
                    {
                        intersections.Add(intersection.Value);
                    }
                }
            }

            return intersections;
        }

        /// <summary>
        ///     计算向量叉乘
        /// </summary>
        private static float CrossSave(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        /// <summary>
        ///     检查点C是否在AB线段上
        /// </summary>
        private static bool IsPointOnLineSegment(Vector2 linePointA, Vector2 linePointB, Vector2 pointC)
        {
            return Mathf.Approximately(CrossSave(linePointA, linePointB, pointC), 0f) &&
                   (pointC.x - linePointA.x) * (pointC.x - linePointB.x) <= 0 &&
                   (pointC.y - linePointA.y) * (pointC.y - linePointB.y) <= 0;
        }

        /// <summary>
        ///     检查线段AB和CD是否相交
        /// </summary>
        private static bool DoLineSegmentsIntersect(Vector2 linePointA,
            Vector2 linePointB,
            Vector2 linePointC,
            Vector2 linePointD)
        {
            if (IsPointOnLineSegment(linePointA, linePointB, linePointC) ||
                IsPointOnLineSegment(linePointA, linePointB, linePointD) ||
                IsPointOnLineSegment(linePointC, linePointD, linePointA) ||
                IsPointOnLineSegment(linePointC, linePointD, linePointB))
            {
                return true;
            }

            return CrossSave(linePointA, linePointB, linePointC) * CrossSave(linePointA, linePointB, linePointD) < 0 &&
                   CrossSave(linePointC, linePointD, linePointA) * CrossSave(linePointC, linePointD, linePointB) < 0;
        }

        /// <summary>
        ///     计算两线段AB和CD的交点
        /// </summary>
        private static Vector2? CalculateIntersectionPoint(Vector2 linePointA,
            Vector2 linePointB,
            Vector2 linePointC,
            Vector2 linePointD)
        {
            if (!DoLineSegmentsIntersect(linePointA, linePointB, linePointC, linePointD))
            {
                return null;
            }

            // 计算线性方程的参数
            var denominator = (linePointB.x - linePointA.x) * (linePointD.y - linePointC.y) -
                              (linePointB.y - linePointA.y) * (linePointD.x - linePointC.x);
            if (Mathf.Approximately(denominator, 0))
            {
                return null; // 线段平行或共线
            }

            var u = ((linePointC.x - linePointA.x) * (linePointD.y - linePointC.y) -
                     (linePointC.y - linePointA.y) * (linePointD.x - linePointC.x)) / denominator;
            return new Vector2(linePointA.x + u * (linePointB.x - linePointA.x),
                linePointA.y + u * (linePointB.y - linePointA.y));
        }

        /// <summary>
        ///     计算非重合点
        /// </summary>
        public static List<Vector2> ProcessPolygons(List<Vector2> box1, List<Vector2> box2)
        {
            var filteredBox1 = RemovePointsInsideOtherPolygon(box1, box2);
            var filteredBox2 = RemovePointsInsideOtherPolygon(box2, box1);

            // 合并剔除后的列表
            var result = new List<Vector2>();
            result.AddRange(filteredBox1);
            result.AddRange(filteredBox2);

            return result;
        }

        private static List<Vector2> RemovePointsInsideOtherPolygon(List<Vector2> subjectPolygon,
            List<Vector2> clippingPolygon)
        {
            return subjectPolygon.Where(point => !IsPointInsidePolygon(point, clippingPolygon)).ToList();
        }

        private static bool IsPointInsidePolygon(Vector2 point, List<Vector2> polygon)
        {
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (polygon[i].y > point.y != polygon[j].y > point.y &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) +
                    polygon[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        ///     前面两个相加，减去后面两个
        /// </summary>
        public static List<Vector2> AddAndSubLists(List<Vector2> list1,
            List<Vector2> list2,
            List<Vector2> list3,
            List<Vector2> list4)
        {
            var concatenatedList = AddLists(list1, list2);
            var subtractedResult = SubLists(concatenatedList, list3);
            subtractedResult = SubLists(subtractedResult, list4);

            return subtractedResult;
        }

        /// <summary>
        ///     把List相加
        /// </summary>
        public static List<T> AddLists<T>(List<T> list1, List<T> list2)
        {
            var concatenatedList = new List<T>(list1);
            concatenatedList.AddRange(list2);
            return concatenatedList;
        }

        /// <summary>
        ///     把List相减
        /// </summary>
        private static List<T> SubLists<T>(List<T> sourceList, List<T> subtractedList)
        {
            var result = new List<T>(sourceList);

            foreach (var point in subtractedList)
            {
                result.Remove(point);
            }

            return result;
        }

        private static PathsD ConvertVectorToPath(List<Vector2> vector)
        {
            var doubles = new List<double>();
            var j = 0;
            for (var i = 0; i < vector.Count * 2; i++)
            {
                if (i % 2 == 0) //X
                {
                    doubles.Add(vector[j].x);
                }
                else //Y
                {
                    doubles.Add(vector[j].y);
                    j++;
                }
            }

            return new PathsD { Clipper.MakePath(doubles.ToArray()) };
        }

        private static List<Vector2> ConvertPathToVector(PathsD path)
        {
            var list = new List<Vector2>();
            for (var i = 0; i < path[0].Count; i++)
            {
                list.Add(new Vector2((float)path[0][i].x, (float)path[0][i].y));
            }

            return list;
        }

        /// <summary>
        ///     取交集
        /// </summary>
        public static List<Vector2> GetUnion(List<Vector2> a, List<Vector2> b)
        {
            var subj = ConvertVectorToPath(a);
            var clip = ConvertVectorToPath(b);
            var solution = Clipper.Union(subj, clip, FillRule.NonZero);

            return ConvertPathToVector(solution);
        }

        public static List<Vector2> GetDifference(List<Vector2> origin, List<Vector2> sub)
        {
            var subj = ConvertVectorToPath(origin);
            var clip = ConvertVectorToPath(sub);
            var solution = Clipper.Difference(subj, clip, FillRule.NonZero);
            return ConvertPathToVector(solution);
        }
    }
}