using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    /// 数学相关函数
    /// </summary>
    public class MathUtilityService
    {
        /// <summary>
        /// 随机获取-1或1
        /// </summary>
        public static int Get1Or_1()
        {
            int i;
            do
            {
                i = Random.Range(-1, 2);
            }
            while (i == 0);

            return i;
        }

        /// <summary>
        /// 传入数根据正负返回1/-1。
        /// 传0返1。
        /// </summary>
        public static int Get1Or_1(float i)
        {
            if (i >= 0)
                i = 1;
            else
                i = -1;
            return (int)i;
        }

        /// <summary>
        /// 计算多边形中点
        /// </summary>
        public static Vector2 CalculatePolygonCenter(List<Vector2> vertexPoints)
        {
            var center = Vector2.zero;

            if (vertexPoints == null || vertexPoints.Count == 0)
            {
                return center;
            }

            center = vertexPoints.Aggregate(center, (current, vertex) => current + vertex);

            center /= vertexPoints.Count;

            return center;
        }

        /// <summary>
        /// 在球体表面上生成随机点
        /// </summary>
        public static Vector3 RandomPointOnSphereSurface(float sphereRadius, Vector3 sphereCenter)
        {
            var randomDirection = Random.onUnitSphere;

            randomDirection *= sphereRadius;

            var randomPointOnSphere = sphereCenter + randomDirection;

            return randomPointOnSphere;
        }
    }
}