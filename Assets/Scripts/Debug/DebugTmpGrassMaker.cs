using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Debug
{
    public class DebugTmpGrassMaker : MonoBehaviour
    {
        private TextMeshPro tmp;
        public List<string> strings;
        public float time, timeMax;
        private bool select;

        private void Start()
        {
            strings.Add("²Ý\n²Ý");
            strings.Add("²Ý *\n²Ý *");
            tmp = GetComponent<TextMeshPro>();
            time = timeMax;
        }

        private void Update()
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
}