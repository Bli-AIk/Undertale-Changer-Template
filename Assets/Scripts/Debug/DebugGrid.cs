using System.Globalization;
using TMPro;
using UCT.Global.Core;
using UnityEngine;

namespace Debug
{
    /// <summary>
    ///     Debug网格定位 用于做弹幕啥的
    /// </summary>
    public class DebugGrid : ObjectPool
    {
        [Header("颜色是给到'条'上面的")] public Color colorX;

        public Color colorY;
        public Color colorXForText;
        public Color colorYForText;

        [Header("横纵分割几片(刀数-1) X为横着平铺竖条 Y则反之")] public int divisionX;

        public int divisionY;

        [Header("XY偏移 如果左右对称就和参考一样填个正的数")] public float deviationX;

        public float deviationY;

        [Header("参考坐标")] public Vector2 referenceX;

        public Vector2 referenceY;

        private void Start()
        {
            poolObject = Resources.Load<GameObject>("Template/Grid Template");
            FillPool<Transform>();

            SummonGrid();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G)) SummonGrid();
        }

        private void SummonGrid()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var grid = transform.GetChild(i).gameObject;
                if (grid.activeSelf)
                    ReturnPool(grid, grid.transform);
            }

            //x
            for (var x = 1; x < divisionX; x++)
            {
                var length = Mathf.Abs(referenceX.y - referenceX.x);
                //Debug.Log(length);
                var objGrid = GetFromPool<Transform>();
                objGrid.transform.localPosition = new Vector3(length / divisionX * x - deviationX, 0, 0);
                objGrid.GetComponent<SpriteRenderer>().color = colorY;
                objGrid.transform.localScale = new Vector3(1, 1000, 1);
                var tmp = objGrid.transform.Find("Text").GetComponent<TextMeshPro>();
                tmp.text = objGrid.transform.localPosition.x.ToString(CultureInfo.InvariantCulture);
                tmp.color = colorYForText;
                tmp.transform.localScale = new Vector3(1, 0.001f, 1);
                tmp.transform.localPosition = new Vector3(0.25f, 0.00475f, 0);
                tmp.fontSize = 3;
            }

            //x
            for (var y = 1; y < divisionX; y++)
            {
                var length = Mathf.Abs(referenceY.y - referenceY.x);
                //Debug.Log(length);
                var objGrid = GetFromPool<Transform>();
                objGrid.transform.localPosition = new Vector3(0, length / divisionY * y - deviationY, 0);
                objGrid.GetComponent<SpriteRenderer>().color = colorX;
                objGrid.transform.localScale = new Vector3(1000, 1, 1);
                var tmp = objGrid.transform.Find("Text").GetComponent<TextMeshPro>();
                tmp.text = objGrid.transform.localPosition.y.ToString(CultureInfo.InvariantCulture);
                tmp.color = colorXForText;
                tmp.transform.localScale = new Vector3(0.001f, 1, 1);
                tmp.transform.localPosition = new Vector3(-0.00625f, 0.25f, 0);
                tmp.fontSize = 3;
            }
        }
    }
}