using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class DebugTmpGrassMaker : MonoBehaviour
{
    TextMeshPro tmp;
    public List<string> strings;
    public float time, timeMax;
    bool select;
    
    void Start()
    {
        strings.Add("��\n��");
        strings.Add("�� *\n�� *");
        tmp = GetComponent<TextMeshPro>();
        time = timeMax;
    }

    
    void Update()
    {
        if (time < 0)
        {
            time = timeMax;
            select = !select;
            tmp.text = strings[Convert.ToInt32(select)];
        }
        else time -= Time.deltaTime;
    }
}
