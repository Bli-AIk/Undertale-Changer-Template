using TMPro;
using UnityEngine;

/// <summary>
/// Debug grid positioning for pop-ups and stuff.
/// </summary>
public class DebugGrid : ObjectPool
{
    [Header("Color is given to the top of the 'bar'")]
    public Color colorX;

    public Color colorY;
    public Color colorXForText;
    public Color colorYForText;

    [Header("Split the slices horizontally and vertically (number of cuts - 1) X is horizontal with flat vertical strips Y and vice versa.")]
    public int divisionX;

    public int divisionY;

    [Header("XY offset If left/right symmetric fill in a positive number as for reference")]
    public float deviationX;

    public float deviationY;

    [Header("Reference coordinates")]
    public Vector2 referenceX;

    public Vector2 referenceY;

    private void Start()
    {
        obj = Resources.Load<GameObject>("Template/Grid Template");
        FillPool();

        SummonGrid();
    }

    private void SummonGrid()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (obj.activeSelf)
                ReturnPool(obj);
        }

        //x
        for (int x = 1; x < divisionX; x++)
        {
            float length = Mathf.Abs(referenceX.y - referenceX.x);
            //DebugLogger.Log(length);
            GameObject objGrid = GetFromPool();
            objGrid.transform.localPosition = new Vector3(length / divisionX * x - deviationX, 0, 0);
            objGrid.GetComponent<SpriteRenderer>().color = colorY;
            objGrid.transform.localScale = new Vector3(1, 1000, 1);
            TextMeshPro tmp = objGrid.transform.Find("Text").GetComponent<TextMeshPro>();
            tmp.text = objGrid.transform.localPosition.x.ToString();
            tmp.color = colorYForText;
            tmp.transform.localScale = new Vector3(1, 0.001f, 1);
            tmp.transform.localPosition = new Vector3(0.25f, 0.00475f, 0);
            tmp.fontSize = 3;
        }
        //x
        for (int y = 1; y < divisionX; y++)
        {
            float length = Mathf.Abs(referenceY.y - referenceY.x);
            //DebugLogger.Log(length);
            GameObject objGrid = GetFromPool();
            objGrid.transform.localPosition = new Vector3(0, length / divisionY * y - deviationY, 0);
            objGrid.GetComponent<SpriteRenderer>().color = colorX;
            objGrid.transform.localScale = new Vector3(1000, 1, 1);
            TextMeshPro tmp = objGrid.transform.Find("Text").GetComponent<TextMeshPro>();
            tmp.text = objGrid.transform.localPosition.y.ToString();
            tmp.color = colorXForText;
            tmp.transform.localScale = new Vector3(0.001f, 1, 1);
            tmp.transform.localPosition = new Vector3(-0.00625f, 0.25f, 0);
            tmp.fontSize = 3;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SummonGrid();
        }
    }
}
