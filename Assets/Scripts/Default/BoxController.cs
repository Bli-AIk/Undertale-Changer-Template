using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Õ½¶·¿ò×Ü¿Ø
/// </summary>
public class BoxController : MonoBehaviour
{
    public static BoxController instance;

    public List<BoxDrawer> boxes = new List<BoxDrawer>();

    private void Awake()
    {
        instance = this;
        boxes.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
