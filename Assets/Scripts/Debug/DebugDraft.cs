using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DebugDraft : MonoBehaviour
{
    bool wozhenfule;
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        if (!wozhenfule)
        {
            GetComponent<TypeWritter>().TypeOpen("<color=red>text123</color>", false, 0, 0, GetComponent<TMP_Text>());
            wozhenfule = true;
        }
    }
}
