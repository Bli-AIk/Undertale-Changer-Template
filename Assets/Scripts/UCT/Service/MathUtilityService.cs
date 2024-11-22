using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public static int Get1Or_1()
        {
            int result;
            do
            {
                result = Random.Range(-1, 2);
            } while (result == 0);

            return result;
        }

        /// <summary>
        ///     传入数根据正负返回1/-1。
        ///     传0返1。
        /// </summary>
        public static int Get1Or_1(float input)
        {
            var result = input;

            if (result >= 0)
                result = 1;
            else
                result = -1;

            return (int)result;
        }

        /// <summary>
        ///     计算多边形中点
        /// </summary>
        public static Vector2 CalculatePolygonCenter(List<Vector2> vertexPoints)
        {
            var result = Vector2.zero;
            if (vertexPoints == null || vertexPoints.Count == 0) return result;
            result = vertexPoints.Aggregate(result, (current, vertex) => current + vertex);
            result /= vertexPoints.Count;
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
                if (i < resolutionCutPoint)
                    result.Add(resolutionHeights[i] * 4 / 3);
                else
                    result.Add(resolutionHeights[i] * 16 / 9);

            return result;
        }
    }
}