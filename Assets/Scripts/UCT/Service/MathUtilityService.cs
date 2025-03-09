using System.Collections.Generic;
using System.Linq;
using UCT.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Service
{
    /// <summary>
    ///     数学计算相关函数
    /// </summary>
    public static class MathUtilityService
    {
        /// <summary>
        ///     随机获取-1或1
        /// </summary>
        public static int GetRandomUnit()
        {
            int result;
            do
            {
                result = Random.Range(-1, 2);
            } while (result == 0);

            return result;
        }

        /// <summary>
        ///     在球体表面上生成随机点
        /// </summary>
        public static Vector3 RandomPointOnSphereSurface(float sphereRadius, Vector3 sphereCenter)
        {
            var randomDirection = Random.onUnitSphere;
            randomDirection *= sphereRadius;

            var result = sphereCenter + randomDirection;
            return result;
        }

        /// <summary>
        ///     返回两个数中更大的数
        /// </summary>
        public static float GetGreaterNumber(float number1, float number2)
        {
            return number1 > number2 ? number1 : number2;
        }

        /// <summary>
        ///     返回两个数中更小的数
        /// </summary>
        public static float GetSmallerNumber(float number1, float number2)
        {
            return number1 < number2 ? number1 : number2;
        }

        /// <summary>
        ///     通过分辨率的高度转换得到宽度
        /// </summary>
        /// <param name="resolutionHeights">分辨率高度列表</param>
        /// <param name="resolutionCutPoint">分辨率的切换点，列表中此值之前使用4:3的比例，之后使用16:9的比例</param>
        /// <returns></returns>
        public static List<int> GetResolutionWidthsWithHeights(List<int> resolutionHeights, int resolutionCutPoint)
        {
            var result = new List<int>();

            for (var i = 0; i < resolutionHeights.Count; i++)
            {
                if (i < resolutionCutPoint)
                {
                    result.Add(resolutionHeights[i] * 4 / 3);
                }
                else
                {
                    result.Add(resolutionHeights[i] * 16 / 9);
                }
            }

            return result;
        }

        /// <summary>
        ///     将 AudioMixer 的 dB 值（范围：-80 到 0）转换为归一化值（范围：0 到 1）。
        /// </summary>
        /// <param name="mixerDbValue">AudioMixer 的 dB 值（范围：-80 到 0）。</param>
        /// <returns>归一化的 0 到 1 的值。</returns>
        public static float DbToNormalizedValue(float mixerDbValue)
        {
            return (Mathf.Pow(10, (mixerDbValue + 80) / 80) - 1) / 9;
        }

        /// <summary>
        ///     将归一化值（范围：0 到 1）转换为 AudioMixer 的 dB 值（范围：-80 到 0）。
        /// </summary>
        /// <param name="normalizedValue">归一化的 0 到 1 的值。</param>
        /// <returns>对应的 dB 值（范围：-80 到 0）。</returns>
        public static float NormalizedValueToDb(float normalizedValue)
        {
            return -80 + 80 * Mathf.Log10(1 + 9 * normalizedValue);
        }

        /// <summary>
        ///     判断点是否在多边形内
        /// </summary>
        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            var isInside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (polygon[i].y > point.y != polygon[j].y > point.y &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) +
                    polygon[i].x)
                {
                    isInside = !isInside;
                }
            }

            return isInside; //返回点是否在多边形内的最终结果
        }

        /// <summary>
        ///     计算点到线段最近点（计算垂足）
        /// </summary>
        private static Vector2 GetNearestPointOnLine(Vector2 point, Vector2 start, Vector2 end)
        {
            var line = end - start;
            var len = line.magnitude;
            line.Normalize();

            var v = point - start;
            var d = Vector2.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);
            return start + line * d;
        }

        /// <summary>
        ///     计算位移后垂点位置
        /// </summary>
        public static Vector2 CalculateDisplacedPoint(Vector2 nearestPoint,
            Vector2 lineStart,
            Vector2 lineEnd,
            float displacement)
        {
            var lineDirection = (lineEnd - lineStart).normalized;
            var perpendicularDirection = new Vector2(-lineDirection.y, lineDirection.x);

            return nearestPoint + perpendicularDirection * -displacement;
        }

        /// <summary>
        ///     计算内缩多边形顶点
        /// </summary>
        public static List<Vector2> CalculateInwardOffset(List<Vector2> vertices, float offset)
        {
            if (vertices == null || vertices.Count < 3)
            {
                return new List<Vector2>();
            }

            List<Vector2> offsetVertices = new();
            List<Vector2> intersectionPoints = new();

            var count = vertices.Count;
            for (var i = 0; i < count; i++)
            {
                var currentVertex = vertices[i];
                var nextVertex = vertices[(i + 1) % count];

                var edgeDirection = (nextVertex - currentVertex).normalized;
                var perpendicularDirection = new Vector2(-edgeDirection.y, edgeDirection.x);

                var offsetCurrentVertex = currentVertex + perpendicularDirection * offset;
                var offsetNextVertex = nextVertex + perpendicularDirection * offset;

                offsetVertices.Add(offsetCurrentVertex);
                offsetVertices.Add(offsetNextVertex);

                if (i <= 0)
                {
                    continue;
                }

                var foundIntersection = LineLineIntersection(out var intersection, offsetVertices[i * 2 - 2],
                    offsetVertices[i * 2 - 1], offsetCurrentVertex, offsetNextVertex);
                if (foundIntersection)
                {
                    intersectionPoints.Add(intersection);
                }
            }

            var foundFinalIntersection = LineLineIntersection(out var finalIntersection, offsetVertices[^2],
                offsetVertices[^1], offsetVertices[0], offsetVertices[1]);
            if (foundFinalIntersection)
            {
                intersectionPoints.Add(finalIntersection);
            }

            return intersectionPoints;
        }

        /// <summary>
        ///     线线交点计算
        /// </summary>
        private static bool LineLineIntersection(out Vector2 intersection,
            Vector2 point1,
            Vector2 point2,
            Vector2 point3,
            Vector2 point4)
        {
            intersection = new Vector2(); //初始化交点坐标

            var d = (point1.x - point2.x) * (point3.y - point4.y) -
                    (point1.y - point2.y) * (point3.x - point4.x); //计算分母
            if (Mathf.Approximately(d, 0))
            {
                return false; //如果分母为0，则线段平行或重合，无交点
            }

            float pre = point1.x * point2.y - point1.y * point2.x, post = point3.x * point4.y - point3.y * point4.x;
            intersection.x = (pre * (point3.x - point4.x) - (point1.x - point2.x) * post) / d; //计算交点X坐标
            intersection.y = (pre * (point3.y - point4.y) - (point1.y - point2.y) * post) / d; //计算交点Y坐标

            return true; //返回true，表示找到交点
        }

        public static bool CheckPointBeyondPolygon(Vector3 point,
            float z,
            out Vector2 nearestPoint,
            out Vector2 lineStart,
            out Vector2 lineEnd,
            out bool isParent,
            out Vector3 checkPoint)
        {
            checkPoint = new Vector3();
            nearestPoint = Vector2.zero;
            lineStart = Vector2.zero;
            lineEnd = Vector2.zero;
            var nearestDistance = float.MaxValue;
            isParent = false;

            foreach (var box in BoxController.Instance.boxes.Where(box => Mathf.Approximately(box.localPosition.z, z)))
            {
                for (int i = 0, j = box.GetRealPoints(false).Count - 1;
                     i < box.GetRealPoints(false).Count;
                     j = i++)
                {
                    var tempNearestPoint = GetNearestPointOnLine(point, box.GetRealPoints(false)[i],
                        box.GetRealPoints(false)[j]);
                    var tempDistance = Vector2.Distance(point, tempNearestPoint);
                    if (tempDistance >= nearestDistance)
                    {
                        continue;
                    }

                    nearestPoint = tempNearestPoint;
                    lineStart = box.GetRealPoints(false)[i];
                    lineEnd = box.GetRealPoints(false)[j];
                    nearestDistance = tempDistance;
                    isParent = box.sonBoxDrawer.Count > 0;
                }
            }

            if (nearestDistance < float.MaxValue)
            {
                return false;
            }

            checkPoint = point;
            return true;
        }
    }
}